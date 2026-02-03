using MESS.Services.CRUD.PartDefinitions;

namespace MESS.Services.DTOs.PartDefinitions;

/// <summary>
/// Represents the result of an attempt to delete a <see cref="Data.Models.PartDefinition"/>.
/// </summary>
/// <remarks>
/// This response provides both the high-level outcome of the delete operation and,
/// when applicable, detailed information about why the deletion could not be completed.
/// </remarks>
public sealed class DeletePartDefinitionResponse
{
    /// <summary>
    /// Gets the outcome of the delete operation.
    /// </summary>
    public DeletePartDefinitionResult Result { get; init; }

    /// <summary>
    /// Gets a collection describing where the part definition is currently used.
    /// </summary>
    /// <remarks>
    /// This property is populated only when <see cref="Result"/> is
    /// <see cref="DeletePartDefinitionResult.InUse"/>. Each entry identifies a work instruction
    /// and specific part node position that references the part definition.
    /// </remarks>
    public IReadOnlyList<PartDefinitionUsage> Usages { get; init; }
        = Array.Empty<PartDefinitionUsage>();
}