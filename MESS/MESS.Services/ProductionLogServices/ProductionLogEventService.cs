using System.Diagnostics;
using Serilog;

namespace MESS.Services.ProductionLogServices;
using Data.Models;
/// <inheritdoc />
public class ProductionLogEventService : IProductionLogEventService
{
    private const int DEFAULT_AUTOSAVE_DELAY = 2000; // 2 seconds
    private Timer? _autoSaveTimer;
    private bool _shouldTriggerAutoSave = true;
    
    //private const int DB_SAVE_INTERVAL = 15 * 60 * 1000; // 15 minutes in milliseconds
    private const int DB_SAVE_INTERVAL = 15 * 1000;
    private Timer? _dbSaveTimer;
    
    /// <inheritdoc />
    public event Action? ProductionLogEventChanged;

    /// <inheritdoc />
    public event Action? ProductDetailsChanged;
    /// <inheritdoc />
    public event Action? WorkInstructionDetailsChanged;
    
    /// <inheritdoc />
    public event Action? WorkStationDetailsChanged;

    /// <inheritdoc />
    public event Action? LineOperatorDetailsChanged;
    
    /// <inheritdoc />
    public event Func<List<ProductionLog>, Task>? AutoSaveTriggered;
    
    /// <inheritdoc />
    public event Func<List<ProductionLog>, Task>? DbSaveTriggered;

    /// <inheritdoc />
    public List<ProductionLog> CurrentProductionLogs { get; set; } = [];

    /// <inheritdoc />
    public string CurrentProductName { get; set; } = "";
    /// <inheritdoc />
    public string CurrentWorkStationName { get; set; } = "";
    /// <inheritdoc />
    public string CurrentWorkInstructionName { get; set; } = "";
    /// <inheritdoc />
    public string CurrentLineOperatorName { get; set; } = "";
    
    /// <inheritdoc />
    public bool IsSaved { get; set; } = false;
    
    private bool IsDirty { get; set; } = false;

    /// <inheritdoc />
    public void DisableAutoSave()
    {
        _shouldTriggerAutoSave = false;
    }

    /// <inheritdoc />
    public void EnableAutoSave()
    {
        _shouldTriggerAutoSave = true;
    }

    /// <inheritdoc />
    public async Task ChangeMadeToProductionLog()
    {
        await TriggerAutoSaveAsync();
        ProductionLogEventChanged?.Invoke();
    }

    private async Task TriggerAutoSaveAsync()
    {
        if (CurrentProductionLogs == null || !_shouldTriggerAutoSave)
        {
            Debug.WriteLine("Cannot trigger autosave. Production log or timer is null.");
            return;
        }

        if (_autoSaveTimer != null)
        {
            await _autoSaveTimer.DisposeAsync();
        }

        IsSaved = false;
        _autoSaveTimer = new Timer(_ =>
        {
            AutoSaveTriggered?.Invoke(CurrentProductionLogs);
            IsSaved = true;
        }, null, DEFAULT_AUTOSAVE_DELAY, Timeout.Infinite);
    }
    
    /// <inheritdoc />
    public void SetCurrentProductName(string productName)
    {
        CurrentProductName = productName;
        ProductDetailsChanged?.Invoke();
    }

    /// <inheritdoc />
    public void SetCurrentWorkStationName(string workStationName)
    {
        CurrentWorkStationName = workStationName;
        WorkStationDetailsChanged?.Invoke();
    }

    /// <inheritdoc />
    public void SetCurrentWorkInstructionName(string workInstructionName)
    {
        CurrentWorkInstructionName = workInstructionName;
        WorkInstructionDetailsChanged?.Invoke();
    }

    /// <inheritdoc />
    public void SetCurrentLineOperatorName(string lineOperatorName)
    {
        CurrentLineOperatorName = lineOperatorName;
        LineOperatorDetailsChanged?.Invoke();
    }

    /// <inheritdoc />
    public List<ProductionLog> GetCurrentProductionLogs()
    {
        return CurrentProductionLogs;
    }

    /// <inheritdoc />
    public async Task SetCurrentProductionLogs(List<ProductionLog> productionLogs)
    {
        try
        {
            CurrentProductionLogs = productionLogs;
            await ChangeMadeToProductionLog();
        }
        catch (Exception ex)
        {
            var ids = string.Join(", ", productionLogs.Select(p => p.Id));
            Log.Warning("Exception thrown when attempting to SetCurrentProductionLogs to IDs: {ProductionLogIds}. Exception: {Exception}", ids, ex.ToString());
        }
    }
    
    /// <summary>
    /// Starts the periodic database save timer, which attempts to flush dirty in-memory
    /// production log data to the database every <see cref="DB_SAVE_INTERVAL"/> milliseconds.
    /// If the log is not dirty, no save is triggered.
    /// </summary>
    public void StartDbSaveTimer()
    {
        if (_dbSaveTimer != null)
        {
            _dbSaveTimer.Dispose();
        }

        _dbSaveTimer = new Timer(async _ =>
        {
            if (IsDirty && DbSaveTriggered is not null && CurrentProductionLogs is not null)
            {
                try
                {
                    await DbSaveTriggered.Invoke(CurrentProductionLogs);
                    IsDirty = false;
                }
                catch (Exception ex)
                {
                    Log.Warning("Periodic DB flush failed: {Exception}", ex.ToString());
                }
            }
        }, null, DB_SAVE_INTERVAL, DB_SAVE_INTERVAL); // Start and repeat every 15 min
    }

    /// <summary>
    /// Manually stop the DB save timer.
    /// </summary>
    public async Task StopDbSaveTimerAsync()
    {
        if (_dbSaveTimer != null)
        {
            await _dbSaveTimer.DisposeAsync();
            _dbSaveTimer = null;
        }
    }
    
    /// <summary>
    /// Marks the production log state as dirty, indicating that there are unsaved changes
    /// in memory which should eventually be flushed to the database.
    /// </summary>
    public void MarkDirty()
    {
        IsDirty = true;
    }

    /// <summary>
    /// Marks the current production log state as clean in terms of database autosave, indicating that there are no
    /// unsaved changes.
    /// </summary>
    /// <remarks>
    /// This method should be called after initializing or resetting the form.
    /// </remarks>
    public void MarkClean()
    {
        IsDirty = false;
    }
    
    /// <inheritdoc />
    public async Task<bool> TryTriggerDbSaveAsync()
    {
        if (DbSaveTriggered is null || CurrentProductionLogs is null)
            return false;

        try
        {
            await DbSaveTriggered.Invoke(CurrentProductionLogs);
            MarkClean(); // Mark clean after successful save
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning("Manual DB save failed: {Exception}", ex.ToString());
            return false;
        }
    }
}