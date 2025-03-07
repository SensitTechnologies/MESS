namespace MESS.Services.ProductionLog;
using Data.Models;
public interface IProductionLogEventService
{
    // public event Action? ProductionLogEventChanged;
    public string CurrentProductName { get; set; }
    public event Action? ProductDetailsChanged;
    // public event Action? WorkInstructionDetailsChanged;
    // public event Action? WorkStationDetailsChanged;
    // public event Action? LineOperatorDetailsChanged;
    public void SetCurrentProductName(string productName);
    public ProductionLog? GetCurrentProductionLog();
    public void SetCurrentProductionLog(ProductionLog productionLog);
}