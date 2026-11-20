namespace MESS.Services.DTOs.Rework;

/// <summary>
/// Reason a Rework update was rejected.
/// </summary>
public enum ReworkUpdateErrorCode
{
    /// <summary>The request is malformed or breaks a consistency rule.</summary>
    Validation,

    /// <summary>The attempt no longer exists.</summary>
    NotFound,

    /// <summary>The attempt or its log changed since the dialog was opened.</summary>
    Conflict,

    /// <summary>The instruction cannot change because other steps of the log belong to the old instruction.</summary>
    InstructionChangeBlocked,

    /// <summary>The product/instruction cannot change because the log has part traceability rows.</summary>
    PartTraceabilityBlocked,

    /// <summary>The caller is not a Technician or Administrator.</summary>
    Forbidden,

    /// <summary>An unexpected error occurred; details are in the server log.</summary>
    Unexpected
}

/// <summary>
/// Outcome of <c>IReworkService.UpdateAsync</c>.
/// </summary>
public sealed class ReworkUpdateResult
{
    /// <summary>True when the change was saved.</summary>
    public bool Success { get; init; }

    /// <summary>Why the change was rejected, when <see cref="Success"/> is false.</summary>
    public ReworkUpdateErrorCode? ErrorCode { get; init; }

    /// <summary>User-facing message.</summary>
    public string? Message { get; init; }

    /// <summary>Creates a successful result, optionally with a note for the user.</summary>
    public static ReworkUpdateResult Ok(string? message = null) => new() { Success = true, Message = message };

    /// <summary>Creates a failed result.</summary>
    public static ReworkUpdateResult Fail(ReworkUpdateErrorCode code, string message)
        => new() { Success = false, ErrorCode = code, Message = message };
}
