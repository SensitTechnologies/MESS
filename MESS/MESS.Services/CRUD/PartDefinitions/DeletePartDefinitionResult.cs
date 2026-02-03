using MESS.Data.Models;

namespace MESS.Services.CRUD.PartDefinitions;

/// <summary>
/// Represents the result of attempting to delete a <see cref="PartDefinition"/>.
/// </summary>
public enum DeletePartDefinitionResult
{
    /// <summary>
    /// The part definition was successfully deleted.
    /// </summary>
    Success,

    /// <summary>
    /// The part definition could not be deleted because it is referenced
    /// by one or more dependent entities.
    /// </summary>
    InUse,

    /// <summary>
    /// The part definition could not be found.
    /// </summary>
    NotFound,

    /// <summary>
    /// An unexpected error occurred while attempting to delete the part definition.
    /// </summary>
    Error
}
