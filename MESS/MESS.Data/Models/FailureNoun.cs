namespace MESS.Data.Models;

/// <summary>
/// Represents a noun (component or part) that can experience failures.
/// </summary>
/// <remarks>
/// Each <see cref="FailureNoun"/> can have multiple associated <see cref="FailureAdjective"/>s,
/// allowing operators to select valid failure types for the noun.
/// For example, the noun "Screen" could have adjectives like "Broken" or "Flickering".
/// </remarks>
public class FailureNoun
{
    /// <summary>
    /// Primary key for the <see cref="FailureNoun"/>.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the noun, e.g., "Screen", "Button", or "Motor".
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Collection of adjectives that can describe failures for this noun.
    /// Used for filtering available adjectives when a noun is selected.
    /// </summary>
    public ICollection<FailureAdjective> Adjectives { get; set; } = new List<FailureAdjective>();

    /// <summary>
    /// Work instructions that expose this noun as a selectable defect location.
    /// </summary>
    public ICollection<WorkInstruction> WorkInstructions { get; set; } = new List<WorkInstruction>();
}