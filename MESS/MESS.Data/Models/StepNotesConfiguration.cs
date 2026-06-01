namespace MESS.Data.Models;

/// <summary>
/// Controls when step notes are shown in production and whether a note is required.
/// </summary>
public enum StepNotesConfiguration
{
    /// <summary>Notes appear only after Failure; not required (legacy behavior).</summary>
    OptionalForFailure = 0,

    /// <summary>Notes are always visible; a note is required for Success and Failure.</summary>
    Required = 1,

    /// <summary>Notes are always visible; optional for both outcomes.</summary>
    Optional = 2
}
