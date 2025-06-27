using MESS.Data.Models;

namespace MESS.Services.Serialization;

/// <summary>
/// Interface for serialization services, providing methods and events
/// for managing production log parts and product numbers.
/// </summary>
public interface ISerializationService
{
    /// <summary>
    /// Event triggered when the current production log part changes.
    /// </summary>
    public event Action? CurrentProductionLogPartChanged;

    /// <summary>
    /// Event triggered when the current product number changes.
    /// </summary>
    public event Action? CurrentProductNumberChanged;

    /// <summary>
    /// Gets or sets the current product number.
    /// </summary>
    public string? CurrentProductNumber { get; set; }
    /// <summary>
    /// A List of the Current Production Log Parts for a given Work Instruction Request.
    /// Defaults to an empty List
    /// </summary>
    public List<ProductionLogPart> CurrentProductionLogParts { get; set; }

    /// <summary>
    /// Modifies the CurrentProductionLogParts list to include the associated ProductionLogIds,
    /// and then saves the range.
    /// </summary>
    /// <param name="productionLogId">int value for the associated ProductionLog</param>
    /// <returns>True value if the operation succeeded, false otherwise</returns>
    public Task<bool> SaveCurrentProductionLogPartsAsync(int productionLogId);
    /// <summary>
    /// Retrieves all SerialNumberLogs Async
    /// </summary>
    /// <returns>Nullable List of SerialNumberLogs</returns>
    public Task<List<ProductionLogPart>?> GetAllAsync();
    /// <summary>
    /// Creates and saves a new ProductionLogPart in the database.
    /// </summary>
    /// <param name="productionLogPart">The ProductionLogPart object to be saved in the database</param>
    /// <returns>True value if the operation succeeded, false otherwise</returns>
    public Task<bool> CreateAsync(ProductionLogPart productionLogPart);

    /// <summary>
    /// Creates and saves a List of SerialNumberLogs in the database.
    /// </summary>
    /// <param name="productionLogParts"></param>
    /// <returns>True value if the operation succeeded, false otherwise</returns>
    public Task<bool> CreateRangeAsync(List<ProductionLogPart> productionLogParts);
    /// <summary>
    /// Updates the saved ProductionLogPart in the database.
    /// </summary>
    /// <param name="productionLogPart"></param>
    /// <returns>True value if the operation succeeded, false otherwise</returns>
    public Task<bool> UpdateAsync(ProductionLogPart productionLogPart);
    /// <summary>
    /// Deletes the associated ProductionLogPart with the input integer ID
    /// </summary>
    /// <param name="serialNumberLogId"></param>
    /// <returns>True value if the operation succeeded, false otherwise</returns>
    public Task<bool> DeleteAsync(int serialNumberLogId);
}