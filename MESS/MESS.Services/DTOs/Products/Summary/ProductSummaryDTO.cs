namespace MESS.Services.DTOs.Products.Summary;

/// <summary>
/// Represents a lightweight summary of a product, optimized for list and selection views.
/// </summary>
public class ProductSummaryDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the name of the product, typically derived from its part definition.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the product number or code.
    /// </summary>
    public required string Number { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated part definition.
    /// Used for lightweight cross-referencing without loading full part details.
    /// </summary>
    public int PartDefinitionId { get; set; }
}
