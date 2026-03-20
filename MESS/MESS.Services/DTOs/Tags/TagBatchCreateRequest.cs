using MESS.Services.CRUD.Tags;

namespace MESS.Services.DTOs.Tags;

/// <summary>
/// Represents a request to generate and create a batch of tags using a specified numbering scheme.
/// This request defines how tag codes should be generated, including prefix, range, and formatting options.
/// </summary>
public class TagBatchCreateRequest
{
    /// <summary>
    /// The numbering scheme used to generate tag codes (e.g., Decimal, Hexadecimal, Alphanumeric).
    /// </summary>
    public TagNumberingScheme Scheme { get; set; }

    /// <summary>
    /// Optional prefix applied to each generated tag code.
    /// For example, "TAG-" would produce codes like "TAG-000001".
    /// If null or empty, no prefix is applied.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// The total number of tags to generate.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// The number of digits to pad numeric values with when using the Decimal scheme.
    /// For example, a padding of 6 produces codes like "000001".
    /// Defaults to 6.
    /// </summary>
    public int Padding { get; set; } = 6;
}