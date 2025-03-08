namespace MESS.Services.ProductionLog;
using Data.Models;
public interface IProductionLogEventService
{
    public string CurrentProductName { get; set; }
    public string CurrentWorkStationName { get; set; }
    public string CurrentLineOperatorName { get; set; }
    public bool IsSaved { get; set; }
    public event Action? ProductionLogEventChanged;
    public event Action? ProductDetailsChanged;
    // public event Action? WorkInstructionDetailsChanged;
    public event Action? WorkStationDetailsChanged;
    public event Action? LineOperatorDetailsChanged;
    public event Func<ProductionLog, Task>? AutoSaveTriggered;

    public Task ChangeMadeToProductionLog();
    
    public void SetCurrentProductName(string productName);
    public void SetCurrentWorkStationName(string workStationName);
    public void SetCurrentLineOperatorName(string lineOperatorName);
    public ProductionLog? GetCurrentProductionLog();
    public Task SetCurrentProductionLog(ProductionLog productionLog);
}