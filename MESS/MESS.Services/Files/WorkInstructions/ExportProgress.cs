namespace MESS.Services.Files.WorkInstructions;

/// <summary>
/// Represents the current progress state of a work instruction export operation.
/// </summary>
/// <remarks>
/// Instances of this class are typically reported through an <see cref="IProgress{T}"/>
/// implementation during long-running export operations to provide feedback to the
/// caller (such as a UI progress bar in a Blazor component).
/// </remarks>
public class ExportProgress
{
    /// <summary>
    /// Gets or sets the percentage of completion for the export operation.
    /// </summary>
    /// <value>
    /// An integer between <c>0</c> and <c>100</c> representing the approximate
    /// completion percentage of the export process.
    /// </value>
    public int Percent { get; set; }

    /// <summary>
    /// Gets or sets a human-readable message describing the current stage
    /// of the export process.
    /// </summary>
    /// <remarks>
    /// This message can be displayed to users to indicate what the exporter
    /// is currently doing (for example: "Processing steps", "Embedding images",
    /// or "Saving Excel file").
    /// </remarks>
    public string Message { get; set; } = "";
}