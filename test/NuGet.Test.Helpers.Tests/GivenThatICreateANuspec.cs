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
                Title = string.Empty
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
