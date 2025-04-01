﻿using System.Text.Json;
using System.Text.Json.Serialization;
using MESS.Data.DTO;

namespace MESS.Services.BrowserCacheManager;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Data.Models;
public class LocalCacheManager : ILocalCacheManager
{
    private const string IsWorkflowActiveKey = "IS_WORKFLOW_ACTIVE";
    private const string ActiveWorkInstructionKey = "LAST_KNOWN_WORK_INSTRUCTION_ID";
    private const string ActiveProductKey = "LAST_KNOWN_ACTIVE_PRODUCT";
    private const string ProductionLogFormKey = "PRODUCTION_LOG_FORM_PROGRESS";
    private const string ActiveApplicationUserKey = "LAST_KNOWN_LINE_OPERATOR";
    private const string ActiveWorkStationKey = "LAST_KNOWN_WORK_STATION";
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    
    public LocalCacheManager(ProtectedLocalStorage protectedLocalStorage)
    {
        _protectedLocalStorage = protectedLocalStorage;
    }

    public async Task SetNewProductionLogFormAsync(ProductionLog? productionLog)
    {
        try
        {
            if (productionLog == null)
            {
                await _protectedLocalStorage.DeleteAsync(ProductionLogFormKey);
                return;
            }

            var productionLogForm = MapProductionLogToDto(productionLog);
            await _protectedLocalStorage.SetAsync(ProductionLogFormKey, productionLogForm);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private static ProductionLogFormDTO MapProductionLogToDto(ProductionLog productionLog)
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

    public async Task<ProductionLogFormDTO> GetProductionLogFormAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<ProductionLogFormDTO>(ProductionLogFormKey);

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

    public async Task SetActiveProductAsync(Product product)
    {
        try
        {
            // map to DTO
            var productDTO = new CacheDTO
            {
                Id = product.Id.ToString(),
                Name = product.Name
            };
            
            await _protectedLocalStorage.SetAsync(ActiveProductKey, productDTO);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<CacheDTO> GetActiveProductAsync()
    {
        try
        {
            var t = await _protectedLocalStorage.GetAsync<CacheDTO>(ActiveProductKey);
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

    public async Task<int> GetActiveWorkInstructionIdAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<int>(ActiveWorkInstructionKey);

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
    
    public async Task SetActiveWorkInstructionIdAsync(int workInstructionId)
    {
        try
        {
            await _protectedLocalStorage.SetAsync(ActiveWorkInstructionKey, workInstructionId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    

    public async Task SetIsWorkflowActiveAsync(bool inActive)
    {
        try
        {
            await _protectedLocalStorage.SetAsync(IsWorkflowActiveKey, inActive);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task<bool> GetWorkflowActiveStatusAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<bool>(IsWorkflowActiveKey);

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

    public async Task ResetCachedValuesAsync()
    {
        try
        {
            await _protectedLocalStorage.DeleteAsync(ActiveWorkStationKey);
            await _protectedLocalStorage.DeleteAsync(ActiveProductKey);
            await _protectedLocalStorage.DeleteAsync(ActiveWorkInstructionKey);
            await _protectedLocalStorage.DeleteAsync(IsWorkflowActiveKey);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

}