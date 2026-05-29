using MESS.Data.Context;
using MESS.Services.CRUD.SerializableParts;
using MESS.Services.DTOs.ProductionLogs.Batch;
using MESS.Services.DTOs.ProductionLogs.Archive;
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
    private readonly ISerializablePartService _serializablePartService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionLogService"/> class.
    /// </summary>
    /// <param name="contextFactory">The application database context used for accessing production logs.</param>
    /// <param name="serializablePartService">The service for managing serializable parts.</param>
    /// <remarks>The SerializablePartService is used here for loading the ProductionLogDetailDTO.</remarks>
    public ProductionLogService(IDbContextFactory<ApplicationContext> contextFactory, ISerializablePartService serializablePartService)
    {
        _contextFactory = contextFactory;
        _serializablePartService = serializablePartService;
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
                    ProductName = p.Product != null ? p.Product.PartDefinition.Name : string.Empty,
                    WorkInstructionName = p.WorkInstruction != null ? p.WorkInstruction.Title : string.Empty,
                    CreatedOn = p.CreatedOn,
                    CreatedBy = p.CreatedBy,
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
    public async Task<ProductionLogArchivePageDTO> GetArchivePageAsync(ProductionLogArchiveQuery query)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = query.PageSize is 25 or 50 or 100 or 250 ? query.PageSize : 50;

        await using var context = await _contextFactory.CreateDbContextAsync();

        var archiveQuery = BuildArchiveQuery(context);
        archiveQuery = ApplyArchiveFilters(archiveQuery, query);
        archiveQuery = ApplyArchiveSort(archiveQuery, query.SortBy, query.SortDir);

        var total = await archiveQuery.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)query.PageSize));
        var page = Math.Min(query.Page, totalPages);

        var data = await archiveQuery
            .Skip((page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new ProductionLogArchivePageDTO
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };
    }

    /// <inheritdoc />
    public async Task<ProductionLogArchiveFilterOptionsDTO> GetArchiveFilterOptionsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var archiveQuery = BuildArchiveQuery(context);

        return new ProductionLogArchiveFilterOptionsDTO
        {
            Statuses = ["All Successes", "Has Failures", "Not Started"],
            Products = await DistinctArchiveValuesAsync(archiveQuery.Select(x => x.ProductName)),
            WorkInstructions = await DistinctArchiveValuesAsync(archiveQuery.Select(x => x.WorkInstructionName)),
            PartProduced = await DistinctArchiveValuesAsync(archiveQuery.Select(x => x.PartProducedName)),
            CreatedBy = await DistinctArchiveValuesAsync(archiveQuery.Select(x => x.CreatedBy)),
            ModifiedBy = await DistinctArchiveValuesAsync(archiveQuery.Select(x => x.LastModifiedBy))
        };
    }

    private static IQueryable<ProductionLogArchiveRowDTO> BuildArchiveQuery(ApplicationContext context)
    {
        return context.ProductionLogs
            .AsNoTracking()
            .Select(log => new ProductionLogArchiveRowDTO
            {
                ProductionLogId = log.Id,
                AttemptId = log.LogSteps
                    .SelectMany(step => step.Attempts)
                    .OrderByDescending(attempt => attempt.SubmitTime)
                    .Select(attempt => (int?)attempt.Id)
                    .FirstOrDefault(),
                Status = log.LogSteps.SelectMany(step => step.Attempts).Any(attempt => attempt.Success == false)
                    ? "Has Failures"
                    : log.LogSteps.SelectMany(step => step.Attempts).Any(attempt => attempt.Success == true)
                        ? "All Successes"
                        : "Not Started",
                ProductName = log.Product != null && log.Product.PartDefinition != null
                    ? log.Product.PartDefinition.Name
                    : string.Empty,
                WorkInstructionName = log.WorkInstruction != null
                    ? log.WorkInstruction.Title
                    : string.Empty,
                PartProducedName = log.WorkInstruction != null && log.WorkInstruction.PartProduced != null
                    ? log.WorkInstruction.PartProduced.Name
                    : string.Empty,
                ProducedPartSerialNumber = context.ProductionLogParts
                    .Where(part => part.ProductionLogId == log.Id && part.OperationType == PartOperationType.Produced)
                    .Join(
                        context.SerializableParts,
                        part => part.SerializablePartId,
                        serializablePart => serializablePart.Id,
                        (_, serializablePart) => serializablePart.SerialNumber ?? string.Empty)
                    .FirstOrDefault() ?? string.Empty,
                CreatedOn = log.CreatedOn,
                CreatedBy = log.CreatedBy,
                LastModifiedOn = log.LastModifiedOn,
                LastModifiedBy = log.LastModifiedBy ?? string.Empty
            });
    }

    private static IQueryable<ProductionLogArchiveRowDTO> ApplyArchiveFilters(
        IQueryable<ProductionLogArchiveRowDTO> query,
        ProductionLogArchiveQuery filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.Trim().ToLower();
            var hasAttemptSearch = int.TryParse(search, out var attemptId);
            query = query.Where(row =>
                row.ProductionLogId.ToString().Contains(search)
                || row.ProductName.ToLower().Contains(search)
                || row.WorkInstructionName.ToLower().Contains(search)
                || row.PartProducedName.ToLower().Contains(search)
                || row.ProducedPartSerialNumber.ToLower().Contains(search)
                || row.CreatedBy.ToLower().Contains(search)
                || row.LastModifiedBy.ToLower().Contains(search)
                || row.Status.ToLower().Contains(search)
                || (hasAttemptSearch && row.AttemptId == attemptId));
        }

        if (filters.FilterAttemptId is int filterAttemptId)
            query = query.Where(row => row.AttemptId == filterAttemptId);

        if (filters.FilterLogId is int filterLogId)
            query = query.Where(row => row.ProductionLogId == filterLogId);

        if (!string.IsNullOrWhiteSpace(filters.FilterStatus))
        {
            var value = filters.FilterStatus.Trim().ToLower();
            query = query.Where(row => row.Status.ToLower().Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(filters.FilterProduct))
        {
            var value = filters.FilterProduct.Trim().ToLower();
            query = query.Where(row => row.ProductName.ToLower().Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(filters.FilterWorkInstruction))
        {
            var value = filters.FilterWorkInstruction.Trim().ToLower();
            query = query.Where(row => row.WorkInstructionName.ToLower().Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(filters.FilterPartProduced))
        {
            var value = filters.FilterPartProduced.Trim().ToLower();
            query = query.Where(row => row.PartProducedName.ToLower().Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(filters.FilterProducedPartSerialNumber))
        {
            var value = filters.FilterProducedPartSerialNumber.Trim().ToLower();
            query = query.Where(row => row.ProducedPartSerialNumber.ToLower().Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(filters.FilterCreatedBy))
        {
            var value = filters.FilterCreatedBy.Trim().ToLower();
            query = query.Where(row => row.CreatedBy.ToLower().Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(filters.FilterModifiedBy))
        {
            var value = filters.FilterModifiedBy.Trim().ToLower();
            query = query.Where(row => row.LastModifiedBy.ToLower().Contains(value));
        }

        if (filters.FilterCreatedOnFrom is DateTimeOffset createdFrom)
            query = query.Where(row => row.CreatedOn >= createdFrom);

        if (filters.FilterCreatedOnTo is DateTimeOffset createdTo)
            query = query.Where(row => row.CreatedOn <= createdTo);

        if (filters.FilterModifiedOnFrom is DateTimeOffset modifiedFrom)
            query = query.Where(row => row.LastModifiedOn >= modifiedFrom);

        if (filters.FilterModifiedOnTo is DateTimeOffset modifiedTo)
            query = query.Where(row => row.LastModifiedOn <= modifiedTo);

        return query;
    }

    private static IQueryable<ProductionLogArchiveRowDTO> ApplyArchiveSort(
        IQueryable<ProductionLogArchiveRowDTO> query,
        string? sortBy,
        string? sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy ?? "createdOn") switch
        {
            "logId" => descending
                ? query.OrderByDescending(row => row.ProductionLogId)
                : query.OrderBy(row => row.ProductionLogId),
            "attemptId" => descending
                ? query.OrderByDescending(row => row.AttemptId)
                : query.OrderBy(row => row.AttemptId),
            "status" => descending
                ? query.OrderByDescending(row => row.Status)
                : query.OrderBy(row => row.Status),
            "product" => descending
                ? query.OrderByDescending(row => row.ProductName)
                : query.OrderBy(row => row.ProductName),
            "workInstruction" => descending
                ? query.OrderByDescending(row => row.WorkInstructionName)
                : query.OrderBy(row => row.WorkInstructionName),
            "partProduced" => descending
                ? query.OrderByDescending(row => row.PartProducedName)
                : query.OrderBy(row => row.PartProducedName),
            "producedPartSerialNumber" => descending
                ? query.OrderByDescending(row => row.ProducedPartSerialNumber)
                : query.OrderBy(row => row.ProducedPartSerialNumber),
            "createdBy" => descending
                ? query.OrderByDescending(row => row.CreatedBy)
                : query.OrderBy(row => row.CreatedBy),
            "modifiedOn" => descending
                ? query.OrderByDescending(row => row.LastModifiedOn)
                : query.OrderBy(row => row.LastModifiedOn),
            "modifiedBy" => descending
                ? query.OrderByDescending(row => row.LastModifiedBy)
                : query.OrderBy(row => row.LastModifiedBy),
            _ => descending
                ? query.OrderByDescending(row => row.CreatedOn)
                : query.OrderBy(row => row.CreatedOn)
        };
    }

    private static async Task<List<string>> DistinctArchiveValuesAsync(IQueryable<string> query)
    {
        return await query
            .Where(value => value != "")
            .Distinct()
            .OrderBy(value => value)
            .ToListAsync();
    }
    
    /// <inheritdoc />
    public async Task<ProductionLogBatchResult> SaveOrUpdateBatchAsync(
        IEnumerable<ProductionLogFormDTO> formDtos,
        string createdBy,
        string operatorId,
        int productId,
        int workInstructionId)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(formDtos);

            var result = new ProductionLogBatchResult();

            await using var context = await _contextFactory.CreateDbContextAsync();

            var product = await context.Products.FindAsync(productId)
                          ?? throw new InvalidOperationException($"Product {productId} not found.");

            var workInstruction = await context.WorkInstructions.FindAsync(workInstructionId)
                                  ?? throw new InvalidOperationException(
                                      $"WorkInstruction {workInstructionId} not found.");

            Log.Information(
                "Starting SaveOrUpdateBatchAsync for ProductId={ProductId}, WorkInstructionId={WorkInstructionId}, OperatorId={OperatorId}, CreatedBy={CreatedBy}",
                productId, workInstructionId, operatorId, createdBy);

            // Gather IDs of logs that might already exist
            var productionLogFormDtos = formDtos.ToList();
            var dtoIds = productionLogFormDtos.Where(f => f.Id > 0).Select(f => f.Id).Distinct().ToList();
            
            var existingLogs = dtoIds.Count > 0
                ? await context.ProductionLogs
                    .Where(l => dtoIds.Contains(l.Id))
                    .Include(l => l.LogSteps)
                    .ThenInclude(ls => ls.Attempts)
                    .ToListAsync()
                : [];
           
            if (existingLogs.Count > 0)
            {
                Log.Debug(
                    "Fetched {ExistingCount} existing logs (targeted lookup) for OperatorId={OperatorId}, WorkInstructionId={WorkInstructionId}",
                    existingLogs.Count, operatorId, workInstructionId);
            }
            else
            {
                Log.Debug(
                    "All logs in batch are new — skipping lookup for OperatorId={OperatorId}, WorkInstructionId={WorkInstructionId}",
                    operatorId, workInstructionId);
            }

            var existingLogsDict = existingLogs.ToDictionary(l => l.Id, l => l);


            var logsToCreate = new List<(ProductionLogFormDTO FormDto, ProductionLog Log)>();

            foreach (var formDto in productionLogFormDtos)
            {
                if (formDto.Id > 0 && existingLogsDict.TryGetValue(formDto.Id, out var existingLog))
                {
                    existingLog.ApplyUpdateRequest(formDto.ToUpdateRequest(), modifiedBy: createdBy);
                    result.UpdatedCount++;
                    result.UpdatedIds.Add(existingLog.Id);
                    Log.Debug("Updated existing ProductionLog Id={LogId}", existingLog.Id);
                }
                else
                {
                    logsToCreate.Add((formDto, CreateLog(formDto)));
                    result.CreatedCount++;
                    Log.Debug("Queued new ProductionLog for creation (TempId={TempId})", formDto.Id);
                }
            }

            // Bulk insert
            if (logsToCreate.Count > 0)
            {
                await context.ProductionLogs.AddRangeAsync(logsToCreate.Select(item => item.Log));
                Log.Information("Queued {CreatedCount} new ProductionLogs for creation", logsToCreate.Count);
            }
            
            // CLEAN UP: remove steps with zero attempts
            foreach (var existing in existingLogsDict.Values)
                RemoveEmptySteps(existing, context);

            foreach (var (_, newLog) in logsToCreate)
                RemoveEmptySteps(newLog, context);

            // EF Core tracks changes to existing logs, so updates are auto-detected

            await context.SaveChangesAsync();

            foreach (var formDto in productionLogFormDtos)
            {
                ProductionLog? persistedLog = null;
                if (formDto.Id > 0)
                {
                    existingLogsDict.TryGetValue(formDto.Id, out persistedLog);
                }
                else
                {
                    persistedLog = logsToCreate.FirstOrDefault(item => ReferenceEquals(item.FormDto, formDto)).Log;
                }

                if (persistedLog != null)
                {
                    ApplyPersistedIds(formDto, persistedLog);
                }
            }

            // Collect IDs of newly created logs
            result.CreatedIds.AddRange(logsToCreate.Select(item => item.Log.Id));

            Log.Information(
                "Successfully processed batch: Created={CreatedCount}, Updated={UpdatedCount}, ProductId={ProductId}, WorkInstructionId={WorkInstructionId}, OperatorId={OperatorId}",
                result.CreatedCount, result.UpdatedCount, productId, workInstructionId, operatorId);

            return result;

            // Local Helper Function
            ProductionLog CreateLog(ProductionLogFormDTO dto)
            {
                var createRequest = dto.ToCreateRequest(createdBy, operatorId, productId, workInstructionId);
                var log = createRequest.ToEntity() ??
                          throw new InvalidOperationException("Failed to map create request to entity.");
                log.Product = product;
                log.WorkInstruction = workInstruction;
                return log;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "Exception thrown in SaveOrUpdateBatchAsync for ProductId={ProductId}, WorkInstructionId={WorkInstructionId}, OperatorId={OperatorId}, CreatedBy={CreatedBy}",
                productId, workInstructionId, operatorId, createdBy);
            throw; // rethrow to preserve stack trace
        }
    }

    private static void ApplyPersistedIds(ProductionLogFormDTO dto, ProductionLog entity)
    {
        dto.Id = entity.Id;

        foreach (var formStep in dto.LogSteps)
        {
            var persistedStep = entity.LogSteps
                .FirstOrDefault(step => step.WorkInstructionStepId == formStep.WorkInstructionStepId);

            if (persistedStep == null)
                continue;

            formStep.ProductionLogStepId = persistedStep.Id;

            var formAttempts = formStep.Attempts
                .OrderBy(attempt => attempt.SubmitTime)
                .ToList();

            var persistedAttempts = persistedStep.Attempts
                .OrderBy(attempt => attempt.SubmitTime)
                .ToList();

            for (var i = 0; i < formAttempts.Count && i < persistedAttempts.Count; i++)
            {
                formAttempts[i].AttemptId = persistedAttempts[i].Id;
            }
        }
    }
    
    /// <summary>
    /// Removes any ProductionLogStep that contains zero attempts.
    /// For existing logs, steps are deleted from the DbContext.
    /// For new logs, steps are removed from the navigation collection before saving.
    /// </summary>
    private static void RemoveEmptySteps(ProductionLog log, ApplicationContext context)
    {
        if (log.LogSteps.Count == 0)
            return;

        // Steps with no attempts
        var emptySteps = log.LogSteps
            .Where(s => s.Attempts.Count == 0)
            .ToList();

        if (emptySteps.Count == 0)
            return;

        foreach (var step in emptySteps)
        {
            // Remove from parent collection (works for both new and existing)
            log.LogSteps.Remove(step);

            // If the step already exists in the DB, remove it
            if (step.Id > 0)
                context.ProductionLogSteps.Remove(step);
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
    public async Task<bool> UpdateDetailAsync(ProductionLogDetailDTO dto)
    {
        var request = dto.ToUpdateRequest();
        return await UpdateAsync(request, dto.LastModifiedBy ?? "system");
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
                .ThenInclude(prod => prod!.PartDefinition)
                .Include(p => p.WorkInstruction)
                .ThenInclude(w => w!.Nodes)
                .Include(p => p.LogSteps)
                .ThenInclude(ls => ls.Attempts)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (log is null)
            {
                Log.Warning(
                    "ProductionLog with ID {LogId} not found in GetDetailByIdAsync.",
                    id);
                return null;
            }

            // Map core log → DTO
            var dto = log.ToDetailDTO();

            // Load produced part separately (NO tracking)
            var producedPart = await _serializablePartService
                .GetProducedForProductionLogAsync(log.Id);
            
            var installedParts = await _serializablePartService
                .GetInstalledForProductionLogAsync(log.Id);

            dto.ProducedPartName = producedPart?.PartDefinition?.Name;
            dto.ProducedPartSerialNumber = producedPart?.SerialNumber;
            dto.InstalledParts = installedParts;

            return dto;
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
    public async Task<List<ProductionLogSummaryDTO>> GetSummariesByOperatorIdAsync(string operatorId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ProductionLogs
                .Where(p => p.OperatorId == operatorId)
                .OrderByDescending(p => p.CreatedOn)
                .Select(p => new ProductionLogSummaryDTO
                {
                    Id = p.Id,
                    ProductName = p.Product != null ? p.Product.PartDefinition.Name : string.Empty,
                    WorkInstructionName = p.WorkInstruction != null ? p.WorkInstruction.Title : string.Empty,
                    CreatedOn = p.CreatedOn,
                    LastModifiedOn = p.LastModifiedOn,
                    LastModifiedBy = p.LastModifiedBy,
                    CreatedBy = p.CreatedBy
                })
                .ToListAsync();
        }
        catch (Exception e)
        {
            Log.Warning(
                "Exception thrown when attempting to GetProductionLogSummariesByOperatorIdAsync for OperatorId: {OperatorId} in ProductionLogService: {Exception}",
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