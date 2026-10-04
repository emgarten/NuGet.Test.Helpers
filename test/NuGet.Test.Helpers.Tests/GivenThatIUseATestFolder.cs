using System.IO;
using AwesomeAssertions;
using Xunit;

namespace NuGet.Test.Helpers.Tests
{
    public class GivenThatIUseATestFolder
    {
        [Fact]
        public void VerifyRootIsCreatedInTheTempDirectory()
        {
            using (var folder = new TestFolder())
            {
                Directory.Exists(folder.Root).Should().BeTrue();
                folder.Root.Should().StartWith(Path.GetFullPath(Path.GetTempPath()));
                folder.RootDirectory.FullName.Should().Be(folder.Root);
            }
        }

        [Fact]
        public void VerifyEachFolderIsUnique()
        {
            using (var folderA = new TestFolder())
            using (var folderB = new TestFolder())
            {
                folderA.Root.Should().NotBe(folderB.Root);
            }
        }

        [Fact]
        public void VerifyFolderIsDeletedOnDispose()
        {
            string root;
            string parent;

            using (var folder = new TestFolder())
            {
                root = folder.Root;
                parent = folder.RootDirectory.Parent.FullName;

                File.WriteAllText(Path.Combine(root, "a.txt"), "a");
            }

            Directory.Exists(root).Should().BeFalse();
            Directory.Exists(parent).Should().BeFalse();
        }

        [Fact]
        public void VerifyFolderIsKeptWhenCleanUpIsDisabled()
        {
            string root;
            string parent;

            using (var folder = new TestFolder() { CleanUp = false })
            {
                root = folder.Root;
                parent = folder.RootDirectory.Parent.FullName;
            }

            try
            {
                Directory.Exists(root).Should().BeTrue();
            }
            finally
            {
                Directory.Delete(parent, recursive: true);
            }
        }

        [Fact]
        public void VerifyDisposeCanBeCalledMoreThanOnce()
        {
            var folder = new TestFolder();
            folder.Dispose();

            var action = () => folder.Dispose();

            action.Should().NotThrow();
        }

        [Fact]
        public void VerifyFolderConvertsToTheRootPath()
        {
            using (var folder = new TestFolder())
            {
                string path = folder;

                path.Should().Be(folder.Root);
                folder.ToString().Should().Be(folder.Root);
            }
        }
    }
}
