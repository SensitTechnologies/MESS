using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Detail;

namespace MESS.Services.DTOs.ProductionLogs.Detail;

/// <summary>
/// Represents a detailed view of a production log, including all step attempts and relevant metadata.
/// </summary>
public class ProductionLogDetailDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the production log.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product associated with this production log.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the work instruction associated with this production log.
    /// </summary>
    public string WorkInstructionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serial number of the product, if applicable.
    /// </summary>
    public string? ProductSerialNumber { get; set; }

    /// <summary>
    /// Gets or sets the batch size from which this production log was created.
    /// A value of 1 indicates a single-piece flow.
    /// </summary>
    public int FromBatchOf { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the production log was created, including time zone offset.
    /// </summary>
    public DateTimeOffset CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the production log was last modified, including time zone offset.
    /// </summary>
    public DateTimeOffset LastModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the production log.
    /// May be <c>null</c> if the log has never been modified after creation.
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets all step attempts in the log, each including associated step metadata.
    /// This collection is flattened for display and editing purposes.
    /// </summary>
    public List<StepAttemptDetailDTO> Attempts { get; set; } = [];
}