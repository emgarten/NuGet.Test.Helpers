using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AwesomeAssertions;
using NuGet.Frameworks;
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

        [Fact]
        public void VerifyDefaultIdAndVersion()
        {
            using (var folder = new TestFolder())
            {
                var path = new TestNupkg().Save(folder);

                path.Name.Should().Be("a.1.0.0.nupkg");

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    reader.NuspecReader.GetId().Should().Be("a");
                    reader.NuspecReader.GetVersion().ToString().Should().Be("1.0.0");
                    reader.GetNuspecFile().Should().Be("a.nuspec");
                }
            }
        }

        [Fact]
        public void VerifyNullNuspecThrows()
        {
            var action = () => new TestNupkg((TestNuspec)null);

            action.Should().Throw<ArgumentNullException>().WithParameterName("nuspec");
        }

        [Fact]
        public void VerifyNupkgHasAddedFileContent()
        {
            using (var folder = new TestFolder())
            {
                var bytes = new byte[] { 1, 2, 3 };

                var nupkg = TestNupkg.Create("packageA", "1.0.0");
                nupkg.AddFile("content/a.txt", bytes);
                var path = nupkg.Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                using (var stream = reader.GetStream("content/a.txt"))
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);

                    memoryStream.ToArray().Should().Equal(bytes);
                }
            }
        }

        [Fact]
        public void VerifyNupkgHasAddedFilesWithDefaultContent()
        {
            using (var folder = new TestFolder())
            {
                var nupkg = TestNupkg.Create("packageA", "1.0.0");
                nupkg.AddFile("lib/net45/a.dll", "lib/net45/b.dll");
                var path = nupkg.Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    reader.GetLibItems().Single().Items.Should().BeEquivalentTo("lib/net45/a.dll", "lib/net45/b.dll");

                    using (var stream = reader.GetStream("lib/net45/a.dll"))
                    {
                        stream.ReadByte().Should().Be(0);
                        stream.ReadByte().Should().Be(-1);
                    }
                }
            }
        }

        [Fact]
        public void VerifyNupkgHasDependencies()
        {
            using (var folder = new TestFolder())
            {
                var nupkg = TestNupkg.Create("packageA", "1.0.0");
                nupkg.AddDependency("packageB");
                nupkg.AddDependency("packageC", "[1.0.0, 2.0.0)");
                nupkg.AddDependency(TestNupkg.Create("packageD", "3.0.0"));
                var path = nupkg.Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    var group = reader.GetPackageDependencies().Single();

                    group.TargetFramework.IsAny.Should().BeTrue();
                    group.Packages.Select(e => $"{e.Id} {e.VersionRange.ToNormalizedString()}").Should().Equal(
                        "packageB (, )",
                        "packageC [1.0.0, 2.0.0)",
                        "packageD [3.0.0, )");
                }
            }
        }

        [Fact]
        public void VerifyNupkgHasFrameworkDependency()
        {
            using (var folder = new TestFolder())
            {
                var nupkg = TestNupkg.Create("packageA", "1.0.0");
                nupkg.AddDependency(NuGetFramework.Parse("net8.0"), TestNupkg.Create("packageB", "2.0.0"));
                var path = nupkg.Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    var group = reader.GetPackageDependencies().Single();
                    var dependency = group.Packages.Single();

                    group.TargetFramework.GetShortFolderName().Should().Be("net8.0");
                    dependency.Id.Should().Be("packageB");
                    dependency.VersionRange.ToNormalizedString().Should().Be("[2.0.0, )");
                }
            }
        }

        [Fact]
        public void VerifyAddingANullDependencyThrows()
        {
            var nupkg = TestNupkg.Create("packageA", "1.0.0");

            var nullDependency = () => nupkg.AddDependency((TestNupkg)null);
            var nullFramework = () => nupkg.AddDependency((NuGetFramework)null, TestNupkg.Create("packageB"));

            nullDependency.Should().Throw<ArgumentNullException>().WithParameterName("dependencyContext");
            nullFramework.Should().Throw<ArgumentNullException>().WithParameterName("framework");
        }

        [Fact]
        public void VerifyNuspecXmlOverrideIsSaved()
        {
            using (var folder = new TestFolder())
            {
                var nuspec = new TestNuspec()
                {
                    Id = "a",
                    Version = "1.0.0",
                    XMLOverride = XDocument.Parse("<package><metadata><id>a</id><version>1.0.0</version><title>Override</title></metadata></package>")
                };

                var path = nuspec.CreateNupkg().Save(folder);

                using (var reader = new PackageArchiveReader(path.FullName))
                {
                    reader.NuspecReader.GetTitle().Should().Be("Override");
                }
            }
        }

        [Fact]
        public void VerifySaveCreatesTheOutputDirectory()
        {
            using (var folder = new TestFolder())
            {
                var outputDir = Path.Combine(folder.Root, "a", "b");
                var nupkg = TestNupkg.Create("packageA", "1.0.0");

                nupkg.LastSavePath.Should().BeNull();

                var path = nupkg.Save(outputDir);

                File.Exists(path.FullName).Should().BeTrue();
                path.DirectoryName.Should().Be(outputDir);
                nupkg.LastSavePath.Should().Be(path.FullName);
            }
        }

        [Fact]
        public void VerifySaveReturnsTheSavedFile()
        {
            using (var folder = new TestFolder())
            {
                var path = TestNupkg.Create("packageA", "1.0.0").Save(folder);

                path.Exists.Should().BeTrue();
                path.Length.Should().Be(new FileInfo(path.FullName).Length);
            }
        }

        [Fact]
        public void VerifySavingOverAnExistingNupkgThrows()
        {
            using (var folder = new TestFolder())
            {
                var nupkg = TestNupkg.Create("packageA", "1.0.0");
                nupkg.Save(folder);

                var action = () => nupkg.Save(folder);

                action.Should().Throw<InvalidOperationException>().WithMessage("File already exists: *");
            }
        }

        [Fact]
        public void VerifyMultipleNupkgsAreSaved()
        {
            using (var folder = new TestFolder())
            {
                TestNupkg.Save(folder, TestNupkg.Create("packageA", "1.0.0"), TestNupkg.Create("packageB", "2.0.0"));

                Directory.GetFiles(folder.Root).Select(Path.GetFileName).Should().BeEquivalentTo("packageA.1.0.0.nupkg", "packageB.2.0.0.nupkg");
            }
        }

        [Fact]
        public void VerifyToStringHasIdAndVersion()
        {
            var nupkg = TestNupkg.Create("packageA", "2.0.0");

            nupkg.ToString().Should().Be("packageA 2.0.0");
        }
    }
}
