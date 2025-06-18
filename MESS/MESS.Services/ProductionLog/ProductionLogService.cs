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
            Log.Warning("Exception thrown when attempting to GetAllAsync Production Logs, in ProductionLogService: Exception: {Exception}", e);
            return new List<ProductionLog>();
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
            Log.Warning("Exception thrown when attempting to UpdateAsync, with ID: {productionLogId} in Production Logs, in ProductionLogService. Exception: {Exception}", existingProductionLog.Id, e.ToString());
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
                .ThenInclude(p => p.WorkInstructionStep)
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

            // Mark existing Product as Unchanged
            if (productionLog.Product is { Id: > 0 })
            {
                context.Entry(productionLog.Product).State = EntityState.Unchanged;
            }

            // Mark WorkInstruction and its Nodes as Unchanged
            if (productionLog.WorkInstruction is { Id: > 0 })
            {
                context.Entry(productionLog.WorkInstruction).State = EntityState.Unchanged;

                foreach (var node in productionLog.WorkInstruction.Nodes.Where(n => n.Id > 0))
                {
                    context.Entry(node).State = EntityState.Unchanged;
                }
            }

            // For each LogStep
            if (productionLog.LogSteps is { Count: > 0 })
            {
                foreach (var step in productionLog.LogSteps)
                {
                    // Remove any EF-tracked reference and use only the FK
                    if (step.WorkInstructionStep is { Id: > 0 })
                    {
                        context.Entry(step.WorkInstructionStep).State = EntityState.Unchanged;
                    }

                    // Avoid attaching navigation property directly if possible
                    step.WorkInstructionStep = null; // prevent EF from trying to insert it again
                }
            }

            productionLog.CreatedOn = DateTimeOffset.UtcNow;
            productionLog.LastModifiedOn = DateTimeOffset.UtcNow;
            
            // Debug safeguard to catch bad entity states
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added && entry.Metadata.FindPrimaryKey() != null)
                {
                    var idProperty = entry.Properties.FirstOrDefault(p =>
                        string.Equals(p.Metadata.Name, "Id", StringComparison.OrdinalIgnoreCase));

                    if (idProperty?.CurrentValue is int id && id > 0)
                    {
                        Log.Warning("⚠️ Entity of type {EntityType} is in Added state but has existing ID: {Id}",
                            entry.Entity.GetType().Name, id);
                    }
                }
            }
            
            // Save the ProductionLog (with FK references only)
            await context.ProductionLogs.AddAsync(productionLog);
            await context.SaveChangesAsync();

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
}