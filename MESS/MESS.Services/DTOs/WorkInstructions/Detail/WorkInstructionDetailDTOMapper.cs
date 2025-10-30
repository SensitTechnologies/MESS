using MESS.Data.Models;
using MESS.Services.DTOs.Products.Summary;
using MESS.Services.DTOs.WorkInstructions.Nodes.View;

namespace MESS.Services.DTOs.WorkInstructions.Detail;

/// <summary>
/// Provides extension methods for mapping between <see cref="WorkInstruction"/> entities
/// and <see cref="WorkInstructionDetailDTO"/> objects.
/// </summary>
public static class WorkInstructionDetailDTOMapper
{
    /// <summary>
    /// Maps a <see cref="WorkInstruction"/> entity to a <see cref="WorkInstructionDetailDTO"/>.
    /// </summary>
    /// <param name="entity">The work instruction entity.</param>
    /// <returns>A populated <see cref="WorkInstructionDetailDTO"/> instance.</returns>
    public static WorkInstructionDetailDTO ToDetailDTO(this WorkInstruction entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new WorkInstructionDetailDTO
        {
            Id = entity.Id,
            Title = entity.Title,
            Version = entity.Version,
            OriginalId = entity.OriginalId,
            IsLatest = entity.IsLatest,
            IsActive = entity.IsActive,
            ShouldGenerateQrCode = entity.ShouldGenerateQrCode,
            PartProducedIsSerialized = entity.PartProducedIsSerialized,
            PartProducedId = entity.PartProducedId,
            PartProducedName = entity.PartProduced?.Name,
            PartProducedNumber = entity.PartProduced?.Number,
            Products = entity.Products?
                .Select(p => p.ToSummaryDTO())
                .ToList() ?? [],
            Nodes = entity.Nodes?
                .Select(n => n.ToDTO())
                .ToList() ?? []
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="WorkInstruction"/> entities to <see cref="WorkInstructionDetailDTO"/> objects.
    /// </summary>
    /// <param name="entities">The collection of work instruction entities.</param>
    /// <returns>An enumerable of <see cref="WorkInstructionDetailDTO"/> instances.</returns>
    public static IEnumerable<WorkInstructionDetailDTO> ToDetailDTOs(this IEnumerable<WorkInstruction> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Select(e => e.ToDetailDTO());
    }
}