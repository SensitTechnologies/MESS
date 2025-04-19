using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.ProductionLog;
using Data.Models;

/// <inheritdoc />
public class ProductionLogService : IProductionLogService
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionLogService"/> class.
    /// </summary>
    /// <param name="contextFactory">The application database context used for accessing production logs.</param>
    public ProductionLogService(IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    /// <inheritdoc />
    public async Task<List<ProductionLog>?> GetAllAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProductionLogs
                .Include(p => p.WorkInstruction)
                .ThenInclude(w => w!.Nodes)
                .Include(p => p.LogSteps)
                .ThenInclude(p => p.WorkInstructionStep)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to GetAllAsync Production Logs, in ProductionLogService: Exception Type: {exceptionType}", e.GetType());
            return new List<ProductionLog>();
        }
    }

    /// <inheritdoc />
    public async Task<ProductionLog?> GetByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var productionLog = await context.ProductionLogs
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            


            if (productionLog.Product is { Id: > 0 })
            {
                context.Entry(productionLog.Product).State = EntityState.Unchanged;
            }
            
            if (productionLog.WorkInstruction is { Id: > 0 })
            {
                context.Entry(productionLog.WorkInstruction).State = EntityState.Unchanged;
                
                foreach (var node in productionLog.WorkInstruction.Nodes.Where(node => node.Id > 0))
                {
                    context.Entry(node).State = EntityState.Unchanged;
                }
            }
            
            productionLog.CreatedOn = DateTimeOffset.UtcNow;
            productionLog.LastModifiedOn = DateTimeOffset.UtcNow;
            
            await context.ProductionLogs.AddAsync(productionLog);
            await context.SaveChangesAsync();

            if (productionLog.LogSteps is {Count: > 0})
            {
                foreach (var logStep in productionLog.LogSteps)
                {
                    logStep.ProductionLogId = productionLog.Id;
                    if (logStep.WorkInstructionStep is { Id: > 0 })
                    {
                        context.Entry(logStep.WorkInstructionStep).State = EntityState.Unchanged;
                    }
                }
                await context.SaveChangesAsync();
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            var productionLogToDelete = await context.ProductionLogs.FindAsync(id);
            
            if (productionLogToDelete == null)
            {
                return false;
            }
            
            context.ProductionLogs.Remove(productionLogToDelete);
            await context.SaveChangesAsync();
            
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
            
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.ProductionLogs.Update(existingProductionLog);
            await context.SaveChangesAsync();
            
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
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ProductionLogs
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