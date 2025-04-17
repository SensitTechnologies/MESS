namespace MESS.Data.Models;

/// <summary>
/// Represents a product entity with properties for identification, name, 
/// active status, and associated work instructions.
/// </summary>
public class Product : AuditableEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product. This field is required.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is active.
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets the list of work instructions associated with the product.
    /// </summary>
    public List<WorkInstruction>? WorkInstructions { get; set; } = [];
}