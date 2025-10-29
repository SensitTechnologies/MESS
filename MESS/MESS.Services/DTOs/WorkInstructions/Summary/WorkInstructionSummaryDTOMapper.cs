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
            PartProducedNumber = entity.PartProduced?.Number
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

    /// <summary>
    /// Maps a <see cref="WorkInstructionSummaryDTO"/> back to a <see cref="WorkInstruction"/> entity.
    /// </summary>
    /// <param name="dto">The DTO to map from.</param>
    /// <returns>A new <see cref="WorkInstruction"/> entity populated from the DTO.</returns>
    public static WorkInstruction ToEntity(this WorkInstructionSummaryDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new WorkInstruction
        {
            Id = dto.Id,
            Title = dto.Title,
            Version = dto.Version,
            OriginalId = dto.OriginalId,
            IsLatest = dto.IsLatest,
            IsActive = dto.IsActive,
            PartProducedId = dto.PartProducedId,
            PartProduced = dto.PartProducedId.HasValue
                ? new PartDefinition
                {
                    Id = dto.PartProducedId.Value,
                    Name = dto.PartProducedName ?? string.Empty,
                    Number = dto.PartProducedNumber ?? string.Empty
                }
                : null
        };
    }

    /// <summary>
    /// Updates an existing <see cref="WorkInstruction"/> entity with values from a <see cref="WorkInstructionSummaryDTO"/>.
    /// </summary>
    /// <param name="dto">The DTO containing updated values.</param>
    /// <param name="entity">The entity to update.</param>
    public static void UpdateEntity(this WorkInstructionSummaryDTO dto, WorkInstruction entity)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(entity);

        entity.Title = dto.Title;
        entity.Version = dto.Version;
        entity.OriginalId = dto.OriginalId;
        entity.IsLatest = dto.IsLatest;
        entity.IsActive = dto.IsActive;
        entity.PartProducedId = dto.PartProducedId;

        if (entity.PartProduced != null && dto.PartProducedId.HasValue)
        {
            entity.PartProduced.Id = dto.PartProducedId.Value;
            entity.PartProduced.Name = dto.PartProducedName ?? entity.PartProduced.Name;
            entity.PartProduced.Number = dto.PartProducedNumber ?? entity.PartProduced.Number;
        }
        else if (dto.PartProducedId.HasValue)
        {
            entity.PartProduced = new PartDefinition
            {
                Id = dto.PartProducedId.Value,
                Name = dto.PartProducedName ?? string.Empty,
                Number = dto.PartProducedNumber ?? string.Empty
            };
        }
        else
        {
            entity.PartProduced = null;
        }
    }
}

