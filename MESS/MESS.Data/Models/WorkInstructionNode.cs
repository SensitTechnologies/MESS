namespace MESS.Data.Models;

/// <summary>
/// Represents an abstract base class for a work instruction node.
/// Primarily used for UI purposes (i.e. Drag and Drop, etc.)
/// </summary>
public abstract class WorkInstructionNode
{
    /// <summary>
    /// Gets or sets the unique identifier of the work instruction node.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the position of the work instruction node.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the type of the work instruction node.
    /// </summary>
    public WorkInstructionNodeType NodeType { get; set; }
}

/// <summary>
/// Enum of the possible types of a WorkInstructionNode.
/// </summary>
public enum WorkInstructionNodeType
{
    /// <summary>
    /// Represents a part type of work instruction node.
    /// </summary>
    Part,

    /// <summary>
    /// Represents a step type of work instruction node.
    /// </summary>
    Step
}