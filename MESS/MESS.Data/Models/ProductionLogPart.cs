namespace MESS.Data.Models;

/// <summary>
/// Represents a log entry for part serial numbers associated with a production process.
/// </summary>
public class ProductionLogPart
{
    /// <summary>
    /// Gets or sets the unique identifier for the production log part.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the part serial number.
    /// </summary>
    public string? PartSerialNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the associated production log.
    /// </summary>
    public int ProductionLogId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the quality control submission occurred.
    /// </summary>
    public DateTimeOffset SubmitTimeQc { get; set; }

    /// <summary>
    /// Gets or sets the associated part entity.
    /// </summary>
    public PartDefinition? Part { get; set; }
    
    // NEW (nullable) - will be backfilled in later migration
    // These are nullable for backfilling
    /// <summary>
    /// The ID of the serializable associated with this production log part.
    /// </summary>
    public int? SerializablePartId { get; set; }
    
    /// <summary>
    /// The serializable part associated with this production log part.
    /// </summary>
    public SerializablePart? SerializablePart { get; set; }
}