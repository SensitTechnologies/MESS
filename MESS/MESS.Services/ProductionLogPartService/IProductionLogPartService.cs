using MESS.Data.Models;

namespace MESS.Services.ProductionLogPartService;

/// <summary>
/// Interface for managing part entry during production log creation. Provides methods and events
/// for managing production log parts and product numbers.
/// </summary>
public interface IProductionLogPartService
{
    /// <summary>
    /// Event triggered when the current product number changes.
    /// </summary>
    public event Action? CurrentProductNumberChanged;

    /// <summary>
    /// Gets or sets the current product number.
    /// </summary>
    public string? CurrentProductNumber { get; set; }
    
    /// <summary>
    /// Retrieves the list of <see cref="ProductionLogPart"/>s associated with the specified production log index.
    /// </summary>
    /// <param name="logIndex">The index of the production log in the batch (0-based).</param>
    /// <returns>A list of parts associated with the specified log index. Returns an empty list if none are found.</returns>
    List<ProductionLogPart> GetPartsForLogIndex(int logIndex);

    /// <summary>
    /// Stores the list of <see cref="ProductionLogPart"/>s for the specified production log index.
    /// </summary>
    /// <param name="logIndex">The index of the production log in the batch (0-based).</param>
    /// <param name="parts">The list of parts to associate with the specified log index.</param>
    void SetPartsForLogIndex(int logIndex, List<ProductionLogPart> parts);

    /// <summary>
    /// Persists all <see cref="ProductionLogPart"/> entries that have been associated with cached production logs,
    /// assigning the correct <see cref="ProductionLog.Id"/> to each part based on its log index.
    /// This method should only be called after all <see cref="ProductionLog"/>s have been saved to the database,
    /// and their corresponding IDs are available.
    /// </summary>
    /// <param name="savedLogs">
    /// A list of saved <see cref="ProductionLog"/> instances. The index of each log in this list
    /// must match the index used when calling <see cref="SetPartsForLogIndex(int, List&lt;ProductionLogPart&gt;)"/>.
    /// </param>
    /// <returns>
    /// A task that resolves to <c>true</c> if all parts were saved successfully; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> SaveAllLogPartsAsync(List<ProductionLog> savedLogs);
    
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
    
    /// <summary>
    /// Clears any parts associated with the specified production log index.
    /// </summary>
    /// <param name="logIndex">The index of the production log in the batch (0-based).</param>
    void ClearPartsForLogIndex(int logIndex);
    
    /// <summary>
    /// Occurs when all production log parts are cleared and need to be reloaded.
    /// Components can subscribe to this event to refresh their local state.
    /// </summary>
    public event Action? PartsReloadRequested;

    /// <summary>
    /// Invokes the <see cref="PartsReloadRequested"/> event to notify subscribers
    /// that production log parts should be reloaded.
    /// </summary>
    public void RequestPartsReload();

    /// <summary>
    /// Clears all stored part data for every production log index and triggers a reload event.
    /// Use this when the active work instruction or product context changes.
    /// </summary>
    public void ClearAllLogParts();
}