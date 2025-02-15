namespace MESS.Services.ProductionLog;
using Data.Models;
public interface IProductionLogService
{
    public IEnumerable<ProductionLog> GetAll();
    public ProductionLog Get(string id);
    public bool Create(ProductionLog productionLog);
    public bool Delete(string id);
    public ProductionLog Edit(ProductionLog existing, ProductionLog updated);
}