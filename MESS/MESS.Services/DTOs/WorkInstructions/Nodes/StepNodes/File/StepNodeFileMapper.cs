using MESS.Data.Models;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.File;

/// <summary>
/// Provides mapping extensions between <see cref="Step"/> and <see cref="StepNodeFileDTO"/>.
/// </summary>
public static class StepNodeFileMapper
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
            SecondaryMedia = entity.SecondaryMedia.ToList()
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
            SecondaryMedia = dto.SecondaryMedia.ToList()
        };
    }
}
