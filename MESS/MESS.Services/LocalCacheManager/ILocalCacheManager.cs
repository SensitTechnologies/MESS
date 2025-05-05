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
    /// If the ProductionLog is null, it will clear the cache.
    /// Otherwise, it will set the ProductionLog to the client-side localstorage cache.
    /// </summary>
    /// <param name="productionLog">The current ProductionLog object</param>
    /// <returns></returns>
    public Task SetNewProductionLogFormAsync(ProductionLog? productionLog);
    /// <summary>
    /// Retrieves the current ProductionLog form data.
    /// </summary>
    /// <returns>A <see cref="ProductionLogFormDTO"/> object containing the ProductionLog form data.</returns>
    public Task<ProductionLogFormDTO> GetProductionLogFormAsync();

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