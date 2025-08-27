using MESS.Data.Models;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.UpdateRequest;

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
    /// </para>
    /// </summary>
    /// <param name="entity">The <see cref="ProductionLogStep"/> to update.</param>
    /// <param name="dto">The <see cref="LogStepUpdateRequest"/> containing updated attempt data.</param>
    public static void ApplyUpdate(this ProductionLogStep entity, LogStepUpdateRequest dto)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        // Delegate the attempt updates to StepAttemptUpdateRequestMapper
        if (dto.Attempts?.Any() == true)
        {
            entity.Attempts.ApplyUpdateList(dto.Attempts);
        }
    }

    /// <summary>
    /// Converts a <see cref="LogStepUpdateRequest"/> into a new <see cref="ProductionLogStep"/> entity.
    /// </summary>
    /// <param name="dto">The update request DTO.</param>
    /// <returns>A new <see cref="ProductionLogStep"/> initialized with the attempts.</returns>
    public static ProductionLogStep ToEntity(this LogStepUpdateRequest dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var step = new ProductionLogStep();

        if (dto.Attempts?.Any() == true)
        {
            step.Attempts = dto.Attempts.Select(a => a.ToEntity()).ToList();
        }

        return step;
    }
}