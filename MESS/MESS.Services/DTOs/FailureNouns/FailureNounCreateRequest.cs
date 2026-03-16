namespace MESS.Services.DTOs.FailureNouns;


/// <summary>
/// Represents a request to create a new <see cref="MESS.Data.Models.FailureNoun"/>.
/// </summary>
public class FailureNounCreateRequest
{
    /// <summary>
    /// The name of the noun, e.g., "Screen" or "Button".
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The identifiers of adjectives that should be associated with this noun.
    /// Optional; can be empty for a noun without any adjectives initially.
    /// </summary>
    public ICollection<int> AdjectiveIds { get; set; } = new List<int>();
}
