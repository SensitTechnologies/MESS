using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Form;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Form;

/// <summary>
/// Represents a production log step as part of a form submission.
/// Used for creating or updating steps and their associated attempts in a production log.
/// </summary>
public class LogStepFormDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the production log step (database PK).
    /// Will be <c>null</c> if this step has not yet been persisted.
    /// Required for updates.
    /// </summary>
    public int? ProductionLogStepId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the associated work instruction step (foreign key).
    /// Immutable after creation — used to tie the log step to its template definition.
    /// </summary>
    public int WorkInstructionStepId { get; set; }

    /// <summary>
    /// Gets or sets the collection of attempts associated with this step.
    /// This includes both new attempts and any existing attempts being updated.
    /// </summary>
    public List<StepAttemptFormDTO> Attempts { get; set; } = new();
}