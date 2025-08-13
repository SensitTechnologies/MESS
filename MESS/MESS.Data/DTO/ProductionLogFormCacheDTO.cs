namespace MESS.Data.DTO;

/// <summary>
/// Represents a data transfer object for a production log form.
/// </summary>
public class ProductionLogFormCacheDTO
{
    /// <summary>
    /// Gets or sets the unique identifier for the production log.
    /// </summary>
    public int ProductionLogId { get; set; }

    /// <summary>
    /// Gets or sets the list of steps associated with the production log.
    /// </summary>
    public List<ProductionLogStepDTO> LogSteps { get; set; } = [];
}