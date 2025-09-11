namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Cache;

/// <summary>
/// Represents an individual attempt at completing a production log step.
/// </summary>
public class StepAttemptCacheDTO
{
    /// <summary>
    /// Gets or sets a value indicating whether the attempt was successful.
    /// </summary>
    public bool? Success { get; set; }

    /// <summary>
    /// Gets or sets the time the attempt was submitted.
    /// </summary>
    public DateTimeOffset SubmitTime { get; set; }

    /// <summary>
    /// Gets or sets the notes associated with this specific attempt.
    /// </summary>
    public string? FailureNote { get; set; }
}