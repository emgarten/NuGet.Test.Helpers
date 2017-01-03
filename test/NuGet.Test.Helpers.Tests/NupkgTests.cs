using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
