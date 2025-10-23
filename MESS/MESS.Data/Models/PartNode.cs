namespace MESS.Data.Models;

/// <summary>
/// Represents an "Action" or node in a work instruction that is associated with a single part.
/// </summary>
public class PartNode : WorkInstructionNode
{
    /// <summary>
    /// Gets or sets the foreign key of the part associated with this node.
    /// </summary>
    public int PartDefinitionId { get; set; }

    /// <summary>
    /// Gets or sets the part associated with this node.
    /// </summary>
    public required PartDefinition PartDefinition { get; set; }
}