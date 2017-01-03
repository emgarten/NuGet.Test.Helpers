using System;
using System.Collections.Concurrent;
using System.Linq;
using NuGet.Common;

namespace NuGet.Test.Helpers
{
    public class TestLogger : ILogger
    {
        public ConcurrentQueue<LogEntry> Messages { get; } = new ConcurrentQueue<LogEntry>();

        public void LogDebug(string data)
        {
            Messages.Enqueue(new LogEntry(LogLevel.Debug, data));
        }

        public void LogError(string data)
        {
            Messages.Enqueue(new LogEntry(LogLevel.Error, data));
        }

        public void LogErrorSummary(string data)
        {
            // Ignored
        }

        public void LogInformation(string data)
        {
            Messages.Enqueue(new LogEntry(LogLevel.Information, data));
        }

        public void LogInformationSummary(string data)
        {
            // Ignored
        }

        public void LogMinimal(string data)
        {
            Messages.Enqueue(new LogEntry(LogLevel.Minimal, data));
        }

        public void LogSummary(string data)
        {
            // Ignored
        }

        public void LogVerbose(string data)
        {
            Messages.Enqueue(new LogEntry(LogLevel.Verbose, data));
        }

        public void LogWarning(string data)
        {
            Messages.Enqueue(new LogEntry(LogLevel.Warning, data));
        }

        public override string ToString()
        {
            return GetMessages();
        }

        /// <summary>
        /// Logged messages of all types.
        /// </summary>
        public string GetMessages()
        {
            return string.Join("\n", Messages.Select(e => e.Message));
        }

        /// <summary>
        /// Logged messages of type.
        /// </summary>
        public string GetMessages(LogLevel level)
        {
            return string.Join("\n", Messages.Where(e => e.Level == level).Select(e => e.Message));
        }

        public class LogEntry
        {
            public LogLevel Level { get; }

            public string Message { get; }

            public DateTimeOffset Time { get; }

            public LogEntry(LogLevel level, string message)
            {
                Level = level;
                Message = message;
                Time = DateTimeOffset.UtcNow;
            }
        }
    }
}