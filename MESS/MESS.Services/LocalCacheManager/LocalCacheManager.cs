using MESS.Data.DTO;
using MESS.Data.DTO.ProductionLogDTOs;
using MESS.Data.DTO.ProductionLogDTOs.LogSteps;
using MESS.Data.DTO.ProductionLogDTOs.LogSteps.Attempts;
using MESS.Services.BrowserCacheManager;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Serilog;

namespace MESS.Services.LocalCacheManager;

using Data.Models;
/// <inheritdoc />
public class LocalCacheManager : ILocalCacheManager
{
    private const string IS_WORKFLOW_ACTIVE_KEY = "IS_WORKFLOW_ACTIVE";
    private const string ACTIVE_WORK_INSTRUCTION_KEY = "LAST_KNOWN_WORK_INSTRUCTION_ID";
    private const string ACTIVE_PRODUCT_KEY = "LAST_KNOWN_ACTIVE_PRODUCT";
    private const string PRODUCTION_LOG_FORM_KEY = "PRODUCTION_LOG_FORM_PROGRESS";
    private const string ACTIVE_WORK_STATION_KEY = "LAST_KNOWN_WORK_STATION";
    private const string PRODUCTION_LOG_BATCH_KEY = "PRODUCTION_LOG_BATCH";
    private const string BATCH_SIZE_KEY = "PRODUCTION_LOG_BATCH_SIZE";

    private readonly ProtectedLocalStorage _protectedLocalStorage;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalCacheManager"/> class.
    /// </summary>
    /// <param name="protectedLocalStorage">
    /// An instance of <see cref="ProtectedLocalStorage"/> used for managing local storage operations.
    /// </param>
    public LocalCacheManager(ProtectedLocalStorage protectedLocalStorage)
    {
        _protectedLocalStorage = protectedLocalStorage;
    }
    
    /// <inheritdoc />
    public async Task<List<ProductionLogFormCacheDTO>> GetProductionLogBatchAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<List<ProductionLogFormCacheDTO>>(PRODUCTION_LOG_BATCH_KEY);

            if (result.Success && result.Value != null)
            {
                Log.Information("Retrieved {Count} logs from ProductionLogBatch cache", result.Value.Count);
                return result.Value;
            }

            return new List<ProductionLogFormCacheDTO>();
        }
        catch (Exception ex)
        {
            Log.Warning("Error while retrieving ProductionLogBatchAsync: {Exception}", ex);
            return new List<ProductionLogFormCacheDTO>();
        }
    }
    
    /// <inheritdoc />
    public async Task SetProductionLogBatchAsync(List<ProductionLog> logs)
    {
        try
        {
            if (logs == null || logs.Count == 0)
            {
                await _protectedLocalStorage.DeleteAsync(PRODUCTION_LOG_BATCH_KEY);
                return;
            }

            var dtoList = logs.Select(MapProductionLogToDto).ToList();
            
            await _protectedLocalStorage.SetAsync(PRODUCTION_LOG_BATCH_KEY, dtoList);
        }
        catch (Exception ex)
        {
            Log.Error("Error while setting ProductionLogBatchAsync: {Exception}", ex);
        }
    }
    
    /// <inheritdoc />
    public async Task ClearProductionLogBatchAsync()
    {
        try
        {
            await _protectedLocalStorage.DeleteAsync(PRODUCTION_LOG_BATCH_KEY);
            Log.Information("Cleared ProductionLogBatch from local cache");
        }
        catch (Exception ex)
        {
            Log.Warning("Error while clearing ProductionLogBatchAsync: {Exception}", ex);
        }
    }
    
    private static ProductionLogFormCacheDTO MapProductionLogToDto(ProductionLog productionLog)
    {
        var productionLogFormDto = new ProductionLogFormCacheDTO();
    
        foreach (var step in productionLog.LogSteps)
        {
            var stepDto = new LogStepCacheDTO
            {
                WorkInstructionStepId = step.WorkInstructionStepId,
                ProductionLogId = step.ProductionLogId,
                // Map all attempts
                Attempts = step.Attempts.Select(a => new StepAttemptCacheDTO
                {
                    Success = a.Success,
                    SubmitTime = a.SubmitTime,
                    FailureNote = a.Notes,
                }).ToList()
            };

            productionLogFormDto.LogSteps.Add(stepDto);
        }

        productionLogFormDto.ProductionLogId = productionLog.Id;
        return productionLogFormDto;
    }

    /// <inheritdoc />
    public async Task<ProductionLogFormCacheDTO> GetProductionLogFormAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<ProductionLogFormCacheDTO>(PRODUCTION_LOG_FORM_KEY);

            if (result is { Success: true, Value: not null })
            {
                return result.Value;
            }

            Log.Information("Successfully retrieved ProductionLogFormAsync from local cache");
            return new ProductionLogFormCacheDTO();
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to GetProductionLogFormAsync: {Exception}", e.ToString());
            return new ProductionLogFormCacheDTO();
        }
    }

    /// <inheritdoc />
    public async Task SetActiveProductAsync(Product? product)
    {
        try
        {
            if (product is null)
            {
                await _protectedLocalStorage.DeleteAsync(ACTIVE_PRODUCT_KEY);
                return;
            }
            // map to DTO
            var productDto = new CacheDTO
            {
                Id = product.Id.ToString(),
                Name = product.Name
            };
            
            Log.Information("Successfully SetActiveProductAsync with ID: {ProductID}", product.Id);
            await _protectedLocalStorage.SetAsync(ACTIVE_PRODUCT_KEY, productDto);
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to SetActiveProductAsync: {Exception}", e.ToString());
        }
    }

    /// <inheritdoc />
    public async Task<CacheDTO> GetActiveProductAsync()
    {
        try
        {
            var productCache = await _protectedLocalStorage.GetAsync<CacheDTO>(ACTIVE_PRODUCT_KEY);
            if (productCache is { Success: true, Value: not null })
            {
                Log.Information("Successfully SetActiveProductAsync with ID: {ProductID}", productCache.Value.Id);
                return productCache.Value;
            }
            
            return new CacheDTO();
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to GetActiveProductAsync: {Exception}", e.ToString());
            return new CacheDTO();
        }
    }

    /// <inheritdoc />
    public async Task<int> GetActiveWorkInstructionIdAsync()
    {
        try
        {
            var workInstructionCache = await _protectedLocalStorage.GetAsync<int>(ACTIVE_WORK_INSTRUCTION_KEY);

            if (workInstructionCache.Success)
            {
                Log.Information("Successfully Retrieved WorkInstructionIDAsync, ID: {WIID} from Local Cache", workInstructionCache.Value);
                return workInstructionCache.Value;
            }

            return -1;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to GetActiveWorkInstructionIdAsync: {Exception}", e.ToString());
            return -1;
        }
    }
    
    /// <inheritdoc />
    public async Task SetActiveWorkInstructionIdAsync(int workInstructionId)
    {
        try
        {
            await _protectedLocalStorage.SetAsync(ACTIVE_WORK_INSTRUCTION_KEY, workInstructionId);
            Log.Information("Successfully SetWorkInstructionIdAsync, ID: {WIID} to the Local Cache", workInstructionId);
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to SetActiveWorkInstructionIdAsync: {Exception}", e.ToString());
        }
    }
    

    /// <inheritdoc />
    public async Task SetIsWorkflowActiveAsync(bool isActive)
    {
        try
        {
            await _protectedLocalStorage.SetAsync(IS_WORKFLOW_ACTIVE_KEY, isActive);
            Log.Information("Successfully SetIsWorkflowActiveAsync, Value: {IsWorkFlowActiveValue} to the Local Cache", isActive);
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to SetIsWorkflowActiveAsync: {Exception}", e.ToString());
        }
    }

    /// <inheritdoc />
    public async Task<bool> GetWorkflowActiveStatusAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<bool>(IS_WORKFLOW_ACTIVE_KEY);

            if (!result.Success)
            {
                await SetIsWorkflowActiveAsync(false);
            }

            return result.Value;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to GetWorkflowActiveStatusAsync: {Exception}", e.ToString());
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<int> GetBatchSizeAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<int>(BATCH_SIZE_KEY);
            if (result.Success) return result.Value;
        }
        catch (Exception ex)
        {
            Log.Warning("Error retrieving batch size: {Exception}", ex);
        }

        return 1; // default
    }

    /// <inheritdoc />
    public async Task SetBatchSizeAsync(int size)
    {
        try
        {
            await _protectedLocalStorage.SetAsync(BATCH_SIZE_KEY, size);
            Log.Information("Saved batch size: {Size}", size);
        }
        catch (Exception ex)
        {
            Log.Warning("Error setting batch size: {Exception}", ex);
        }
    }

    /// <inheritdoc />
    public async Task ResetCachedValuesAsync()
    {
        try
        {
            await _protectedLocalStorage.DeleteAsync(ACTIVE_WORK_STATION_KEY);
            await _protectedLocalStorage.DeleteAsync(ACTIVE_PRODUCT_KEY);
            await _protectedLocalStorage.DeleteAsync(ACTIVE_WORK_INSTRUCTION_KEY);
            await _protectedLocalStorage.DeleteAsync(IS_WORKFLOW_ACTIVE_KEY);
            Log.Information("Successfully ResetCachedValuesAsync for the Local Cache");
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to ResetCachedValuesAsync: {Exception}", e.ToString());
        }
    }
    
}