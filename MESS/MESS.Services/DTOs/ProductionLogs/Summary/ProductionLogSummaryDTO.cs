namespace MESS.Services.DTOs.ProductionLogs.Summary;

/// <summary>
/// Summary information for a production log.
/// Intended for displaying in list views or table overviews.
/// Contains only the most relevant fields for quick reference.
/// </summary>
public class ProductionLogSummaryDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the production log.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product associated with the production log.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the work instruction associated with the production log.
    /// </summary>
    public string WorkInstructionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the production log was created.
    /// </summary>
    public DateTimeOffset CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the production log was last modified.
    /// </summary>
    public DateTimeOffset LastModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who last modified the production log.
    /// </summary>
    public string? LastModifiedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the user who originally created the production log.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
}