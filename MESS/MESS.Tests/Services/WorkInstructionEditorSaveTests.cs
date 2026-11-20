using MESS.Data.Models;
using MESS.Services.CRUD.PartDefinitions;
using MESS.Services.CRUD.WorkInstructions;
using MESS.Services.DTOs.WorkInstructions.Form;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.Form;
using MESS.Services.Media.WorkInstructions;
using MESS.Services.UI.WorkInstructionEditor;
using Moq;

namespace MESS.Tests.Services;

/// <summary>
/// Save routing in the work instruction editor. Regression: a brand-new instruction (File → New →
/// Add Step → Save) was routed to the new-version path because IsEditable is false for an unsaved
/// instruction, and the save failed with "OriginalId is required for versioning."
/// </summary>
public class WorkInstructionEditorSaveTests
{
    private readonly Mock<IWorkInstructionService> _instructions = new();

    private WorkInstructionEditorService CreateEditor() =>
        new(_instructions.Object, Mock.Of<IWorkInstructionImageService>(), Mock.Of<IPartDefinitionService>());

    private static StepNodeFormDTO NewStep(string name) => new()
    {
        Name = name, Body = "<p>Sample text</p>", NodeType = WorkInstructionNodeType.Step
    };

    [Fact]
    public async Task NewInstruction_CanSaveInPlace_WithoutCheckingProductionLogs()
    {
        var editor = CreateEditor();
        editor.StartNew("Bicycle Frame", ["Bicycle"]);

        Assert.True(await editor.CanSaveInPlaceAsync());
        _instructions.Verify(s => s.IsEditable(It.IsAny<WorkInstruction>()), Times.Never);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task NewInstruction_WithSteps_SavesThroughCreate(int stepCount)
    {
        _instructions.Setup(s => s.CreateAsync(It.IsAny<WorkInstructionFormDTO>(), It.IsAny<string>()))
            .Callback((WorkInstructionFormDTO dto, string _) => dto.Id = 42)
            .ReturnsAsync(true);
        _instructions.Setup(s => s.GetFormByIdAsync(42))
            .ReturnsAsync(new WorkInstructionFormDTO { Id = 42, Title = "Bicycle Frame", Version = "1.0" });

        var editor = CreateEditor();
        editor.StartNew("Bicycle Frame", ["Bicycle"]);
        for (var i = 1; i <= stepCount; i++)
            editor.Current!.Nodes.Add(NewStep($"Step {i}"));

        Assert.True(await editor.CanSaveInPlaceAsync());
        Assert.True(await editor.SaveAsync("tech"));

        _instructions.Verify(s => s.CreateAsync(
            It.Is<WorkInstructionFormDTO>(d => d.Nodes.Count == stepCount && d.OriginalId == null), "tech"), Times.Once);
        _instructions.Verify(s => s.CreateNewVersionAsync(It.IsAny<WorkInstructionFormDTO>(), It.IsAny<string>()), Times.Never);
        Assert.Equal(EditorMode.EditExisting, editor.Mode);
        Assert.Equal(42, editor.Current!.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingInstruction_CanSaveInPlace_OnlyWhenNoProductionLogs(bool editable)
    {
        _instructions.Setup(s => s.GetFormByIdAsync(7))
            .ReturnsAsync(new WorkInstructionFormDTO { Id = 7, Title = "Existing", Version = "1.0" });
        _instructions.Setup(s => s.IsEditable(It.IsAny<WorkInstruction>())).ReturnsAsync(editable);

        var editor = CreateEditor();
        await editor.LoadForEditAsync(7);

        Assert.Equal(editable, await editor.CanSaveInPlaceAsync());
    }

    [Fact]
    public async Task NoInstructionLoaded_CannotSave()
    {
        Assert.False(await CreateEditor().CanSaveInPlaceAsync());
    }
}
