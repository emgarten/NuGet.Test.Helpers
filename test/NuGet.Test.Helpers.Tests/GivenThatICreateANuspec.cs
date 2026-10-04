using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AwesomeAssertions;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Xunit;

namespace NuGet.Test.Helpers.Tests
{
    public class GivenThatICreateANuspec
    {
        [Fact]
        public void VerifyDefaultIdAndVersion()
        {
            var reader = new NuspecReader(new TestNuspec().Create());

            reader.GetId().Should().Be("a");
            reader.GetVersion().ToString().Should().Be("1.0.0");
        }

        [Fact]
        public void VerifyMetadataIsSet()
        {
            var nuspec = new TestNuspec()
            {
                Id = "a",
                Version = "1.0.0",
                Title = "Title",
                Authors = "Author",
                Owners = "Owner",
                Description = "Description",
                ReleaseNotes = "Release notes",
                Summary = "Summary",
                Language = "en-US",
                ProjectUrl = "https://example.org/project",
                IconUrl = "https://example.org/icon.png",
                LicenseUrl = "https://example.org/license",
                Copyright = "Copyright",
                RequireLicenseAcceptance = "true",
                Tags = "tagA tagB",
                DevelopmentDependency = "true",
                Serviceable = "true"
            };

            var reader = new NuspecReader(nuspec.Create());

            reader.GetTitle().Should().Be("Title");
            reader.GetAuthors().Should().Be("Author");
            reader.GetOwners().Should().Be("Owner");
            reader.GetDescription().Should().Be("Description");
            reader.GetReleaseNotes().Should().Be("Release notes");
            reader.GetSummary().Should().Be("Summary");
            reader.GetLanguage().Should().Be("en-US");
            reader.GetProjectUrl().Should().Be("https://example.org/project");
            reader.GetIconUrl().Should().Be("https://example.org/icon.png");
            reader.GetLicenseUrl().Should().Be("https://example.org/license");
            reader.GetCopyright().Should().Be("Copyright");
            reader.GetRequireLicenseAcceptance().Should().BeTrue();
            reader.GetTags().Should().Be("tagA tagB");
            reader.GetDevelopmentDependency().Should().BeTrue();
            reader.IsServiceable().Should().BeTrue();
        }

        [Fact]
        public void VerifyEmptyMetadataIsNotWritten()
        {
            var nuspec = new TestNuspec()
            {
                MinClientVersion = string.Empty,
                Title = string.Empty,
                LicenseExpression = string.Empty,
                LicenseFile = string.Empty,
                RepositoryType = string.Empty
            };

            var metadata = nuspec.Create().Root.Element("metadata");

            metadata.Elements().Select(e => e.Name.LocalName).Should().Equal("id", "version");
            metadata.Attributes().Should().BeEmpty();
        }

        [Fact]
        public void VerifyMinClientVersionIsSet()
        {
            var nuspec = new TestNuspec()
            {
                MinClientVersion = "5.0.0"
            };

            var reader = new NuspecReader(nuspec.Create());

            reader.GetMinClientVersion().ToString().Should().Be("5.0.0");
        }

        [Fact]
        public void VerifyXmlOverrideIsUsed()
        {
            var xml = XDocument.Parse("<package><metadata><id>b</id><version>2.0.0</version></metadata></package>");

            var nuspec = new TestNuspec()
            {
                Id = "a",
                Version = "1.0.0",
                XMLOverride = xml
            };

            nuspec.Create().Should().BeSameAs(xml);
        }

        [Fact]
        public void VerifyDependenciesWithoutAFrameworkAreNotGrouped()
        {
            var nuspec = new TestNuspec();
            nuspec.AddDependency("b");
            nuspec.AddDependency("c", "[1.0.0, 2.0.0)");
            nuspec.AddDependency(new PackageDependency("d", VersionRange.Parse("[3.0.0]")));
            nuspec.AddDependency(new TestNuspec() { Id = "e", Version = "4.0.0" });

            var xml = nuspec.Create();

            xml.Root.Element("metadata").Element("dependencies").Elements("group").Should().BeEmpty();

            var group = new NuspecReader(xml).GetDependencyGroups().Single();

            group.TargetFramework.IsAny.Should().BeTrue();
            group.Packages.Select(e => $"{e.Id} {e.VersionRange.ToNormalizedString()}").Should().Equal(
                "b (, )",
                "c [1.0.0, 2.0.0)",
                "d [3.0.0, 3.0.0]",
                "e [4.0.0, )");
        }

        [Fact]
        public void VerifyDependenciesAreGroupedByFramework()
        {
            var nuspec = new TestNuspec();
            nuspec.AddDependency(NuGetFramework.Parse("net8.0"), "b", "1.0.0");
            nuspec.AddDependency(NuGetFramework.Parse("netstandard2.0"), "c", "2.0.0");
            nuspec.AddDependency(NuGetFramework.Parse("net8.0"), "d", "3.0.0");

            var groups = new NuspecReader(nuspec.Create()).GetDependencyGroups()
                .ToDictionary(e => e.TargetFramework.GetShortFolderName(), e => e.Packages.Select(p => p.Id).ToList());

            groups.Should().HaveCount(2);
            groups["net8.0"].Should().Equal("b", "d");
            groups["netstandard2.0"].Should().Equal("c");
        }

        [Fact]
        public void VerifyDependenciesWithoutAFrameworkAreGroupedWhenFrameworksAreUsed()
        {
            var nuspec = new TestNuspec();
            nuspec.AddDependency("b", "1.0.0");
            nuspec.AddDependency(NuGetFramework.Parse("net8.0"), "c", "2.0.0");

            var xml = nuspec.Create();

            xml.Root.Element("metadata").Element("dependencies").Elements("group")
                .Select(e => (string)e.Attribute("targetFramework"))
                .Should().Equal(null, "net8.0");

            var groups = new NuspecReader(xml).GetDependencyGroups().ToList();

            groups.Should().HaveCount(2);
            groups.Single(e => e.TargetFramework.IsAny).Packages.Single().Id.Should().Be("b");
            groups.Single(e => e.TargetFramework.GetShortFolderName() == "net8.0").Packages.Single().Id.Should().Be("c");
        }

        [Fact]
        public void VerifyDependencyGroupsWithoutAFrameworkAreMerged()
        {
            var nuspec = new TestNuspec();
            nuspec.Dependencies.Add(new PackageDependencyGroup(NuGetFramework.AnyFramework, new List<PackageDependency>() { new PackageDependency("b") }));
            nuspec.Dependencies.Add(new PackageDependencyGroup(NuGetFramework.AnyFramework, new List<PackageDependency>() { new PackageDependency("c") }));

            var xml = nuspec.Create();

            xml.Root.Element("metadata").Element("dependencies").Elements("group").Should().BeEmpty();

            var group = new NuspecReader(xml).GetDependencyGroups().Single();

            group.TargetFramework.IsAny.Should().BeTrue();
            group.Packages.Select(e => e.Id).Should().Equal("b", "c");
        }

        [Fact]
        public void VerifyDependencyIncludeAndExcludeAreSet()
        {
            var nuspec = new TestNuspec();
            nuspec.AddDependency(
                NuGetFramework.Parse("net8.0"),
                new PackageDependency("b", VersionRange.Parse("1.0.0"), new List<string>() { "Compile", "Runtime" }, new List<string>() { "Build" }));

            var dependency = new NuspecReader(nuspec.Create()).GetDependencyGroups().Single().Packages.Single();

            dependency.Include.Should().Equal("Compile", "Runtime");
            dependency.Exclude.Should().Equal("Build");
        }

        [Fact]
        public void VerifyDependencyIncludeAndExcludeAreSetWithoutAFramework()
        {
            var nuspec = new TestNuspec();
            nuspec.AddDependency(new PackageDependency("b", VersionRange.Parse("1.0.0"), new List<string>() { "Compile", "Runtime" }, new List<string>() { "Build" }));

            var xml = nuspec.Create();

            xml.Root.Element("metadata").Element("dependencies").Elements("group").Should().BeEmpty();

            var dependency = new NuspecReader(xml).GetDependencyGroups().Single().Packages.Single();

            dependency.Include.Should().Equal("Compile", "Runtime");
            dependency.Exclude.Should().Equal("Build");
        }

        [Fact]
        public void VerifyFrameworkAssembliesAreSet()
        {
            var nuspec = new TestNuspec();
            nuspec.FrameworkAssemblies.Add(KeyValuePair.Create(
                "System.Net.Http",
                new List<NuGetFramework>() { NuGetFramework.Parse("net45"), NuGetFramework.Parse("net46") }));

            var groups = new NuspecReader(nuspec.Create()).GetFrameworkAssemblyGroups().ToList();

            groups.Select(e => e.TargetFramework.GetShortFolderName()).Should().Equal("net45", "net46");
            groups.Should().AllSatisfy(e => e.Items.Should().Equal("System.Net.Http"));
        }

        [Fact]
        public void VerifyContentFilesAreSet()
        {
            var nuspec = new TestNuspec();
            nuspec.ContentFiles.Add(new ContentFilesEntry("cs/net45/*.cs", "cs/net45/b.cs", "Compile", true, false));
            nuspec.ContentFiles.Add(new ContentFilesEntry("any/any/*.txt", null, null, null, null));

            var contentFiles = new NuspecReader(nuspec.Create()).GetContentFiles();

            contentFiles.Should().BeEquivalentTo(nuspec.ContentFiles, options => options.WithStrictOrdering());
        }

        [Fact]
        public void VerifyLicenseExpressionIsSet()
        {
            var nuspec = new TestNuspec()
            {
                LicenseExpression = "MIT OR Apache-2.0"
            };

            var xml = nuspec.Create();

            xml.Root.Element("metadata").Element("license").Attribute("version").Should().BeNull();

            var license = new NuspecReader(xml).GetLicenseMetadata();

            license.Type.Should().Be(LicenseType.Expression);
            license.License.Should().Be("MIT OR Apache-2.0");
            license.LicenseExpression.Should().NotBeNull();
        }

        [Fact]
        public void VerifyLicenseFileIsSet()
        {
            var nuspec = new TestNuspec()
            {
                LicenseFile = "LICENSE.txt"
            };

            var license = new NuspecReader(nuspec.Create()).GetLicenseMetadata();

            license.Type.Should().Be(LicenseType.File);
            license.License.Should().Be("LICENSE.txt");
        }

        [Fact]
        public void VerifyLicenseVersionIsSet()
        {
            var nuspec = new TestNuspec()
            {
                LicenseExpression = "MIT",
                LicenseVersion = "2.0.0"
            };

            var license = new NuspecReader(nuspec.Create()).GetLicenseMetadata();

            license.Version.Should().Be(new Version(2, 0, 0));
        }

        [Fact]
        public void VerifyRepositoryIsSet()
        {
            var nuspec = new TestNuspec()
            {
                RepositoryType = "git",
                RepositoryUrl = "https://example.org/repo.git",
                RepositoryBranch = "main",
                RepositoryCommit = "0123456789abcdef"
            };

            var repository = new NuspecReader(nuspec.Create()).GetRepositoryMetadata();

            repository.Type.Should().Be("git");
            repository.Url.Should().Be("https://example.org/repo.git");
            repository.Branch.Should().Be("main");
            repository.Commit.Should().Be("0123456789abcdef");
        }

        [Fact]
        public void VerifyRepositoryOnlyHasTheSetAttributes()
        {
            var nuspec = new TestNuspec()
            {
                RepositoryType = "git"
            };

            var repository = nuspec.Create().Root.Element("metadata").Element("repository");

            repository.Attributes().Select(e => e.Name.LocalName).Should().Equal("type");
        }

        [Fact]
        public void VerifyFrameworkReferencesAreGroupedByFramework()
        {
            var nuspec = new TestNuspec();
            nuspec.AddFrameworkReference(NuGetFramework.Parse("net8.0"), "Microsoft.AspNetCore.App");
            nuspec.AddFrameworkReference(NuGetFramework.Parse("net9.0"), "Microsoft.WindowsDesktop.App");
            nuspec.AddFrameworkReference(NuGetFramework.Parse("net8.0"), "Microsoft.WindowsDesktop.App");

            var groups = new NuspecReader(nuspec.Create()).GetFrameworkRefGroups()
                .ToDictionary(e => e.TargetFramework.GetShortFolderName(), e => e.FrameworkReferences.Select(r => r.Name).ToList());

            groups.Should().HaveCount(2);
            groups["net8.0"].Should().BeEquivalentTo("Microsoft.AspNetCore.App", "Microsoft.WindowsDesktop.App");
            groups["net9.0"].Should().BeEquivalentTo("Microsoft.WindowsDesktop.App");
        }

        [Fact]
        public void VerifyPackageTypesAreSet()
        {
            var nuspec = new TestNuspec();
            nuspec.AddPackageType("DotnetTool");
            nuspec.AddPackageType("CustomType", "1.2.3");

            var xml = nuspec.Create();

            xml.Root.Element("metadata").Element("packageTypes").Elements("packageType")
                .Select(e => (string)e.Attribute("version"))
                .Should().Equal(null, "1.2.3");

            new NuspecReader(xml).GetPackageTypes().Select(e => $"{e.Name} {e.Version}").Should().Equal(
                "DotnetTool 0.0",
                "CustomType 1.2.3");
        }

        [Fact]
        public void VerifyPackageTypeVersionsIgnoreCase()
        {
            var nuspec = new TestNuspec();
            nuspec.PackageTypes.Add("Dependency");
            nuspec.PackageTypeVersions["dependency"] = "2.0";

            var packageType = new NuspecReader(nuspec.Create()).GetPackageTypes().Single();

            packageType.Name.Should().Be("Dependency");
            packageType.Version.Should().Be(new Version(2, 0));
        }

        [Fact]
        public void VerifyAddingNullValuesThrows()
        {
            var nuspec = new TestNuspec();

            var nullPackageType = () => nuspec.AddPackageType(null);
            var nullPackageTypeVersion = () => nuspec.AddPackageType("a", null);
            var nullFramework = () => nuspec.AddFrameworkReference(null, "a");
            var nullFrameworkReference = () => nuspec.AddFrameworkReference(NuGetFramework.Parse("net8.0"), null);

            nullPackageType.Should().Throw<ArgumentNullException>().WithParameterName("name");
            nullPackageTypeVersion.Should().Throw<ArgumentNullException>().WithParameterName("version");
            nullFramework.Should().Throw<ArgumentNullException>().WithParameterName("framework");
            nullFrameworkReference.Should().Throw<ArgumentNullException>().WithParameterName("name");
            nuspec.PackageTypes.Should().BeEmpty();
            nuspec.FrameworkReferences.Should().BeEmpty();
        }

        [Fact]
        public void VerifyToStringHasIdAndVersion()
        {
            var nuspec = new TestNuspec()
            {
                Id = "b",
                Version = "2.0.0"
            };

            nuspec.ToString().Should().Be("b 2.0.0");
        }
    }
}
