using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Cache;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Cache;

/// <summary>
/// Represents a step in the production log. Used primarily for client-side caching purposes.
/// Now supports multiple attempts per step.
/// </summary>
public class LogStepCacheDTO
{
    /// <summary>
    /// Gets or sets the ID of the work instruction step.
    /// </summary>
    public int WorkInstructionStepId { get; init; }

    /// <summary>
    /// Gets or sets the ID of the production log step.
    /// </summary>
    public int ProductionLogStepId { get; init; }

    /// <summary>
    /// Gets or sets the collection of attempts for this step.
    /// </summary>
    public List<StepAttemptCacheDTO> Attempts { get; init; } = [];
}