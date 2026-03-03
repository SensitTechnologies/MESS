using MESS.Services.DTOs.Products.Summary;

namespace MESS.Services.DTOs.WorkInstructions.Summary;

using Data.Models;

/// <summary>
/// Provides mapping functionality between <see cref="WorkInstruction"/> entities
/// and <see cref="WorkInstructionSummaryDTO"/> objects.
/// </summary>
public static class WorkInstructionSummaryDTOMapper
{
    /// <summary>
    /// Maps a <see cref="WorkInstruction"/> entity to a <see cref="WorkInstructionSummaryDTO"/>.
    /// </summary>
    /// <param name="entity">The work instruction entity to map.</param>
    /// <returns>A summary DTO populated from the entity.</returns>
    public static WorkInstructionSummaryDTO ToSummaryDTO(this WorkInstruction entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new WorkInstructionSummaryDTO
        {
            Id = entity.Id,
            Title = entity.Title,
            Version = entity.Version,
            OriginalId = entity.OriginalId,
            IsLatest = entity.IsLatest,
            IsActive = entity.IsActive,
            PartProducedId = entity.PartProducedId,
            PartProducedName = entity.PartProduced?.Name,
            PartProducedNumber = entity.PartProduced?.Number,
            
            // Use ProductSummaryDTOMapper here
            Products = entity.Products?.ToSummaryDTOList() ?? []
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="WorkInstruction"/> entities to <see cref="WorkInstructionSummaryDTO"/> objects.
    /// </summary>
    /// <param name="entities">The collection of work instruction entities to map.</param>
    /// <returns>A collection of summary DTOs populated from the entities.</returns>
    public static IEnumerable<WorkInstructionSummaryDTO> ToSummaryDTOs(this IEnumerable<WorkInstruction> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Select(e => e.ToSummaryDTO());
    }
}