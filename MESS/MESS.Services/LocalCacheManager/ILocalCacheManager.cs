using MESS.Data.DTO;

namespace MESS.Services.BrowserCacheManager;
using Data.Models;
public interface ILocalCacheManager
{
    /// <summary>
    /// If the ProductionLog is null, it will clear the cache.
    /// Otherwise, it will set the ProductionLog to the client-side localstorage cache.
    /// </summary>
    /// <param name="productionLog">The current ProductionLog object</param>
    /// <returns></returns>
    public Task SetNewProductionLogFormAsync(ProductionLog? productionLog);
    public Task<ProductionLogFormDTO> GetProductionLogFormAsync();
    public Task SetActiveProductAsync(Product product);
    /// <summary>
    /// 
    /// </summary>
    /// <returns>The integer ID value for the currently active Product OR a
    /// negative value if it was not found.
    /// </returns>
    public Task<CacheDTO> GetActiveProductAsync();

    public Task<int> GetActiveWorkInstructionIdAsync();
    public Task<CacheDTO> GetActiveWorkStationAsync();
    public Task SetActiveWorkStationAsync(WorkStation workStation);
    public Task SetActiveWorkInstructionIdAsync(int workInstructionId);
    public Task SetIsWorkflowActiveAsync(bool isActive);
    public Task<bool> GetWorkflowActiveStatusAsync();

    public Task ResetCachedValuesAsync();
}