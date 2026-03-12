using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Nodes.File;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.File;

/// <summary>
/// Represents a file-safe data transfer object for a part node within a work instruction.
/// 
/// This DTO contains sufficient business identity information to resolve
/// a corresponding <see cref="PartDefinition"/> during import.
/// </summary>
public class PartNodeFileDTO : WorkInstructionNodeFileDTO
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartNodeFileDTO"/> class.
    /// Sets the node type to <see cref="WorkInstructionNodeType.Part"/>.
    /// </summary>
    public PartNodeFileDTO()
    {
        NodeType = WorkInstructionNodeType.Part;
    }

    /// <summary>
    /// Gets or sets the name of the associated part.
    /// This value is required and serves as the primary business identifier.
    /// </summary>
    public string PartName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional part number associated with the part.
    /// This value strengthens uniqueness during import resolution.
    /// </summary>
    public string? PartNumber { get; set; }

    /// <summary>
    /// Gets or sets the type of input expected for this part node.
    /// </summary>
    public PartInputType InputType { get; set; }
    
    /// <summary>
    /// Gets a combined display string for the part, including both name and number if available.
    /// </summary>
    public string PartNameWithNumber => string.IsNullOrWhiteSpace(PartNumber) ? PartName : $"({PartName}, {PartNumber})";
}
