namespace MESS.Services.ProductionLog;
using Data.Models;
public class ProductionLogEventService : IProductionLogEventService
{
    // public event Action? ProductionLogEventChanged;
    public event Action? ProductDetailsChanged;
    // public event Action? WorkInstructionDetailsChanged;
    // public event Action? WorkStationDetailsChanged;
    // public event Action? LineOperatorDetailsChanged;

    public string CurrentProductName { get; set; } = "";
    
    public void SetCurrentProductName(string productName)
    {
        CurrentProductName = productName;
        ProductDetailsChanged?.Invoke();
    }

    public ProductionLog? GetCurrentProductionLog()
    {
        throw new NotImplementedException();
    }

    public void SetCurrentProductionLog(ProductionLog productionLog)
    {
        throw new NotImplementedException();
    }
}