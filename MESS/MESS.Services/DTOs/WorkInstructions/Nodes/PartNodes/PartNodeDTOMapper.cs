using MESS.Data.Models;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes;

using MESS.Data.Models;

/// <summary>
/// Provides extension methods for mapping between <see cref="PartNode"/> entities
/// and <see cref="PartNodeDTO"/> objects.
/// </summary>
public static class PartNodeDTOMapper
{
    /// <summary>
    /// Converts a <see cref="PartNode"/> entity to its corresponding <see cref="PartNodeDTO"/>.
    /// </summary>
    /// <param name="entity">The <see cref="PartNode"/> entity to map.</param>
    /// <returns>A mapped <see cref="PartNodeDTO"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    public static PartNodeDTO ToDTO(this PartNode entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        return new PartNodeDTO
        {
            Id = entity.Id,
            Position = entity.Position,
            NodeType = entity.NodeType,
            PartDefinitionId = entity.PartDefinitionId,
            PartName = entity.PartDefinition?.Name ?? string.Empty,
            PartNumber = entity.PartDefinition?.Number ?? string.Empty
        };
    }

    /// <summary>
    /// Converts a <see cref="PartNodeDTO"/> to its corresponding <see cref="PartNode"/> entity.
    /// </summary>
    /// <param name="dto">The <see cref="PartNodeDTO"/> to map.</param>
    /// <returns>A mapped <see cref="PartNode"/> entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    public static PartNode ToEntity(this PartNodeDTO dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        return new PartNode
        {
            Id = dto.Id,
            Position = dto.Position,
            NodeType = dto.NodeType,
            PartDefinitionId = dto.PartDefinitionId,
            // The PartDefinition is created here as a placeholder;
            // services should attach an existing tracked entity when available.
            PartDefinition = new PartDefinition
            {
                Id = dto.PartDefinitionId,
                Name = dto.PartName,
                Number = dto.PartNumber
            }
        };
    }

    /// <summary>
    /// Converts a collection of <see cref="PartNode"/> entities to DTOs.
    /// </summary>
    /// <param name="entities">The collection of entities to map.</param>
    /// <returns>A list of <see cref="PartNodeDTO"/> objects.</returns>
    public static List<PartNodeDTO> ToDTOList(this IEnumerable<PartNode> entities)
        => entities?.Select(e => e.ToDTO()).ToList() ?? [];

    /// <summary>
    /// Converts a collection of <see cref="PartNodeDTO"/> objects to entities.
    /// </summary>
    /// <param name="dtos">The collection of DTOs to map.</param>
    /// <returns>A list of <see cref="PartNode"/> entities.</returns>
    public static List<PartNode> ToEntityList(this IEnumerable<PartNodeDTO> dtos)
        => dtos?.Select(d => d.ToEntity()).ToList() ?? [];
}
