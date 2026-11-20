namespace MESS.Services.DTOs.Rework;

/// <summary>
/// One attempt plus its parent context, loaded into the Rework edit dialog.
/// </summary>
public sealed class ReworkEditDTO
{
    /// <summary>Attempt id.</summary>
    public int AttemptId { get; set; }

    /// <summary>Owning production log id.</summary>
    public int ProductionLogId { get; set; }

    /// <summary>Attempt submit time (read-only in the dialog).</summary>
    public DateTimeOffset SubmitTime { get; set; }

    /// <summary>Attempt notes (read-only in the dialog).</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Current values of the editable fields.</summary>
    public ReworkEditValues Values { get; set; } = new();

    /// <summary><c>LastModifiedOn</c> of the owning log when loaded; part of the concurrency snapshot.</summary>
    public DateTimeOffset LogLastModifiedOn { get; set; }

    /// <summary>Current noun name, so it can be shown even when no longer offered.</summary>
    public string? FailureNounName { get; set; }

    /// <summary>Current adjective name, so it can be shown even when no longer offered.</summary>
    public string? FailureAdjectiveName { get; set; }

    /// <summary>Current product name.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Current work instruction title (with version).</summary>
    public string InstructionName { get; set; } = string.Empty;

    /// <summary>Current operator display name.</summary>
    public string OperatorName { get; set; } = string.Empty;

    /// <summary>Current step name.</summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>Number of other attempts in the same log (any step).</summary>
    public int OtherAttemptCount { get; set; }

    /// <summary>Number of distinct steps in the same log that carry those other attempts.</summary>
    public int OtherStepCount { get; set; }

    /// <summary>True when the log has produced/installed part traceability rows.</summary>
    public bool HasPartTraceability { get; set; }
}
