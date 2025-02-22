using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

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
            Log.Warning("Exception thrown when attempting to GetAll Production Logs, in ProductionLogService");
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
                .Include(p => p.LogSteps)
                .ThenInclude(p => p.WorkInstructionStep)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception thrown when attempting to GetAllAsync Production Logs, in ProductionLogService");
            return new List<ProductionLog>();
        }
    }

    public TimeSpan? GetTotalTime(ProductionLog log)
    {
        try
        {
            if (log.LogSteps == null || log.LogSteps.Count == 0)
                return TimeSpan.Zero;
            
            var orderedSteps = log.LogSteps.OrderBy(l => l.SubmitTime);

            var start = orderedSteps.FirstOrDefault()?.SubmitTime;
            var end = orderedSteps.LastOrDefault()?.SubmitTime;
            var logSubmitTime = log.SubmitTime;
            
            return logSubmitTime - start;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception thrown when attempting to GetTotalTime from a production log, in ProductionLogService");
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
            Log.Warning("Exception thrown when attempting to GetById with ID: {productionLogId} in Production Logs, in ProductionLogService", id);
            return null;
        }
    }

    public async Task<ProductionLog?> GetByIdAsync(int id)
    {
        try
        {
            var productionLog = await _context.ProductionLogs
                .Include(p => p.LogSteps)
                .ThenInclude(p => p.WorkInstructionStep)
                .FirstOrDefaultAsync(p => p.Id == id);

            return productionLog;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception thrown when attempting to GetByIdAsync with ID: {productionLogId} in Production Logs, in ProductionLogService", id);
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
            
            Log.Information("Successfully created Production Log with ID: {productionLogID}", productionLog.Id);
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception thrown when attempting to Create ProductionLog, in ProductionLogService");
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
            Log.Information("Production Log with ID: {productionLogId}, successfully deleted.", id);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception thrown when attempting to Delete by ID: {productionLogId} in Production Logs, in ProductionLogService", id);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(ProductionLog existingProductionLog)
    {
        try
        {
            if (existingProductionLog == null)
            {
                return false;
            }

            _context.ProductionLogs.Update(existingProductionLog);
            await _context.SaveChangesAsync();
            
            Log.Information("Production Log with ID: {productionLogId}, successfully updated.", existingProductionLog.Id);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception thrown when attempting to UpdateAsync, with ID: {productionLogId} in Production Logs, in ProductionLogService", existingProductionLog.Id);
            return false;
        }
    }

    public bool UpdateWithObjectAsync(ProductionLog existing, ProductionLog updated)
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
            
            Log.Information("Production Log with ID: {productionLogId}, successfully updated with another ProductionLog.", existing.Id);


            return true;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception thrown when attempting to UpdateWithObjectAsync, Existing ID: {existingID} in Production Logs, in ProductionLogService", existing.Id);
            return false;
        }
    }
}