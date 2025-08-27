using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.UpdateRequest;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.UpdateRequest;

/// <summary>
/// Represents a request to update a production log step.
/// </summary>
/// <remarks>
/// Intended for server-side processing when modifying existing steps within a production log.
/// </remarks>
public class LogStepUpdateRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the production log step in the database.
    /// Used to locate the step for updating.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the list of step attempts associated with this step.
    /// These attempts may be updated or added to the existing attempts.
    /// </summary>
    public List<StepAttemptUpdateRequest> Attempts { get; set; } = [];
}
