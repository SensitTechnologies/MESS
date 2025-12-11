using MESS.Services.DTOs.ProductionLogs.LogSteps.UpdateRequest;

namespace MESS.Services.DTOs.ProductionLogs.UpdateRequest;

/// <summary>
/// Represents a request to update an existing production log.
/// Contains the fields that can be modified after creation,
/// including updated step and attempt data.
/// </summary>
public class ProductionLogUpdateRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the production log to update.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the size of the batch from which this production log was created.
    /// </summary>
    public int FromBatchOf { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of step data for this production log.
    /// Each step can contain multiple attempts and their updated outcomes.
    /// </summary>
    public List<LogStepUpdateRequest> LogSteps { get; set; } = [];
}