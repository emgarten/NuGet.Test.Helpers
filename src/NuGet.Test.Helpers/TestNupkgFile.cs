using System;
using System.Linq;

namespace NuGet.Test.Helpers
{
    /// <summary>
    /// File inside a nupkg.
    /// </summary>
    public class TestNupkgFile
    {
        /// <summary>
        /// Relative path in the nupkg.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// File content.
        /// </summary>
        public byte[] Bytes { get; } = Array.Empty<byte>();

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
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(bytes);
            Path = path;

            // Store a copy to prevent external mutations
            Bytes = bytes.ToArray();
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
