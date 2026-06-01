namespace MESS.Services.DTOs.ProductionLogs.Archive;

/// <summary>
/// Distinct filter option values for Log Archives dropdown filters.
/// </summary>
public sealed class ProductionLogArchiveFilterOptionsDTO
{
    /// <summary>Status values.</summary>
    public List<string> Statuses { get; set; } = [];

    /// <summary>Product values.</summary>
    public List<string> Products { get; set; } = [];

    /// <summary>Work instruction values.</summary>
    public List<string> WorkInstructions { get; set; } = [];

    /// <summary>Part produced values.</summary>
    public List<string> PartProduced { get; set; } = [];

    /// <summary>Created by values.</summary>
    public List<string> CreatedBy { get; set; } = [];

    /// <summary>Modified by values.</summary>
    public List<string> ModifiedBy { get; set; } = [];
}
