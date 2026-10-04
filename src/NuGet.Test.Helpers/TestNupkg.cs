using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using NuGet.Frameworks;

namespace NuGet.Test.Helpers
{
    /// <summary>
    /// Nupkg
    /// </summary>
    public class TestNupkg
    {
        public TestNuspec Nuspec { get; set; }

        public List<TestNupkgFile> Files { get; set; } = new List<TestNupkgFile>();

        public string? LastSavePath { get; private set; }

        /// <summary>
        /// File name used when saving to a folder.
        /// Defaults to {id}.{version}.nupkg, or {id}.{version}.symbols.nupkg for symbol packages.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Replace an existing file when saving to a folder. By default saving throws if the file exists.
        /// </summary>
        public bool OverwriteExisting { get; set; }

        /// <summary>
        /// Path of the nuspec entry in the package. Defaults to {id}.nuspec.
        /// </summary>
        public string? NuspecEntryName { get; set; }

        /// <summary>
        /// Last write time of every entry in the package. Set this to produce the same bytes each time the package is saved.
        /// Compressed bytes can still differ between .NET versions.
        /// Zip entries store the date and time without an offset, at two second precision, for years 1980 to 2107.
        /// Defaults to the time each entry is written.
        /// </summary>
        public DateTimeOffset? EntryLastWriteTime { get; set; }

        public TestNupkg()
        {
            Nuspec = new TestNuspec();
        }

        public TestNupkg(string id)
        {
            Nuspec = new TestNuspec()
            {
                Id = id
            };
        }

        public TestNupkg(string id, string version)
        {
            Nuspec = new TestNuspec()
            {
                Id = id,
                Version = version
            };
        }

        public TestNupkg(TestNuspec nuspec)
        {
            Nuspec = nuspec ?? throw new ArgumentNullException(nameof(nuspec));
        }

        public void AddFile(string path, byte[] bytes)
        {
            Files.Add(new TestNupkgFile(path, bytes));
        }

        public void AddFile(params string[] paths)
        {
            foreach (var path in paths)
            {
                Files.Add(new TestNupkgFile(path));
            }
        }

        /// <summary>
        /// Add a file with the given text, encoded as UTF-8 without a byte order mark.
        /// </summary>
        public void AddTextFile(string path, string content)
        {
            ArgumentNullException.ThrowIfNull(content);

            AddFile(path, Encoding.UTF8.GetBytes(content));
        }

        public void AddDependency(string id)
        {
            Nuspec.AddDependency(id);
        }

        public void AddDependency(string id, string versionRange)
        {
            Nuspec.AddDependency(id, versionRange);
        }

        public void AddDependency(TestNupkg dependencyContext)
        {
            AddDependency(NuGetFramework.AnyFramework, dependencyContext);
        }

        public void AddDependency(NuGetFramework framework, TestNupkg dependencyContext)
        {
            ArgumentNullException.ThrowIfNull(dependencyContext);
            ArgumentNullException.ThrowIfNull(framework);

            Nuspec.AddDependency(framework, dependencyContext.Nuspec);
        }

        public FileInfo Save(string outputDir)
        {
            var fileName = FileName;

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"{Nuspec.Id}.{Nuspec.Version}";

                if (Nuspec.IsSymbolPackage)
                {
                    fileName += ".symbols";
                }

                fileName += ".nupkg";
            }

            var nupkgFile = new FileInfo(Path.Combine(outputDir, fileName));

            if (nupkgFile.Exists && !OverwriteExisting)
            {
                throw new InvalidOperationException($"File already exists: {nupkgFile.FullName}");
            }

            nupkgFile.Directory?.Create();

            using (var stream = File.Create(nupkgFile.FullName))
            {
                Save(stream);
            }

            // Update the state cached by the Exists check above.
            nupkgFile.Refresh();

            LastSavePath = nupkgFile.FullName;

            return nupkgFile;
        }

        /// <summary>
        /// Write the package to a stream. The stream is left open.
        /// </summary>
        public void Save(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in Files)
                {
                    AddEntry(zip, file.Path, file.Bytes);
                }

                var nuspecEntryName = string.IsNullOrEmpty(NuspecEntryName) ? $"{Nuspec.Id}.nuspec" : NuspecEntryName;
                var xml = Nuspec.Create().ToString();

                AddEntry(zip, nuspecEntryName, Encoding.UTF8.GetBytes(xml));
            }
        }

        /// <summary>
        /// Create the package in memory.
        /// </summary>
        public byte[] ToByteArray()
        {
            using (var stream = new MemoryStream())
            {
                Save(stream);

                return stream.ToArray();
            }
        }

        public static void Save(string outputDir, params TestNupkg[] nupkgs)
        {
            foreach (var nupkg in nupkgs)
            {
                nupkg.Save(outputDir);
            }
        }

        public static TestNupkg Create(string id)
        {
            return new TestNupkg(id);
        }

        public static TestNupkg Create(string id, string version)
        {
            return new TestNupkg(id, version);
        }

        public static TestNupkg Create(TestNuspec nuspec)
        {
            return new TestNupkg(nuspec);
        }

        public override string ToString()
        {
            return Nuspec.ToString();
        }

        private void AddEntry(ZipArchive zip, string path, byte[] bytes)
        {
            var entry = zip.CreateEntry(path, CompressionLevel.Optimal);

            // The time can't be changed after the entry is opened.
            if (EntryLastWriteTime.HasValue)
            {
                entry.LastWriteTime = EntryLastWriteTime.Value;
            }

            using (var stream = entry.Open())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
