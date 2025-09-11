namespace MESS.Services.DTOs.ProductionLogs.Batch;

/// <summary>
/// Represents the result of a batch operation that creates or updates multiple production logs.
/// </summary>
/// <remarks>
/// This object is used to summarize the outcome of a batch save or update, including
/// the number of logs processed, which were created versus updated, and any error information.
/// It is intended for use by service methods and returned to callers (e.g., UI or API) 
/// to provide feedback on the operation.
/// </remarks>
public class ProductionLogBatchResult
{
    /// <summary>
    /// Gets or sets the total number of logs included in the batch.
    /// </summary>
    public int TotalLogs { get; set; }

    /// <summary>
    /// Gets or sets the number of logs that were successfully created.
    /// </summary>
    public int CreatedCount { get; set; }

    /// <summary>
    /// Gets or sets the number of logs that were successfully updated.
    /// </summary>
    public int UpdatedCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entire batch operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets an optional error message if the batch operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the list of IDs for logs that were created during the batch operation.
    /// </summary>
    public List<int> CreatedIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of IDs for logs that were updated during the batch operation.
    /// </summary>
    public List<int> UpdatedIds { get; set; } = new();
}
