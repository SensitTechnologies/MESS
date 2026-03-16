using MESS.Data.Models;

namespace MESS.Services.DTOs.Tags;

/// <summary>
/// DTO representing a historical event for a tag.
/// Used in <see cref="TagDTO"/> to display tag lifecycle events.
/// </summary>
/// <summary>
/// DTO representing a historical event for a tag.
/// Used in <see cref="TagDTO"/> to display tag lifecycle events.
/// </summary>
public class TagHistoryDTO
{
    /// <summary>
    /// The type of event that occurred for the tag.
    /// </summary>
    public TagEventType EventType { get; set; }

    /// <summary>
    /// Human-readable string version of the event type for UI display.
    /// </summary>
    public string EventTypeDisplay => EventType.ToString(); // Or map to friendly names

    /// <summary>
    /// The timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Optional serial number of the part involved in this event.
    /// </summary>
    public string? PartSerialNumber { get; set; }

    /// <summary>
    /// Optional name of the part involved in this event.
    /// </summary>
    public string? PartName { get; set; }
    
    /// <summary>
    /// The number of the part involved in this event, if available. This provides additional context about the part
    /// associated with the tag event.
    /// </summary>
    /// <remarks>
    /// This is associated with the <see cref="PartDefinition.Number"/> field from the database.
    /// It may be null if the part number is not available or not applicable for this event.
    /// </remarks>
    public string? PartNumber { get; set; }
    
    /// <summary>
    /// Title of the Work Instruction this tag last went through, if any. This provides context on the most
    /// recent production step related to the tag.
    /// </summary>
    public string? WorkInstructionTitle { get; set; }
}