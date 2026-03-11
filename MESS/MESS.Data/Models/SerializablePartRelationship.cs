namespace MESS.Data.Models;

/// <summary>
/// Represents the current parent-child relationship between serialized parts.
/// Each record indicates that a child part is installed in a parent part.
/// </summary>
public class SerializablePartRelationship
{
    /// <summary>
    /// Gets or sets the unique identifier for this relationship.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the child serialized part in this relationship.
    /// </summary>
    public int ChildPartId { get; set; }

    /// <summary>
    /// Navigation property to the child part.
    /// </summary>
    public SerializablePart? ChildPart { get; set; }

    /// <summary>
    /// Gets or sets the parent serialized part in this relationship.
    /// Null if the child part is not currently installed in any parent.
    /// </summary>
    public int? ParentPartId { get; set; }

    /// <summary>
    /// Navigation property to the parent part.
    /// </summary>
    public SerializablePart? ParentPart { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this relationship was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}