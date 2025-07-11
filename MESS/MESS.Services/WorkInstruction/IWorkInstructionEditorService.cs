namespace MESS.Services.WorkInstruction;

using MESS.Data.Models;


/// <summary>
/// Represents the editing mode of the <see cref="IWorkInstructionEditorService"/>,
/// indicating the context in which a WorkInstruction is being edited.
/// </summary>
public enum EditorMode
{
    /// <summary>
    /// No editing mode is currently active. No WorkInstruction is being edited.
    /// </summary>
    None,

    /// <summary>
    /// A brand new WorkInstruction is being created from scratch.
    /// The resulting instruction will not belong to any version chain.
    /// </summary>
    CreateNew,

    /// <summary>
    /// An existing WorkInstruction is being edited in place.
    /// Typically only allowed if the instruction has no associated production logs.
    /// </summary>
    EditExisting,

    /// <summary>
    /// A new version is being created based on the latest WorkInstruction in an existing version chain.
    /// The new version will receive an incremented version number and be marked as the latest.
    /// </summary>
    CreateNewVersion
}


/// <summary>
/// Represents a client-side editor service for managing the state of a single WorkInstruction being edited in the UI.
/// Supports creating new instructions, editing existing ones, and branching new versions in a version chain.
/// Tracks in-memory editing state, dirty flags, and editing modes.
/// </summary>
public interface IWorkInstructionEditorService
{
    /// <summary>
    /// Gets the WorkInstruction currently being edited in the UI.
    /// May be null if no editing session is active.
    /// </summary>
    WorkInstruction? Current { get; }

    /// <summary>
    /// Gets a value indicating whether the current WorkInstruction has unsaved changes.
    /// This flag is used to enable or disable Save buttons and warn users about unsaved work.
    /// </summary>
    bool IsDirty { get; }

    /// <summary>
    /// Gets the current editing mode of the service, indicating whether
    /// the user is creating a new instruction, editing an existing one,
    /// or creating a new version in an existing version chain.
    /// </summary>
    EditorMode Mode { get; }

    /// <summary>
    /// Loads an existing WorkInstruction from the database for editing.
    /// Overwrites the current editing session with the retrieved data.
    /// </summary>
    /// <param name="id">The ID of the WorkInstruction to load for editing.</param>
    /// <returns>
    /// A task that completes when the WorkInstruction has been loaded.
    /// Throws an exception if the WorkInstruction cannot be found.
    /// </returns>
    Task LoadForEditAsync(int id);

    /// <summary>
    /// Loads the latest version of a WorkInstruction chain to serve as a template
    /// for creating a new version. The resulting WorkInstruction is not yet saved
    /// to the database and remains in memory for user editing.
    /// </summary>
    /// <param name="originalId">
    /// The ID of the original WorkInstruction that defines the version chain.
    /// </param>
    /// <returns>
    /// A task that completes when the new version template is prepared.
    /// Throws an exception if the latest version cannot be found.
    /// </returns>
    Task LoadForNewVersionAsync(int originalId);

    /// <summary>
    /// Initializes a new, blank WorkInstruction in memory for user editing.
    /// Sets the editing mode to CreateNew and marks the editing state as dirty.
    /// </summary>
    void StartNew();

    /// <summary>
    /// Marks the current editing session as having unsaved changes.
    /// Typically called by UI components when the user modifies a field.
    /// Raises the OnChanged event to notify listeners of state changes.
    /// </summary>
    void MarkDirty();

    /// <summary>
    /// Reverts the current editing session, clearing any in-memory
    /// WorkInstruction and resetting the dirty flag and editing mode.
    /// Typically used when the user cancels editing.
    /// </summary>
    void RevertChanges();

    /// <summary>
    /// Persists the current WorkInstruction to the database based on its editing mode.
    /// - In CreateNew mode: Creates a brand new WorkInstruction with no OriginalId.
    /// - In EditExisting mode: Updates the existing WorkInstruction in the database.
    /// - In CreateNewVersion mode: Creates a new WorkInstruction in the version chain,
    ///   updates other versions to IsLatest = false, and assigns an incremented version.
    /// Clears the dirty flag if save is successful.
    /// </summary>
    /// <returns>
    /// A boolean indicating true if the save operation succeeded; otherwise, false.
    /// </returns>
    Task<bool> SaveAsync();

    /// <summary>
    /// An event raised whenever the editing state changes.
    /// UI components can subscribe to this event to refresh themselves
    /// when the Current model, dirty flag, or mode changes.
    /// </summary>
    event Action? OnChanged;
}

