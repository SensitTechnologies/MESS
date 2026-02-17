using MESS.Data.Models;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.File;

/// <summary>
/// Represents a file-safe base data transfer object for a work instruction node.
/// 
/// This DTO is persistence-agnostic and designed for import/export scenarios.
/// Concrete node types (such as Step or Part) should inherit from this class.
/// </summary>
public abstract class WorkInstructionNodeFileDTO
{
    /// <summary>
    /// Gets or sets the zero-based position of the node within the work instruction.
    /// This determines ordering when reconstructing the instruction.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the type of node represented by this DTO.
    /// Used during import to determine which concrete node type to instantiate.
    /// </summary>
    public WorkInstructionNodeType NodeType { get; set; }
}