using System.Diagnostics;

namespace MESS.Services.ProductionLog;
using Data.Models;
public class ProductionLogEventService : IProductionLogEventService
{
    private ProductionLog? _currentProductionLog = null;
    private const int DEFAULT_AUTOSAVE_DELAY = 2000; // 4 seconds
    private Timer? _autoSaveTimer;
    
    public event Action? ProductionLogEventChanged;

    public event Action? ProductDetailsChanged;
    // public event Action? WorkInstructionDetailsChanged;
    public event Action? WorkStationDetailsChanged;
    // public event Action? LineOperatorDetailsChanged;
    
    public event Func<ProductionLog, Task>? AutoSaveTriggered;

    public string CurrentProductName { get; set; } = "";
    public string CurrentWorkStationName { get; set; } = "";
    public bool IsSaved { get; set; } = false;

    public async Task ChangeMadeToProductionLog()
    {
        await TriggerAutoSaveAsync();
        ProductionLogEventChanged?.Invoke();
    }

    private async Task TriggerAutoSaveAsync()
    {
        if (_currentProductionLog == null)
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
            AutoSaveTriggered?.Invoke(_currentProductionLog);
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

    public ProductionLog? GetCurrentProductionLog()
    {
        return _currentProductionLog;
    }

    public async Task SetCurrentProductionLog(ProductionLog productionLog)
    {
        try
        {
            _currentProductionLog = productionLog ?? throw new ArgumentNullException(nameof(productionLog));
            await ChangeMadeToProductionLog();
            Debug.WriteLine($"Production log set successfully: {_currentProductionLog.Id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error setting production log: {ex.Message}");
            throw;
        }
    }
}