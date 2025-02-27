using MESS.Data.DTO;

namespace MESS.Services.BrowserCacheManager;
using Data.Models;
public interface ILocalCacheManager
{
    public Task SetActiveProductAsync(Product product);
    /// <summary>
    /// 
    /// </summary>
    /// <returns>The integer ID value for the currently active Product OR a
    /// negative value if it was not found.
    /// </returns>
    public Task<ProductCacheDTO> GetActiveProductAsync();

    public Task<int> GetActiveWorkInstructionIdAsync();
    public Task SetActiveWorkInstructionIdAsync(int workInstructionId);
    public Task SetInProgressAsync(bool inProgress);
    public Task<bool> GetInProgressAsync();

    public Task ResetCachedValuesAsync();
}