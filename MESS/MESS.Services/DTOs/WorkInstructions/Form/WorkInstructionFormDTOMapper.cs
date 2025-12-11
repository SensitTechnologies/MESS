using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Nodes.Form;

namespace MESS.Services.DTOs.WorkInstructions.Form;

/// <summary>
/// Provides extension methods for mapping between
/// <see cref="WorkInstruction"/> entities and
/// <see cref="WorkInstructionFormDTO"/> objects.
/// </summary>
/// <remarks>
/// This mapper delegates node-level conversions to
/// <see cref="WorkInstructionNodeFormMapper"/> to avoid redundancy.
/// </remarks>
public static class WorkInstructionFormDTOMapper
{
    /// <summary>
    /// Converts a <see cref="WorkInstruction"/> entity into a <see cref="WorkInstructionFormDTO"/>.
    /// </summary>
    /// <param name="entity">The work instruction entity to convert.</param>
    /// <returns>A <see cref="WorkInstructionFormDTO"/> representation of the entity.</returns>
    public static WorkInstructionFormDTO ToFormDTO(this WorkInstruction entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        return new WorkInstructionFormDTO
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
            ProductIds = entity.Products.Select(p => p.Id).ToList(),
            Nodes = entity.Nodes.Select(n => n.ToFormDTO(Guid.NewGuid().ToString())).ToList()
        };
    }

    /// <summary>
    /// Converts a <see cref="WorkInstructionFormDTO"/> into a <see cref="WorkInstruction"/> entity.
    /// </summary>
    /// <param name="dto">The form DTO to convert.</param>
    /// <returns>A <see cref="WorkInstruction"/> entity populated from the DTO.</returns>
    public static WorkInstruction ToEntity(this WorkInstructionFormDTO dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        return new WorkInstruction
        {
            Id = dto.Id ?? 0,
            Title = dto.Title,
            Version = dto.Version,
            OriginalId = dto.OriginalId,
            IsLatest = dto.IsLatest,
            IsActive = dto.IsActive,
            ShouldGenerateQrCode = dto.ShouldGenerateQrCode,
            PartProducedIsSerialized = dto.PartProducedIsSerialized,
            PartProducedId = dto.PartProducedId,
            // PartProduced and Products can be attached separately by services if needed
            Nodes = dto.Nodes.Select(n => n.ToEntity()).ToList()
        };
    }
}