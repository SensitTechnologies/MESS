namespace MESS.Services.ProductionLogServices;
using Data.Models;
/// <summary>
/// Interface for managing production log events and related details.
/// </summary>
public interface IProductionLogEventService
{
    /// <summary>
    /// Gets or sets the current production logs.
    /// </summary>
    public List<ProductionLog> CurrentProductionLogs { get; set; }

    /// <summary>
    /// Gets or sets the name of the current product.
    /// </summary>
    public string CurrentProductName { get; set; }

    /// <summary>
    /// Gets or sets the name of the current workstation.
    /// </summary>
    public string CurrentWorkStationName { get; set; }

    /// <summary>
    /// Gets or sets the name of the current work instruction.
    /// </summary>
    public string CurrentWorkInstructionName { get; set; }

    /// <summary>
    /// Gets or sets the name of the current line operator.
    /// </summary>
    public string CurrentLineOperatorName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the production log is saved.
    /// </summary>
    public bool IsSaved { get; set; }

    /// <summary>
    /// Event triggered when the production log changes.
    /// </summary>
    public event Action? ProductionLogEventChanged;

    /// <summary>
    /// Event triggered when product details change.
    /// </summary>
    public event Action? ProductDetailsChanged;

    /// <summary>
    /// Event triggered when work instruction details change.
    /// </summary>
    public event Action? WorkInstructionDetailsChanged;

    /// <summary>
    /// Event triggered when workstation details change.
    /// </summary>
    public event Action? WorkStationDetailsChanged;

    /// <summary>
    /// Event triggered when line operator details change.
    /// </summary>
    public event Action? LineOperatorDetailsChanged;

    /// <summary>
    /// Event triggered to handle auto-save functionality.
    /// </summary>
    public event Func<List<ProductionLog>, Task>? AutoSaveTriggered;
    
    /// <summary>
    /// Event invoked when it's time to save logs to the database.
    /// </summary>
    public event Func<List<ProductionLog>, Task>? DbSaveTriggered;


    /// <summary>
    /// Disables the auto-save functionality.
    /// </summary>
    public void DisableAutoSave();

    /// <summary>
    /// Enables the auto-save functionality.
    /// </summary>
    public void EnableAutoSave();

    /// <summary>
    /// Handles changes made to the production log.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ChangeMadeToProductionLog();

    /// <summary>
    /// Sets the current product name.
    /// </summary>
    /// <param name="productName">The name of the product to set.</param>
    public void SetCurrentProductName(string productName);

    /// <summary>
    /// Sets the current workstation name.
    /// </summary>
    /// <param name="workStationName">The name of the workstation to set.</param>
    public void SetCurrentWorkStationName(string workStationName);

    /// <summary>
    /// Sets the current work instruction name.
    /// </summary>
    /// <param name="workInstructionName">The name of the work instruction to set.</param>
    public void SetCurrentWorkInstructionName(string workInstructionName);

    /// <summary>
    /// Sets the current line operator name.
    /// </summary>
    /// <param name="lineOperatorName">The name of the line operator to set.</param>
    public void SetCurrentLineOperatorName(string lineOperatorName);

    /// <summary>
    /// Gets the current list of production logs in memory.
    /// </summary>
    /// <returns>A list of current production logs. Returns an empty list if none are set.</returns>
    List<ProductionLog> GetCurrentProductionLogs();

    /// <summary>
    /// Sets the current list of production logs in memory.
    /// Replaces any previously stored logs.
    /// </summary>
    /// <param name="productionLogs">The list of production logs to set.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetCurrentProductionLogs(List<ProductionLog> productionLogs);

    /// <summary>
    /// Starts a periodic timer that ensures dirty production logs are persisted to the database
    /// at regular intervals (e.g., every 30 minutes), even if frequent changes are made.
    /// </summary>
    /// <remarks>
    /// This method guarantees that unsaved changes will eventually be written to the database,
    /// regardless of whether the user pauses editing or not. The timer checks the <c>IsDirty</c>
    /// flag to determine whether changes exist, and if so, invokes the <see cref="DbSaveTriggered"/>
    /// delegate to persist <see cref="CurrentProductionLogs"/> to the database.
    /// </remarks>
    /// <example>
    /// Call this once when the application or component starts to ensure background flushing:
    /// <code>
    /// productionLogEventService.StartDbSaveTimer();
    /// </code>
    /// </example>
    void StartDbSaveTimer();
    
    /// <summary>
    /// Stops the periodic database save timer, preventing future automatic database saves.
    /// This should be called when the component or page is being disposed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopDbSaveTimerAsync();

    /// <summary>
    /// Marks the current production log as dirty, indicating it has unsaved changes.
    /// </summary>
    /// <remarks>
    /// This should be called whenever a change is made to a cached production log
    /// that has not yet been saved to the database.
    /// </remarks>
    void MarkDirty();

    /// <summary>
    /// Marks the current production log state as clean in terms of database autosave, indicating that there are no
    /// unsaved changes.
    /// </summary>
    /// <remarks>
    /// This method should be called after initializing or resetting the form.
    /// </remarks>
    void MarkClean();

    /// <summary>
    /// Attempts to manually trigger a database save of the current production logs
    /// by invoking the <see cref="DbSaveTriggered"/> event, if it has subscribers.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the save was successfully triggered and completed without exception;
    /// <c>false</c> if the event was not subscribed or an exception occurred during invocation.
    /// </returns>
    /// <remarks>
    /// This method can be safely called from external components to force a database save,
    /// bypassing the periodic timer or debounced auto-save mechanism. If the save succeeds,
    /// the internal dirty state is marked as clean using <see cref="MarkClean"/>.
    /// </remarks>
    public Task<bool> TryTriggerDbSaveAsync();
}