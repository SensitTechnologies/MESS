namespace MESS.Services.SessionManager;
using Data.Models;
public interface ISessionManager
{
    public Task AddProductionLogAsync(int logId);
    public Task<List<int>?> GetProductionLogIdsAsync();
    public Task ClearProductionLogsAsync();
}