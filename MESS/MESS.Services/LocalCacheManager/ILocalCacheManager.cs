namespace MESS.Services.BrowserCacheManager;

public interface ILocalCacheManager
{
    public Task SetActiveProductIdAsync(int productId);
    /// <summary>
    /// 
    /// </summary>
    /// <returns>The integer ID value for the currently active Product OR a
    /// negative value if it was not found.
    /// </returns>
    public Task<int> GetActiveProductIdAsync();
    public Task SetActiveWorkInstructionIdAsync(int workInstructionId);
    public Task SetInProgressAsync(bool inProgress);
    public Task GetInProgressAsync();
}