using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Nodes.File;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.File;

/// <summary>
/// Represents a file-safe data transfer object for a step node within a work instruction.
/// 
/// This DTO contains all information necessary to reconstruct a <see cref="Step"/>
/// during import, without including any database-specific identifiers.
/// </summary>
public class StepNodeFileDTO : WorkInstructionNodeFileDTO
{
    /// <summary>
    /// Gets or sets the name of the step.
    /// This value is required and should match the display name of the step.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary body content of the step.
    /// This typically contains the main instruction text and may include formatted or HTML content.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional detailed body content of the step.
    /// This may contain extended instructions, notes, or supplemental information.
    /// </summary>
    public string? DetailedBody { get; set; }

    /// <summary>
    /// Gets or sets the collection of primary media references associated with the step.
    /// These may represent file paths, URLs, or identifiers depending on the export format.
    /// </summary>
    public List<string> PrimaryMedia { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of secondary media references associated with the step.
    /// These may represent additional visual aids or supporting content.
    /// </summary>
    public List<string> SecondaryMedia { get; set; } = new();
}