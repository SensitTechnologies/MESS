using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.File;

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
    public static StepNodeFormDTO ToFormDTO(this Step entity, Guid clientId)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new StepNodeFormDTO
        {
            Id = entity.Id,
            ClientId =  clientId, // required for form DTOs
            Position = entity.Position,
            NodeType = entity.NodeType,
            Name = entity.Name,
            Body = entity.Body,
            DetailedBody = entity.DetailedBody,
            PrimaryMedia = entity.PrimaryMedia.ToList(),
            SecondaryMedia = entity.SecondaryMedia.ToList(),
            NotesConfiguration = entity.NotesConfiguration
        };
    }

    /// <summary>
    /// Converts a <see cref="StepNodeFormDTO"/> back to a <see cref="Step"/> entity.
    /// </summary>
    public static Step ToNewEntity(this StepNodeFormDTO dto)
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
            SecondaryMedia = dto.SecondaryMedia.ToList(),
            NotesConfiguration = dto.NotesConfiguration
        };
    }
    
    /// <summary>
    /// Converts a <see cref="StepNodeFormDTO"/> to a <see cref="StepNodeFileDTO"/> suitable for file export.
    /// </summary>
    /// <param name="dto">The form DTO representing a step node.</param>
    /// <returns>A <see cref="StepNodeFileDTO"/> containing the exportable step data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
    public static StepNodeFileDTO ToFileDTO(this StepNodeFormDTO dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        return new StepNodeFileDTO
        {
            Position = dto.Position,
            Name = dto.Name,
            Body = dto.Body,
            DetailedBody = dto.DetailedBody,
            PrimaryMedia = dto.PrimaryMedia.ToList(),
            SecondaryMedia = dto.SecondaryMedia.ToList(),
            NotesConfiguration = dto.NotesConfiguration
        };
    }

    /// <summary>
    /// Converts a collection of <see cref="StepNodeFormDTO"/> DTOs to entities.
    /// </summary>
    public static List<Step> ToEntityList(this IEnumerable<StepNodeFormDTO> dtos)
        => dtos.Select(d => d.ToNewEntity()).ToList();
}