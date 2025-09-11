namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Detail;

/// <summary>
/// Represents a single attempt for a production log step, optimized for
/// read/edit in the attempt detail dialog.
/// </summary>
public class StepAttemptDetailDTO
{
    /// <summary>
    /// Unique identifier of this attempt.
    /// </summary>
    public int AttemptId { get; set; }
    
    /// <summary>
    /// The ID of the ProductionLogStep associated with this attempt.
    /// </summary>
    /// <remarks>
    /// This is here for the purposes of saving.
    /// </remarks>
    public int LogStepId { get; set; }
    
    /// <summary>
    /// The ID of the work instruction step associated with this attempt.
    /// </summary>
    /// <remarks>
    /// This is here for the purposes of saving attempts after they have been updated.
    /// </remarks>
    public int WorkInstructionStepId { get; set; }

    /// <summary>
    /// Name of the step associated with this attempt.
    /// </summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when this attempt was submitted.
    /// </summary>
    public DateTimeOffset SubmitTime { get; set; }

    /// <summary>
    /// Whether this attempt was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Failure note for unsuccessful attempts. <c>null</c> or empty for successes.
    /// </summary>
    public string? FailureNote { get; set; }
}