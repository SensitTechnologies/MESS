using MESS.Data.Models;

namespace MESS.Services.Serialization;

/// <summary>
/// Interface for serialization services, providing methods and events
/// for managing serial number logs and product numbers.
/// </summary>
public interface ISerializationService
{
    /// <summary>
    /// Event triggered when the current serial number log changes.
    /// </summary>
    public event Action? CurrentSerialNumberLogChanged;

    /// <summary>
    /// Event triggered when the current product number changes.
    /// </summary>
    public event Action? CurrentProductNumberChanged;

    /// <summary>
    /// Gets or sets the current product number.
    /// </summary>
    public string? CurrentProductNumber { get; set; }
    /// <summary>
    /// A List of the Current Serial Number Logs for a given Work Instruction Request.
    /// Defaults to an empty List
    /// </summary>
    public List<SerialNumberLog> CurrentSerialNumberLogs { get; set; }
    /// <summary>
    /// Modifies the CurrentSerialNumberLogs list to include the associated ProductionLogIds,
    /// and then saves the range.
    /// </summary>
    /// <param name="productionLogId">int value for the associated ProductionLog</param>
    /// <returns>True value if the operation succeeded, false otherwise</returns>
    public Task<bool> SaveCurrentSerialNumberLogsAsync(int productionLogId);
    /// <summary>
    /// Retrieves all SerialNumberLogs Async
    /// </summary>
    /// <returns>Nullable List of SerialNumberLogs</returns>
    public Task<List<SerialNumberLog>?> GetAllAsync();
    /// <summary>
    /// Creates and saves a new SerialNumberLog in the database.
    /// </summary>
    /// <param name="serialNumberLog">The SerialNumberLog object to be saved in the database</param>
    /// <returns>True value if the operation succeeded, false otherwise</returns>
    public Task<bool> CreateAsync(SerialNumberLog serialNumberLog);
    /// <summary>
    /// Creates and saves a List of SerialNumberLogs in the database.
    /// </summary>
    /// <param name="serialNumberLogs"></param>
    /// <returns>True value if the operation succeeded, false otherwise</returns>
    public Task<bool> CreateRangeAsync(List<SerialNumberLog> serialNumberLogs);
    /// <summary>
    /// Updates the saved SerialNumberLog in the database.
    /// </summary>
    /// <param name="serialNumberLog"></param>
    /// <returns>True value if the operation succeeded, false otherwise</returns>
    public Task<bool> UpdateAsync(SerialNumberLog serialNumberLog);
    /// <summary>
    /// Deletes the associated SerialNumberLog with the input integer ID
    /// </summary>
    /// <param name="serialNumberLogId"></param>
    /// <returns>True value if the operation succeeded, false otherwise</returns>
    public Task<bool> DeleteAsync(int serialNumberLogId);
}