namespace MESS.Data.DTO.ProductionLogDTOs.LogSteps.Attempts;

/// <summary>
/// Represents a request to update an existing step attempt in a production log.
/// Intended for server-side processing when modifying persisted attempts.
/// </summary>
/// <remarks>This DTO will primarily be used after the production log editor updates existing attempts</remarks>
public class StepAttemptUpdateRequest
{
    /// <summary>
    /// Gets or sets the unique database identifier of the attempt being updated.
    /// This value must match an existing persisted attempt.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the outcome of this attempt.
    /// <c>true</c> indicates the step passed, <c>false</c> indicates it failed,
    /// and <c>null</c> leaves the outcome unchanged.
    /// </summary>
    public bool? IsSuccess { get; set; }
    
    /// <summary>
    /// Gets or sets any notes associated with the attempt.
    /// Typically used to record failure reasons or supplemental information.
    /// Will overwrite the existing value when provided.
    /// </summary>
    public string? FailureNote { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the attempt was submitted,
    /// including the time zone offset.
    /// </summary>
    public DateTimeOffset SubmitTime { get; set; }
}