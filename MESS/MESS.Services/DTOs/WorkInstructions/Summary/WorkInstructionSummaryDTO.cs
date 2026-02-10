using MESS.Services.DTOs.Products.Summary;

namespace MESS.Services.DTOs.WorkInstructions.Summary;

/// <summary>
/// Represents a lightweight summary of a work instruction,
/// suitable for lists, dropdowns, and product association displays.
/// </summary>
public class WorkInstructionSummaryDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the work instruction.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the work instruction.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the version string of the work instruction (e.g., "v1.2" or "Rev B").
    /// May be <c>null</c> if not versioned.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the original work instruction
    /// that this instruction was derived from, if applicable.
    /// </summary>
    public int? OriginalId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the latest version
    /// in its version chain.
    /// </summary>
    public bool IsLatest { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this work instruction is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the ID of the part produced by this work instruction, if any.
    /// </summary>
    public int? PartProducedId { get; set; }

    /// <summary>
    /// Gets or sets the name of the part produced by this work instruction, if available.
    /// </summary>
    public string? PartProducedName { get; set; }

    /// <summary>
    /// Gets or sets the part number of the part produced by this work instruction, if available.
    /// </summary>
    public string? PartProducedNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the list of associated products in summary form.
    /// </summary>
    public List<ProductSummaryDTO> Products { get; set; } = new();
}