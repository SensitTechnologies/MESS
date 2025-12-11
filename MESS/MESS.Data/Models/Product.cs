namespace MESS.Data.Models;

/// <summary>
/// Represents a product entity that references a part definition,
/// determines whether it is active, and associates related work instructions.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the associated part definition.
    /// </summary>
    public int PartDefinitionId { get; set; }

    /// <summary>
    /// Gets or sets the part definition associated with this product.
    /// </summary>
    public required PartDefinition PartDefinition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the list of work instructions associated with the product.
    /// </summary>
    public List<WorkInstruction>? WorkInstructions { get; set; } = [];
}