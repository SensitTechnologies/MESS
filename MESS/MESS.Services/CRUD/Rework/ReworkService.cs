using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.DTOs.Rework;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.CRUD.Rework;

/// <inheritdoc />
public class ReworkService : IReworkService
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReworkService"/> class.
    /// </summary>
    /// <param name="contextFactory">Factory for the application database context.</param>
    public ReworkService(IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<ReworkPageDTO> GetPageAsync(ReworkQuery query)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = query.PageSize is 25 or 50 or 100 or 250 ? query.PageSize : 50;

        await using var context = await _contextFactory.CreateDbContextAsync();

        var rows = BuildRowQuery(context);
        rows = ApplyFilters(rows, query);
        rows = ApplySort(rows, query.SortBy, query.SortDir);

        var total = await rows.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)query.PageSize));
        var page = Math.Min(query.Page, totalPages);

        var data = await rows
            .Skip((page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new ReworkPageDTO
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };
    }

    /// <inheritdoc />
    public async Task<ReworkFilterOptionsDTO> GetFilterOptionsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var rows = BuildRowQuery(context);

        return new ReworkFilterOptionsDTO
        {
            Nouns = await DistinctValuesAsync(rows.Select(x => x.NounName)),
            Adjectives = await DistinctValuesAsync(rows.Select(x => x.AdjectiveName)),
            Products = await DistinctValuesAsync(rows.Select(x => x.ProductName)),
            Operators = await DistinctValuesAsync(rows.Select(x => x.OperatorName))
        };
    }

    /// <inheritdoc />
    public async Task<ReworkEditDTO?> GetForEditAsync(int attemptId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var attempt = await context.ProductionLogStepAttempts
            .AsNoTracking()
            .Include(a => a.FailureNoun)
            .Include(a => a.FailureAdjective)
            .Include(a => a.ProductionLogStep)
                .ThenInclude(s => s!.WorkInstructionStep)
            .Include(a => a.ProductionLogStep)
                .ThenInclude(s => s!.ProductionLog)
                .ThenInclude(l => l!.Product)
                .ThenInclude(p => p!.PartDefinition)
            .Include(a => a.ProductionLogStep)
                .ThenInclude(s => s!.ProductionLog)
                .ThenInclude(l => l!.WorkInstruction)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        var step = attempt?.ProductionLogStep;
        var log = step?.ProductionLog;
        if (attempt == null || step == null || log == null)
            return null;

        var otherAttemptStepIds = await context.ProductionLogStepAttempts
            .AsNoTracking()
            .Where(a => a.ProductionLogStep!.ProductionLogId == log.Id && a.Id != attemptId)
            .Select(a => a.ProductionLogStepId)
            .ToListAsync();

        var operatorName = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == log.OperatorId)
            .Select(u => new { u.FirstName, u.LastName, u.UserName })
            .FirstOrDefaultAsync();

        return new ReworkEditDTO
        {
            AttemptId = attempt.Id,
            ProductionLogId = log.Id,
            SubmitTime = attempt.SubmitTime,
            LogLastModifiedOn = log.LastModifiedOn,
            Values = ToEditValues(attempt, step, log),
            FailureNounName = attempt.FailureNoun?.Name,
            FailureAdjectiveName = attempt.FailureAdjective?.Name,
            ProductName = log.Product?.PartDefinition?.Name ?? string.Empty,
            InstructionName = log.WorkInstruction == null
                ? string.Empty
                : FormatInstruction(log.WorkInstruction.Title, log.WorkInstruction.Version),
            OperatorName = operatorName == null
                ? string.Empty
                : FormatOperator(operatorName.FirstName, operatorName.LastName, operatorName.UserName),
            StepName = step.WorkInstructionStep?.Name ?? string.Empty,
            OtherAttemptCount = otherAttemptStepIds.Count,
            OtherStepCount = otherAttemptStepIds.Distinct().Count(),
            HasPartTraceability = await context.ProductionLogParts.AnyAsync(p => p.ProductionLogId == log.Id)
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReworkStepOptionDTO>> GetStepsForInstructionAsync(int workInstructionId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await StepsOfInstruction(context, workInstructionId)
            .AsNoTracking()
            .OrderBy(s => s.Position)
            .Select(s => new ReworkStepOptionDTO { Id = s.Id, Name = s.Name, Position = s.Position })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ReworkUpdateResult> UpdateAsync(ReworkUpdateRequest request, string modifiedBy)
    {
        try
        {
            var validation = await new ReworkUpdateRequestValidator().ValidateAsync(request);
            if (!validation.IsValid)
            {
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation,
                    string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            if (string.IsNullOrWhiteSpace(modifiedBy) || !await CanReworkAsync(context, modifiedBy))
            {
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Forbidden,
                    "Only Technicians and Administrators can edit production attempts.");
            }

            var attempt = await context.ProductionLogStepAttempts
                .Include(a => a.ProductionLogStep)
                .ThenInclude(s => s!.ProductionLog)
                .FirstOrDefaultAsync(a => a.Id == request.AttemptId);

            var oldStep = attempt?.ProductionLogStep;
            var log = oldStep?.ProductionLog;
            if (attempt == null || oldStep == null || log == null)
            {
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.NotFound,
                    $"Attempt #{request.AttemptId} no longer exists.");
            }

            // Tracked load also fixes up log.LogSteps and every step's Attempts collection.
            var logSteps = await context.ProductionLogSteps
                .Include(s => s.Attempts)
                .Where(s => s.ProductionLogId == log.Id)
                .ToListAsync();

            var current = ToEditValues(attempt, oldStep, log);
            if (current != request.Original || log.LastModifiedOn != request.OriginalLogLastModifiedOn)
            {
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Conflict,
                    "This attempt was changed by someone else since you opened it. Close and reopen to see the latest values.");
            }

            var updated = request.Updated with { };
            if (updated == current)
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation, "There are no changes to save.");

            var instructionChanged = updated.WorkInstructionId != current.WorkInstructionId;
            var productChanged = updated.ProductId != current.ProductId;
            var operatorChanged = !string.Equals(updated.OperatorId, current.OperatorId, StringComparison.Ordinal);
            var stepChanged = updated.WorkInstructionStepId != current.WorkInstructionStepId;
            var nounChanged = updated.FailureNounId != current.FailureNounId;
            var adjectiveChanged = updated.FailureAdjectiveId != current.FailureAdjectiveId;

            var otherAttemptCount = logSteps.Sum(s => s.Attempts.Count) - 1;
            if (updated.ChangesLogFrom(current) && otherAttemptCount > 0 && !request.ConfirmSharedLogChange)
            {
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation,
                    $"This changes Production Log #{log.Id}, which also contains {otherAttemptCount} other attempt(s). Confirm the change to continue.");
            }

            // Rule 6: part traceability rows are tied to part nodes of the original instruction.
            if ((instructionChanged || productChanged)
                && await context.ProductionLogParts.AnyAsync(p => p.ProductionLogId == log.Id))
            {
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.PartTraceabilityBlocked,
                    $"Production Log #{log.Id} has part traceability records. Correct them in Part Traceability before changing its product or instruction.");
            }

            // Rule 2: product must be associated with the instruction.
            if (instructionChanged && !await context.WorkInstructions.AnyAsync(w => w.Id == updated.WorkInstructionId))
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation, "The selected instruction no longer exists.");

            if (productChanged && !await context.Products.AnyAsync(p => p.Id == updated.ProductId))
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation, "The selected product no longer exists.");

            if ((instructionChanged || productChanged)
                && !await context.WorkInstructions.AnyAsync(w =>
                    w.Id == updated.WorkInstructionId && w.Products.Any(p => p.Id == updated.ProductId)))
            {
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation,
                    "The selected product is not associated with the selected instruction.");
            }

            // Rule 1 and rule 5: the step belongs to the instruction, and every step left in the log does too.
            if (stepChanged || instructionChanged)
            {
                var instructionStepIds = await StepsOfInstruction(context, updated.WorkInstructionId)
                    .Select(s => s.Id)
                    .ToListAsync();

                if (!instructionStepIds.Contains(updated.WorkInstructionStepId))
                {
                    return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation,
                        "The selected step does not belong to the selected instruction.");
                }

                if (instructionChanged)
                {
                    var blockingSteps = logSteps
                        .Where(s => s.Id != oldStep.Id || s.Attempts.Count > 1)
                        .Where(s => !instructionStepIds.Contains(s.WorkInstructionStepId))
                        .ToList();

                    if (blockingSteps.Count > 0)
                    {
                        var blockingAttempts = blockingSteps.Sum(s => s.Id == oldStep.Id ? s.Attempts.Count - 1 : s.Attempts.Count);
                        return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.InstructionChangeBlocked,
                            $"The instruction can't be changed: Production Log #{log.Id} has {blockingAttempts} other attempt(s) on " +
                            $"{blockingSteps.Count} step(s) of the current instruction. The instruction can only be changed when this is the only attempt in its log.");
                    }
                }
            }

            // Rule 3: noun linked to the instruction (or the instruction curates no nouns, matching
            // DefectCodeService's full-catalog fallback), adjective linked to the noun.
            var adjectiveCleared = false;
            if (nounChanged && !adjectiveChanged
                && updated is { FailureNounId: int newNounId, FailureAdjectiveId: int keptAdjectiveId }
                && !await IsAdjectiveLinkedAsync(context, newNounId, keptAdjectiveId))
            {
                updated.FailureAdjectiveId = null;
                adjectiveChanged = updated.FailureAdjectiveId != current.FailureAdjectiveId;
                adjectiveCleared = true;
            }

            if (updated.FailureNounId is int nounId && (nounChanged || instructionChanged))
            {
                if (!await context.FailureNouns.AnyAsync(n => n.Id == nounId))
                    return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation, "The selected noun no longer exists.");

                var instructionCuratesNouns = await context.FailureNouns
                    .AnyAsync(n => n.WorkInstructions.Any(w => w.Id == updated.WorkInstructionId));

                if (instructionCuratesNouns && !await context.FailureNouns.AnyAsync(n =>
                        n.Id == nounId && n.WorkInstructions.Any(w => w.Id == updated.WorkInstructionId)))
                {
                    return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation,
                        "The selected noun is not linked to the selected instruction.");
                }
            }

            if (updated is { FailureNounId: int linkedNounId, FailureAdjectiveId: int adjectiveId }
                && (adjectiveChanged || nounChanged)
                && !await IsAdjectiveLinkedAsync(context, linkedNounId, adjectiveId))
            {
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation,
                    "The selected adjective is not linked to the selected noun.");
            }

            // Rule 4: operator exists. IsActive is not checked: nothing in MESS maintains it for users.
            if (operatorChanged && !await context.Users.AnyAsync(u => u.Id == updated.OperatorId))
            {
                return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Validation,
                    "The selected operator no longer exists.");
            }

            var changes = DescribeChanges(current, updated);

            attempt.Notes = updated.Notes;
            attempt.FailureNounId = updated.FailureNounId;
            attempt.FailureAdjectiveId = updated.FailureAdjectiveId;
            log.OperatorId = updated.OperatorId;
            log.ProductId = updated.ProductId;
            log.WorkInstructionId = updated.WorkInstructionId;

            if (stepChanged)
                MoveAttemptToStep(context, attempt, oldStep, log, logSteps, updated.WorkInstructionStepId);

            log.LastModifiedBy = modifiedBy;
            log.LastModifiedOn = DateTimeOffset.UtcNow;

            await context.SaveChangesAsync();

            Log.Information(
                "Rework edit saved for AttemptId={AttemptId}, ProductionLogId={ProductionLogId} by {ModifiedBy}: {Changes}",
                attempt.Id, log.Id, modifiedBy, changes);

            return ReworkUpdateResult.Ok(adjectiveCleared
                ? "The adjective was cleared because it is not linked to the new noun."
                : null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception thrown in ReworkService.UpdateAsync for AttemptId={AttemptId} by {ModifiedBy}",
                request.AttemptId, modifiedBy);
            return ReworkUpdateResult.Fail(ReworkUpdateErrorCode.Unexpected,
                "An unexpected error occurred while saving. The change was not saved.");
        }
    }

    /// <summary>
    /// Re-parents the attempt to the log's step for <paramref name="workInstructionStepId"/> (creating it if needed)
    /// without mutating the step shared by sibling attempts. Removes the old step if it is left empty,
    /// same convention as <c>ProductionLogService.RemoveEmptySteps</c>.
    /// </summary>
    private static void MoveAttemptToStep(
        ApplicationContext context,
        ProductionLogStepAttempt attempt,
        ProductionLogStep oldStep,
        ProductionLog log,
        List<ProductionLogStep> logSteps,
        int workInstructionStepId)
    {
        var target = logSteps.FirstOrDefault(s => s.WorkInstructionStepId == workInstructionStepId);
        if (target == null)
        {
            target = new ProductionLogStep { ProductionLogId = log.Id, WorkInstructionStepId = workInstructionStepId };
            context.ProductionLogSteps.Add(target);
        }

        oldStep.Attempts.Remove(attempt);
        target.Attempts.Add(attempt);
        attempt.ProductionLogStep = target;

        if (oldStep.Attempts.Count == 0)
        {
            log.LogSteps.Remove(oldStep);
            context.ProductionLogSteps.Remove(oldStep);
        }
    }

    private static ReworkEditValues ToEditValues(ProductionLogStepAttempt attempt, ProductionLogStep step, ProductionLog log)
        => new()
        {
            Success = attempt.Success,
            Notes = attempt.Notes,
            FailureNounId = attempt.FailureNounId,
            FailureAdjectiveId = attempt.FailureAdjectiveId,
            ProductId = log.ProductId,
            WorkInstructionId = log.WorkInstructionId,
            OperatorId = log.OperatorId,
            WorkInstructionStepId = step.WorkInstructionStepId
        };

    private static List<string> DescribeChanges(ReworkEditValues before, ReworkEditValues after)
    {
        var changes = new List<string>();
        if (before.Notes != after.Notes)
            changes.Add($"{nameof(before.Notes)}: changed");
        Add(nameof(before.FailureNounId), before.FailureNounId, after.FailureNounId);
        Add(nameof(before.FailureAdjectiveId), before.FailureAdjectiveId, after.FailureAdjectiveId);
        Add(nameof(before.ProductId), before.ProductId, after.ProductId);
        Add(nameof(before.WorkInstructionId), before.WorkInstructionId, after.WorkInstructionId);
        Add(nameof(before.OperatorId), before.OperatorId, after.OperatorId);
        Add(nameof(before.WorkInstructionStepId), before.WorkInstructionStepId, after.WorkInstructionStepId);
        return changes;

        void Add<T>(string field, T oldValue, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
                changes.Add($"{field}: {oldValue?.ToString() ?? "null"} -> {newValue?.ToString() ?? "null"}");
        }
    }

    private static async Task<bool> CanReworkAsync(ApplicationContext context, string userName)
    {
        var normalized = userName.ToUpperInvariant();
        return await (
            from user in context.Users
            where user.NormalizedUserName == normalized
            join userRole in context.UserRoles on user.Id equals userRole.UserId
            join role in context.Roles on userRole.RoleId equals role.Id
            where role.Name == "Technician" || role.Name == "Administrator"
            select role.Id).AnyAsync();
    }

    private static Task<bool> IsAdjectiveLinkedAsync(ApplicationContext context, int nounId, int adjectiveId)
        => context.FailureNouns.AnyAsync(n => n.Id == nounId && n.Adjectives.Any(a => a.Id == adjectiveId));

    private static IQueryable<Step> StepsOfInstruction(ApplicationContext context, int workInstructionId)
        => context.Steps.Where(s => EF.Property<int?>(s, "WorkInstructionId") == workInstructionId);

    private static IQueryable<ReworkRowDTO> BuildRowQuery(ApplicationContext context)
    {
        return context.ProductionLogStepAttempts
            .AsNoTracking()
            .Select(attempt => new ReworkRowDTO
            {
                Id = attempt.Id,
                ProductionLogId = attempt.ProductionLogStep!.ProductionLogId,
                Success = attempt.Success,
                Notes = attempt.Notes,
                SubmitTime = attempt.SubmitTime,
                NounName = attempt.FailureNoun != null ? attempt.FailureNoun.Name : string.Empty,
                AdjectiveName = attempt.FailureAdjective != null ? attempt.FailureAdjective.Name : string.Empty,
                ProductName = attempt.ProductionLogStep.ProductionLog!.Product != null
                              && attempt.ProductionLogStep.ProductionLog.Product.PartDefinition != null
                    ? attempt.ProductionLogStep.ProductionLog.Product.PartDefinition.Name
                    : string.Empty,
                InstructionName = attempt.ProductionLogStep.ProductionLog.WorkInstruction == null
                    ? string.Empty
                    : attempt.ProductionLogStep.ProductionLog.WorkInstruction.Version == null
                      || attempt.ProductionLogStep.ProductionLog.WorkInstruction.Version == ""
                        ? attempt.ProductionLogStep.ProductionLog.WorkInstruction.Title
                        : attempt.ProductionLogStep.ProductionLog.WorkInstruction.Title + " ("
                          + attempt.ProductionLogStep.ProductionLog.WorkInstruction.Version + ")",
                OperatorName = context.Users
                    .Where(user => user.Id == attempt.ProductionLogStep.ProductionLog.OperatorId)
                    .Select(user => ((user.FirstName ?? "") + " " + (user.LastName ?? "")).Trim() != ""
                        ? ((user.FirstName ?? "") + " " + (user.LastName ?? "")).Trim()
                        : user.UserName ?? "")
                    .FirstOrDefault() ?? string.Empty,
                StepName = attempt.ProductionLogStep.WorkInstructionStep != null
                    ? attempt.ProductionLogStep.WorkInstructionStep.Name
                    : string.Empty
            });
    }

    private static IQueryable<ReworkRowDTO> ApplyFilters(IQueryable<ReworkRowDTO> query, ReworkQuery filters)
    {
        if (filters.FilterId is int id)
            query = query.Where(row => row.Id == id);

        if (filters.FilterSuccessNotRecorded)
            query = query.Where(row => row.Success == null);
        else if (filters.FilterSuccess is bool success)
            query = query.Where(row => row.Success == success);

        if (Normalize(filters.FilterNotes) is { } notes)
            query = query.Where(row => row.Notes.ToLower().Contains(notes));

        if (Normalize(filters.FilterNoun) is { } noun)
            query = query.Where(row => row.NounName.ToLower().Contains(noun));

        if (Normalize(filters.FilterAdjective) is { } adjective)
            query = query.Where(row => row.AdjectiveName.ToLower().Contains(adjective));

        if (Normalize(filters.FilterProduct) is { } product)
            query = query.Where(row => row.ProductName.ToLower().Contains(product));

        if (Normalize(filters.FilterInstruction) is { } instruction)
            query = query.Where(row => row.InstructionName.ToLower().Contains(instruction));

        if (Normalize(filters.FilterOperator) is { } operatorName)
            query = query.Where(row => row.OperatorName.ToLower().Contains(operatorName));

        if (Normalize(filters.FilterStep) is { } step)
            query = query.Where(row => row.StepName.ToLower().Contains(step));

        if (filters.FilterSubmitFrom is DateTimeOffset from)
            query = query.Where(row => row.SubmitTime >= from);

        if (filters.FilterSubmitTo is DateTimeOffset to)
            query = query.Where(row => row.SubmitTime <= to);

        return query;

        static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLower();
    }

    private static IQueryable<ReworkRowDTO> ApplySort(IQueryable<ReworkRowDTO> query, string? sortBy, string? sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<ReworkRowDTO> ordered = (sortBy ?? "submitTime") switch
        {
            "id" => descending ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id),
            "success" => descending ? query.OrderByDescending(r => r.Success) : query.OrderBy(r => r.Success),
            "notes" => descending ? query.OrderByDescending(r => r.Notes) : query.OrderBy(r => r.Notes),
            "noun" => descending ? query.OrderByDescending(r => r.NounName) : query.OrderBy(r => r.NounName),
            "adjective" => descending ? query.OrderByDescending(r => r.AdjectiveName) : query.OrderBy(r => r.AdjectiveName),
            "product" => descending ? query.OrderByDescending(r => r.ProductName) : query.OrderBy(r => r.ProductName),
            "instruction" => descending ? query.OrderByDescending(r => r.InstructionName) : query.OrderBy(r => r.InstructionName),
            "operator" => descending ? query.OrderByDescending(r => r.OperatorName) : query.OrderBy(r => r.OperatorName),
            "step" => descending ? query.OrderByDescending(r => r.StepName) : query.OrderBy(r => r.StepName),
            _ => descending ? query.OrderByDescending(r => r.SubmitTime) : query.OrderBy(r => r.SubmitTime)
        };

        // Tie-break on the primary key so paging is stable when sort values repeat.
        return descending ? ordered.ThenByDescending(r => r.Id) : ordered.ThenBy(r => r.Id);
    }

    private static async Task<List<string>> DistinctValuesAsync(IQueryable<string> query)
    {
        return await query
            .Where(value => value != "")
            .Distinct()
            .OrderBy(value => value)
            .ToListAsync();
    }

    private static string FormatInstruction(string title, string? version)
        => string.IsNullOrEmpty(version) ? title : $"{title} ({version})";

    private static string FormatOperator(string? firstName, string? lastName, string? userName)
    {
        var fullName = $"{firstName} {lastName}".Trim();
        return fullName.Length > 0 ? fullName : userName ?? string.Empty;
    }
}
