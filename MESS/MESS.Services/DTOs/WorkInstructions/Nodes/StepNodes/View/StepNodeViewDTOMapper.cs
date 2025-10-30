using MESS.Data.Models;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.View;

/// <summary>
/// Provides extension methods for mapping between <see cref="Step"/> entities and <see cref="StepNodeViewDTO"/> objects.
/// </summary>
public static class StepNodeViewDTOMapper
{
    /// <summary>
    /// Maps a <see cref="Step"/> entity to a <see cref="StepNodeViewDTO"/>.
    /// </summary>
    /// <param name="entity">The <see cref="Step"/> entity to map.</param>
    /// <returns>A new <see cref="StepNodeViewDTO"/> instance.</returns>
    public static StepNodeViewDTO ToDTO(this Step entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new StepNodeViewDTO
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
    /// Maps a <see cref="StepNodeViewDTO"/> back to a <see cref="Step"/> entity.
    /// </summary>
    /// <param name="viewDTO">The <see cref="StepNodeViewDTO"/> to map.</param>
    /// <returns>A new <see cref="Step"/> instance.</returns>
    public static Step ToEntity(this StepNodeViewDTO viewDTO)
    {
        ArgumentNullException.ThrowIfNull(viewDTO);

        return new Step
        {
            Id = viewDTO.Id,
            Position = viewDTO.Position,
            NodeType = viewDTO.NodeType,
            Name = viewDTO.Name,
            Body = viewDTO.Body,
            DetailedBody = viewDTO.DetailedBody,
            PrimaryMedia = viewDTO.PrimaryMedia?.ToList() ?? [],
            SecondaryMedia = viewDTO.SecondaryMedia?.ToList() ?? []
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="Step"/> entities to <see cref="StepNodeViewDTO"/>s.
    /// </summary>
    public static List<StepNodeViewDTO> ToDTOs(this IEnumerable<Step> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return entities.Select(e => e.ToDTO()).ToList();
    }

    /// <summary>
    /// Maps a collection of <see cref="StepNodeViewDTO"/> DTOs to <see cref="Step"/> entities.
    /// </summary>
    public static List<Step> ToEntities(this IEnumerable<StepNodeViewDTO> dtos)
    {
        ArgumentNullException.ThrowIfNull(dtos);
        return dtos.Select(d => d.ToEntity()).ToList();
    }
}