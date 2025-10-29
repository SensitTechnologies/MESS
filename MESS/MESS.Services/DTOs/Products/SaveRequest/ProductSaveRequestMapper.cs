namespace MESS.Services.DTOs.Products.SaveRequest;

using Data.Models;

/// <summary>
/// Provides extension methods for mapping between <see cref="ProductSaveRequest"/> DTOs
/// and <see cref="Product"/> entities.
/// </summary>
public static class ProductSaveRequestMapper
{
    /// <summary>
    /// Creates a new <see cref="Product"/> entity from a <see cref="ProductSaveRequest"/>.
    /// Does not attach related entities — this should be handled by the service layer.
    /// </summary>
    public static Product ToEntity(this ProductSaveRequest dto)
    {
        return new Product
        {
            PartDefinitionId = dto.PartDefinitionId,
            IsActive = dto.IsActive,
            PartDefinition = null!, // The service layer will attach the real entity later
            // WorkInstructions should be attached by the service layer using WorkInstructionIds
        };
    }

    /// <summary>
    /// Updates an existing <see cref="Product"/> entity with data from a <see cref="ProductSaveRequest"/>.
    /// Does not modify related entities — this should be handled by the service layer.
    /// </summary>
    public static void UpdateEntity(this ProductSaveRequest dto, Product product)
    {
        product.PartDefinitionId = dto.PartDefinitionId;
        product.IsActive = dto.IsActive;
        // WorkInstructions should be updated by the service layer based on WorkInstructionIds
    }
}