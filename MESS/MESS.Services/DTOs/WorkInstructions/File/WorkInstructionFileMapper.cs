using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Nodes.File;
using MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.File;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.File;

namespace MESS.Services.DTOs.WorkInstructions.File;

/// <summary>
/// Provides mapping extensions between <see cref="WorkInstruction"/> 
/// and <see cref="WorkInstructionFileDTO"/>.
/// </summary>
public static class WorkInstructionFileMapper
{
    /// <summary>
    /// Converts a <see cref="WorkInstruction"/> entity to a <see cref="WorkInstructionFileDTO"/>.
    /// </summary>
    public static WorkInstructionFileDTO ToFileDTO(this WorkInstruction entity)
    {
        return new WorkInstructionFileDTO
        {
            Title = entity.Title,
            Version = entity.Version,
            IsActive = entity.IsActive,
            ShouldGenerateQrCode = entity.ShouldGenerateQrCode,
            PartProducedIsSerialized = entity.PartProducedIsSerialized,
            ProducedPartName = entity.PartProduced?.Name,
            AssociatedProductNames = entity.Products
                .Select(p => p.PartDefinition.Name)
                .ToList(),

            Nodes = entity.Nodes
                .OrderBy(n => n.Position)
                .Select(n => n switch
                {
                    Step step => (WorkInstructionNodeFileDTO)step.ToFileDTO(),
                    PartNode part => (WorkInstructionNodeFileDTO)part.ToFileDTO(),
                    _ => throw new NotSupportedException(
                        $"Unsupported node type: {n.GetType().Name}")
                })
                .ToList()
        };
    }
}