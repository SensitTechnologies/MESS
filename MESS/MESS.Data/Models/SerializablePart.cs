namespace MESS.Data.Models;

/// <summary>
/// Represents an individual serialized part instance derived from a part definition.
/// Serialized parts can be nested within one another to represent assemblies.
/// </summary>
public class SerializablePart
{
    /// <summary>
    /// Gets or sets the unique identifier for the serialized part.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the base part definition.
    /// </summary>
    public int PartId { get; set; }

    /// <summary>
    /// Gets or sets the base part definition.
    /// </summary>
    public PartDefinition? Part { get; set; }

    /// <summary>
    /// Gets or sets the serial number identifying this particular instance.
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Gets or sets the collection of production log associations for this serialized part.
    /// </summary>
    public List<ProductionLogPart> ProductionLogParts { get; set; } = [];

    /// <summary>
    /// Gets or sets the identifier of the parent serialized part, if this part is a subcomponent.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the parent serialized part that this part belongs to, if any.
    /// </summary>
    public SerializablePart? Parent { get; set; }

    /// <summary>
    /// Gets or sets the collection of child serialized parts that are assembled into this part.
    /// </summary>
    public List<SerializablePart> Children { get; set; } = [];
}