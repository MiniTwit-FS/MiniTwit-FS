using Microsoft.AspNetCore.SignalR;
using MiniTwitAPI.Hubs;

namespace MiniTwitAPI.Extentions
{
    public static class LoggerExtensions
    {
        public static void RLogInformation(this ILogger ilogger, string logMessage, IHubContext<LogHub> loghub)
        {
            ilogger.LogInformation(logMessage);

            var ts = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
            var formatted = $"{ts} [INF] {logMessage}";

            loghub.Clients.All.SendAsync("ReceiveLogUpdate", formatted);
        }

        public static void RLogWarning(this ILogger ilogger, string logMessage, IHubContext<LogHub> loghub)
        {
            ilogger.LogWarning(logMessage);

            var ts = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
            var formatted = $"{ts} [WAR] {logMessage}";

            loghub.Clients.All.SendAsync("ReceiveLogUpdate", formatted);
        }

        public static void RLogError(this ILogger ilogger, Exception e, string logMessage, IHubContext<LogHub> loghub)
        {
            ilogger.LogError(logMessage);

            var ts = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
            var formatted = $"{ts} [ERR] {logMessage}";

            loghub.Clients.All.SendAsync("ReceiveLogUpdate", formatted);
        }
        public static void RLogDebug(this ILogger ilogger, string logMessage, IHubContext<LogHub> loghub)
        {
            ilogger.LogDebug(logMessage);

            var ts = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
            var formatted = $"{ts} [DBG] {logMessage}";

            loghub.Clients.All.SendAsync("ReceiveLogUpdate", formatted);
        }
    }
}
