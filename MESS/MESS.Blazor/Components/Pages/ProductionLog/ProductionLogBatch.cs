using System.ComponentModel.DataAnnotations;

namespace MESS.Blazor.Components.Pages.ProductionLog;

/// <summary>
/// Represents a temporary collection of production logs used for batch mode.
/// Not persisted to the database.
/// </summary>
public class ProductionLogBatch
{
    /// <summary>
    /// Gets or sets the list of production logs in the batch.
    /// </summary>
    [Required]
    public List<Data.Models.ProductionLog> Logs { get; set; } = new();
}