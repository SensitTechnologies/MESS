namespace MESS.Data.Models;

/// <summary>
/// Represents a part entity with an ID, part number, and part name.
/// </summary>
public class Part
{
    /// <summary>
    /// Gets or sets the unique identifier for the part.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the part number. This field is required.
    /// </summary>
    public required string PartNumber { get; set; }

    /// <summary>
    /// Gets or sets the name of the part. This field is required.
    /// </summary>
    public required string PartName { get; set; }
}