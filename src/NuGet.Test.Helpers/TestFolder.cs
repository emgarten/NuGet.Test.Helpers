using System;
using System.IO;

namespace NuGet.Test.Helpers
{
    /// <summary>
    /// Temp directory that cleans up on dispose.
    /// </summary>
    public class TestFolder : IDisposable
    {
        public string Root
        {
            get
            {
                return RootDirectory.FullName;
            }
        }

        public string Working
        {
            get
            {
                return WorkingDirectory.FullName;
            }
        }

        /// <summary>
        /// Root directory, parent of the working directory
        /// </summary>
        public DirectoryInfo RootDirectory { get; }

        /// <summary>
        /// Working directory
        /// </summary>
        public DirectoryInfo WorkingDirectory { get; }

        public TestFolder()
        {
            RootDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            RootDirectory.Create();

            File.WriteAllText(Path.Combine(Root, "trace.txt"), Environment.StackTrace);

            WorkingDirectory = new DirectoryInfo(Path.Combine(RootDirectory.FullName, "working"));

            WorkingDirectory.Create();
        }

        public static implicit operator string(TestFolder folder)
        {
            return folder.Root;
        }

        public override string ToString()
        {
            return Root;
        }

        public void Dispose()
        {
            try
            {
                RootDirectory.Delete(true);
            }
            catch
            {
            }
        }
    }
}