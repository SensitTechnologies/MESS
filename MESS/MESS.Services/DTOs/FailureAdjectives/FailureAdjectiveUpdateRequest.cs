namespace MESS.Services.DTOs.FailureAdjectives;


/// <summary>
/// Represents a request to update an existing <see cref="MESS.Data.Models.FailureAdjective"/>.
/// </summary>
public class FailureAdjectiveUpdateRequest
{
    /// <summary>
    /// The unique identifier of the adjective to update.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The new name of the adjective, e.g., "Broken" or "Flickering".
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The identifiers of the nouns that this adjective should apply to.
    /// This replaces the existing associations.
    /// </summary>
    public ICollection<int> NounIds { get; set; } = new List<int>();
}
