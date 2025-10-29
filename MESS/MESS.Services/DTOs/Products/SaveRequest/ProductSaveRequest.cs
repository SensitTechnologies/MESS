namespace MESS.Services.DTOs.Products.SaveRequest;

/// <summary>
/// Represents the data required to create or update a product.
/// </summary>
public class ProductSaveRequest
{
    /// <summary>
    /// Gets or sets the ID of the product to update.
    /// If null, a new product will be created.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key of the associated part definition.
    /// </summary>
    public int PartDefinitionId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the IDs of work instructions associated with this product.
    /// </summary>
    public List<int> WorkInstructionIds { get; set; } = [];
}