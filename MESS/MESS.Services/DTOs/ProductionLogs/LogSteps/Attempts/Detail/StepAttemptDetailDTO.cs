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
    /// Name of the step associated with this attempt.
    /// </summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when this attempt was submitted (local time).
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