using MESS.Data.Models;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.UpdateRequest;

/// <summary>
/// Mapper for applying StepAttempt Update Requests to entities.
/// <para>
/// This mapper is the single point where both updates to existing attempts
/// and creation of new attempts (triggered from the Production Log page UI)
/// are handled.
/// </para>
/// </summary>
public static class StepAttemptUpdateRequestMapper
{
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