using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    public static class Extensions
    {
        public static void LogTraceWithData(this ILogger logger, EventId eventId, string message, Exception exception = null, params object[] args)
        {
            logger.Log(LogLevel.Trace, eventId, args, exception, (_, __) => message);
        }
        public static void LogTraceWithData(this ILogger logger, EventId eventId, string message, params object[] args)
        {
            logger.LogTraceWithData(eventId, message, default, args);
        }
        public static void LogDebugWithData(this ILogger logger, EventId eventId, string message, Exception exception = null, params object[] args)
        {
            logger.Log(LogLevel.Debug, eventId, args, exception, (_, __) => message);
        }
        public static void LogDebugWithData(this ILogger logger, EventId eventId, string message, params object[] args)
        {
            logger.LogDebugWithData(eventId, message, default, args);
        }
        public static void LogInformationWithData(this ILogger logger, EventId eventId, string message, Exception exception = null, params object[] args)
        {
            logger.Log(LogLevel.Information, eventId, args, exception, (_, __) => message);
        }
        public static void LogInformationWithData(this ILogger logger, EventId eventId, string message, params object[] args)
        {
            logger.LogInformationWithData(eventId, message, default, args);
        }
        public static void LogWarningWithData(this ILogger logger, EventId eventId, string message, Exception exception = null, params object[] args)
        {
            logger.Log(LogLevel.Warning, eventId, args, exception, (_, __) => message);
        }
        public static void LogWarningWithData(this ILogger logger, EventId eventId, string message, params object[] args)
        {
            logger.LogWarningWithData(eventId, message, default, args);
        }
        public static void LogErrorWithData(this ILogger logger, EventId eventId, string message, Exception exception = null, params object[] args)
        {
            logger.Log(LogLevel.Error, eventId, args, exception, (_, __) => message);
        }
        public static void LogErrorWithData(this ILogger logger, EventId eventId, string message, params object[] args)
        {
            logger.LogErrorWithData(eventId, message, default, args);
        }
        public static void LogCriticalWithData(this ILogger logger, EventId eventId, string message, Exception exception = null, params object[] args)
        {
            logger.Log(LogLevel.Critical, eventId, args, exception, (_, __) => message);
        }
        public static void LogCriticalWithData(this ILogger logger, EventId eventId, string message, params object[] args)
        {
            logger.LogCriticalWithData(eventId, message, default, args);
        }
    }
}
