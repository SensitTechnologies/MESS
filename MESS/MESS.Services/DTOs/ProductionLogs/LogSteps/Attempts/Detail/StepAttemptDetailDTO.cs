namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Detail;

/// <summary>
/// Represents a single attempt for a production log step, including associated step metadata.
/// Used for displaying detailed information about an attempt in the UI.
/// </summary>
/// <remarks>
/// Currently used in tables that edit and display attempts.
/// </remarks>
public class StepAttemptDetailDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of this attempt.
    /// </summary>
    public int AttemptId { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this attempt was made, including the time zone offset.
    /// </summary>
    public DateTimeOffset AttemptedOn { get; set; }
    
    /// <summary>
    /// Gets or sets the user who performed this attempt.
    /// </summary>
    public string AttemptedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether this attempt was successful.
    /// <c>true</c> if the step passed, <c>false</c> if it failed.
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Gets or sets the failure note for this attempt, if it was unsuccessful.
    /// Will be <c>null</c> if no note was recorded or the attempt was successful.
    /// </summary>
    public string? FailureNote { get; set; }

    // Step metadata
    /// <summary>
    /// Gets or sets the unique identifier of the step associated with this attempt.
    /// </summary>
    public int StepId { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the step associated with this attempt.
    /// </summary>
    public string StepName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the identifier of the corresponding work instruction step.
    /// Useful for referencing the step in the context of the work instruction.
    /// </summary>
    public int WorkInstructionStepId { get; set; }
}