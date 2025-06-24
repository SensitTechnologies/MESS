using FluentValidation;

namespace MESS.Data.Models;

/// <summary>
/// Represents a step in the production log.
/// Similar to the <see cref="Step"/> but is used to persist the data within the database.
/// </summary>
public class ProductionLogStep
{
    /// <summary>
    /// Gets or sets the unique identifier for the production log step.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated production log.
    /// </summary>
    public int ProductionLogId { get; set; }
    
    /// <summary>
    /// Gets or sets the associated ProductionLog
    /// </summary>
    public ProductionLog? ProductionLog { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated work instruction step.
    /// </summary>
    public int WorkInstructionStepId { get; set; }
    
    /// <summary>
    /// Gets or sets the associated work instruction step details.
    /// </summary>
    public Step? WorkInstructionStep { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of attempts for this step
    /// </summary>
    public List<ProductionLogStepAttempt> Attempts { get; set; } = new();
    
    /// <summary>
    /// Helper property to get the most recent attempt
    /// </summary>
    public ProductionLogStepAttempt LatestAttempt => 
        Attempts.OrderByDescending(a => a.SubmitTime).FirstOrDefault() 
        ?? new ProductionLogStepAttempt { Success = null };

}