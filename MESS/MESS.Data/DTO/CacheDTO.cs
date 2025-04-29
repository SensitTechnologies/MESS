namespace MESS.Data.DTO;

/// <summary>
/// Represents a caching Data Transfer Object.
/// </summary>
public class CacheDTO
{
    /// <summary>
    /// Gets or sets the unique identifier for the cache entry.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the cache entry.
    /// </summary>
    public string Name { get; set; } = "";
}