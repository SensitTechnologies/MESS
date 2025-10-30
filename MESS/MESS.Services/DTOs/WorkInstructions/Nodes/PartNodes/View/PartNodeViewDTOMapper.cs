namespace MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.View;

using Data.Models;

/// <summary>
/// Provides extension methods for mapping between <see cref="PartNode"/> entities
/// and <see cref="PartNodeViewDTO"/> objects.
/// </summary>
public static class PartNodeViewDTOMapper
{
    /// <summary>
    /// Converts a <see cref="PartNode"/> entity to its corresponding <see cref="PartNodeViewDTO"/>.
    /// </summary>
    /// <param name="entity">The <see cref="PartNode"/> entity to map.</param>
    /// <returns>A mapped <see cref="PartNodeViewDTO"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    public static PartNodeViewDTO ToDTO(this PartNode entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        return new PartNodeViewDTO
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
    /// Converts a <see cref="PartNodeViewDTO"/> to its corresponding <see cref="PartNode"/> entity.
    /// </summary>
    /// <param name="viewDTO">The <see cref="PartNodeViewDTO"/> to map.</param>
    /// <returns>A mapped <see cref="PartNode"/> entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewDTO"/> is null.</exception>
    public static PartNode ToEntity(this PartNodeViewDTO viewDTO)
    {
        if (viewDTO is null)
            throw new ArgumentNullException(nameof(viewDTO));

        return new PartNode
        {
            Id = viewDTO.Id,
            Position = viewDTO.Position,
            NodeType = viewDTO.NodeType,
            PartDefinitionId = viewDTO.PartDefinitionId,
            // The PartDefinition is created here as a placeholder;
            // services should attach an existing tracked entity when available.
            PartDefinition = new PartDefinition
            {
                Id = viewDTO.PartDefinitionId,
                Name = viewDTO.PartName,
                Number = viewDTO.PartNumber
            }
        };
    }

    /// <summary>
    /// Converts a collection of <see cref="PartNode"/> entities to DTOs.
    /// </summary>
    /// <param name="entities">The collection of entities to map.</param>
    /// <returns>A list of <see cref="PartNodeViewDTO"/> objects.</returns>
    public static List<PartNodeViewDTO> ToDTOList(this IEnumerable<PartNode> entities)
        => entities?.Select(e => e.ToDTO()).ToList() ?? [];

    /// <summary>
    /// Converts a collection of <see cref="PartNodeViewDTO"/> objects to entities.
    /// </summary>
    /// <param name="dtos">The collection of DTOs to map.</param>
    /// <returns>A list of <see cref="PartNode"/> entities.</returns>
    public static List<PartNode> ToEntityList(this IEnumerable<PartNodeViewDTO> dtos)
        => dtos?.Select(d => d.ToEntity()).ToList() ?? [];
}
