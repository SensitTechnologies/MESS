using MESS.Data.Models;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.File;

/// <summary>
/// Provides mapping extensions between <see cref="PartNode"/> and <see cref="PartNodeFileDTO"/>.
/// </summary>
public static class PartNodeFileMapper
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
}