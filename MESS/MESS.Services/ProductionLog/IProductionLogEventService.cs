namespace MESS.Services.ProductionLog;
using Data.Models;
public interface IProductionLogEventService
{
    // public event Action? ProductionLogEventChanged;
    public string CurrentProductName { get; set; }
    public string CurrentWorkStationName { get; set; }
    public event Action? ProductDetailsChanged;
    // public event Action? WorkInstructionDetailsChanged;
    public event Action? WorkStationDetailsChanged;
    // public event Action? LineOperatorDetailsChanged;
    public void SetCurrentProductName(string productName);
    public void SetCurrentWorkStationName(string workStationName);
    public ProductionLog? GetCurrentProductionLog();
    public void SetCurrentProductionLog(ProductionLog productionLog);
}