namespace MESS.Services.CRUD.Tags;

/// <summary>
/// Defines the numbering schemes available for generating tag codes.
/// These schemes determine how sequential values are formatted when creating tags.
/// </summary>
public enum TagNumberingScheme
{
    /// <summary>
    /// Generates tag codes using standard decimal numbering (base-10).
    /// Typically used with zero-padding for consistent formatting (e.g., 000001, 000002).
    /// </summary>
    Decimal,

    /// <summary>
    /// Generates tag codes using hexadecimal numbering (base-16).
    /// Values are represented using digits 0–9 and letters A–F (e.g., 1A, 2F).
    /// </summary>
    Hexadecimal,

    /// <summary>
    /// Generates tag codes using alphabetical sequences.
    /// Values progress as A, B, ..., Z, AA, AB, etc., similar to spreadsheet column naming.
    /// </summary>
    Alphanumeric
}