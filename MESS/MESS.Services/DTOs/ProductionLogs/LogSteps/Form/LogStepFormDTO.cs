using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Form;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Form;

/// <summary>
/// Represents a production log step as part of a form submission.
/// Used for creating or updating steps and their associated attempts in a production log.
/// </summary>
public class LogStepFormDTO
{
    /// <summary>
    /// Gets or sets the ID of the production log this step belongs to.
    /// This will be null if the production log has not yet been persisted.
    /// </summary>
    public int ProductionLogId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the associated work instruction step.
    /// </summary>
    public int WorkInstructionStepId { get; set; }

    /// <summary>
    /// Gets or sets the collection of attempts associated with this step.
    /// This includes both new attempts and any existing attempts being updated.
    /// </summary>
    public List<StepAttemptFormDTO> Attempts { get; set; } = new();
}