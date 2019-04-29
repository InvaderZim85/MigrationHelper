using System;
using System.Diagnostics;
using System.Security;
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

            WriteEventLog(LogLevel.Info, message);
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

            WriteEventLog(LogLevel.Error, message, ex);
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

        /// <summary>
        /// Writes a message to the windows event log
        /// </summary>
        /// <param name="level">The log level</param>
        /// <param name="message">The message</param>
        /// <param name="ex">The execption (optional)</param>
        private static void WriteEventLog(LogLevel level, string message, Exception ex = null)
        {
            try
            {
                using (var log = new EventLog("Application"){Source = nameof(MigrationHelper)})
                {
                    log.WriteEntry(message,
                        level == LogLevel.Error ? EventLogEntryType.Error : EventLogEntryType.Information);
                }
            }
            catch (SecurityException se)
            {
                WriteLog(new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Message = nameof(WriteEventLog),
                    Exception = se
                });
            }
        }
    }
}
