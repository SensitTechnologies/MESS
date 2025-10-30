using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.View;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.View;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.View;

/// <summary>
/// Provides extension methods for mapping between <see cref="WorkInstructionNode"/> entities
/// and <see cref="WorkInstructionNodeViewDTO"/> objects.
/// </summary>
public static class WorkInstructionNodeViewDTOMapper
{
    /// <summary>
    /// Maps a <see cref="WorkInstructionNode"/> to its corresponding <see cref="WorkInstructionNodeViewDTO"/>.
    /// </summary>
    /// <param name="entity">The node entity.</param>
    /// <returns>The mapped DTO.</returns>
    public static WorkInstructionNodeViewDTO ToDTO(this WorkInstructionNode entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        return entity switch
        {
            PartNode partNode => partNode.ToDTO(),
            Step stepNode => stepNode.ToDTO(),
            _ => throw new NotSupportedException($"Unknown node type: {entity.GetType().Name}")
        };
    }

    /// <summary>
    /// Maps a <see cref="WorkInstructionNodeViewDTO"/> to its corresponding <see cref="WorkInstructionNode"/> entity.
    /// </summary>
    /// <param name="viewDTO">The node DTO.</param>
    /// <returns>The mapped entity.</returns>
    public static WorkInstructionNode ToEntity(this WorkInstructionNodeViewDTO viewDTO)
    {
        if (viewDTO is null)
            throw new ArgumentNullException(nameof(viewDTO));

        return viewDTO switch
        {
            PartNodeViewDTO partDto => partDto.ToEntity(),
            StepNodeViewDTO stepDto => stepDto.ToEntity(),
            _ => throw new NotSupportedException($"Unknown DTO type: {viewDTO.GetType().Name}")
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="WorkInstructionNode"/> entities to DTOs.
    /// </summary>
    public static List<WorkInstructionNodeViewDTO> ToDTOList(this IEnumerable<WorkInstructionNode> entities)
        => entities?.Select(e => e.ToDTO()).ToList() ?? [];

    /// <summary>
    /// Maps a collection of <see cref="WorkInstructionNodeViewDTO"/> objects to entities.
    /// </summary>
    public static List<WorkInstructionNode> ToEntityList(this IEnumerable<WorkInstructionNodeViewDTO> dtos)
        => dtos?.Select(d => d.ToEntity()).ToList() ?? [];
}
