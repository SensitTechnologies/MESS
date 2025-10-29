using MESS.Data.Models;

namespace MESS.Services.DTOs.PartDefinitions;

/// <summary>
/// Provides mapping functionality between <see cref="PartDefinition"/> entities
/// and <see cref="PartDefinitionDTO"/> objects.
/// </summary>
public static class PartDefinitionDTOMapper
{
    /// <summary>
    /// Maps a <see cref="PartDefinition"/> entity to a <see cref="PartDefinitionDTO"/>.
    /// </summary>
    /// <param name="entity">The part definition entity.</param>
    /// <returns>A DTO populated from the entity.</returns>
    public static PartDefinitionDTO ToDTO(this PartDefinition entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new PartDefinitionDTO
        {
            PartDefinitionId = entity.Id,
            Number = entity.Number,
            Name = entity.Name
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="PartDefinition"/> entities to DTOs.
    /// </summary>
    /// <param name="entities">The collection of part definition entities.</param>
    /// <returns>A collection of DTOs populated from the entities.</returns>
    public static IEnumerable<PartDefinitionDTO> ToDTOs(this IEnumerable<PartDefinition> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Select(e => e.ToDTO());
    }

    /// <summary>
    /// Maps a <see cref="PartDefinitionDTO"/> back to a <see cref="PartDefinition"/> entity.
    /// </summary>
    /// <param name="dto">The DTO to map from.</param>
    /// <returns>A new <see cref="PartDefinition"/> entity populated from the DTO.</returns>
    public static PartDefinition ToEntity(this PartDefinitionDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new PartDefinition
        {
            Id = dto.PartDefinitionId,
            Number = dto.Number,
            Name = dto.Name
        };
    }

    /// <summary>
    /// Updates an existing <see cref="PartDefinition"/> entity with values from a <see cref="PartDefinitionDTO"/>.
    /// </summary>
    /// <param name="dto">The DTO containing updated values.</param>
    /// <param name="entity">The entity to update.</param>
    public static void UpdateEntity(this PartDefinitionDTO dto, PartDefinition entity)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(entity);

        entity.Number = dto.Number;
        entity.Name = dto.Name;
    }
}