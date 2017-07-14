using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Common;

namespace NuGet.Test.Helpers
{
    /// <summary>
    /// NuGet ILogger that stores messages in memory.
    /// </summary>
    public class TestLogger : LoggerBase
    {
        public ConcurrentQueue<LogEntry> Messages { get; } = new ConcurrentQueue<LogEntry>();

        public override void Log(ILogMessage message)
        {
            Messages.Enqueue(new LogEntry(message));
        }

        public override Task LogAsync(ILogMessage message)
        {
            Log(message);

            return Task.FromResult(true);
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
            public LogLevel Level => OriginalMessage.Level;

            public string Message => OriginalMessage.Message;

            public ILogMessage OriginalMessage { get; }

            public DateTimeOffset Time { get; }

            public LogEntry(ILogMessage message)
            {
                OriginalMessage = message;
                Time = DateTimeOffset.UtcNow;
            }

            public override string ToString()
            {
                return Message;
            }
        }
    }
}