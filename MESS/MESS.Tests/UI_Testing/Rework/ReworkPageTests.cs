using Bunit;
using MESS.Blazor.Components.Pages.Phoebe.Rework;
using MESS.Data.Models;
using MESS.Services.CRUD.ApplicationUser;
using MESS.Services.CRUD.Defects;
using MESS.Services.CRUD.Products;
using MESS.Services.CRUD.Rework;
using MESS.Services.CRUD.WorkInstructions;
using MESS.Services.DTOs.Defects;
using MESS.Services.DTOs.Products.Detail;
using MESS.Services.DTOs.Products.Summary;
using MESS.Services.DTOs.Rework;
using MESS.Services.DTOs.WorkInstructions.Summary;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Moq;

namespace MESS.Tests.UI_Testing.Rework;

/// <summary>
/// bUnit tests for the Rework page and its edit dialog, with every service mocked.
/// </summary>
public class ReworkPageTests : BunitContext
{
    private readonly Mock<IReworkService> _rework = new();
    private readonly Mock<IProductService> _products = new();
    private readonly Mock<IWorkInstructionService> _instructions = new();
    private readonly Mock<IDefectCodeService> _defects = new();
    private readonly Mock<IApplicationUserService> _users = new();
    private List<ReworkRowDTO> _rows = [Row(5), Row(6)];

    public ReworkPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var auth = this.AddAuthorization();
        auth.SetAuthorized("tech");
        auth.SetRoles("Technician");

        Services.AddSingleton(_rework.Object);
        Services.AddSingleton(_products.Object);
        Services.AddSingleton(_instructions.Object);
        Services.AddSingleton(_defects.Object);
        Services.AddSingleton(_users.Object);
        Services.AddSingleton(Mock.Of<IToastService>());

        _rework.Setup(s => s.GetFilterOptionsAsync()).ReturnsAsync(new ReworkFilterOptionsDTO());
        _rework.Setup(s => s.GetPageAsync(It.IsAny<ReworkQuery>()))
            .ReturnsAsync(() => new ReworkPageDTO { Data = _rows, Total = _rows.Count, Page = 1, PageSize = 50, TotalPages = 1 });

        _rework.Setup(s => s.GetForEditAsync(5)).ReturnsAsync(EditDto);
        _rework.Setup(s => s.GetStepsForInstructionAsync(1))
            .ReturnsAsync([new ReworkStepOptionDTO { Id = 100, Name = "Step 1" }]);
        _rework.Setup(s => s.GetStepsForInstructionAsync(2))
            .ReturnsAsync([new ReworkStepOptionDTO { Id = 200, Name = "Other Step" }]);

        _products.Setup(s => s.GetAllDetailsAsync()).ReturnsAsync(
        [
            new ProductDetailDTO { ProductId = 1, Name = "Prod One", IsActive = true }
        ]);
        var productSummary = new ProductSummaryDTO { ProductId = 1, Name = "Prod One", Number = "P-1" };
        _instructions.Setup(s => s.GetAllSummariesAsync()).ReturnsAsync(
        [
            new WorkInstructionSummaryDTO { Id = 1, Title = "WI-A", Version = "1.0", Products = [productSummary] },
            new WorkInstructionSummaryDTO { Id = 2, Title = "WI-B", Products = [productSummary] }
        ]);
        _users.Setup(s => s.GetAllAsync()).ReturnsAsync(
        [
            // op1 (the log's operator) is deliberately absent, as if the user was deleted.
            new ApplicationUser { Id = "op2", FirstName = "Bob", LastName = "Op", IsActive = true },
            new ApplicationUser { Id = "op3", FirstName = "Cy", LastName = "Op", IsActive = false }
        ]);
        _defects.Setup(s => s.GetNounsForWorkInstructionAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DefectCodeOptionDto { Id = 10, Name = "Housing" }]);
        _defects.Setup(s => s.GetNounsForWorkInstructionAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DefectCodeOptionDto { Id = 30, Name = "Cable" }]);
        _defects.Setup(s => s.GetAdjectivesForNounAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DefectCodeOptionDto { Id = 20, Name = "Cracked" }]);
    }

    private static ReworkRowDTO Row(int id) => new()
    {
        Id = id, ProductionLogId = 9, Success = false, Notes = $"note {id}", SubmitTime = DateTimeOffset.UtcNow,
        NounName = "Housing", AdjectiveName = "Cracked", ProductName = "Prod One", InstructionName = "WI-A (1.0)",
        OperatorName = "Ann Op", StepName = "Step 1"
    };

    private static ReworkEditDTO EditDto() => new()
    {
        AttemptId = 5,
        ProductionLogId = 9,
        SubmitTime = DateTimeOffset.UtcNow,
        Values = new ReworkEditValues
        {
            Success = false, Notes = "Cracked housing near the clip", FailureNounId = 10, FailureAdjectiveId = 20, ProductId = 1, WorkInstructionId = 1,
            OperatorId = "op1", WorkInstructionStepId = 100
        },
        ProductName = "Prod One",
        InstructionName = "WI-A (1.0)",
        OperatorName = "Ann Op",
        StepName = "Step 1",
        FailureNounName = "Housing",
        FailureAdjectiveName = "Cracked",
        OtherAttemptCount = 2,
        OtherStepCount = 1
    };

    private IRenderedComponent<ReworkEditDialog> RenderDialog(Action<string?>? onSaved = null)
        => Render<ReworkEditDialog>(p => p
            .Add(d => d.AttemptId, 5)
            .Add(d => d.ModifiedBy, "tech")
            .Add(d => d.OnSaved, EventCallback.Factory.Create<string?>(this, m => onSaved?.Invoke(m))));

    private static bool SaveDisabled(IRenderedComponent<ReworkEditDialog> cut)
        => cut.Find(".modal-footer .btn-primary").HasAttribute("disabled");

    // ── Page ────────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Table_RendersTenColumnHeadersAndOneRowPerDto()
    {
        var cut = Render<MESS.Blazor.Components.Pages.Phoebe.Rework.Rework>();

        var headers = cut.FindAll("thead tr:first-child .rework-sort-button").Select(b => b.TextContent.Trim(' ', '▲', '▼'));
        Assert.Equal(["ID", "Success", "Notes", "SubmitTime", "Noun", "Adjective", "Product", "Instruction", "Operator", "Step"], headers);
        Assert.Equal(2, cut.FindAll("tbody tr").Count);
    }

    [Fact]
    public void DeepLinkId_PrefillsIdFilterAndQueriesIt()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("rework?id=123");

        var cut = Render<MESS.Blazor.Components.Pages.Phoebe.Rework.Rework>();

        Assert.Equal("123", cut.Find("input[aria-label='Filter by ID']").GetAttribute("value"));
        _rework.Verify(s => s.GetPageAsync(It.Is<ReworkQuery>(q => q.FilterId == 123)), Times.AtLeastOnce);
    }

    [Fact]
    public void EnterInIdFilter_AppliesImmediatelyAndResetsPage()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("rework?page=3");
        var cut = Render<MESS.Blazor.Components.Pages.Phoebe.Rework.Rework>();

        var idInput = cut.Find("input[aria-label='Filter by ID']");
        idInput.Input("42");
        idInput.KeyDown("Enter");

        _rework.Verify(s => s.GetPageAsync(It.Is<ReworkQuery>(q => q.FilterId == 42 && q.Page == 1)), Times.AtLeastOnce);
    }

    [Fact]
    public void ClearAllFilters_ShownOnlyWhenAFilterIsActive()
    {
        var unfiltered = Render<MESS.Blazor.Components.Pages.Phoebe.Rework.Rework>();
        Assert.DoesNotContain(unfiltered.FindAll("button"), b => b.TextContent.Contains("Clear all filters"));

        Services.GetRequiredService<NavigationManager>().NavigateTo("rework?noun=housing");
        var filtered = Render<MESS.Blazor.Components.Pages.Phoebe.Rework.Rework>();
        Assert.Contains(filtered.FindAll("button"), b => b.TextContent.Contains("Clear all filters"));
    }

    [Fact]
    public void NoRows_ShowsEmptyState()
    {
        _rows = [];

        var cut = Render<MESS.Blazor.Components.Pages.Phoebe.Rework.Rework>();

        Assert.Contains("No attempts match the current filters.", cut.Markup);
    }

    [Fact]
    public void ClickingRow_OpensDialogWithTitleAndReadOnlyContext()
    {
        var cut = Render<MESS.Blazor.Components.Pages.Phoebe.Rework.Rework>();

        cut.Find("tbody tr").Click();

        cut.WaitForAssertion(() => Assert.Equal("Edit Attempt #5", cut.Find(".modal-title").TextContent.Trim()));
        Assert.Equal("Cracked housing near the clip", cut.Find("textarea[id$='-notes']").GetAttribute("value"));
        Assert.Contains("#9", cut.Find(".modal-body dl").TextContent);
        Assert.Contains("table-active", cut.Find("tbody tr").ClassName);
    }

    // ── Dialog ──────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Save_DisabledUntilAFieldChanges()
    {
        var cut = RenderDialog();
        cut.WaitForAssertion(() => cut.Find("textarea[id$='-notes']"));

        Assert.True(SaveDisabled(cut));

        cut.Find("textarea[id$='-notes']").Input("Housing replaced");

        Assert.False(SaveDisabled(cut));
    }

    [Fact]
    public void Success_IsShownButNotEditable()
    {
        var cut = RenderDialog();
        cut.WaitForAssertion(() => cut.Find("select[id$='-success']"));

        var success = cut.Find("select[id$='-success']");
        Assert.True(success.HasAttribute("disabled"));
        Assert.Equal("false", success.GetAttribute("value"));
    }

    [Fact]
    public void LogLevelChangeWithSiblings_RequiresConfirmation()
    {
        var cut = RenderDialog();
        cut.WaitForAssertion(() => cut.Find("select[id$='-operator']"));

        cut.Find("select[id$='-operator']").Change("op2");

        Assert.Contains("which also contains 2 other attempt(s) on 1 step(s)", cut.Markup);
        Assert.True(SaveDisabled(cut));

        cut.Find(".modal-body input[type=checkbox]").Change(true);

        Assert.False(SaveDisabled(cut));
    }

    [Fact]
    public void CurrentValue_IsShownEvenWhenNoLongerOffered()
    {
        var cut = RenderDialog();

        cut.WaitForAssertion(() =>
        {
            var selected = cut.Find("select[id$='-operator'] option[value='op1']");
            Assert.Contains("Ann Op (no longer available)", selected.TextContent);
        });
    }

    [Fact]
    public void Operators_IncludeUsersWithIsActiveFalse()
    {
        var cut = RenderDialog();

        cut.WaitForAssertion(() =>
            Assert.Equal("Cy Op", cut.Find("select[id$='-operator'] option[value='op3']").TextContent));
    }

    [Fact]
    public void ChangingInstruction_ReloadsStepsAndNounsAndClearsInvalidSelections()
    {
        var cut = RenderDialog();
        cut.WaitForAssertion(() => cut.Find("select[id$='-instruction']"));

        cut.Find("select[id$='-instruction']").Change("2");

        cut.WaitForAssertion(() =>
        {
            var step = cut.Find("select[id$='-step']");
            Assert.Contains("is-invalid", step.ClassName);
            Assert.Contains(step.QuerySelectorAll("option"), o => o.TextContent == "Other Step");
            Assert.Contains("is-invalid", cut.Find("select[id$='-noun']").ClassName);
            Assert.Contains(cut.Find("select[id$='-noun']").QuerySelectorAll("option"), o => o.TextContent == "Cable");
        });
        _rework.Verify(s => s.GetStepsForInstructionAsync(2), Times.Once);
        _defects.Verify(s => s.GetNounsForWorkInstructionAsync(2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ServerConflict_IsShownInsideDialogWhichStaysOpen()
    {
        _rework.Setup(s => s.UpdateAsync(It.IsAny<ReworkUpdateRequest>(), "tech"))
            .ReturnsAsync(ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Conflict, "This attempt was changed by someone else."));
        var saved = false;
        var cut = RenderDialog(_ => saved = true);
        cut.WaitForAssertion(() => cut.Find("textarea[id$='-notes']"));

        cut.Find("textarea[id$='-notes']").Input("Housing replaced");
        cut.Find(".modal-footer .btn-primary").Click();

        cut.WaitForAssertion(() => Assert.Contains("changed by someone else", cut.Find(".modal-body .alert-danger").TextContent));
        Assert.NotEmpty(cut.FindAll(".modal-title"));
        Assert.False(saved);
    }

    [Fact]
    public void SuccessfulSave_SendsSnapshotAndRaisesOnSaved()
    {
        ReworkUpdateRequest? sent = null;
        _rework.Setup(s => s.UpdateAsync(It.IsAny<ReworkUpdateRequest>(), "tech"))
            .Callback<ReworkUpdateRequest, string>((r, _) => sent = r)
            .ReturnsAsync(ReworkUpdateResult.Ok());
        var saved = false;
        var cut = RenderDialog(_ => saved = true);
        cut.WaitForAssertion(() => cut.Find("textarea[id$='-notes']"));

        cut.Find("textarea[id$='-notes']").Input("Housing replaced");
        cut.Find(".modal-footer .btn-primary").Click();

        cut.WaitForAssertion(() => Assert.True(saved));
        Assert.NotNull(sent);
        Assert.Equal("Cracked housing near the clip", sent!.Original.Notes);
        Assert.Equal("Housing replaced", sent.Updated.Notes);
        Assert.Equal(sent.Original.Success, sent.Updated.Success);
        Assert.Equal(5, sent.AttemptId);
    }
}
