using MESS.Services.DTOs.ProductionLogs.LogSteps.Form;
namespace MESS.Services.DTOs.ProductionLogs.Form;

/// <summary>
/// Represents the state of a production log on the form page.
/// Used for client-side caching and for submitting the form to the server.
/// </summary>
public class ProductionLogFormDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the production log.
    /// Will be <c>0</c> if the log has not yet been created in the database.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the product associated with this production log.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the work instruction associated with this production log.
    /// </summary>
    public int WorkInstructionId { get; set; }

    /// <summary>
    /// Gets or sets the serial number of the product, if applicable.
    /// </summary>
    public string? ProductSerialNumber { get; set; }

    /// <summary>
    /// Gets or sets the size of the batch this production log is being created for.
    /// A value of 1 indicates a single-piece flow.
    /// </summary>
    public int FromBatchOf { get; set; }

    /// <summary>
    /// Gets or sets the list of steps for this production log.
    /// Each step includes its associated attempts and any client-side state needed for the form.
    /// </summary>
    public List<LogStepFormDTO> LogSteps { get; set; } = [];
}
