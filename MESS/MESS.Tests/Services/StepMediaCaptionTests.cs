using ClosedXML.Excel;
using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.CRUD.PartDefinitions;
using MESS.Services.CRUD.Products;
using MESS.Services.CRUD.WorkInstructions;
using MESS.Services.DTOs.WorkInstructions.File;
using MESS.Services.DTOs.WorkInstructions.Form;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.File;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.Form;
using MESS.Services.Files.WorkInstructions;
using MESS.Services.Media.WorkInstructions;
using MESS.Services.UI.WorkInstructionEditor;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MESS.Tests.Services;

/// <summary>
/// Per-photo captions on work instruction steps: persistence (jsonb conversion + value comparer),
/// mappers, the updater (normal and minimal mode), version cloning, image deletion and xlsx export/import.
/// </summary>
public class StepMediaCaptionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationContext> _options;
    private readonly string _webRoot;

    public StepMediaCaptionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<ApplicationContext>().UseSqlite(_connection).Options;

        using var context = new ApplicationContext(_options);
        context.Database.EnsureCreated();

        _webRoot = Path.Combine(Path.GetTempPath(), "mess-caption-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_webRoot, "WorkInstructionImages"));
    }

    public void Dispose()
    {
        _connection.Dispose();
        try { Directory.Delete(_webRoot, recursive: true); } catch (IOException) { }
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ApplicationContext NewContext() => new(_options);

    private IWebHostEnvironment WebHost()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.SetupGet(e => e.WebRootPath).Returns(_webRoot);
        return env.Object;
    }

    private string CreateImage(string name)
    {
        var relative = Path.Combine("WorkInstructionImages", name);
        using var image = new Image<Rgba32>(4, 4);
        image.SaveAsPng(Path.Combine(_webRoot, relative));
        return relative;
    }

    private static Step NewStep(List<StepMediaItem> primary, List<StepMediaItem>? secondary = null) => new()
    {
        Name = "Step",
        Body = "Body",
        NodeType = WorkInstructionNodeType.Step,
        PrimaryMedia = primary,
        SecondaryMedia = secondary ?? []
    };

    private async Task<(int InstructionId, int StepId)> SeedStepAsync(List<StepMediaItem> primary, List<StepMediaItem>? secondary = null)
    {
        await using var context = NewContext();
        var step = NewStep(primary, secondary);
        var instruction = new WorkInstruction { Title = "WI", Version = "1.0", Nodes = [step] };
        context.WorkInstructions.Add(instruction);
        await context.SaveChangesAsync(Ct);
        return (instruction.Id, step.Id);
    }

    private async Task<Step> LoadStepAsync(int stepId)
    {
        await using var context = NewContext();
        return await context.Steps.AsNoTracking().SingleAsync(s => s.Id == stepId, Ct);
    }

    // ---------------- Persistence ----------------

    [Fact]
    public async Task Media_RoundTripsThroughEf_WithCaptionsAndOrder()
    {
        var (_, stepId) = await SeedStepAsync(
            [new StepMediaItem { Path = "a.png", Caption = "Figure 1" }, new StepMediaItem { Path = "b.png" }],
            [new StepMediaItem { Path = "c.png", Caption = "Detail" }]);

        var step = await LoadStepAsync(stepId);

        Assert.Equal(["a.png", "b.png"], step.PrimaryMedia.Select(m => m.Path));
        Assert.Equal(["Figure 1", null], step.PrimaryMedia.Select(m => m.Caption));
        Assert.Equal("Detail", Assert.Single(step.SecondaryMedia).Caption);
    }

    [Fact]
    public async Task Media_StepWithNoImages_LoadsAsEmptyLists()
    {
        var (_, stepId) = await SeedStepAsync([]);

        var step = await LoadStepAsync(stepId);

        Assert.Empty(step.PrimaryMedia);
        Assert.Empty(step.SecondaryMedia);
    }

    [Fact]
    public async Task Media_InPlaceCaptionEdit_IsDetectedAndPersisted()
    {
        // Exercises the ValueComparer: only a nested property changes, the list reference does not.
        var (_, stepId) = await SeedStepAsync([new StepMediaItem { Path = "a.png", Caption = "Old" }]);

        await using (var context = NewContext())
        {
            var step = await context.Steps.SingleAsync(s => s.Id == stepId, Ct);
            step.PrimaryMedia[0].Caption = "New";

            Assert.True(context.Entry(step).Property(s => s.PrimaryMedia).IsModified
                        || context.ChangeTracker.HasChanges());
            await context.SaveChangesAsync(Ct);
        }

        Assert.Equal("New", (await LoadStepAsync(stepId)).PrimaryMedia[0].Caption);
    }

    [Fact]
    public async Task Media_UnchangedMedia_IsNotMarkedModified()
    {
        var (_, stepId) = await SeedStepAsync([new StepMediaItem { Path = "a.png", Caption = "Same" }]);

        await using var context = NewContext();
        var step = await context.Steps.SingleAsync(s => s.Id == stepId, Ct);
        step.PrimaryMedia = [new StepMediaItem { Path = "a.png", Caption = "Same" }];
        context.ChangeTracker.DetectChanges();

        Assert.False(context.Entry(step).Property(s => s.PrimaryMedia).IsModified);
    }

    // ---------------- Mappers ----------------

    [Fact]
    public void FormMapper_PreservesPathAndCaption_AndDoesNotShareItems()
    {
        var entity = NewStep([new StepMediaItem { Path = "a.png", Caption = "Cap" }], [new StepMediaItem { Path = "b.png" }]);

        var dto = entity.ToFormDTO(Guid.NewGuid());
        dto.PrimaryMedia[0].Caption = "Changed in DTO";

        Assert.Equal("Cap", entity.PrimaryMedia[0].Caption);
        Assert.Equal("b.png", Assert.Single(dto.SecondaryMedia).Path);

        var back = dto.ToNewEntity();
        Assert.Equal("a.png", back.PrimaryMedia[0].Path);
        Assert.Equal("Changed in DTO", back.PrimaryMedia[0].Caption);
    }

    [Fact]
    public void FileMapper_RoundTripsCaptions()
    {
        var entity = NewStep([new StepMediaItem { Path = "a.png", Caption = "Cap" }]);

        var form = entity.ToFileDTO().ToFormDTO();

        var item = Assert.Single(form.PrimaryMedia);
        Assert.Equal("a.png", item.Path);
        Assert.Equal("Cap", item.Caption);
    }

    [Fact]
    public void ToPersistableItems_TrimsBlankAndOverlongCaptions()
    {
        var items = new List<StepMediaItem>
        {
            new() { Path = "a.png", Caption = "  padded  " },
            new() { Path = "b.png", Caption = "   " },
            new() { Path = "c.png", Caption = new string('x', 250) }
        }.ToPersistableItems();

        Assert.Equal("padded", items[0].Caption);
        Assert.Null(items[1].Caption);
        Assert.Equal(StepMediaItem.CaptionMaxLength, items[2].Caption!.Length);
    }

    // ---------------- Updater ----------------

    private static WorkInstructionUpdater CreateUpdater()
    {
        var products = new Mock<IProductResolver>();
        products.Setup(p => p.ResolveProductsAsync(It.IsAny<ApplicationContext>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);
        return new WorkInstructionUpdater(products.Object, Mock.Of<IPartDefinitionResolver>());
    }

    private async Task ApplyCaptionEditAsync(bool minimalMode, int instructionId, int stepId, string caption)
    {
        await using var context = NewContext();
        var entity = await context.WorkInstructions
            .Include(w => w.Nodes)
            .SingleAsync(w => w.Id == instructionId, Ct);

        var step = (Step)entity.Nodes.Single();
        var stepDto = step.ToFormDTO(Guid.NewGuid());
        stepDto.PrimaryMedia[0].Caption = caption;

        var dto = new WorkInstructionFormDTO
        {
            Id = instructionId,
            Title = entity.Title,
            Version = entity.Version,
            Nodes = [stepDto]
        };

        await CreateUpdater().ApplyAsync(dto, entity, context, minimalMode);
        await context.SaveChangesAsync(Ct);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Updater_PersistsCaptionEdits_InNormalAndMinimalMode(bool minimalMode)
    {
        var (instructionId, stepId) = await SeedStepAsync([new StepMediaItem { Path = "a.png" }]);

        await ApplyCaptionEditAsync(minimalMode, instructionId, stepId, "  Figure 1  ");

        var saved = await LoadStepAsync(stepId);
        Assert.Equal("a.png", saved.PrimaryMedia[0].Path);
        Assert.Equal("Figure 1", saved.PrimaryMedia[0].Caption);
    }

    // ---------------- Version cloning ----------------

    [Fact]
    public async Task Clone_CopiesCaptions_WithNewImageFiles()
    {
        var images = new Mock<IWorkInstructionImageService>();
        images.Setup(i => i.SaveImageFileAsync(It.IsAny<string>()))
            .ReturnsAsync((string path) => "copy-" + path);

        var editor = new WorkInstructionEditorService(
            Mock.Of<IWorkInstructionService>(), images.Object, Mock.Of<IPartDefinitionService>());

        await editor.LoadImportedAsync(new WorkInstructionFormDTO
        {
            Title = "WI",
            Nodes =
            [
                new StepNodeFormDTO
                {
                    Name = "S", Body = "B", NodeType = WorkInstructionNodeType.Step,
                    PrimaryMedia = [new StepMediaItem { Path = "a.png", Caption = "Front" }, new StepMediaItem { Path = "b.png" }],
                    SecondaryMedia = [new StepMediaItem { Path = "c.png", Caption = "Back" }]
                }
            ]
        });

        var cloned = Assert.IsType<StepNodeFormDTO>(Assert.Single(editor.Current!.Nodes));
        Assert.Equal(["copy-a.png", "copy-b.png"], cloned.PrimaryMedia.Select(m => m.Path));
        Assert.Equal(["Front", null], cloned.PrimaryMedia.Select(m => m.Caption));
        Assert.Equal(("copy-c.png", "Back"), (cloned.SecondaryMedia[0].Path, cloned.SecondaryMedia[0].Caption));
    }

    // ---------------- Image deletion ----------------

    [Fact]
    public async Task DeleteImagesByNodes_DeletesFilesForEveryMediaItem()
    {
        var primary = CreateImage("p.png");
        var secondary = CreateImage("s.png");
        var service = new WorkInstructionImageService(WebHost());

        await service.DeleteImagesByNodesAsync(
            [NewStep([new StepMediaItem { Path = primary, Caption = "Cap" }], [new StepMediaItem { Path = secondary }])]);

        Assert.False(File.Exists(Path.Combine(_webRoot, primary)));
        Assert.False(File.Exists(Path.Combine(_webRoot, secondary)));
    }

    // ---------------- XLSX export / import ----------------

    [Fact]
    public void CaptionsCell_FormatAndParse_KeepPositions()
    {
        var media = new List<StepMediaItem>
        {
            new() { Path = "a", Caption = "One" },
            new() { Path = "b" },
            new() { Path = "c", Caption = "Three\r\nlines" }
        };

        var cell = WorkInstructionFileService.FormatCaptionsCell(media);
        var parsed = WorkInstructionFileService.ParseCaptionsCell(cell);

        Assert.Equal(["One", null, "Three lines"], parsed);
    }

    [Fact]
    public void CaptionsCell_NoCaptions_IsEmpty()
    {
        Assert.Equal(string.Empty, WorkInstructionFileService.FormatCaptionsCell([new StepMediaItem { Path = "a" }]));
        Assert.Empty(WorkInstructionFileService.ParseCaptionsCell(null));
    }

    private static IBrowserFile BrowserFile(string path)
    {
        var file = new Mock<IBrowserFile>();
        file.SetupGet(f => f.Name).Returns(Path.GetFileName(path));
        file.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(() => File.OpenRead(path));
        return file.Object;
    }

    private async Task<string> ExportCaptionedInstructionAsync(WorkInstructionFileService service)
    {
        var instruction = new WorkInstructionFileDTO
        {
            Title = "Caption WI",
            Version = "1.0",
            AssociatedProductNames = ["Prod"],
            Nodes =
            [
                new StepNodeFileDTO
                {
                    Name = "Step", Body = "Body", Position = 0, NodeType = WorkInstructionNodeType.Step,
                    PrimaryMedia =
                    [
                        new StepMediaItem { Path = CreateImage("one.png"), Caption = "Figure 1" },
                        new StepMediaItem { Path = CreateImage("two.png") }
                    ],
                    SecondaryMedia = [new StepMediaItem { Path = CreateImage("three.png"), Caption = "Detail" }]
                }
            ]
        };

        var exported = await service.ExportToXlsxAsync(instruction, cancellationToken: Ct);
        Assert.NotNull(exported);
        return exported;
    }

    [Fact]
    public async Task Xlsx_ExportThenImport_RoundTripsCaptions()
    {
        var service = new WorkInstructionFileService(WebHost());
        var exported = await ExportCaptionedInstructionAsync(service);

        var result = await service.ImportFromXlsx(BrowserFile(exported));

        var step = Assert.IsType<StepNodeFileDTO>(Assert.Single(result.WorkInstruction!.Nodes));
        Assert.Equal(["Figure 1", null], step.PrimaryMedia.Select(m => m.Caption));
        Assert.Equal("Detail", Assert.Single(step.SecondaryMedia).Caption);
        Assert.All(step.PrimaryMedia.Concat(step.SecondaryMedia),
            m => Assert.True(File.Exists(Path.Combine(_webRoot, m.Path))));
    }

    [Fact]
    public async Task Xlsx_LegacyFileWithoutCaptionColumns_ImportsImagesUncaptioned()
    {
        var service = new WorkInstructionFileService(WebHost());
        var exported = await ExportCaptionedInstructionAsync(service);

        // Simulate a pre-caption export: the caption columns did not exist.
        using (var workbook = new XLWorkbook(exported))
        {
            var sheet = workbook.Worksheet(1);
            foreach (var column in new[] { 7, 8 })
                foreach (var cell in sheet.Column(column).CellsUsed())
                    cell.Clear();
            workbook.Save();
        }

        var result = await service.ImportFromXlsx(BrowserFile(exported));

        var step = Assert.IsType<StepNodeFileDTO>(Assert.Single(result.WorkInstruction!.Nodes));
        Assert.Equal(2, step.PrimaryMedia.Count);
        Assert.Single(step.SecondaryMedia);
        Assert.All(step.PrimaryMedia.Concat(step.SecondaryMedia), m => Assert.Null(m.Caption));
    }

    // ---------------- Validation ----------------

    [Fact]
    public void StepValidator_RejectsOverlongCaption_AcceptsMissingCaption()
    {
        var validator = new StepValidator();

        Assert.True(validator.Validate(NewStep([new StepMediaItem { Path = "a.png" }])).IsValid);
        Assert.True(validator.Validate(NewStep([new StepMediaItem { Path = "a.png", Caption = new string('x', 200) }])).IsValid);

        var result = validator.Validate(NewStep([], [new StepMediaItem { Path = "a.png", Caption = new string('x', 201) }]));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void WorkInstructionValidator_ValidatesStepCaptions()
    {
        var instruction = new WorkInstruction
        {
            Title = "WI",
            Nodes = [NewStep([new StepMediaItem { Path = "a.png", Caption = new string('x', 201) }])]
        };

        Assert.False(new WorkInstructionValidator().Validate(instruction).IsValid);
    }
}
