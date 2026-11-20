namespace MESS.Services.DTOs.Rework;

/// <summary>
/// Distinct values offered as suggestions in the Rework filter row.
/// </summary>
public sealed class ReworkFilterOptionsDTO
{
    /// <summary>Distinct noun names used on attempts.</summary>
    public List<string> Nouns { get; set; } = [];

    /// <summary>Distinct adjective names used on attempts.</summary>
    public List<string> Adjectives { get; set; } = [];

    /// <summary>Distinct product names used on attempts' logs.</summary>
    public List<string> Products { get; set; } = [];

    /// <summary>Distinct operator display names used on attempts' logs.</summary>
    public List<string> Operators { get; set; } = [];
}
