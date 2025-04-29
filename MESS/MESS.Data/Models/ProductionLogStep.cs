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
    /// Gets or sets the identifier of the associated work instruction step.
    /// </summary>
    public int WorkInstructionStepId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the step was successful.
    /// </summary>
    public bool? Success { get; set; } = null;

    /// <summary>
    /// Gets or sets any additional notes for the step.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time the step was submitted.
    /// </summary>
    public DateTimeOffset SubmitTime { get; set; }

    /// <summary>
    /// Gets or sets the associated work instruction step details.
    /// </summary>
    public Step? WorkInstructionStep { get; set; }
}