using System;
using AwesomeAssertions;
using Xunit;

namespace NuGet.Test.Helpers.Tests
{
    public class GivenThatICreateANupkgFile
    {
        [Fact]
        public void VerifyDefaultContentIsASingleZeroByte()
        {
            var file = new TestNupkgFile("lib/net45/a.dll");

            file.Bytes.Should().Equal(new byte[] { 0 });
        }

        [Fact]
        public void VerifyContentIsCopied()
        {
            var bytes = new byte[] { 1, 2, 3 };

            var file = new TestNupkgFile("content/a.txt", bytes);
            bytes[0] = 4;

            file.Bytes.Should().Equal(new byte[] { 1, 2, 3 });
        }

        [Fact]
        public void VerifyNullArgumentsThrow()
        {
            var nullPath = () => new TestNupkgFile(null);
            var nullBytes = () => new TestNupkgFile("content/a.txt", null);

            nullPath.Should().Throw<ArgumentNullException>().WithParameterName("path");
            nullBytes.Should().Throw<ArgumentNullException>().WithParameterName("bytes");
        }

        [Fact]
        public void VerifyToStringIsThePath()
        {
            var file = new TestNupkgFile("lib/net45/a.dll");

            file.ToString().Should().Be("lib/net45/a.dll");
        }
    }
}
