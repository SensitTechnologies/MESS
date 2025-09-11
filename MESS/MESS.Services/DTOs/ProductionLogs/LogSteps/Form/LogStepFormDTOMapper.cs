using MESS.Data.Models;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Form;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Form;

/// <summary>
/// Provides mapping functionality between <see cref="ProductionLogStep"/> entities
/// and <see cref="LogStepFormDTO"/> objects for use in client-side form state
/// and server-side persistence.
/// </summary>
public static class LogStepFormDTOMapper
{
    /// <summary>
    /// Maps a <see cref="ProductionLogStep"/> entity to a <see cref="LogStepFormDTO"/>.
    /// </summary>
    /// <param name="step">The production log step entity to map.</param>
    /// <returns>A populated <see cref="LogStepFormDTO"/>.</returns>
    public static LogStepFormDTO ToFormDTO(this ProductionLogStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        return new LogStepFormDTO
        {
            ProductionLogStepId = step.Id, // PK of the log step
            WorkInstructionStepId = step.WorkInstructionStepId, // FK to template step
            Attempts = step.Attempts.ToFormDTOList()
        };
    }

    /// <summary>
    /// Maps a <see cref="LogStepFormDTO"/> to a new <see cref="ProductionLogStep"/> entity.
    /// </summary>
    /// <param name="dto">The form DTO containing step data.</param>
    /// <returns>A new <see cref="ProductionLogStep"/> entity.</returns>
    public static ProductionLogStep ToEntity(this LogStepFormDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new ProductionLogStep
        {
            Id = dto.ProductionLogStepId ?? 0, // 0 = new step (not yet persisted)
            WorkInstructionStepId = dto.WorkInstructionStepId,
            Attempts = dto.Attempts.ToEntityList()
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="ProductionLogStep"/> entities
    /// into a list of <see cref="LogStepFormDTO"/> objects.
    /// </summary>
    public static List<LogStepFormDTO> ToFormDTOList(this IEnumerable<ProductionLogStep> steps)
        => steps.Select(s => s.ToFormDTO()).ToList();

    /// <summary>
    /// Maps a collection of <see cref="LogStepFormDTO"/>s
    /// into a list of <see cref="ProductionLogStep"/> entities.
    /// </summary>
    public static List<ProductionLogStep> ToEntityList(this IEnumerable<LogStepFormDTO> dtos)
        => dtos.Select(dto => dto.ToEntity()).ToList();
}