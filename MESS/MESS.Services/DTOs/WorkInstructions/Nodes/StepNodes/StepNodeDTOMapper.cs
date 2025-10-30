using MESS.Data.Models;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes;

/// <summary>
/// Provides extension methods for mapping between <see cref="Step"/> entities and <see cref="StepNodeDTO"/> objects.
/// </summary>
public static class StepNodeDTOMapper
{
    /// <summary>
    /// Maps a <see cref="Step"/> entity to a <see cref="StepNodeDTO"/>.
    /// </summary>
    /// <param name="entity">The <see cref="Step"/> entity to map.</param>
    /// <returns>A new <see cref="StepNodeDTO"/> instance.</returns>
    public static StepNodeDTO ToDTO(this Step entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new StepNodeDTO
        {
            Id = entity.Id,
            Position = entity.Position,
            NodeType = entity.NodeType,
            Name = entity.Name,
            Body = entity.Body,
            DetailedBody = entity.DetailedBody,
            PrimaryMedia = entity.PrimaryMedia?.ToList() ?? [],
            SecondaryMedia = entity.SecondaryMedia?.ToList() ?? []
        };
    }

    /// <summary>
    /// Maps a <see cref="StepNodeDTO"/> back to a <see cref="Step"/> entity.
    /// </summary>
    /// <param name="dto">The <see cref="StepNodeDTO"/> to map.</param>
    /// <returns>A new <see cref="Step"/> instance.</returns>
    public static Step ToEntity(this StepNodeDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Step
        {
            Id = dto.Id,
            Position = dto.Position,
            NodeType = dto.NodeType,
            Name = dto.Name,
            Body = dto.Body,
            DetailedBody = dto.DetailedBody,
            PrimaryMedia = dto.PrimaryMedia?.ToList() ?? [],
            SecondaryMedia = dto.SecondaryMedia?.ToList() ?? []
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="Step"/> entities to <see cref="StepNodeDTO"/>s.
    /// </summary>
    public static List<StepNodeDTO> ToDTOs(this IEnumerable<Step> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return entities.Select(e => e.ToDTO()).ToList();
    }

    /// <summary>
    /// Maps a collection of <see cref="StepNodeDTO"/> DTOs to <see cref="Step"/> entities.
    /// </summary>
    public static List<Step> ToEntities(this IEnumerable<StepNodeDTO> dtos)
    {
        ArgumentNullException.ThrowIfNull(dtos);
        return dtos.Select(d => d.ToEntity()).ToList();
    }
}