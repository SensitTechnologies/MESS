namespace MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes;

/// <summary>
/// Represents a read-only step node within a work instruction.
/// </summary>
public class StepNodeDTO : WorkInstructionNodeDTO
{
    /// <summary>
    /// Gets or sets the name of the step.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the body of the step.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional detailed body content.
    /// </summary>
    public string? DetailedBody { get; set; }

    /// <summary>
    /// Gets or sets the list of primary media URLs associated with the step.
    /// </summary>
    public List<string> PrimaryMedia { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of secondary media URLs associated with the step.
    /// </summary>
    public List<string> SecondaryMedia { get; set; } = [];
}