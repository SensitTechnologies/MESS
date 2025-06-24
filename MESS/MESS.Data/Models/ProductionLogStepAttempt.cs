namespace MESS.Data.Models;

/// <summary>
/// Represents an attempt for a step in the production log. One step can have multiple of these.
/// Similar to the <see cref="Step"/> but is used to persist the data within the database.
/// </summary>
public class ProductionLogStepAttempt
{
    /// <summary>
    /// Gets or sets the unique identifier for the production log step attempt.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the associated production log step.
    /// </summary>
    public int ProductionLogStepId { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the step attempt was successful.
    /// </summary>
    public bool? Success { get; set; } = null;

    /// <summary>
    /// Gets or sets any additional notes for the step attempt.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time the step attempt occured.
    /// </summary>
    public DateTimeOffset SubmitTime { get; set; }
    
    /// <summary>
    /// Gets or sets the associated production log step
    /// </summary>
    public ProductionLogStep? ProductionLogStep { get; set; }
    
}