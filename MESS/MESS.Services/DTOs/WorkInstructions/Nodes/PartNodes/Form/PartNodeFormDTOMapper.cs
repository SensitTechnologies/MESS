using MESS.Data.Models;
using MESS.Services.DTOs.PartDefinitions;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.Form;

/// <summary>
/// Provides extension methods to map between <see cref="PartNode"/> entities
/// and <see cref="PartNodeFormDTO"/> objects for create/update operations.
/// </summary>
public static class PartNodeFormDTOMapper
{
    /// <summary>
    /// Converts a <see cref="PartNode"/> entity to a <see cref="PartNodeFormDTO"/>.
    /// </summary>
    /// <param name="entity">The <see cref="PartNode"/> to convert.</param>
    /// <returns>A mapped <see cref="PartNodeFormDTO"/>.</returns>
    public static PartNodeFormDTO ToFormDTO(this PartNode entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        return new PartNodeFormDTO
        {
            Id = entity.Id,
            ClientId = Guid.NewGuid(),
            Position = entity.Position,
            NodeType = entity.NodeType,
            PartDefinitionId = entity.PartDefinitionId,
            PartDefinition = entity.PartDefinition.ToDTO()
        };
    }

    /// <summary>
    /// Converts a <see cref="PartNodeFormDTO"/> to a <see cref="PartNode"/> entity.
    /// </summary>
    /// <param name="dto">The <see cref="PartNodeFormDTO"/> to convert.</param>
    /// <returns>A mapped <see cref="PartNode"/> entity.</returns>
    public static PartNode ToEntity(this PartNodeFormDTO dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        return new PartNode
        {
            Id = dto.Id,
            Position = dto.Position,
            NodeType = dto.NodeType,
            PartDefinitionId = dto.PartDefinitionId,
            PartDefinition = dto.PartDefinition is not null
                ? dto.PartDefinition.ToEntity()
                : new PartDefinition { Id = dto.PartDefinitionId, Number = null!, Name = null! }
        };
    }

    /// <summary>
    /// Converts a collection of <see cref="PartNode"/> entities to a list of <see cref="PartNodeFormDTO"/>s.
    /// </summary>
    public static List<PartNodeFormDTO> ToFormDTOList(this IEnumerable<PartNode> entities)
        => entities.Select(e => e.ToFormDTO()).ToList();

    /// <summary>
    /// Converts a collection of <see cref="PartNodeFormDTO"/> DTOs to a list of <see cref="PartNode"/> entities.
    /// </summary>
    public static List<PartNode> ToEntityList(this IEnumerable<PartNodeFormDTO> dtos)
        => dtos.Select(d => d.ToEntity()).ToList();
}
