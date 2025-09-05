using MESS.Data.Context;
using MESS.Services.DTOs.ProductionLogs.Batch;
using MESS.Services.DTOs.ProductionLogs.CreateRequest;
using MESS.Services.DTOs.ProductionLogs.Detail;
using MESS.Services.DTOs.ProductionLogs.Form;
using MESS.Services.DTOs.ProductionLogs.Summary;
using MESS.Services.DTOs.ProductionLogs.UpdateRequest;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.CRUD.ProductionLogs;
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
    public async Task<List<ProductionLogSummaryDTO>> GetAllSummariesAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ProductionLogs
                .Include(p => p.Product)             // Only include what the DTO needs
                .Include(p => p.WorkInstruction)
                .Select(p => new ProductionLogSummaryDTO
                {
                    Id = p.Id,
                    ProductName = p.Product != null ? p.Product.Name : string.Empty,
                    WorkInstructionName = p.WorkInstruction != null ? p.WorkInstruction.Title : string.Empty,
                    ProductSerialNumber = p.ProductSerialNumber,
                    CreatedOn = p.CreatedOn,
                    LastModifiedOn = p.LastModifiedOn,
                    LastModifiedBy = p.LastModifiedBy
                })
                .ToListAsync();
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to GetAllSummariesAsync Production Logs: {Exception}", e);
            return [];
        }
    }
    
    /// <inheritdoc />
    public async Task<ProductionLogBatchResult> SaveOrUpdateBatchAsync(
        IEnumerable<ProductionLogFormDTO> formDtos,
        string createdBy,
        string operatorId,
        int productId,
        int workInstructionId)
    {
        if (formDtos == null) throw new ArgumentNullException(nameof(formDtos));

        var result = new ProductionLogBatchResult();

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var product = await context.Products.FindAsync(productId)
                      ?? throw new InvalidOperationException($"Product {productId} not found.");
        
        var workInstruction = await context.WorkInstructions.FindAsync(workInstructionId)
                              ?? throw new InvalidOperationException($"WorkInstruction {workInstructionId} not found.");

        // Pre-fetch all existing logs for this operator and work instruction
        var existingLogs = await context.ProductionLogs
            .Where(l => l.OperatorId == operatorId && l.WorkInstructionId == workInstructionId)
            .Include(l => l.LogSteps)
            .ThenInclude(ls => ls.Attempts)
            .ToListAsync();
        var existingLogsDict = existingLogs.ToDictionary(l => l.Id);
        
        var logsToCreate = new List<ProductionLog>();

        foreach (var formDto in formDtos)
        {
            if (formDto.Id > 0 && existingLogsDict.TryGetValue(formDto.Id, out var existingLog))
            {
                existingLog.ApplyUpdateRequest(formDto.ToUpdateRequest(), modifiedBy: createdBy);
                result.UpdatedCount++;
                result.UpdatedIds.Add(existingLog.Id);
            }
            else
            {
                logsToCreate.Add(CreateLog(formDto));
                result.CreatedCount++;
            }
        }
        
        // Bulk insert
        if (logsToCreate.Count > 0)
            await context.ProductionLogs.AddRangeAsync(logsToCreate);

        // EF Core tracks changes to existing logs, so updates are auto-detected

        await context.SaveChangesAsync();

        // Collect IDs of newly created logs
        result.CreatedIds.AddRange(logsToCreate.Select(l => l.Id));

        return result;
        
        // Local Helper Function
        ProductionLog CreateLog(ProductionLogFormDTO dto)
        {
            var createRequest = dto.ToCreateRequest(createdBy, operatorId, productId, workInstructionId);
            var log = createRequest.ToEntity() ?? throw new InvalidOperationException("Failed to map create request to entity.");
            log.Product = product;
            log.WorkInstruction = workInstruction;
            return log;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> UpdateAsync(ProductionLogUpdateRequest request, string modifiedBy)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Load existing log with steps and attempts
            var existingLog = await context.ProductionLogs
                .Include(p => p.LogSteps)
                .ThenInclude(ls => ls.Attempts)
                .FirstOrDefaultAsync(p => p.Id == request.Id);

            if (existingLog == null)
            {
                Log.Warning("ProductionLog with ID {LogId} not found for update.", request.Id);
                return false;
            }

            // Apply updates from DTO (mapper handles step/attempt updates)
            existingLog.ApplyUpdateRequest(request, modifiedBy);

            // No need to attach WorkInstructionStep; EF tracks attempts via LogSteps
            await context.SaveChangesAsync();

            Log.Information("Successfully updated Production Log with ID: {LogId}", request.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Exception thrown when attempting to UpdateAsync Production Log with ID: {LogId}", request.Id);
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
    public async Task<ProductionLogDetailDTO?> GetDetailByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Load log with its steps and attempts, tracked by EF
            var log = await context.ProductionLogs
                .Include(p => p.Product)
                .Include(p => p.WorkInstruction)
                .ThenInclude(w => w!.Nodes)
                .Include(p => p.LogSteps)
                .ThenInclude(ls => ls.Attempts)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (log != null) return log.ToDetailDTO();
            Log.Warning("ProductionLog with ID {LogId} not found in GetDetailByIdAsync.", id);
            return null;

            // Map to DTO for UI consumption
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Exception thrown when attempting to GetDetailByIdAsync Production Log with ID: {LogId}", id);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(ProductionLogCreateRequest request)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Map DTO → Entity
            var productionLog = request.ToEntity();
            if (productionLog == null)
            {
                Log.Warning("ProductionLogCreateRequest mapping returned null");
                return -1;
            }

            // Assign FK references only
            if (request.ProductId > 0)
                productionLog.ProductId = request.ProductId;

            if (request.WorkInstructionId > 0)
                productionLog.WorkInstructionId = request.WorkInstructionId;

            foreach (var step in productionLog.LogSteps)
            {
                if (step.WorkInstructionStepId > 0)
                    step.WorkInstructionStepId = step.WorkInstructionStepId;
            }

            // Add root entity; EF will cascade inserts to steps and attempts
            await context.ProductionLogs.AddAsync(productionLog);
            await context.SaveChangesAsync();

            Log.Information("Successfully created Production Log with ID: {LogId}", productionLog.Id);
            return productionLog.Id;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to CreateAsync ProductionLog: {Exception}", e);
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
            return [];
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

    /// <summary>
    /// Deletes all production logs associated with a work instruction
    /// </summary>
    /// <param name="workInstruction">work instruction to query all production logs by</param>
    /// <returns></returns>
    public async Task<bool> DeleteByWorkInstructionAsync(WorkInstruction workInstruction)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var logs = await context.ProductionLogs
            .Include(p => p.WorkInstruction)
            .Include(p => p.LogSteps)
            .Where(p => p.WorkInstruction == workInstruction)
            .ToListAsync();

        if (logs.Count == 0)
            return false;
        
        context.ProductionLogs.RemoveRange(logs);
        await context.SaveChangesAsync();
        return true;
    }
}