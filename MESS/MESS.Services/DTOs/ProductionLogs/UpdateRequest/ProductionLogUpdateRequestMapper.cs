using MESS.Data.Models;
using MESS.Services.DTOs.ProductionLogs.LogSteps.UpdateRequest;

namespace MESS.Services.DTOs.ProductionLogs.UpdateRequest;

/// <summary>
/// Provides extension methods for applying updates from a 
/// <see cref="ProductionLogUpdateRequest"/> DTO to a <see cref="ProductionLog"/> entity.
/// </summary>
public static class ProductionLogUpdateRequestMapper
{
    /// <summary>
    /// Applies updates from the provided <see cref="ProductionLogUpdateRequest"/> to an existing <see cref="ProductionLog"/>.
    /// Updates scalar fields, log steps, and their associated attempts.
    /// </summary>
    /// <param name="entity">The existing <see cref="ProductionLog"/> entity.</param>
    /// <param name="dto">The DTO containing the updated values.</param>
    /// <param name="modifiedBy">The identifier of the user performing the update.</param>
    public static void ApplyUpdateRequest(this ProductionLog entity, ProductionLogUpdateRequest dto, string modifiedBy)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        // Update scalar properties
        entity.FromBatchOf = dto.FromBatchOf;

        // Update audit info
        entity.LastModifiedBy = modifiedBy;
        entity.LastModifiedOn = DateTimeOffset.UtcNow;

        // Track which steps should remain
        var stepIdsToKeep = dto.LogSteps
            .Where(step => step.Id != 0) // Only consider persisted steps
            .Select(step => step.Id)
            .ToHashSet();

        // Remove steps not in DTO
        var stepsToRemove = entity.LogSteps
            .Where(step => step.Id != 0 && !stepIdsToKeep.Contains(step.Id))
            .ToList();

        foreach (var stepToRemove in stepsToRemove)
        {
            entity.LogSteps.Remove(stepToRemove);
        }

        // Apply updates to existing steps or add new ones
        foreach (var stepDto in dto.LogSteps)
        {
            var existingStep = entity.LogSteps.FirstOrDefault(s => s.Id == stepDto.Id);

            if (existingStep != null)
            {
                // Delegate update logic to step mapper
                existingStep.ApplyUpdate(stepDto);
            }
            else if (stepDto.Id == 0)
            {
                // Create a new step using the ToEntity helper in LogStepUpdateRequestMapper
                var newStep = stepDto.ToEntity();
                entity.LogSteps.Add(newStep);
            }
            // else: skip silently if invalid (should not happen in normal flow)
        }
    }
}
