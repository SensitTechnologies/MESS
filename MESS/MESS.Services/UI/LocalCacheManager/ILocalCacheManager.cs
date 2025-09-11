using MESS.Services.DTOs;
using MESS.Services.DTOs.ProductionLogs.Cache;
using MESS.Services.DTOs.ProductionLogs.Form;

namespace MESS.Services.UI.LocalCacheManager;
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
    public Task<ProductionLogCacheDTO> GetProductionLogFormAsync();
    
    /// <summary>
    /// Persists a batch of production logs into local storage, replacing any previously cached logs.
    /// </summary>
    /// <param name="logs">
    /// The collection of production log form DTOs to cache.  
    /// If <c>null</c> or empty, any existing batch will be removed from storage.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetProductionLogBatchAsync(List<ProductionLogFormDTO>? logs);

    /// <summary>
    /// Retrieves the cached batch of production log form DTOs.
    /// </summary>
    /// <returns>A task that returns the list of <see cref="ProductionLogCacheDTO"/> objects. Returns an empty list if nothing is cached.</returns>
    Task<List<ProductionLogCacheDTO>> GetProductionLogBatchAsync();

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
    
    /// <summary>
    /// Retrieves the last cached batch size from local storage.
    /// </summary>
    /// <returns>
    /// An <see cref="int"/> representing the last known batch size. 
    /// If no batch size is found or an error occurs, this returns a default value of <c>1</c>.
    /// </returns>
    public Task<int> GetBatchSizeAsync();

    /// <summary>
    /// Stores the specified batch size in local storage for future retrieval.
    /// </summary>
    /// <param name="size">
    /// The <see cref="int"/> value representing the desired batch size to cache.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public Task SetBatchSizeAsync(int size);
}