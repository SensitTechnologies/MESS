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
    public List<ProductionLog> GetAll()
    {
        try
        {
            return _context.ProductionLogs
                .Include(p => p.WorkInstruction)
                .ThenInclude(w => w!.Steps)
                .Include(p => p.LineOperator)
                .Include(p => p.LogSteps)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return [];
        }
        
    }

    public async Task<List<ProductionLog>?> GetAllAsync()
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
            return new List<ProductionLog>();
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

    public ProductionLog? GetById(int id)
    {
        try
        {
            var productionLog = _context.ProductionLogs.Find(id);

            return productionLog;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<ProductionLog?> GetByIdAsync(int id)
    {
        try
        {
            var productionLog = await _context.ProductionLogs.FindAsync(id);

            return productionLog;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
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

    public bool Delete(int id)
    {
        try
        {
            var productionLogToDelete = _context.ProductionLogs.Find(id);
            
            if (productionLogToDelete == null)
            {
                return false;
            }
            
            _context.ProductionLogs.Remove(productionLogToDelete);
            _context.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public ProductionLog Edit(ProductionLog existing, ProductionLog updated)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(existing);
            ArgumentNullException.ThrowIfNull(updated);

            existing.LogSteps = updated.LogSteps;
            existing.LineOperator = updated.LineOperator;
            existing.SubmitTime = updated.SubmitTime;
            existing.LastModifiedOn = DateTimeOffset.UtcNow;

            _context.ProductionLogs.Update(existing);
            _context.SaveChanges();
            
            return existing;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}