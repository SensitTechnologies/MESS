using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.ProductionLog;
using Data.Models;

/// <inheritdoc />
public class ProductionLogService : IProductionLogService
{
    private readonly ApplicationContext _context;
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionLogService"/> class.
    /// </summary>
    /// <param name="context">The application database context used for accessing production logs.</param>
    public ProductionLogService(ApplicationContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<List<ProductionLog>?> GetAllAsync()
    {
        try
        {
            return await _context.ProductionLogs
                .Include(p => p.WorkInstruction)
                .ThenInclude(w => w!.Nodes)
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

    /// <inheritdoc />
    public async Task<ProductionLog?> GetByIdAsync(int id)
    {
        try
        {
            var productionLog = await _context.ProductionLogs
                .Include(p => p.LogSteps)
                .ThenInclude(p => p.WorkInstructionStep)
                .Include(w => w.WorkInstruction)
                .Include(p => p.Product)
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

    /// <inheritdoc />
    public async Task<int> CreateAsync(ProductionLog productionLog)
    {
        try
        {
            if (productionLog.WorkInstruction != null)
            {
                productionLog.WorkInstruction = await _context.WorkInstructions
                    .FindAsync(productionLog.WorkInstruction.Id);
            }
            
            productionLog.CreatedOn = DateTimeOffset.UtcNow;
            productionLog.LastModifiedOn = DateTimeOffset.UtcNow;
            
            await _context.ProductionLogs.AddAsync(productionLog);
            
            await _context.SaveChangesAsync();

            if (productionLog.LogSteps is {Count: > 0})
            {
                foreach (var logStep in productionLog.LogSteps)
                {
                    logStep.ProductionLogId = productionLog.Id;
                }
                await _context.SaveChangesAsync();
            }
            
            Log.Information("Successfully created Production Log with ID: {productionLogID}", productionLog.Id);
            
            return productionLog.Id;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception thrown when attempting to Create ProductionLog, in ProductionLogService");
            return -1;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var productionLogToDelete = _context.ProductionLogs.Find(id);
            
            if (productionLogToDelete == null)
            {
                return false;
            }
            
            _context.ProductionLogs.Remove(productionLogToDelete);
            await _context.SaveChangesAsync();
            
            Log.Information("Production Log with ID: {productionLogId}, successfully deleted.", id);
            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to Delete by ID: {productionLogId} in Production Logs, in ProductionLogService. Exception: {ExceptionMessage}", id, e.Message);
            return false;
        }
    }

    /// <inheritdoc />
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
            Log.Warning("Exception thrown when attempting to UpdateAsync, with ID: {productionLogId} in Production Logs, in ProductionLogService. Exception: {exceptionMessage}", existingProductionLog.Id, e.Message);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<List<ProductionLog>?> GetProductionLogsByListOfIdsAsync(List<int> logIds)
    {
        try
        {
            if (logIds.Count <= 0)
            {
                return [];
            }
            
            return await _context.ProductionLogs
                .Include(p => p.WorkInstruction)
                .ThenInclude(w => w!.Nodes)
                .Include(p => p.LogSteps)
                .Where(p => logIds.Contains(p.Id))
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return [];
        }
    }
}