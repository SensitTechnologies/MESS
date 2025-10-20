namespace MESS.Data.Models;

/// <summary>
/// Represents an "Action" or a node in a work instruction that contains a list of parts.
/// </summary>
public class PartNode : WorkInstructionNode
{
    /// <summary>
    /// Gets or sets the list of parts associated with this node.
    /// </summary>
    public List<PartDefinition> Parts { get; set; } = [];
}