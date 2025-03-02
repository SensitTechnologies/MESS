namespace MESS.Services.SessionManager;

public interface ISessionManager
{
    public void AddProductionLog(string log);
    public List<string>? GetProductionLogs();
    public void ClearProductionLogs();
}