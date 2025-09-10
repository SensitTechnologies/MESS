using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.UpdateRequest;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Detail;

/// <summary>
/// Provides mapping functionality for converting 
/// <see cref="StepAttemptDetailDTO"/> objects into 
/// <see cref="StepAttemptUpdateRequest"/> objects.
/// <para>
/// This mapper is used when production log detail data 
/// (such as step attempt outcomes and notes) has been 
/// viewed or edited in the UI and needs to be resubmitted 
/// to the backend as an update request.
/// </para>
/// </summary>
public static class StepAttemptDetailRequestMapper
{
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
}