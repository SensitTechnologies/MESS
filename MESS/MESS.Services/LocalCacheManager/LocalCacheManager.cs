using MESS.Data.DTO;
using MESS.Services.BrowserCacheManager;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MESS.Services.LocalCacheManager;

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
    public async Task SetNewProductionLogFormAsync(Data.Models.ProductionLog? productionLog)
    {
        try
        {
            if (productionLog == null)
            {
                await _protectedLocalStorage.DeleteAsync(PRODUCTION_LOG_FORM_KEY);
                return;
            }

            var productionLogForm = MapProductionLogToDto(productionLog);
            await _protectedLocalStorage.SetAsync(PRODUCTION_LOG_FORM_KEY, productionLogForm);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private static ProductionLogFormDTO MapProductionLogToDto(Data.Models.ProductionLog productionLog)
    {
        var productionLogFormDto = new ProductionLogFormDTO();
        foreach (var step in productionLog.LogSteps)
        {
            productionLogFormDto.LogSteps.Add(new ProductionLogStepDTO
            {
                WorkInstructionStepId = step.WorkInstructionStepId,
                ProductionLogId = step.ProductionLogId,
                Success = step.Success,
                SubmitTime = step.SubmitTime,
                Notes = step.Notes,
                ShowNotes = step.Notes.Length > 0,
            });
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

            return new ProductionLogFormDTO();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ProductionLogFormDTO();
        }
    }

    /// <inheritdoc />
    public async Task SetActiveProductAsync(Data.Models.Product product)
    {
        try
        {
            // map to DTO
            var productDTO = new CacheDTO
            {
                Id = product.Id.ToString(),
                Name = product.Name
            };
            
            await _protectedLocalStorage.SetAsync(ACTIVE_PRODUCT_KEY, productDTO);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CacheDTO> GetActiveProductAsync()
    {
        try
        {
            var t = await _protectedLocalStorage.GetAsync<CacheDTO>(ACTIVE_PRODUCT_KEY);
            if (t is { Success: true, Value: not null })
            {
                return t.Value;
            }

            return new CacheDTO();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new CacheDTO();
        }
    }

    /// <inheritdoc />
    public async Task<int> GetActiveWorkInstructionIdAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<int>(ACTIVE_WORK_INSTRUCTION_KEY);

            if (result.Success)
            {
                return result.Value;
            }

            return -1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return -1;
        }
    }
    
    /// <inheritdoc />
    public async Task SetActiveWorkInstructionIdAsync(int workInstructionId)
    {
        try
        {
            await _protectedLocalStorage.SetAsync(ACTIVE_WORK_INSTRUCTION_KEY, workInstructionId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    

    /// <inheritdoc />
    public async Task SetIsWorkflowActiveAsync(bool inActive)
    {
        try
        {
            await _protectedLocalStorage.SetAsync(IS_WORKFLOW_ACTIVE_KEY, inActive);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
            Console.WriteLine(e);
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

}