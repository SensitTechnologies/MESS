using MESS.Data.Models;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.UpdateRequest;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Form;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.UpdateRequest;

/// <summary>
/// Provides extension methods for applying a <see cref="LogStepUpdateRequest"/>
/// to a <see cref="ProductionLogStep"/> entity.
/// <para>
/// This mapper updates existing log steps and their child attempts.  
/// New attempts (with Id == 0) created in the Production Log UI are added here.  
/// Creation of entirely new steps is not performed in this mapper; that happens 
/// during log creation (via <see cref="LogStepFormDTO"/> mapping).
/// </para>
/// </summary>
public static class LogStepUpdateRequestMapper
{
    /// <summary>
    /// Applies updates from a <see cref="LogStepUpdateRequest"/> to an existing <see cref="ProductionLogStep"/>.
    /// <para>
    /// - Existing attempts are updated in place.  
    /// - New attempts (Id == 0) coming from the Production Log page are created and attached.  
    /// - The <see cref="ProductionLogStep.WorkInstructionStepId"/> is updated if it differs.  
    /// </para>
    /// </summary>
    /// <param name="entity">The <see cref="ProductionLogStep"/> to update.</param>
    /// <param name="dto">The <see cref="LogStepUpdateRequest"/> containing updated step + attempt data.</param>
    public static void ApplyUpdate(this ProductionLogStep entity, LogStepUpdateRequest dto)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        // Update FK relationship if needed
        entity.WorkInstructionStepId = dto.WorkInstructionStepId;

        // Delegate the attempt updates
        if (dto.Attempts?.Any() == true)
        {
            entity.Attempts.ApplyUpdateList(dto.Attempts);
        }
    }

    /// <summary>
    /// Converts a <see cref="LogStepUpdateRequest"/> into a new <see cref="ProductionLogStep"/> entity.
    /// </summary>
    /// <param name="dto">The update request DTO.</param>
    /// <returns>A new <see cref="ProductionLogStep"/> initialized with the provided data.</returns>
    public static ProductionLogStep ToEntity(this LogStepUpdateRequest dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var step = new ProductionLogStep
        {
            Id = dto.Id,
            WorkInstructionStepId = dto.WorkInstructionStepId,
            Attempts = dto.Attempts?.Select(a => a.ToEntity()).ToList() ?? new List<ProductionLogStepAttempt>()
        };

        return step;
    }
}