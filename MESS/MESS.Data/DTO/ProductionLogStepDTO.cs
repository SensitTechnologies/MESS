using MESS.Data.Models;

namespace MESS.Data.DTO;

/// <summary>
/// Represents a step in the production log. Used primarily for client-side caching purposes.
/// Now supports multiple attempts per step.
/// </summary>
public class ProductionLogStepDTO
{
    /// <summary>
    /// Gets or sets the ID of the work instruction step.
    /// </summary>
    public int WorkInstructionStepId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the production log.
    /// </summary>
    public int ProductionLogId { get; set; }

    /// <summary>
    /// Gets or sets the collection of attempts for this step.
    /// </summary>
    public List<ProductionLogStepAttemptDTO> Attempts { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to show notes for the step.
    /// </summary>
    public bool ShowNotes { get; set; }
}