namespace MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes;

/// <summary>
/// Represents a read-only node that references a part within a work instruction.
/// </summary>
public class PartNodeDTO : WorkInstructionNodeDTO
{
    /// <summary>
    /// Gets or sets the identifier of the associated part definition.
    /// </summary>
    public int PartDefinitionId { get; set; }

    /// <summary>
    /// Gets or sets the name of the associated part.
    /// </summary>
    public string PartName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of the associated part.
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;
}