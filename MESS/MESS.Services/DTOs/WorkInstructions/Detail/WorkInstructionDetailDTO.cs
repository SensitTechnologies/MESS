using MESS.Services.DTOs.Products.Summary;
using MESS.Services.DTOs.WorkInstructions.Nodes.View;

namespace MESS.Services.DTOs.WorkInstructions.Detail;

/// <summary>
/// Represents a detailed view of a work instruction,
/// including associated nodes, products, and produced part information.
/// </summary>
public class WorkInstructionDetailDTO
{
    /// <summary>
    /// Gets or sets the unique identifier for the work instruction.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the work instruction.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the version label of the work instruction.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the ID of the original work instruction for versioning.
    /// </summary>
    public int? OriginalId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the latest version.
    /// </summary>
    public bool IsLatest { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this work instruction is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a QR code should be generated
    /// when this instruction is completed.
    /// </summary>
    public bool ShouldGenerateQrCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the part produced is serialized.
    /// </summary>
    public bool PartProducedIsSerialized { get; set; }

    /// <summary>
    /// Gets or sets the ID of the part produced by this instruction, if any.
    /// </summary>
    public int? PartProducedId { get; set; }

    /// <summary>
    /// Gets or sets the name of the part produced by this instruction, if any.
    /// </summary>
    public string? PartProducedName { get; set; }

    /// <summary>
    /// Gets or sets the part number of the part produced by this instruction, if any.
    /// </summary>
    public string? PartProducedNumber { get; set; }

    /// <summary>
    /// Gets or sets the list of associated products.
    /// </summary>
    public List<ProductSummaryDTO> Products { get; set; } = [];

    /// <summary>
    /// Gets or sets the hierarchical list of nodes (steps, parts, etc.)
    /// belonging to this work instruction.
    /// </summary>
    public List<WorkInstructionNodeViewDTO> Nodes { get; set; } = [];
}