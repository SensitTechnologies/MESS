using MESS.Data.Models;

namespace MESS.Services.CRUD.ProductionLogParts;

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
    /// Retrieves the list of <see cref="ProductionLogPart"/> entries associated with a specific 
    /// <see cref="PartNode"/> in a given production log index.
    /// </summary>
    /// <param name="logIndex">The index of the production log in the session.</param>
    /// <param name="partNodeId">The ID of the <see cref="PartNode"/> whose parts should be retrieved.</param>
    /// <returns>A list of <see cref="ProductionLogPart"/> entries, or an empty list if none are found.</returns>
    public List<ProductionLogPart> GetPartsForNode(int logIndex, int partNodeId);

    /// <summary>
    /// Sets the list of <see cref="ProductionLogPart"/> entries associated with a specific <see cref="PartNode"/> 
    /// within a given production log index.
    /// </summary>
    /// <param name="logIndex">The index of the production log in the session.</param>
    /// <param name="partNodeId">The ID of the <see cref="PartNode"/> to associate the parts with.</param>
    /// <param name="parts">The list of <see cref="ProductionLogPart"/> entries to assign to this node.</param>
    public void SetPartsForNode(int logIndex, int partNodeId, List<ProductionLogPart> parts);

    /// <summary>
    /// Saves all in-memory <see cref="ProductionLogPart"/> entries to the database for the given list of saved production logs.
    /// Each part will be assigned the corresponding <see cref="ProductionLog"/> ID before persistence.
    /// </summary>
    /// <param name="savedLogs">The list of production logs that have been saved, mapped by index.</param>
    /// <returns><c>true</c> if all parts were saved successfully; otherwise, <c>false</c>.</returns>
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
    /// Clears all <see cref="ProductionLogPart"/> entries associated with the specified <see cref="PartNode"/> 
    /// in the given production log index.
    /// </summary>
    /// <param name="logIndex">The index of the production log in the session.</param>
    /// <param name="partNodeId">The ID of the <see cref="PartNode"/> to clear parts from.</param>
    public void ClearPartsForNode(int logIndex, int partNodeId);
    
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
    /// Ensures that a <see cref="ProductionLogPart"/> exists for each required <see cref="Part"/>
    /// within the specified log index and part node.
    /// Any missing parts will result in new <see cref="ProductionLogPart"/> entries being added.
    /// </summary>
    /// <param name="logIndex">The index of the production log.</param>
    /// <param name="partNodeId">The ID of the <see cref="PartNode"/> to associate the parts with.</param>
    /// <param name="requiredParts">The list of required <see cref="Part"/> entities.</param>
    public void EnsureRequiredPartsLogged(int logIndex, int partNodeId, List<Part> requiredParts);

    /// <summary>
    /// Clears all in-memory <see cref="ProductionLogPart"/> entries across all production log indexes and part nodes.
    /// Triggers a parts reload notification via <see cref="PartsReloadRequested"/>.
    /// </summary>
    public void ClearAllLogParts();

    /// <summary>
    /// Gets the total number of <see cref="ProductionLogPart"/> entries
    /// currently stored across all logs and nodes in memory.
    /// </summary>
    /// <returns>
    /// The total count of parts stored in the service's in-memory log entries.
    /// </returns>
    public int GetTotalPartsLogged();
}