namespace MESS.Data.Models;

/// <summary>
/// Represents a step in a work instruction, inheriting from <see cref="WorkInstructionNode"/>.
/// </summary>
public class Step : WorkInstructionNode
{
    /// <summary>
    /// Gets or sets the name of the step. This property is required.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the body content of the step. This property is optional.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the list of primary media associated with the step.
    /// </summary>
    public List<string> PrimaryMedia { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of secondary media associated with the step.
    /// </summary>
    public List<string> SecondaryMedia { get; set; } = [];
}