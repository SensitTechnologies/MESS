using MESS.Data.Models;
using MESS.Services.DTOs.PartDefinitions;
using MESS.Services.DTOs.WorkInstructions.Nodes.Form;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.Form;

/// <summary>
/// Represents a form DTO for editing or creating a <see cref="PartNode"/> in the Blazor UI.
/// </summary>
/// <remarks>
/// A part node links a specific <see cref="PartDefinition"/> to a step or action
/// in a work instruction, allowing operators to perform actions tied to physical parts.
/// </remarks>
public class PartNodeFormDTO : WorkInstructionNodeFormDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the part definition associated
    /// with this node.
    /// </summary>
    public int PartDefinitionId { get; set; }

    /// <summary>
    /// Gets or sets a minimal representation of the associated part definition.
    /// </summary>
    /// <remarks>
    /// This property is optional and may be null when only the
    /// <see cref="PartDefinitionId"/> is needed for binding or updates.
    /// </remarks>
    public PartDefinitionDTO? PartDefinition { get; set; }
}