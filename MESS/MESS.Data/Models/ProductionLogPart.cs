namespace MESS.Data.Models;

/// <summary>
/// Represents a log entry linking a serialized part to a specific production log event,
/// including operations such as production, installation, and removal.
/// </summary>
/// <remarks>
/// This table captures the full lifecycle history of serialized parts. Each record reflects
/// a single event (Produced, Installed, or Removed) during a production log entry.
/// </remarks>
public class ProductionLogPart
{
    /// <summary>
    /// Gets or sets the identifier of the associated production log in which this event occurred.
    /// </summary>
    public int ProductionLogId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the serialized part involved in this event.
    /// </summary>
    public int SerializablePartId { get; set; }

    /// <summary>
    /// Gets or sets the type of operation performed on this part during the production log event.
    /// </summary>
    /// <remarks>
    /// Possible operations include:
    /// <list type="bullet">
    /// <item><see cref="PartOperationType.Produced"/> – A new part was created.</item>
    /// <item><see cref="PartOperationType.Installed"/> – A part was installed into a parent assembly.</item>
    /// <item><see cref="PartOperationType.Removed"/> – A part was removed from a parent assembly.</item>
    /// </list>
    /// </remarks>
    public PartOperationType OperationType { get; set; }

    /// <summary>
    /// Navigation property for the associated production log.
    /// </summary>
    public ProductionLog? ProductionLog { get; set; }

    /// <summary>
    /// Navigation property for the serialized part involved in this event.
    /// </summary>
    public SerializablePart? SerializablePart { get; set; }
}

/// <summary>
/// Defines the type of operation performed on a serialized part within a production log.
/// </summary>
public enum PartOperationType
{
    /// <summary>
    /// The part was installed into a parent assembly.
    /// </summary>
    Installed,
    
    /// <summary>
    /// The part was newly produced or created during the production process.
    /// </summary>
    Produced,

    /// <summary>
    /// The part was removed from a parent assembly.
    /// </summary>
    Removed
}