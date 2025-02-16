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

    public async Task<List<ProductionLog>> GetAllAsync()
    {
        try
        {
            return await _context.ProductionLogs
                .Include(p => p.WorkInstruction)
                .ThenInclude(w => w!.Steps)
                .Include(p => p.LineOperator)
                .Include(p => p.LogSteps).ThenInclude(p => p.WorkInstructionStep)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public TimeSpan? GetTotalTime(ProductionLog log)
    {
        try
        {
            if (log.LogSteps == null || log.LogSteps.Count == 0)
                return TimeSpan.Zero;
            
            return TimeSpan.FromTicks(log.LogSteps
                    .Where(l => l.Duration.HasValue)
                    .Sum(l => l.Duration!.Value.Ticks));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public ProductionLog Get(string id)
    {
        throw new NotImplementedException();
    }

    public bool Create(ProductionLog productionLog)
    {
        try
        {
            productionLog.CreatedOn = DateTimeOffset.UtcNow;
            productionLog.SubmitTime = DateTimeOffset.UtcNow;
            productionLog.LastModifiedOn = DateTimeOffset.UtcNow;
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