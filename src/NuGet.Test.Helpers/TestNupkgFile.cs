using System;

namespace NuGet.Test.Helpers
{
    public class TestNupkgFile
    {
        /// <summary>
        /// Relative path in the nupkg.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// File content.
        /// </summary>
        public byte[] Bytes { get; } = new byte[0];

        /// <summary>
        /// Nupkg file.
        /// </summary>
        /// <param name="path">Relative path in the nupkg. Ex: lib/net45/a.dll</param>
        public TestNupkgFile(string path)
            : this(path, new byte[] { 0 })
        {
        }

        /// <summary>
        /// Nupkg file.
        /// </summary>
        /// <param name="path">Relative path in the nupkg. Ex: lib/net45/a.dll</param>
        public TestNupkgFile(string path, byte[] bytes)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Bytes = bytes;
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
