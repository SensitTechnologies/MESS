using MESS.Services.DTOs.WorkInstructions.Nodes.Form;

namespace MESS.Services.DTOs.WorkInstructions.Form;

/// <summary>
/// Represents the editable fields of a work instruction used when
/// creating or updating a record from the Blazor UI.
/// </summary>
public class WorkInstructionFormDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the work instruction.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the work instruction.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the version string.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the ID of the original work instruction in the version chain.
    /// </summary>
    public int? OriginalId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the latest version.
    /// </summary>
    public bool IsLatest { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the instruction is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets whether a QR code should be generated when completed.
    /// </summary>
    public bool ShouldGenerateQrCode { get; set; }

    /// <summary>
    /// Gets or sets whether the part produced is serialized.
    /// </summary>
    public bool PartProducedIsSerialized { get; set; }

    /// <summary>
    /// Gets or sets the ID of the part produced, if any.
    /// </summary>
    public int? PartProducedId { get; set; }

    /// <summary>
    /// Gets or sets the IDs of the products associated with this work instruction.
    /// </summary>
    public List<int> ProductIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the editable list of nodes in the instruction.
    /// </summary>
    public List<WorkInstructionNodeFormDTO> Nodes { get; set; } = [];
}
