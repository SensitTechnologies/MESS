using MESS.Data.DTO;

namespace MESS.Services.BrowserCacheManager;
using Data.Models;

/// <summary>
/// Interface for managing local cache operations, including setting and retrieving
/// production logs, active products, work instructions, and workflow statuses.
/// </summary>
public interface ILocalCacheManager
{
    /// <summary>
    /// Retrieves the current ProductionLog form data.
    /// </summary>
    /// <returns>A <see cref="ProductionLogFormDTO"/> object containing the ProductionLog form data.</returns>
    public Task<ProductionLogFormDTO> GetProductionLogFormAsync();
    
    /// <summary>
    /// Stores a batch of production logs in local cache storage. 
    /// If the provided list is null or empty, the cached logs will be removed.
    /// </summary>
    /// <param name="logs">The list of production logs to cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetProductionLogBatchAsync(List<ProductionLog> logs);

    /// <summary>
    /// Retrieves the cached batch of production log form DTOs.
    /// </summary>
    /// <returns>A task that returns the list of <see cref="ProductionLogFormDTO"/> objects. Returns an empty list if nothing is cached.</returns>
    Task<List<ProductionLogFormDTO>> GetProductionLogBatchAsync();

    /// <summary>
    /// Clears the cached batch of production log forms from local storage.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearProductionLogBatchAsync();

    /// <summary>
    /// Sets the active product in the cache.
    /// </summary>
    /// <param name="product">The <see cref="Product"/> object to set as active.</param>
    public Task SetActiveProductAsync(Product? product);
    /// <summary>
    /// 
    /// </summary>
    /// <returns>The integer ID value for the currently active Product OR a
    /// negative value if it was not found.
    /// </returns>
    public Task<CacheDTO> GetActiveProductAsync();

    /// <summary>
    /// Retrieves the ID of the currently active Work Instruction.
    /// </summary>
    /// <returns>The integer ID of the active Work Instruction, or a negative value if not found.</returns>
    public Task<int> GetActiveWorkInstructionIdAsync();

    /// <summary>
    /// Sets the ID of the active Work Instruction.
    /// </summary>
    /// <param name="workInstructionId">The ID of the Work Instruction to set as active.</param>
    public Task SetActiveWorkInstructionIdAsync(int workInstructionId);

    /// <summary>
    /// Sets the workflow active status.
    /// </summary>
    /// <param name="isActive">A boolean indicating whether the workflow is active.</param>
    public Task SetIsWorkflowActiveAsync(bool isActive);

    /// <summary>
    /// Retrieves the current workflow active status.
    /// </summary>
    /// <returns>A boolean indicating whether the workflow is active.</returns>
    public Task<bool> GetWorkflowActiveStatusAsync();

    /// <summary>
    /// Resets all cached values to their default state.
    /// </summary>
    public Task ResetCachedValuesAsync();
}