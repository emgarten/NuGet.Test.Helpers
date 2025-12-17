using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NuGet.Packaging;
using Xunit;

namespace NuGet.Test.Helpers.Tests
{
    public class GivenThatICreateANupkg
    {
        [Fact]
        public void VerifyTheIdIsCorrect()
        {
            using (var folder = new TestFolder())
            {
                var nupkg = TestNupkg.Create("packageA");
                var path = nupkg.Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    reader.NuspecReader.GetId().Should().Be("packageA");
                }
            }
        }

        [Fact]
        public void VerifyVersionIsCorrect()
        {
            using (var folder = new TestFolder())
            {
                var nupkg = TestNupkg.Create("packageA", "2.0.1-rc.5.10+hash.11");
                var path = nupkg.Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    reader.NuspecReader.GetVersion().ToFullString().Should().Be("2.0.1-rc.5.10+hash.11");
                }
            }
        }

        [Fact]
        public void VerifyPackageVersionIsNonNormalized()
        {
            using (var folder = new TestFolder())
            {
                var nupkg = TestNupkg.Create("packageA", "1.0");
                var path = nupkg.Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    reader.NuspecReader.GetVersion().ToString().Should().Be("1.0");
                }
            }
        }

        [Fact]
        public void VerifyNupkgHasAddedLibFile()
        {
            using (var folder = new TestFolder())
            {
                var nupkg = TestNupkg.Create("packageA", "1.0.0");
                nupkg.AddFile("lib/net45/a.dll");
                var path = nupkg.Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    var dll = reader.GetLibItems().Single().Items.Single();
                    dll.Should().Be("lib/net45/a.dll");
                }
            }
        }

        [Fact]
        public void VerifySymbolsNupkgHasCorrectName()
        {
            using (var folder = new TestFolder())
            {
                var nuspec = new TestNuspec()
                {
                    Id = "a",
                    Version = "1.0.0",
                    IsSymbolPackage = true
                };

                var path = nuspec.CreateNupkg().Save(folder);

                path.Name.Should().Be("a.1.0.0.symbols.nupkg");
            }
        }

        [Fact]
        public void VerifyNonSymbolsNupkgHasCorrectName()
        {
            using (var folder = new TestFolder())
            {
                var nuspec = new TestNuspec()
                {
                    Id = "a",
                    Version = "1.0.0",
                    IsSymbolPackage = false
                };

                var path = nuspec.CreateNupkg().Save(folder);

                path.Name.Should().Be("a.1.0.0.nupkg");
            }
        }

        [Fact]
        public void VerifyPackageTypeNotSetByDefault()
        {
            using (var folder = new TestFolder())
            {
                var nupkg = TestNupkg.Create("packageA", "1.0.0");
                var path = nupkg.Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    reader.NuspecReader.GetPackageTypes().Should().BeEmpty();
                }
            }
        }

        [Fact]
        public void VerifyPackageTypeWithDotNetCliTool()
        {
            using (var folder = new TestFolder())
            {
                var nuspec = new TestNuspec()
                {
                    Id = "a",
                    Version = "1.0.0",
                    PackageTypes = new List<string>() { "DotNetCliTool" }
                };

                var nupkg = nuspec.CreateNupkg();
                var path = nupkg.Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    var type = reader.NuspecReader.GetPackageTypes().Single();

                    type.Name.Should().Be("DotNetCliTool");
                    type.Version.ToString().Should().Be("0.0");
                }
            }
        }

        [Fact]
        public void VerifyIconIsSet()
        {
            using (var folder = new TestFolder())
            {
                var nuspec = new TestNuspec()
                {
                    Id = "a",
                    Version = "1.0.0",
                    Icon = "images/icon.png"
                };

                var path = nuspec.CreateNupkg().Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    var result = reader.NuspecReader.GetIcon();

                    result.Should().Be("images/icon.png");
                }
            }
        }

        [Fact]
        public void VerifyReadmeIsSet()
        {
            using (var folder = new TestFolder())
            {
                var nuspec = new TestNuspec()
                {
                    Id = "a",
                    Version = "1.0.0",
                    Readme = "README.md"
                };

                var path = nuspec.CreateNupkg().Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    var result = reader.NuspecReader.GetReadme();

                    result.Should().Be("README.md");
                }
            }
        }
    }
}
