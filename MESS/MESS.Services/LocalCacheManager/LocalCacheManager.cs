using MESS.Data.DTO;
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
    public async Task SetNewProductionLogFormAsync(ProductionLog? productionLog)
    {
        try
        {
            if (productionLog == null)
            {
                await _protectedLocalStorage.DeleteAsync(PRODUCTION_LOG_FORM_KEY);
                return;
            }

            Log.Information("Successfully Set New Production Log Form with ID: {PLogID}", productionLog.Id);
            var productionLogForm = MapProductionLogToDto(productionLog);
            
            try
            {
                await _protectedLocalStorage.SetAsync(PRODUCTION_LOG_FORM_KEY, productionLogForm);
            }
            catch (TaskCanceledException ex)
            {
                Log.Warning("SetNewProductionLogFormAsync was canceled. The app may have been navigating or busy. Exception: {Exception}", ex);
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected error while setting ProductionLogForm in local storage: {Exception}", ex);
            }
            
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to SetNewProductionLogFormAsync: {Exception}", e.ToString());
        }
    }
    
    private static ProductionLogFormDTO MapProductionLogToDto(ProductionLog productionLog)
    {
        var productionLogFormDto = new ProductionLogFormDTO();
    
        foreach (var step in productionLog.LogSteps)
        {
            var stepDto = new ProductionLogStepDTO
            {
                WorkInstructionStepId = step.WorkInstructionStepId,
                ProductionLogId = step.ProductionLogId,
                ShowNotes = step.Attempts.Any(a => !string.IsNullOrEmpty(a.Notes))
            };

            // Map all attempts
            stepDto.Attempts = step.Attempts.Select(a => new ProductionLogStepAttemptDTO
            {
                Success = a.Success,
                SubmitTime = a.SubmitTime,
                Notes = a.Notes,
            }).ToList();

            productionLogFormDto.LogSteps.Add(stepDto);
        }

        productionLogFormDto.ProductionLogId = productionLog.Id;
        return productionLogFormDto;
    }

    /// <inheritdoc />
    public async Task<ProductionLogFormDTO> GetProductionLogFormAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<ProductionLogFormDTO>(PRODUCTION_LOG_FORM_KEY);

            if (result is { Success: true, Value: not null })
            {
                return result.Value;
            }

            Log.Information("Successfully retrieved ProductionLogFormAsync from local cache");
            return new ProductionLogFormDTO();
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to GetProductionLogFormAsync: {Exception}", e.ToString());
            return new ProductionLogFormDTO();
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