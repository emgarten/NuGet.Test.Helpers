using System;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using NuGet.Common;
using Xunit;

namespace NuGet.Test.Helpers.Tests
{
    public class GivenThatIUseATestLogger
    {
        [Fact]
        public void VerifyMessagesOfAllLevelsAreStored()
        {
            var logger = new TestLogger();

            logger.LogDebug("debug");
            logger.LogVerbose("verbose");
            logger.LogInformation("information");
            logger.LogMinimal("minimal");
            logger.LogWarning("warning");
            logger.LogError("error");

            logger.Messages.Select(e => e.Level).Should().Equal(
                LogLevel.Debug,
                LogLevel.Verbose,
                LogLevel.Information,
                LogLevel.Minimal,
                LogLevel.Warning,
                LogLevel.Error);

            logger.Messages.Select(e => e.Message).Should().Equal("debug", "verbose", "information", "minimal", "warning", "error");
        }

        [Fact]
        public async Task VerifyAsyncMessagesAreStored()
        {
            var logger = new TestLogger();

            await logger.LogAsync(LogLevel.Warning, "warning");

            var entry = logger.Messages.Single();

            entry.Level.Should().Be(LogLevel.Warning);
            entry.Message.Should().Be("warning");
        }

        [Fact]
        public void VerifyLogEntryHasTheOriginalMessage()
        {
            var logger = new TestLogger();
            var message = LogMessage.CreateWarning(NuGetLogCode.NU1603, "warning");

            var before = DateTimeOffset.UtcNow;
            logger.Log(message);
            var after = DateTimeOffset.UtcNow;

            var entry = logger.Messages.Single();

            entry.OriginalMessage.Should().BeSameAs(message);
            entry.Level.Should().Be(LogLevel.Warning);
            entry.Message.Should().Be("warning");
            entry.ToString().Should().Be("warning");
            entry.Time.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }

        [Fact]
        public void VerifyGetMessagesReturnsAllMessages()
        {
            var logger = new TestLogger();

            logger.LogInformation("a");
            logger.LogError("b");

            logger.GetMessages().Should().Be("a\nb");
            logger.ToString().Should().Be("a\nb");
        }

        [Fact]
        public void VerifyGetMessagesFiltersByLevel()
        {
            var logger = new TestLogger();

            logger.LogInformation("a");
            logger.LogError("b");
            logger.LogInformation("c");

            logger.GetMessages(LogLevel.Information).Should().Be("a\nc");
            logger.GetMessages(LogLevel.Error).Should().Be("b");
            logger.GetMessages(LogLevel.Warning).Should().BeEmpty();
        }

        [Fact]
        public void VerifyGetMessagesIsEmptyWithoutMessages()
        {
            var logger = new TestLogger();

            logger.GetMessages().Should().BeEmpty();
        }
    }
}
