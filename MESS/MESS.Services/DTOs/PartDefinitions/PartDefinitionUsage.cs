namespace MESS.Services.DTOs.PartDefinitions;

/// <summary>
/// Represents a single usage of a <see cref="Data.Models.PartDefinition"/> within a work instruction.
/// </summary>
/// <remarks>
/// This DTO is used when an operation (such as deletion) needs to report where a part definition
/// is currently referenced, including the specific work instruction and node position in which
/// it appears.
/// </remarks>
public sealed class PartDefinitionUsage
{
    /// <summary>
    /// Gets the identifier of the work instruction that references the part definition.
    /// </summary>
    public int WorkInstructionId { get; init; }

    /// <summary>
    /// Gets the display title of the work instruction that references the part definition.
    /// </summary>
    public string WorkInstructionTitle { get; init; } = string.Empty;

    /// <summary>
    /// Gets the identifier of the part node that references the part definition.
    /// </summary>
    public int PartNodeId { get; init; }

    /// <summary>
    /// Gets the positional index of the part node within the work instruction.
    /// </summary>
    /// <remarks>
    /// This value represents the order or sequence of the node as defined by the work instruction,
    /// and can be used to locate the reference within the instruction's structure.
    /// </remarks>
    public int NodePosition { get; init; }
}