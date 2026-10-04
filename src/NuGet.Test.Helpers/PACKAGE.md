# NuGet.Test.Helpers

NuGet.Test.Helpers makes it easy to generate NuGet packages and nuspecs in unit and functional tests, for testing libraries that work with NuGet packages and tools that generate NuGet feeds.

## Quick start

```csharp
using (var folder = new TestFolder())
{
    // Create a package with files and dependencies
    var nupkg = TestNupkg.Create("MyPackage", "1.0.0");
    nupkg.AddFile("lib/net8.0/MyPackage.dll");
    nupkg.AddDependency("Newtonsoft.Json", "[13.0.0, )");

    // Save the .nupkg to the temp folder, which is deleted on dispose
    var path = nupkg.Save(folder);
}
```

Use `TestNuspec` to set package metadata such as authors, tags, licenses, repositories, package types, framework references, icons, and readmes, and `TestLogger` to capture NuGet log messages. Use `TestNupkg.ToByteArray` or `TestNupkg.Save(Stream)` to create packages in memory, and `TestNupkg.EntryLastWriteTime` to get the same bytes each time.

## Documentation

* [Usage and API overview](https://github.com/emgarten/NuGet.Test.Helpers#readme)
* [Release notes](https://github.com/emgarten/NuGet.Test.Helpers/blob/main/ReleaseNotes.md)

Source code and issues: [github.com/emgarten/NuGet.Test.Helpers](https://github.com/emgarten/NuGet.Test.Helpers)
