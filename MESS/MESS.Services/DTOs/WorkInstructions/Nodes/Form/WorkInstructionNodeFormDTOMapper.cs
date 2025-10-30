using MESS.Data.Models;
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
    public static WorkInstructionNodeFormDTO ToFormDTO(this WorkInstructionNode entity, string clientId)
    {
        return entity switch
        {
            PartNode part => part.ToFormDTO(clientId),
            Step step => step.ToFormDTO(clientId),
            _ => throw new NotSupportedException($"Unsupported node entity type: {entity.GetType().Name}")
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
    public static WorkInstructionNode ToEntity(this WorkInstructionNodeFormDTO dto)
    {
        return dto switch
        {
            PartNodeFormDTO partDto => partDto.ToEntity(),
            StepNodeFormDTO stepDto => stepDto.ToEntity(),
            _ => throw new NotSupportedException($"Unsupported node DTO type: {dto.GetType().Name}")
        };
    }
}