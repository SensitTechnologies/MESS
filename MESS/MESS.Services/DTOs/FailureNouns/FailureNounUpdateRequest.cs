namespace MESS.Services.DTOs.FailureNouns;

/// <summary>
/// Represents a request to update an existing <see cref="MESS.Data.Models.FailureNoun"/>.
/// </summary>
public class FailureNounUpdateRequest
{
    /// <summary>
    /// The unique identifier of the noun to update.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The new name of the noun, e.g., "Screen" or "Button".
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The identifiers of the adjectives that should be associated with this noun.
    /// This replaces the existing associations.
    /// </summary>
    public ICollection<int> AdjectiveIds { get; set; } = new List<int>();
}
