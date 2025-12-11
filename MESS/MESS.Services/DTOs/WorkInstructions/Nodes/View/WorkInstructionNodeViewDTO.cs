using MESS.Data.Models;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.View;

/// <summary>
/// Represents a base data transfer object for work instruction nodes
/// used in read-only or operator-facing contexts.
/// </summary>
public abstract class WorkInstructionNodeViewDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the node in the database.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the position of the node within the work instruction.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the type of node (e.g., Step or Part).
    /// </summary>
    public WorkInstructionNodeType NodeType { get; set; }
}