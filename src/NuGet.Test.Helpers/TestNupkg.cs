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

        public string LastSavePath { get; private set; }

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
            if (dependencyContext == null)
            {
                throw new ArgumentNullException(nameof(dependencyContext));
            }

            if (framework == null)
            {
                throw new ArgumentNullException(nameof(framework));
            }

            Nuspec.AddDependency(framework, dependencyContext.Nuspec);
        }

        public FileInfo Save(string outputDir)
        {
            var id = Nuspec.Id;
            var version = Nuspec.Version;

            var fileName = $"{id}.{version}";

            if (Nuspec.IsSymbolPackage)
            {
                fileName += ".symbols";
            }

            fileName += ".nupkg";

            var nupkgFile = new FileInfo(Path.Combine(outputDir, fileName));

            if (nupkgFile.Exists)
            {
                throw new InvalidOperationException($"File already exists: {nupkgFile.FullName}");
            }

            nupkgFile.Directory.Create();

            using (var zip = new ZipArchive(File.Create(nupkgFile.FullName), ZipArchiveMode.Create))
            {
                foreach (var file in Files)
                {
                    var entry = zip.CreateEntry(file.Path, CompressionLevel.Optimal);

                    using (var stream = entry.Open())
                    {
                        stream.Write(file.Bytes, 0, file.Bytes.Length);
                    }
                }

                var nuspecEntry = zip.CreateEntry($"{id}.nuspec", CompressionLevel.Optimal);

                using (var stream = nuspecEntry.Open())
                {
                    var xml = Nuspec.Create().ToString();

                    var xmlBytes = Encoding.UTF8.GetBytes(xml);

                    stream.Write(xmlBytes, 0, xmlBytes.Length);
                }
            }

            LastSavePath = nupkgFile.FullName;

            return nupkgFile;
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
    }
}