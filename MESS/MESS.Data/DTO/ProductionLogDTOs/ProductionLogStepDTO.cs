namespace MESS.Data.DTO.ProductionLogDTOs;

/// <summary>
/// Represents a step in the production log. Used primarily for client-side caching purposes.
/// Now supports multiple attempts per step.
/// </summary>
public class ProductionLogStepDTO
{
    /// <summary>
    /// Gets or sets the ID of the work instruction step.
    /// </summary>
    public int WorkInstructionStepId { get; init; }

    /// <summary>
    /// Gets or sets the ID of the production log.
    /// </summary>
    public int ProductionLogId { get; init; }

    /// <summary>
    /// Gets or sets the collection of attempts for this step.
    /// </summary>
    public List<StepAttemptCacheDTO> Attempts { get; set; } = new();
}