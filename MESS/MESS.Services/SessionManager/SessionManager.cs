using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MESS.Services.SessionManager;
using Data.Models;

public class SessionManager : ISessionManager
{
    private readonly ProtectedSessionStorage _protectedSessionStorage;
    private const string PRODUCTION_LOG_SESSION_KEY = "CURRENT_PRODUCTION_LOGS";

    public SessionManager(ProtectedSessionStorage protectedSessionStorage)
    {
        _protectedSessionStorage = protectedSessionStorage;
    }

    public async Task AddProductionLogAsync(int log)
    {
        var logs = await GetProductionLogIdsAsync();
        logs?.Add(log);
        if (logs == null)
        {
            return;
        }
        await _protectedSessionStorage.SetAsync(PRODUCTION_LOG_SESSION_KEY, logs);
    }

    public async Task<List<int>?> GetProductionLogIdsAsync()
    {
        try
        {
            var logList = await _protectedSessionStorage.GetAsync<List<int>>(PRODUCTION_LOG_SESSION_KEY);
            return logList.Success ? logList.Value : [];
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return [];
        }

    }

    public async Task ClearProductionLogsAsync()
    {
        try
        {
            await _protectedSessionStorage.DeleteAsync(PRODUCTION_LOG_SESSION_KEY);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
