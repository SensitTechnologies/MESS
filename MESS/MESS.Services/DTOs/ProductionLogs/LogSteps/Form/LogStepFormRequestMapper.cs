using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.UpdateRequest;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Form;
using MESS.Services.DTOs.ProductionLogs.LogSteps.UpdateRequest;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Form;

/// <summary>
/// Provides mapping functionality for converting 
/// <see cref="LogStepFormDTO"/> objects into request DTOs.
/// </summary>
public static class LogStepFormRequestMapper
{
    /// <summary>
    /// Converts a <see cref="LogStepFormDTO"/> into a <see cref="LogStepUpdateRequest"/>.
    /// </summary>
    public static LogStepUpdateRequest ToUpdateRequest(this LogStepFormDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new LogStepUpdateRequest
        {
            Id = dto.ProductionLogStepId ?? 0, // map to the PK of the log step, 0 = new
            WorkInstructionStepId = dto.WorkInstructionStepId,
            Attempts = dto.Attempts
                .Select(a => a.ToUpdateRequest())
                .ToList()
        };
    }
}