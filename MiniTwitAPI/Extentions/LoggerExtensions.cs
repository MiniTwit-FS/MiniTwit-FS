using MiniTwitAPI.Hubs;

namespace MiniTwitAPI.Extentions
{
    public static class LoggerExtensions
    {
        public static void RLogInformation(this ILogger ilogger, string logMessage, LogHub loghub)
        {
            ilogger.LogInformation(logMessage);
            loghub.SendLogUpdate(logMessage);
        }

        public static void RLogWarning(this ILogger ilogger, string logMessage, LogHub loghub)
        {
            ilogger.LogWarning(logMessage);
            loghub.SendLogUpdate(logMessage);
        }

        public static void RLogError(this ILogger ilogger, Exception e, string logMessage, LogHub loghub)
        {
            ilogger.LogError(logMessage);
            loghub.SendLogUpdate(logMessage);
        }
        public static void RLogDebug(this ILogger ilogger, string logMessage, LogHub loghub)
        {
            ilogger.LogDebug(logMessage);
            loghub.SendLogUpdate(logMessage);
        }
    }
}
