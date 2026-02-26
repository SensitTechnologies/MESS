using MESS.Data.Models;
using MESS.Services.DTOs.PartDefinitions;
using MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.File;

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
    /// <param name="clientId">A unique client-generated identifier for nodes.</param>
    /// <returns>A mapped <see cref="PartNodeFormDTO"/>.</returns>
    public static PartNodeFormDTO ToFormDTO(this PartNode entity, Guid clientId)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        return new PartNodeFormDTO
        {
            Id = entity.Id,
            ClientId = clientId,
            Position = entity.Position,
            NodeType = entity.NodeType,
            InputType = entity.InputType,
            PartDefinitionId = entity.PartDefinitionId,
            PartDefinition = entity.PartDefinition.ToDTO()
        };
    }
    
        
    /// <summary>
    /// Converts a PartNodeFormDTO to a file-safe DTO for export.
    /// </summary>
    public static PartNodeFileDTO ToFileDTO(this PartNodeFormDTO form)
    {
        return new PartNodeFileDTO
        {
            Position = form.Position,
            PartName = form.PartDefinition?.Name ?? "UNKNOWN",
            PartNumber = form.PartDefinition?.Number ?? "UNKNOWN",
            InputType = form.InputType,
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
            InputType = dto.InputType,
            PartDefinitionId = dto.PartDefinitionId,
            PartDefinition = dto.PartDefinition is not null
                ? dto.PartDefinition.ToEntity()
                : new PartDefinition { Id = dto.PartDefinitionId, Number = null!, Name = null! }
        };
    }

    /// <summary>
    /// Converts a collection of <see cref="PartNodeFormDTO"/> DTOs to a list of <see cref="PartNode"/> entities.
    /// </summary>
    public static List<PartNode> ToEntityList(this IEnumerable<PartNodeFormDTO> dtos)
        => dtos.Select(d => d.ToEntity()).ToList();
}
