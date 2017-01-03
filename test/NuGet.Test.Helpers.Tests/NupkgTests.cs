using System.Linq;
using FluentAssertions;
using NuGet.Packaging;
using Xunit;

namespace NuGet.Test.Helpers.Tests
{
    public class NupkgTests
    {
        [Fact]
        public void VerifyNupkgPackageIdIsExpected()
        {
            using (var folder = new TestFolder())
            {
                // Arrange
                var nupkg = TestNupkg.Create("packageA");

                // Act
                var path = nupkg.Save(folder);

                var reader = new PackageArchiveReader(path.FullName);

                // Verify
                reader.NuspecReader.GetId().Should().Be("packageA");
            }
        }

        [Fact]
        public void VerifyNupkgPackageVersionIsExpected()
        {
            using (var folder = new TestFolder())
            {
                // Arrange
                var nupkg = TestNupkg.Create("packageA", "2.0.1-rc.5.10+hash.11");

                // Act
                var path = nupkg.Save(folder);

                var reader = new PackageArchiveReader(path.FullName);

                // Verify
                reader.NuspecReader.GetVersion().ToFullString().Should().Be("2.0.1-rc.5.10+hash.11");
            }
        }

        [Fact]
        public void VerifyNupkgPackageVersionIsNonNormalized()
        {
            using (var folder = new TestFolder())
            {
                // Arrange
                var nupkg = TestNupkg.Create("packageA", "1.0");

                // Act
                var path = nupkg.Save(folder);

                var reader = new PackageArchiveReader(path.FullName);

                // Verify
                reader.NuspecReader.GetVersion().ToString().Should().Be("1.0");
            }
        }

        [Fact]
        public void VerifyNupkgHasAddedLibFile()
        {
            using (var folder = new TestFolder())
            {
                // Arrange
                var nupkg = TestNupkg.Create("packageA", "1.0.0");
                nupkg.AddFile("lib/net45/a.dll");

                // Act
                var path = nupkg.Save(folder);
                var reader = new PackageArchiveReader(path.FullName);

                var dll = reader.GetLibItems().Single().Items.Single();

                // Verify
                dll.Should().Be("lib/net45/a.dll");
            }
        }
    }
}
