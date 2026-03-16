namespace MESS.Data.Models;

/// <summary>
/// Represents an adjective describing a type of failure that can occur to one or more nouns (components or parts).
/// </summary>
/// <remarks>
/// Each <see cref="FailureAdjective"/> can be associated with multiple <see cref="FailureNoun"/>s,
/// allowing the system to filter which adjectives are valid based on the selected noun.
/// For example, the adjective "Broken" could apply to both "Screen" and "Button".
/// </remarks>
public class FailureAdjective
{
    /// <summary>
    /// Primary key for the <see cref="FailureAdjective"/>.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the adjective, e.g., "Broken", "Scratched", or "Flickering".
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Collection of nouns that this adjective can apply to.
    /// Used for filtering available adjectives based on the selected noun.
    /// </summary>
    public ICollection<FailureNoun> Nouns { get; set; } = new List<FailureNoun>();
}