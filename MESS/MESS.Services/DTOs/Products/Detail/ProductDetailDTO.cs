using MESS.Services.DTOs.PartDefinitions;
using MESS.Services.DTOs.WorkInstructions.Summary;

namespace MESS.Services.DTOs.Products.Detail;

/// <summary>
/// Represents a detailed view of a product, including its part definition
/// and associated work instructions.  
/// Used for displaying and editing products in the Blazor UI.
/// </summary>
public class ProductDetailDTO : PartDefinitionDTO
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the list of work instructions associated with this product.  
    /// Typically used to manage product-instruction associations in the UI.
    /// </summary>
    public List<WorkInstructionSummaryDTO> WorkInstructions { get; set; } = [];
}