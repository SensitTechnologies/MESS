using System.Diagnostics;

namespace MESS.Services.ProductionLog;
using Data.Models;
public class ProductionLogEventService : IProductionLogEventService
{
    private const int DEFAULT_AUTOSAVE_DELAY = 2000; // 2 seconds
    private Timer? _autoSaveTimer;
    
    public event Action? ProductionLogEventChanged;

    public event Action? ProductDetailsChanged;

    // public event Action? WorkInstructionDetailsChanged;

    public event Action? WorkStationDetailsChanged;

    public event Action? LineOperatorDetailsChanged;
    
    public event Func<ProductionLog, Task>? AutoSaveTriggered;

    public ProductionLog? CurrentProductionLog { get; set; }
    public string CurrentProductName { get; set; } = "";
    public string CurrentWorkStationName { get; set; } = "";
    public string CurrentLineOperatorName { get; set; } = "";
    public bool IsSaved { get; set; } = false;

    public async Task ChangeMadeToProductionLog()
    {
        await TriggerAutoSaveAsync();
        ProductionLogEventChanged?.Invoke();
    }

    private async Task TriggerAutoSaveAsync()
    {
        if (CurrentProductionLog == null)
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
            AutoSaveTriggered?.Invoke(CurrentProductionLog);
            IsSaved = true;
        }, null, DEFAULT_AUTOSAVE_DELAY, Timeout.Infinite);
    }

    public void SetCurrentProductName(string productName)
    {
        CurrentProductName = productName;
        ProductDetailsChanged?.Invoke();
    }

    public void SetCurrentWorkStationName(string workStationName)
    {
        CurrentWorkStationName = workStationName;
        WorkStationDetailsChanged?.Invoke();
    }

    public void SetCurrentLineOperatorName(string lineOperatorName)
    {
        CurrentLineOperatorName = lineOperatorName;
        LineOperatorDetailsChanged?.Invoke();
    }

    public ProductionLog? GetCurrentProductionLog()
    {
        return CurrentProductionLog;
    }

    public async Task SetCurrentProductionLog(ProductionLog productionLog)
    {
        try
        {
            CurrentProductionLog = productionLog ?? throw new ArgumentNullException(nameof(productionLog));
            await ChangeMadeToProductionLog();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error setting production log: {ex.Message}");
            throw;
        }
    }
}