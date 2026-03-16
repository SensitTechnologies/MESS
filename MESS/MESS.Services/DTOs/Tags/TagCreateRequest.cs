namespace MESS.Services.DTOs.Tags;

/// <summary>
/// A data transfer object specifically designed for creating a single reusable tag.
/// </summary>
public class TagCreateRequest
{
    /// <summary>
    /// Human readable tag code (QR or barcode value).
    /// Example: TAG-000123
    /// </summary>
    public string Code { get; set; } = null!;
}