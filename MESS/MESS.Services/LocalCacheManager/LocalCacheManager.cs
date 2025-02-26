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

    public async Task SetActiveWorkInstructionIdAsync(int workInstructionId)
    {
        await Task.Delay(1);
    }
    

    public async Task SetInProgressAsync(bool inProgress)
    {
        await Task.Delay(1);
    }

    public async Task GetInProgressAsync()
    {
        await Task.Delay(1);
    }
}