namespace MESS.Data.Models;

/// <summary>
/// Represents a log entry for serial numbers associated with a production process.
/// </summary>
public class SerialNumberLog : AuditableEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the serial number log.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the part serial number.
    /// </summary>
    public string? PartSerialNumber { get; set; }

    /// <summary>
    /// Gets or sets the product serial number.
    /// </summary>
    public string? ProductSerialNumber { get; set; }

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
    public Part? Part { get; set; }
}