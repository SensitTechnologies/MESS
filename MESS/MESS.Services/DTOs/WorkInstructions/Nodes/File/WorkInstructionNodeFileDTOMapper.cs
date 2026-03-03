using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.File;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.File;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.File;

/// <summary>
/// A mapper class for WorkInstructionNodeFileDTOs
/// </summary>
public static class WorkInstructionNodeFileDTOMapper
{
    /// <summary>
    /// Converts a <see cref="WorkInstructionNode"/> entity into its corresponding
    /// file/export DTO representation.
    /// </summary>
    /// <param name="entity">The node entity to convert.</param>
    /// <returns>A file DTO representation of the node.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the entity type is not recognized.
    /// </exception>
    public static WorkInstructionNodeFileDTO ToFileDTO(this WorkInstructionNode entity)
    {
        return entity switch
        {
            PartNode part => PartNodeFileDTOMapper.ToFileDTO(part), // call child mapper
            Step step => StepNodeFileDTOMapper.ToFileDTO(step),  
            _ => throw new NotSupportedException(
                $"Unsupported node entity type: {entity.GetType().Name}")
        };
    }
}