namespace MESS.Services.BrowserCacheManager;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

public class LocalCacheManager : ILocalCacheManager
{
    private const string InProgressKey = "IN_PROGRESS";
    private const string ActiveWorkInstructionKey = "LAST_KNOWN_WORK_INSTRUCTION_ID";
    private const string ActiveProductKey = "LAST_KNOWN_ACTIVE_PRODUCT_ID";
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    
    public LocalCacheManager(ProtectedLocalStorage protectedLocalStorage)
    {
        _protectedLocalStorage = protectedLocalStorage;
    }
    
    public async Task SetActiveProductIdAsync(int productId)
    {
        try
        {
            await _protectedLocalStorage.SetAsync(ActiveProductKey, productId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<int> GetActiveProductIdAsync()
    {
        try
        {
            var t = await _protectedLocalStorage.GetAsync<int>(ActiveProductKey);
            if (t.Success)
            {
                return t.Value;
            }

            return -1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return -1;
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
    

    public async Task SetInProgressAsync(bool inProgress)
    {
        try
        {
            await _protectedLocalStorage.SetAsync(InProgressKey, inProgress);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task<bool> GetInProgressAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<bool>(InProgressKey);

            if (!result.Success)
            {
                await SetInProgressAsync(false);
            }

            return result.Value;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new KeyNotFoundException("Unable to find InProgress key from local storage");
        }
    }

    public async Task ResetCachedValuesAsync()
    {
        try
        {
            await _protectedLocalStorage.DeleteAsync(ActiveProductKey);
            await _protectedLocalStorage.DeleteAsync(ActiveWorkInstructionKey);
            await _protectedLocalStorage.DeleteAsync(InProgressKey);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

}