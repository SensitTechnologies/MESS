using System.Diagnostics;
using Serilog;

namespace MESS.Services.ProductionLog;
using Data.Models;
/// <inheritdoc />
public class ProductionLogEventService : IProductionLogEventService
{
    private const int DEFAULT_AUTOSAVE_DELAY = 2000; // 2 seconds
    private Timer? _autoSaveTimer;
    private bool _shouldTriggerAutoSave = true;
    
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
        if (CurrentProductionLogs == null || !CurrentProductionLogs.Any() || !_shouldTriggerAutoSave)
        {
            Debug.WriteLine("Cannot trigger autosave. No production logs available or autosave is disabled.");
            return;
        }

        if (_autoSaveTimer != null)
        {
            await _autoSaveTimer.DisposeAsync();
        }

        IsSaved = false;
        _autoSaveTimer = new Timer(_ =>
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (AutoSaveTriggered != null)
                    {
                        await AutoSaveTriggered.Invoke(CurrentProductionLogs);
                    }

                    IsSaved = true;
                }
                catch (Exception e)
                {
                    Log.Warning("AutoSave failed: {Message}", e.Message);
                    Debug.WriteLine($"AutoSave exception: {e}");
                }
            });
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
}