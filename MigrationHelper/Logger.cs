using System;
using NLog;

namespace MigrationHelper
{
    public static class Logger
    {
        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="message">The message</param>
        public static void Info(string message)
        {
            var log = new LogEventInfo
            {
                Level = LogLevel.Info,
                Message = message
            };

            WriteLog(log);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="ex">The exception</param>
        public static void Error(string message, Exception ex)
        {
            WriteLog(new LogEventInfo
            {
                Level = LogLevel.Error,
                Message = message,
                Exception = ex
            });
        }

        /// <summary>
        /// Writes the log
        /// </summary>
        /// <param name="log">The log data</param>
        private static void WriteLog(LogEventInfo log)
        {
            var logger = LogManager.GetLogger("*");
            logger.Log(log);
        }
    }
}
