using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.UpdateRequest;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Form;

/// <summary>
/// Provides mapping functionality for converting 
/// <see cref="StepAttemptFormDTO"/> objects into 
/// <see cref="StepAttemptUpdateRequest"/> objects.
/// <para>
/// This mapper is used when new step attempts are created 
/// through the Production Log form UI. If the 
/// <see cref="StepAttemptFormDTO.AttemptId"/> is <c>null</c>, 
/// the mapper assigns an ID of <c>0</c> to indicate that the 
/// attempt should be treated as new and persisted on update.
/// </para>
/// </summary>
public static class StepAttemptFormRequestMapper
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
}