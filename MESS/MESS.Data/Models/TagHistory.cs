namespace MESS.Data.Models;

/// <summary>
/// Represents a historical event for a <see cref="Tag"/> in the system.
/// Each record captures a single action or state change for traceability.
/// </summary>
public class TagHistory
{
    /// <summary>
    /// The primary key for this history record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The foreign key to the <see cref="Tag"/> associated with this event.
    /// </summary>
    public int TagId { get; set; }

    /// <summary>
    /// Navigation property to the <see cref="Tag"/> associated with this event.
    /// </summary>
    public Tag Tag { get; set; } = null!;

    /// <summary>
    /// The type of event that occurred, representing the tag's lifecycle action.
    /// </summary>
    public TagEventType EventType { get; set; }

    /// <summary>
    /// The date and time when the event occurred.
    /// Stored as <see cref="DateTimeOffset"/> to preserve time zone information.
    /// Defaults to the current UTC time when the record is created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional foreign key to a <see cref="SerializablePart"/> involved in this event.
    /// Null if the event is not related to a specific part.
    /// </summary>
    public int? SerializablePartId { get; set; }
}

/// <summary>
/// Defines the types of events that can occur for a <see cref="Tag"/>.
/// These events capture the tag's lifecycle and provide traceability.
/// </summary>
public enum TagEventType
{
    /// <summary>
    /// The tag record was created in the system.
    /// </summary>
    Created,

    /// <summary>
    /// A physical label or QR code for the tag was printed.
    /// </summary>
    Printed,

    /// <summary>
    /// The tag was assigned to a <see cref="SerializablePart"/> and is currently in use.
    /// </summary>
    Assigned,

    /// <summary>
    /// The tag was unassigned from a <see cref="SerializablePart"/>, for example during replacement or error correction.
    /// </summary>
    Unassigned,

    /// <summary>
    /// The tag has been permanently removed from use.
    /// This could be due to being lost, damaged, replaced, or otherwise retired.
    /// </summary>
    Retired,
    
    /// <summary>
    /// This tag was deleted before it could ever be assigned or retired.
    /// </summary>
    Deleted
}