using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MESS.Services.ProductionLog;
using Data.Models;

public class ProductionLogService : IProductionLogService
{
    private readonly ApplicationContext _context;
    public ProductionLogService(ApplicationContext context)
    {
        _context = context;
    }
    public IEnumerable<ProductionLog> GetAll()
    {
        return _context.ProductionLogs
            .Include(p => p.WorkInstruction)
                .ThenInclude(w => w!.Steps)
            .Include(p => p.LineOperator)
            .Include(p => p.LogSteps)
            .ToList();
    }

    public ProductionLog Get(string id)
    {
        throw new NotImplementedException();
    }

    public bool Create(ProductionLog productionLog)
    {
        try
        {
            _context.ProductionLogs.Add(productionLog);
            _context.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool Delete(string id)
    {
        throw new NotImplementedException();
    }

    public ProductionLog Edit(ProductionLog existing, ProductionLog updated)
    {
        throw new NotImplementedException();
    }
}