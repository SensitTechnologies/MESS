namespace MESS.Services.SessionManager;
using Data.Models;
/// <summary>
/// Interface for managing session-related operations.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Adds a production log entry asynchronously.
    /// </summary>
    /// <param name="logId">The ID of the production log to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddProductionLogAsync(int logId);

    /// <summary>
    /// Retrieves a list of production log IDs asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a list of production log IDs or null.</returns>
    public Task<List<int>?> GetProductionLogIdsAsync();

    /// <summary>
    /// Clears all production logs asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ClearProductionLogsAsync();
}