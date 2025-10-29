namespace MESS.Services.DTOs.PartDefinitions;

/// <summary>
/// Represents the base part definition data shared between parts and products.
/// </summary>
public class PartDefinitionDTO
{
    /// <summary>
    /// Gets or sets the unique identifier for the part definition.
    /// </summary>
    public int PartDefinitionId { get; set; }

    /// <summary>
    /// Gets or sets the part number associated with this definition.
    /// </summary>
    public required string Number { get; set; }

    /// <summary>
    /// Gets or sets the descriptive name of the part.
    /// </summary>
    public required string Name { get; set; }
}
