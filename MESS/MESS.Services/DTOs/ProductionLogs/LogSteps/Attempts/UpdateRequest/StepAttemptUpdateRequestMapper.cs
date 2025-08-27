using MESS.Data.Models;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Detail;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Form;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.UpdateRequest;

/// <summary>
/// Provides mapping functionality for converting attempt-related DTOs
/// into <see cref="StepAttemptUpdateRequest"/> objects and applying them to entities.
/// <para>
/// This mapper is the single point where both updates to existing attempts
/// and creation of new attempts (triggered from the Production Log page UI)
/// are handled.
/// </para>
/// </summary>
public static class StepAttemptUpdateRequestMapper
{
    /// <summary>
    /// Converts a <see cref="StepAttemptFormDTO"/> to a <see cref="StepAttemptUpdateRequest"/>.
    /// <para>
    /// If <see cref="StepAttemptFormDTO.AttemptId"/> is <c>null</c>, the request
    /// represents a new attempt that was created in the Production Log UI
    /// when an operator submitted a result.
    /// </para>
    /// </summary>
    public static StepAttemptUpdateRequest ToUpdateRequest(this StepAttemptFormDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new StepAttemptUpdateRequest
        {
            Id = dto.AttemptId ?? 0, // 0 = new attempt
            IsSuccess = dto.IsSuccess,
            FailureNote = dto.FailureNote,
            SubmitTime = dto.SubmitTime
        };
    }

    /// <summary>
    /// Converts a <see cref="StepAttemptDetailDTO"/> to a <see cref="StepAttemptUpdateRequest"/>.
    /// Used primarily when detail data is edited and then re-submitted.
    /// </summary>
    public static StepAttemptUpdateRequest ToUpdateRequest(this StepAttemptDetailDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new StepAttemptUpdateRequest
        {
            Id = dto.AttemptId,
            IsSuccess = dto.IsSuccess,
            FailureNote = dto.FailureNote,
            SubmitTime = dto.SubmitTime
        };
    }

    /// <summary>
    /// Converts a <see cref="StepAttemptUpdateRequest"/> to a <see cref="ProductionLogStepAttempt"/> entity.
    /// Used when creating a new attempt (Id == 0).
    /// </summary>
    public static ProductionLogStepAttempt ToEntity(this StepAttemptUpdateRequest dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new ProductionLogStepAttempt
        {
            Id = dto.Id,
            Success = dto.IsSuccess,
            Notes = dto.FailureNote ?? string.Empty,
            SubmitTime = dto.SubmitTime
        };
    }

    /// <summary>
    /// Applies updates from a <see cref="StepAttemptUpdateRequest"/> to an existing <see cref="ProductionLogStepAttempt"/>.
    /// </summary>
    public static void ApplyUpdate(this ProductionLogStepAttempt entity, StepAttemptUpdateRequest dto)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        entity.Success = dto.IsSuccess;
        entity.Notes = dto.FailureNote ?? string.Empty;
        entity.SubmitTime = dto.SubmitTime;
    }

    /// <summary>
    /// Applies updates from a collection of <see cref="StepAttemptUpdateRequest"/> to a collection of entities.
    /// <para>
    /// - Existing attempts (Id > 0) are updated in place.  
    /// - New attempts (Id == 0) are created and added to the collection.  
    /// </para>
    /// <para>
    /// This is the exact point in the workflow where new attempts created
    /// on the Production Log page are turned into persistent entities.
    /// </para>
    /// </summary>
    public static void ApplyUpdateList(this ICollection<ProductionLogStepAttempt> entities, IEnumerable<StepAttemptUpdateRequest> dtos)
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(dtos);

        foreach (var dto in dtos)
        {
            if (dto.Id > 0)
            {
                var existing = entities.FirstOrDefault(e => e.Id == dto.Id);
                if (existing != null)
                {
                    existing.ApplyUpdate(dto);
                }
            }
            else
            {
                entities.Add(dto.ToEntity());
            }
        }
    }
}