using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazored.TextEditor;
using MESS.Data.Models;
using Serilog;

namespace MESS.Blazor.Components.Pages.Phoebe.WorkInstruction;

/// <summary>
/// Represents a Blazor component responsible for displaying and editing a single <see cref="Step"/> node
/// within a work instruction. This component provides functionality for rich text editing, image/media
/// management, and field swapping between the short and detailed step bodies.
/// </summary>
/// <remarks>
/// <para>
/// This class handles the synchronization of user edits from the BlazoredTextEditor via JavaScript interop,
/// tracks changes to mark the parent editor as dirty, and provides UI controls to manipulate the step's
/// name, body, detailed body, and media content.
/// </para>
/// <para>
/// The component communicates changes to its parent using the <see cref="OnChanged"/> callback and
/// supports interaction with step-level controls such as insert, delete, and move via <see cref="OnAction"/>.
/// </para>
/// <para>
/// This partial class is coupled with a Razor markup file that defines the user interface for editing steps.
/// </para>
/// </remarks>
public partial class StepNodeView : IDisposable
{
    /// <summary>
    /// Gets or sets the step node data to be displayed and edited in this component.
    /// </summary>
    /// <remarks>
    /// This is required and represents the underlying data model for the rich text editor,
    /// including fields like Name and Body.
    /// </remarks>
    [Parameter, EditorRequired]
    public required Step Step { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of editor references, keyed by Step ID.
    /// </summary>
    /// <remarks>
    /// This dictionary enables sharing and reusing editor instances across nodes.
    /// It must be provided by the parent component.
    /// </remarks>
    [Parameter, EditorRequired]
    public required Dictionary<int, BlazoredTextEditor> EditorRefs { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of currently active fields being edited, keyed by Step ID.
    /// </summary>
    /// <remarks>
    /// This tracks which field (e.g., Name or Body) is currently active for editing per step.
    /// </remarks>
    [Parameter, EditorRequired]
    public required Dictionary<int, string> ActiveFields { get; set; }
    
    /// <summary>
    /// Event callback to handle a node action like move, insert, or delete.
    /// </summary>
    /// <remarks>
    /// The parent handles this action by modifying the list of nodes appropriately.
    /// </remarks>
    [Parameter]
    public EventCallback<(WorkInstructionNode step, string action)> OnAction { get; set; }
    
    /// <summary>
    /// Event callback that is triggered when the step data has been modified by the user.
    /// Use this to notify the parent component that the editor state should be marked as dirty.
    /// </summary>
    [Parameter]
    public EventCallback OnChanged { get; set; }
    
    /// <summary>
    /// This method is invoked from JavaScript via JS interop when the content of the rich text editor changes.
    /// It retrieves the current HTML content from the editor and compares it to the corresponding property
    /// on the <see cref="Step"/> model (either <see cref="Step.DetailedBody"/> or <see cref="Step.Body"/> depending on the active field).
    /// If the content has changed, it updates the model and invokes the <see cref="OnChanged"/> event callback to notify
    /// parent components or services about the change.
    /// </summary>
    /// <remarks>
    /// This method is decorated with <see cref="JSInvokableAttribute"/> to allow JavaScript to call it asynchronously.
    /// It ensures that changes in the editor are properly propagated to the Blazor component state and can trigger UI updates or dirty state marking.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [JSInvokable]
    public async Task NotifyContentChanged()
    {
        if (EditorRef == null) return;

        var currentHtml = await EditorRef.GetHTML();

        if (ShowBody)
        {
            if (Step.DetailedBody != currentHtml)
            {
                Step.DetailedBody = currentHtml;
                await OnChanged.InvokeAsync(null);
            }
        }
        else
        {
            if (Step.Body != currentHtml)
            {
                Step.Body = currentHtml;
                await OnChanged.InvokeAsync(null);
            }
        }
    }
    
    // Track which field is showing
    private bool ShowBody
    {
        get
        {
            if (ActiveFields.TryGetValue(Step.Id, out var field))
                return field == "Body";

            ActiveFields[Step.Id] = "Name";  // default to Name
            return false;                    // false = show Name
        }
        set => ActiveFields[Step.Id] = value ? "Body" : "Name";
    }
    
    private BlazoredTextEditor? _editorRef;

    private BlazoredTextEditor? EditorRef
    {
        get
        {
            if (EditorRefs.TryGetValue(Step.Id, out var editor))
            {
                return editor;
            }

            return _editorRef;
        }
        set
        {
            if (value != null)
            {
                EditorRefs[Step.Id] = value;
                _editorRef = value;
            }
        }
    }
    
    private DotNetObjectReference<StepNodeView>? objRef;
    private bool NeedsLoadAfterRender { get; set; } = true;
    
    private async Task HandleMoveAction(string action)
    {
        if (OnAction.HasDelegate)
        {
            await OnAction.InvokeAsync((Step, action));
        }
    }
    
    private async Task SwapField()
    {
        if (EditorRef == null) return;

        // Save current
        var currentHtml = await EditorRef.GetHTML();
        if (ShowBody)
        {
            Step.DetailedBody = currentHtml;
        }
        else
        {
            Step.Body = currentHtml;
        }

        // Swap
        ShowBody = !ShowBody;
        NeedsLoadAfterRender = true;

        StateHasChanged();
    }
    
    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (EditorRef == null)
            return;

        if (!NeedsLoadAfterRender)
            return;

        try
        {
            if (await WaitForEditorReady(EditorRef))
            {
                var contentToLoad = ShowBody ? (Step.DetailedBody ?? "") : (Step.Body ?? "");

                int retries = 0;
                bool success = false;
                while (!success && retries++ < 10)
                {
                    try
                    {
                        await EditorRef.LoadHTMLContent(contentToLoad);
                        success = true;
                    }
                    catch
                    {
                        await Task.Delay(200);
                    }
                }

                if (success)
                {
                    NeedsLoadAfterRender = false;

                    if (objRef == null)
                        objRef = DotNetObjectReference.Create(this);

                    string containerId = $"step-editor-{Step.Id}";

                    await JS.InvokeVoidAsync("quillInterop.attachChangeHandler", containerId, objRef);
                }
                else
                {
                    Log.Warning("Failed to load editor content after multiple attempts for Step {StepId}", Step.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading HTML content for field in step {StepId}", Step.Id);
        }
    }

    /// <summary>
    /// Releases unmanaged resources and disposes of managed resources used by the component,
    /// including any JavaScript object references to prevent memory leaks.
    /// </summary>
    public void Dispose()
    {
        objRef?.Dispose();
    }
    
    private async Task<bool> WaitForEditorReady(BlazoredTextEditor editor)
    {
        const int maxRetries = 15;
        const int delayMs = 200;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (editor == null) return false;
                await editor.GetHTML();
                return true;
            }
            catch
            {
                // Retry
            }
            await Task.Delay(delayMs);
        }
        return false;
    }
    
    private string DefaultToolbarHtml => @"
    <select class='ql-header'>
        <option selected></option>
        <option value='1'></option>
        <option value='2'></option>
        <option value='3'></option>
        <option value='4'></option>
        <option value='5'></option>
    </select>
    <span class='ql-formats'>
        <button class='ql-bold'></button>
        <button class='ql-italic'></button>
        <button class='ql-underline'></button>
        <button class='ql-strike'></button>
    </span>
    <span class='ql-formats'>
        <select class='ql-color'></select>
        <select class='ql-background'></select>
    </span>
    <span class='ql-formats'>
        <button class='ql-list' value='ordered'></button>
        <button class='ql-list' value='bullet'></button>
    </span>
    <span class='ql-formats'>
        <button class='ql-link'></button>
    </span>
    ";

    private async Task HandleImageMediaChanged((List<string> Primary, List<string> Secondary) updated)
    {
        Step.PrimaryMedia = new List<string>(updated.Primary);
        Step.SecondaryMedia = new List<string>(updated.Secondary);

        await NotifyChangeAsync();
        StateHasChanged();
    }
    
    private async Task OnNameChanged(ChangeEventArgs e)
    {
        Step.Name = e.Value?.ToString() ?? "";
        await NotifyChangeAsync();
    }
    
    private async Task NotifyChangeAsync()
    {
        if (OnChanged.HasDelegate)
        {
            await OnChanged.InvokeAsync();
        }
    }
}
