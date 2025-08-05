using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.ProductionLogServices;
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
                .Include(p => p.Product)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to GetAllAsync Production Logs, in ProductionLogService: Exception: {Exception}", e);
            return new List<ProductionLog>();
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> UpdateAsync(ProductionLog updatedLog)
{
    try
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load existing ProductionLog with related data from DB
        var existingLog = await context.ProductionLogs
            .Include(p => p.LogSteps)
            .ThenInclude(ls => ls.Attempts)
            .Include(p => p.WorkInstruction)
            .ThenInclude(wi => wi!.Nodes)
            .FirstOrDefaultAsync(p => p.Id == updatedLog.Id);

        if (existingLog == null)
        {
            Log.Warning("ProductionLog with ID {LogId} not found for update.", updatedLog.Id);
            return false;
        }

        // Update simple properties on ProductionLog
        existingLog.LastModifiedOn = DateTimeOffset.UtcNow;

        // Update or add LogSteps and their Attempts
        foreach (var updatedStep in updatedLog.LogSteps)
        {
            // Try to find matching existing step
            var existingStep = existingLog.LogSteps.FirstOrDefault(s => s.Id == updatedStep.Id);

            if (existingStep == null)
            {
                // New step: add it 
                existingLog.LogSteps.Add(updatedStep);
            }
            else
            {
                // Sync Attempts
                foreach (var updatedAttempt in updatedStep.Attempts)
                {
                    var existingAttempt = existingStep.Attempts.FirstOrDefault(a => a.Id == updatedAttempt.Id);

                    if (existingAttempt == null)
                    {
                        // New attempt
                        existingStep.Attempts.Add(updatedAttempt);
                    }
                    else
                    {
                        // Update attempt properties
                        existingAttempt.SubmitTime = updatedAttempt.SubmitTime;
                        existingAttempt.Success = updatedAttempt.Success;
                        existingAttempt.Notes = updatedAttempt.Notes;
                    }
                }
            }
        }

        // Save changes
        await context.SaveChangesAsync();

        Log.Information("Successfully updated Production Log with ID: {LogId}", updatedLog.Id);
        return true;
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Exception thrown when attempting to UpdateAsync, with ID: {LogId} in Production Logs, in ProductionLogService.", updatedLog.Id);
        return false;
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
                .ThenInclude(ls => ls.WorkInstructionStep)
                .Include(p => p.LogSteps)
                .ThenInclude(ls => ls.Attempts)
                .Include(w => w.WorkInstruction)
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            return productionLog;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to GetByIdAsync with ID: {productionLogId} in Production Logs, in ProductionLogService: {Exception}", id, e.ToString());
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
            Log.Warning("Exception thrown when attempting to CreateAsync ProductionLog, in ProductionLogService: {Exception}", e.ToString());
            return -1;
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
            Log.Warning("Exception thrown when attempting to GetProductionLogsByListOfIdsAsync, in ProductionLogService: {Exception}", e.ToString());
            return [];
        }
    }
    
    /// <inheritdoc />
    public async Task<List<ProductionLog>?> GetProductionLogsByOperatorIdAsync(string operatorId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ProductionLogs
                .Include(p => p.WorkInstruction)
                .ThenInclude(w => w!.Nodes)
                .Include(p => p.Product)
                .Include(p => p.LogSteps)
                .ThenInclude(p => p.WorkInstructionStep)
                .Where(p => p.OperatorId == operatorId)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Log.Warning(
                "Exception thrown when attempting to GetProductionLogsByOperatorIdAsync for OperatorId: {OperatorId} in ProductionLogService: {Exception}",
                operatorId,
                e.ToString()
            );
            return new List<ProductionLog>();
        }
    }
    
    /// <inheritdoc />
    public async Task DeleteAttemptAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.ProductionLogStepAttempts.FindAsync(id);
        if (entity != null)
        {
            context.ProductionLogStepAttempts.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
    
    /// <summary>
    /// Deletes a <see cref="ProductionLog"/> and all associated log steps and step attempts from the database by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the production log to delete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// Returns <c>true</c> if the log was found and deleted; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> DeleteProductionLogAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var log = await context.ProductionLogs
            .Include(l => l.LogSteps)
            .ThenInclude(s => s.Attempts)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (log == null)
            return false;

        context.ProductionLogs.Remove(log);
        await context.SaveChangesAsync();
        return true;
    }
}