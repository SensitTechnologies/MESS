namespace MESS.Services.DTOs.FailureAdjectives;


/// <summary>
/// Represents a request to create a new <see cref="MESS.Data.Models.FailureAdjective"/>.
/// </summary>
public class FailureAdjectiveCreateRequest
{
    /// <summary>
    /// The name of the adjective, e.g., "Broken" or "Flickering".
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The identifiers of nouns that this adjective should apply to.
    /// Optional; can be empty for an adjective without any associated nouns initially.
    /// </summary>
    public ICollection<int> NounIds { get; set; } = new List<int>();
}
