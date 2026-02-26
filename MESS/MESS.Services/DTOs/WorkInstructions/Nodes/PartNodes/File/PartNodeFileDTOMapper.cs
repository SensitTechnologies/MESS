using MESS.Data.Models;
using MESS.Services.DTOs.PartDefinitions;
using MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.Form;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.File;

/// <summary>
/// Provides mapping extensions between <see cref="PartNode"/> and <see cref="PartNodeFileDTO"/>.
/// </summary>
public static class PartNodeFileDTOMapper
{
    /// <summary>
    /// Converts a <see cref="PartNode"/> entity to a <see cref="PartNodeFileDTO"/>.
    /// </summary>
    public static PartNodeFileDTO ToFileDTO(this PartNode entity)
    {
        return new PartNodeFileDTO
        {
            Position = entity.Position,
            PartName = entity.PartDefinition.Name,
            PartNumber = entity.PartDefinition.Number,
            InputType = entity.InputType
        };
    }

    /// <summary>
    /// Converts a <see cref="PartNodeFileDTO"/> to a <see cref="PartNode"/> entity.
    /// 
    /// Requires a resolved <see cref="PartDefinition"/> instance.
    /// </summary>
    public static PartNode ToEntity(
        this PartNodeFileDTO dto,
        PartDefinition resolvedPart)
    {
        return new PartNode
        {
            Position = dto.Position,
            NodeType = WorkInstructionNodeType.Part,
            PartDefinition = resolvedPart,
            PartDefinitionId = resolvedPart.Id,
            InputType = dto.InputType
        };
    }
    
    /// <summary>
    /// Converts a <see cref="PartNodeFileDTO"/> into a <see cref="PartNodeFormDTO"/> suitable for use in the UI.
    /// </summary>
    /// <param name="dto">The file DTO representing a part node, typically imported from a work instruction file.</param>
    /// <param name="resolvedPart">The resolved <see cref="PartDefinition"/> entity that corresponds to the part referenced in the DTO.</param>
    /// <returns>
    /// A new <see cref="PartNodeFormDTO"/> containing all properties from the file DTO,
    /// including position, node type, part definition, and input type, ready for UI consumption.
    /// </returns>
    /// <remarks>
    /// This method first converts the file DTO to a <see cref="PartNode"/> entity using the provided <paramref name="resolvedPart"/>.
    /// The <see cref="PartDefinition"/> is also mapped to its DTO representation using the ToDTO extension method.
    /// </remarks>
    public static PartNodeFormDTO ToFormDTO(this PartNodeFileDTO dto, PartDefinition resolvedPart)
    {
        var entity = dto.ToEntity(resolvedPart);

        return new PartNodeFormDTO
        {
            Position = entity.Position,
            NodeType = entity.NodeType,
            PartDefinitionId = entity.PartDefinitionId,
            PartDefinition = entity.PartDefinition.ToDTO(),
            InputType = entity.InputType
        };
    }
}