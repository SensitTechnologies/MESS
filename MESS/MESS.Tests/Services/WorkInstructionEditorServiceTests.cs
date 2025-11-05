using MESS.Data.Models;
using MESS.Services.CRUD.WorkInstructions;
using MESS.Services.Media.WorkInstructions;
using MESS.Services.UI.WorkInstructionEditor;
using Moq;

namespace MESS.Tests.Services;

public class WorkInstructionEditorServiceTests
{
    private readonly Mock<IWorkInstructionService> _mockWorkInstructionService;
    private readonly Mock<IWorkInstructionImageService> _mockImageService;
    private readonly WorkInstructionEditorService _sut;

    public WorkInstructionEditorServiceTests()
    {
        _mockWorkInstructionService = new Mock<IWorkInstructionService>();
        _mockImageService = new Mock<IWorkInstructionImageService>();
        _sut = new WorkInstructionEditorService(_mockWorkInstructionService.Object, _mockImageService.Object);
    }

    [Fact]
    public void StartNew_ShouldInitializeNewInstruction()
    {
        // Act
        _sut.StartNew("Test WI");

        // Assert
        Assert.NotNull(_sut.Current);
        Assert.Equal("Test WI", _sut.Current!.Title);
        Assert.Equal("1.0", _sut.Current.Version);
        Assert.Equal(EditorMode.CreateNew, _sut.Mode);
        Assert.True(_sut.IsDirty);
    }

    [Fact]
    public async Task StartNewFromCurrent_ShouldCloneFromCurrent()
    {
        // Arrange
        _sut.StartNew("Base WI");
        _sut.Current!.ShouldGenerateQrCode = true;
        _sut.Current.Nodes.Add(new Step
        {
            Name = "Step 1",
            Body = "Body 1"
        });

        _mockImageService.Setup(s => s.SaveImageFileAsync(It.IsAny<string>()))
            .ReturnsAsync("cloned.png");

        // Act
        await _sut.StartNewFromCurrent("Cloned WI");

        // Assert
        Assert.NotNull(_sut.Current);
        Assert.Equal("Cloned WI", _sut.Current!.Title);
        Assert.Single(_sut.Current.Nodes);
        Assert.Equal(EditorMode.CreateNew, _sut.Mode);
    }

    [Fact]
    public async Task LoadForEditAsync_ShouldLoadExistingInstruction()
    {
        // Arrange
        var wi = new WorkInstruction { Id = 42, Title = "Existing" };
        _mockWorkInstructionService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync(wi);

        // Act
        await _sut.LoadForEditAsync(42);

        // Assert
        Assert.Equal(wi, _sut.Current);
        Assert.Equal(EditorMode.EditExisting, _sut.Mode);
        Assert.False(_sut.IsDirty);
    }

    [Fact]
    public async Task LoadForEditAsync_NotFound_ShouldThrow()
    {
        _mockWorkInstructionService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((WorkInstruction?)null);

        await Assert.ThrowsAsync<Exception>(() => _sut.LoadForEditAsync(99));
    }

    [Fact]
    public async Task SaveAsync_CreateNew_ShouldCallCreateAndMarkClean()
    {
        // Arrange
        _sut.StartNew("New WI");
        _mockWorkInstructionService.Setup(s => s.Create(It.IsAny<WorkInstruction>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.SaveAsync();

        // Assert
        Assert.True(result);
        Assert.False(_sut.IsDirty);
        Assert.Equal(EditorMode.EditExisting, _sut.Mode);
        _mockWorkInstructionService.Verify(s => s.Create(It.IsAny<WorkInstruction>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_EditExisting_ShouldCallUpdate()
    {
        // Arrange
        var wi = new WorkInstruction { Id = 5, Title = "Existing" };
        _mockWorkInstructionService.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(wi);
        _mockWorkInstructionService.Setup(s => s.UpdateWorkInstructionAsync(It.IsAny<WorkInstruction>()))
            .ReturnsAsync(true);

        // put service into EditExisting state properly
        await _sut.LoadForEditAsync(5);

        // Act
        var result = await _sut.SaveAsync();

        // Assert
        Assert.True(result);
        _mockWorkInstructionService.Verify(s => s.UpdateWorkInstructionAsync(It.Is<WorkInstruction>(w => w.Id == 5)), Times.Once);
    }

    [Fact]
    public void QueueNodeForDeletion_ShouldAddNode()
    {
        var node = new Step
        {
            Name = "Step A",
            Body = "Body A"
        };

        _sut.QueueNodeForDeletion(node);

        Assert.Contains(node, _sut.NodesQueuedForDeletion);
    }

    [Fact]
    public void ToggleActive_ShouldFlipAndMarkDirty()
    {
        _sut.StartNew("WI");
        var before = _sut.Current!.IsActive;

        _sut.ToggleActive();

        Assert.NotEqual(before, _sut.Current.IsActive);
        Assert.True(_sut.IsDirty);
    }

    [Fact]
    public void CurrentHasPartsOrSteps_ShouldReturnExpected()
    {
        _sut.StartNew("WI");
        Assert.False(_sut.CurrentHasParts());
        Assert.False(_sut.CurrentHasSteps());

        _sut.Current!.Nodes.Add(new PartNode{ PartDefinition = new PartDefinition { Name = "Some Part", Number = "Some Number"}});
        _sut.Current!.Nodes.Add(new Step { Name = "Some name", Body = "Some body" });

        Assert.True(_sut.CurrentHasParts());
        Assert.True(_sut.CurrentHasSteps());
    }
}
