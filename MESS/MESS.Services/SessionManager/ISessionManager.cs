namespace MESS.Services.SessionManager;
using Data.Models;
public interface ISessionManager
{
    public void AddProductionLog(int logId);
    public List<int>? GetProductionLogIds();
    public void ClearProductionLogs();
}