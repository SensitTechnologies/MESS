using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace MESS.Services.SessionManager
{
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

        public void AddProductionLog(string log)
        {
            var context = HttpContextAccessor.HttpContext;
            var logs = GetProductionLogs();
            logs?.Add(log);
            var logsJson = JsonSerializer.Serialize(logs);
            context?.Response.Cookies.Append(PRODUCTION_LOG_COOKIE_KEY, logsJson, new CookieOptions { HttpOnly = true, Secure = true });
        }

        public List<string>? GetProductionLogs()
        {
            var context = HttpContextAccessor.HttpContext;
            var logsJson = context?.Request.Cookies[PRODUCTION_LOG_COOKIE_KEY];
            if (string.IsNullOrEmpty(logsJson))
            {
                return new List<string>();
            }
            return JsonSerializer.Deserialize<List<string>>(logsJson);
        }

        public void ClearProductionLogs()
        {
            var context = HttpContextAccessor.HttpContext;
            context?.Response.Cookies.Delete(PRODUCTION_LOG_COOKIE_KEY);
        }
    }
}