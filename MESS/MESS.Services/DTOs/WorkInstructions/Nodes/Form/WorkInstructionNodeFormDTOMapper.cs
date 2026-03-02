using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Nodes.File;
using MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.Form;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.Form;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.Form;

/// <summary>
/// Provides extension methods for mapping between
/// <see cref="WorkInstructionNode"/> entities and
/// <see cref="WorkInstructionNodeFormDTO"/> objects.
/// </summary>
/// <remarks>
/// This acts as a centralized entry point that delegates mapping
/// to specific child mappers (e.g. <see cref="PartNodeFormDTOMapper"/>,
/// <see cref="StepNodeFormDTOMapper"/>).
/// </remarks>
public static class WorkInstructionNodeFormMapper
{
    /// <summary>
    /// Converts a <see cref="WorkInstructionNode"/> entity into its corresponding
    /// <see cref="WorkInstructionNodeFormDTO"/> representation.
    /// </summary>
    /// <param name="entity">The node entity to convert.</param>
    /// <param name="clientId">A unique client-generated identifier for unsaved nodes.</param>
    /// <returns>A <see cref="WorkInstructionNodeFormDTO"/> representation of the entity.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the entity type is not recognized.
    /// </exception>
    public static WorkInstructionNodeFormDTO ToFormDTO(this WorkInstructionNode entity, Guid clientId)
    {
        return entity switch
        {
            PartNode part => PartNodeFormDTOMapper.ToFormDTO(part, clientId),
            Step step => StepNodeFormDTOMapper.ToFormDTO(step, clientId),
            _ => throw new NotSupportedException($"Unsupported node entity type: {entity.GetType().Name}")
        };
    }
    
    
    
    /// <summary>
    /// Converts a <see cref="WorkInstructionNodeFormDTO"/> into its corresponding
    /// file/export DTO representation.
    /// </summary>
    /// <param name="dto">The node form DTO to convert.</param>
    /// <returns>A file DTO representation of the node.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the DTO type is not recognized.
    /// </exception>
    public static WorkInstructionNodeFileDTO ToFileDTO(this WorkInstructionNodeFormDTO dto)
    {
        return dto switch
        {
            PartNodeFormDTO partDto => PartNodeFormDTOMapper.ToFileDTO(partDto),
            StepNodeFormDTO stepDto => StepNodeFormDTOMapper.ToFileDTO(stepDto),
            _ => throw new NotSupportedException(
                $"Unsupported node DTO type: {dto.GetType().Name}")
        };
    }

    /// <summary>
    /// Converts a <see cref="WorkInstructionNodeFormDTO"/> into its corresponding
    /// <see cref="WorkInstructionNode"/> entity.
    /// </summary>
    /// <param name="dto">The node form DTO to convert.</param>
    /// <returns>A <see cref="WorkInstructionNode"/> entity.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the DTO type is not recognized.
    /// </exception>
    public static WorkInstructionNode ToNewEntity(this WorkInstructionNodeFormDTO dto)
    {
        return dto switch
        {
            PartNodeFormDTO partDto => PartNodeFormDTOMapper.ToNewEntity(partDto),
            StepNodeFormDTO stepDto => StepNodeFormDTOMapper.ToNewEntity(stepDto),
            _ => throw new NotSupportedException($"Unsupported node DTO type: {dto.GetType().Name}")
        };
    }
}