using MESS.Services.DTOs.ProductionLogs.LogSteps.Form;

namespace MESS.Services.DTOs.ProductionLogs.CreateRequest;

/// <summary>
/// Represents a request to create a new production log.
/// This DTO is intended for server-side processing when creating a new log and its associated steps.
/// </summary>
public class ProductionLogCreateRequest
{
    /// <summary>
    /// Gets or sets the identifier of the product associated with this production log.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the work instruction associated with this production log.
    /// </summary>
    public int WorkInstructionId { get; set; }

    /// <summary>
    /// Gets or sets the size of the batch from which this production log was generated.
    /// A value of 1 indicates a single-piece flow.
    /// </summary>
    public int FromBatchOf { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the operator performing this production log.
    /// Typically corresponds to the user ID in the system.
    /// </summary>
    public string? OperatorId { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who created this production log.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the collection of steps associated with this production log.
    /// Each step includes its metadata and any initial attempts.
    /// </summary>
    public List<LogStepFormDTO> LogSteps { get; set; } = [];
}