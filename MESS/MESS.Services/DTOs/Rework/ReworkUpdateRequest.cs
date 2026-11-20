namespace MESS.Services.DTOs.Rework;

/// <summary>
/// A correction to one production log step attempt.
/// </summary>
public sealed class ReworkUpdateRequest
{
    /// <summary>Attempt being edited.</summary>
    public int AttemptId { get; set; }

    /// <summary>Values as loaded into the dialog; compared against the database to detect stale edits.</summary>
    public ReworkEditValues Original { get; set; } = new();

    /// <summary><c>LastModifiedOn</c> of the owning log as loaded into the dialog.</summary>
    public DateTimeOffset OriginalLogLastModifiedOn { get; set; }

    /// <summary>Values to save.</summary>
    public ReworkEditValues Updated { get; set; } = new();

    /// <summary>
    /// The user acknowledged that a product/instruction/operator change affects other attempts in the same log.
    /// </summary>
    public bool ConfirmSharedLogChange { get; set; }
}
