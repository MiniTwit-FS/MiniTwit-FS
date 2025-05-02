using Microsoft.AspNetCore.SignalR;
using MiniTwitAPI.Hubs;

namespace MiniTwitAPI.Extentions
{
    public static class LoggerExtensions
    {
        public static void RLogInformation(this ILogger ilogger, string logMessage, IHubContext<LogHub> loghub)
        {
            ilogger.LogInformation(logMessage);
            loghub.Clients.All.SendAsync("ReceiveLogUpdate", logMessage);
        }

        public static void RLogWarning(this ILogger ilogger, string logMessage, IHubContext<LogHub> loghub)
        {
            ilogger.LogWarning(logMessage);
            loghub.Clients.All.SendAsync("ReceiveLogUpdate", logMessage);
        }

        public static void RLogError(this ILogger ilogger, Exception e, string logMessage, IHubContext<LogHub> loghub)
        {
            ilogger.LogError(logMessage);
            loghub.Clients.All.SendAsync("ReceiveLogUpdate", logMessage);
        }
        public static void RLogDebug(this ILogger ilogger, string logMessage, IHubContext<LogHub> loghub)
        {
            ilogger.LogDebug(logMessage);
            loghub.Clients.All.SendAsync("ReceiveLogUpdate", logMessage);
        }
    }
}
