using MESS.Data.Models;

namespace MESS.Data.DTO;

/// <summary>
/// Represents a step in the production log. Used primarily for client-side caching purposes.
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
    /// Gets or sets a value indicating whether the step was successful.
    /// </summary>
    public bool? Success { get; set; }

    /// <summary>
    /// Gets or sets the time the step was submitted.
    /// </summary>
    public DateTimeOffset SubmitTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show notes for the step.
    /// </summary>
    public bool ShowNotes { get; set; }

    /// <summary>
    /// Gets or sets the notes associated with the step.
    /// </summary>
    public string? Notes { get; set; }
}