using MESS.Data.Models;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.Form;

/// <summary>
/// Provides extension methods for mapping between <see cref="Step"/> entities
/// and their corresponding <see cref="StepNodeFormDTO"/> objects used in the Blazor UI.
/// </summary>
public static class StepNodeFormDTOMapper
{
    /// <summary>
    /// Converts a <see cref="Step"/> entity to a <see cref="StepNodeFormDTO"/>.
    /// </summary>
    public static StepNodeFormDTO ToFormDTO(this Step entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new StepNodeFormDTO
        {
            Id = entity.Id,
            ClientId = Guid.NewGuid(), // required for form DTOs
            Position = entity.Position,
            NodeType = entity.NodeType,
            Name = entity.Name,
            Body = entity.Body,
            DetailedBody = entity.DetailedBody,
            PrimaryMedia = entity.PrimaryMedia.ToList(),
            SecondaryMedia = entity.SecondaryMedia.ToList()
        };
    }

    /// <summary>
    /// Converts a <see cref="StepNodeFormDTO"/> back to a <see cref="Step"/> entity.
    /// </summary>
    public static Step ToEntity(this StepNodeFormDTO dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        return new Step
        {
            Id = dto.Id,
            Position = dto.Position,
            NodeType = dto.NodeType,
            Name = dto.Name,
            Body = dto.Body,
            DetailedBody = dto.DetailedBody,
            PrimaryMedia = dto.PrimaryMedia.ToList(),
            SecondaryMedia = dto.SecondaryMedia.ToList()
        };
    }

    /// <summary>
    /// Converts a collection of <see cref="Step"/> entities to form DTOs.
    /// </summary>
    public static List<StepNodeFormDTO> ToFormDTOList(this IEnumerable<Step> entities)
        => entities.Select(e => e.ToFormDTO()).ToList();

    /// <summary>
    /// Converts a collection of <see cref="StepNodeFormDTO"/> DTOs to entities.
    /// </summary>
    public static List<Step> ToEntityList(this IEnumerable<StepNodeFormDTO> dtos)
        => dtos.Select(d => d.ToEntity()).ToList();
}