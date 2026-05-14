using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.Form;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.File;

/// <summary>
/// Provides mapping extensions between <see cref="Step"/> and <see cref="StepNodeFileDTO"/>.
/// </summary>
public static class StepNodeFileDTOMapper
{
    /// <summary>
    /// Converts a <see cref="Step"/> entity to a <see cref="StepNodeFileDTO"/>.
    /// </summary>
    public static StepNodeFileDTO ToFileDTO(this Step entity)
    {
        return new StepNodeFileDTO
        {
            Position = entity.Position,
            Name = entity.Name,
            Body = entity.Body,
            DetailedBody = entity.DetailedBody,
            PrimaryMedia = entity.PrimaryMedia.ToList(),
            SecondaryMedia = entity.SecondaryMedia.ToList(),
            NotesConfiguration = entity.NotesConfiguration
        };
    }

    /// <summary>
    /// Converts a <see cref="StepNodeFileDTO"/> to a <see cref="Step"/> entity.
    /// </summary>
    public static Step ToEntity(this StepNodeFileDTO dto)
    {
        return new Step
        {
            Position = dto.Position,
            NodeType = WorkInstructionNodeType.Step,
            Name = dto.Name,
            Body = dto.Body,
            DetailedBody = dto.DetailedBody,
            PrimaryMedia = dto.PrimaryMedia.ToList(),
            SecondaryMedia = dto.SecondaryMedia.ToList(),
            NotesConfiguration = dto.NotesConfiguration
        };
    }
    
    /// <summary>
    /// Converts a <see cref="StepNodeFileDTO"/> into a <see cref="StepNodeFormDTO"/> suitable for use in the UI.
    /// </summary>
    /// <param name="dto">The file DTO representing a step node, typically imported from a work instruction file.</param>
    /// <returns>
    /// A new <see cref="StepNodeFormDTO"/> containing all properties from the file DTO, 
    /// including position, node type, name, body, detailed body, and associated media.
    /// </returns>
    /// <remarks>
    /// This method internally converts the file DTO to a <see cref="Step"/> entity first, 
    /// then maps it to a form DTO. It preserves media collections by copying the lists.
    /// </remarks>
    public static StepNodeFormDTO ToFormDTO(this StepNodeFileDTO dto)
    {
        var entity = dto.ToEntity();

        return new StepNodeFormDTO
        {
            Position = entity.Position,
            NodeType = entity.NodeType,
            Name = entity.Name,
            Body = entity.Body,
            DetailedBody = entity.DetailedBody,
            PrimaryMedia = entity.PrimaryMedia,
            SecondaryMedia = entity.SecondaryMedia,
            NotesConfiguration = entity.NotesConfiguration
        };
    }
}
