using MESS.Data.Models;

namespace MESS.Services.DTOs.Tags;

/// <summary>
/// Data Transfer Object representing a Tag and its associated information for display purposes.
/// Includes details about the tag itself and the serializable part it is assigned to.
/// </summary>
public class TagDTO
{
    /// <summary>
    /// The internal unique identifier for the tag.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The human-readable code of the tag (e.g., QR code or barcode value).
    /// Example: TAG-000123
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// The current status of the tag, indicating whether it is available, assigned, or retired.
    /// </summary>
    public TagStatus Status { get; set; }

    /// <summary>
    /// Human-readable string version of the status for UI display.
    /// </summary>
    public string StatusDisplay => Status.ToString(); // Or a mapping to friendly names

    /// <summary>
    /// The timestamp when the tag was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The ID of the serializable part currently assigned to this tag, if any.
    /// </summary>
    public int? SerializablePartId { get; set; }

    /// <summary>
    /// Serial number of the part assigned to this tag, if any.
    /// </summary>
    public string? PartSerialNumber { get; set; }

    /// <summary>
    /// Name of the part associated with the tag, if any.
    /// </summary>
    public string? PartName { get; set; }

    /// <summary>
    /// Title of the Work Instruction this tag last went through, if any. This provides context on the most
    /// recent production step related to the tag.
    /// </summary>
    public string? WorkInstructionTitle { get; set; }
}