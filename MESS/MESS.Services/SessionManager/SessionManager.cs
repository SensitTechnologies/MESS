using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace MESS.Services.SessionManager;
using Data.Models;

public class SessionManager : ISessionManager
{
    private IHttpContextAccessor HttpContextAccessor { get; set; }
    private HttpClient HttpClient { get; set; }
    private const string PRODUCTION_LOG_COOKIE_KEY = "CURRENT_PRODUCTION_LOGS";

    public SessionManager(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        HttpClient = httpClient;
        HttpContextAccessor = httpContextAccessor;
    }

    public void AddProductionLog(int log)
    {
        var context = HttpContextAccessor.HttpContext;
        var logs = GetProductionLogIds();
        logs?.Add(log);
        var logsJson = JsonSerializer.Serialize(logs);
        context?.Response.Cookies.Append(PRODUCTION_LOG_COOKIE_KEY, logsJson, new CookieOptions { HttpOnly = true, Secure = true });
    }

    public List<int>? GetProductionLogIds()
    {
        var context = HttpContextAccessor.HttpContext;
        var logsJson = context?.Request.Cookies[PRODUCTION_LOG_COOKIE_KEY];
        if (string.IsNullOrEmpty(logsJson))
        {
            return new List<int>();
        }
        return JsonSerializer.Deserialize<List<int>>(logsJson);
    }

    public void ClearProductionLogs()
    {
        var context = HttpContextAccessor.HttpContext;
        context?.Response.Cookies.Delete(PRODUCTION_LOG_COOKIE_KEY);
    }
}
