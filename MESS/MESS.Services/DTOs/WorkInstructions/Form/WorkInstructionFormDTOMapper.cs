using MESS.Data.Models;
using MESS.Services.DTOs.Products.Summary;
using MESS.Services.DTOs.WorkInstructions.File;
using MESS.Services.DTOs.WorkInstructions.Nodes.Form;
using MESS.Services.DTOs.WorkInstructions.Summary;

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
            ProducedPartName = entity.PartProduced?.Name,
            ProductIds = entity.Products.Select(p => p.Id).ToList(),
            Nodes = entity.Nodes.Select(n => n.ToFormDTO(Guid.NewGuid())).ToList()
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
    
    /// <summary>
    /// Maps a <see cref="WorkInstructionFormDTO"/> to a <see cref="WorkInstructionSummaryDTO"/>.
    /// </summary>
    /// <param name="formDto">The form DTO representing the editable work instruction.</param>
    /// <param name="allProducts">
    /// A collection of <see cref="ProductSummaryDTO"/> used to populate the
    /// <see cref="WorkInstructionSummaryDTO.Products"/> property.
    /// Only products whose IDs match <see cref="WorkInstructionFormDTO.ProductIds"/> will be included.
    /// </param>
    /// <returns>A <see cref="WorkInstructionSummaryDTO"/> containing the mapped summary data.</returns>
    public static WorkInstructionSummaryDTO ToSummaryDTO(
        this WorkInstructionFormDTO formDto,
        IEnumerable<ProductSummaryDTO> allProducts)
    {
        ArgumentNullException.ThrowIfNull(formDto);
        ArgumentNullException.ThrowIfNull(allProducts);

        return new WorkInstructionSummaryDTO
        {
            Id = formDto.Id ?? 0,
            Title = formDto.Title,
            Version = formDto.Version,
            OriginalId = formDto.OriginalId,
            IsLatest = formDto.IsLatest,
            IsActive = formDto.IsActive,
            PartProducedId = formDto.PartProducedId,
            PartProducedName = formDto.ProducedPartName,
            Products = allProducts
                .Where(p => formDto.ProductIds.Contains(p.ProductId))
                .ToList()
        };
    }
    
    
    /// <summary>
    /// Converts a <see cref="WorkInstructionFormDTO"/> (editable form DTO)
    /// to a <see cref="WorkInstructionFileDTO"/> for file export.
    /// </summary>
    /// <param name="formDto">The editable work instruction DTO.</param>
    /// <param name="productNameResolver">
    /// A function to resolve product IDs to product names.
    /// Typically, this comes from the loaded product list in the UI.
    /// </param>
    public static WorkInstructionFileDTO ToFileDTO(
        this WorkInstructionFormDTO formDto,
        Func<int, string> productNameResolver)
    {
        if (formDto == null) throw new ArgumentNullException(nameof(formDto));
        if (productNameResolver == null) throw new ArgumentNullException(nameof(productNameResolver));

        return new WorkInstructionFileDTO
        {
            Title = formDto.Title,
            Version = formDto.Version,
            IsActive = formDto.IsActive,
            ShouldGenerateQrCode = formDto.ShouldGenerateQrCode,
            PartProducedIsSerialized = formDto.PartProducedIsSerialized,
            ProducedPartName = formDto.ProducedPartName,
            AssociatedProductNames = formDto.ProductIds
                .Select(productNameResolver)
                .ToList(),
            Nodes = formDto.Nodes
                .Select(n => n.ToFileDTO())
                .ToList()
        };
    }
}