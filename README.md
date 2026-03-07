# NuGet.Test.Helpers

![icon](/build/icons/icon.png)

Helper library for creating NuGet test packages. The intent of this library is to make it easy to generate NuGet packages and nuspecs in unit and functional tests for testing libraries that work with NuGet packages and NuGet feed generators.

| Github | NuGet |
| --- | --- |
| [![.NET test](https://github.com/emgarten/NuGet.Test.Helpers/actions/workflows/dotnet.yml/badge.svg)](https://github.com/emgarten/NuGet.Test.Helpers/actions/workflows/dotnet.yml) | [![NuGet](https://img.shields.io/nuget/v/NuGet.Test.Helpers.svg)](https://www.nuget.org/packages/NuGet.Test.Helpers/) |

## Getting NuGet.Test.Helpers

Install via the .NET CLI:

```
dotnet add package NuGet.Test.Helpers
```

Or find release builds on [NuGet.org](https://www.nuget.org/packages/NuGet.Test.Helpers).

### Supported frameworks

* net8.0
* net9.0
* net10.0

## Usage

### Creating a test package

```csharp
using (var folder = new TestFolder())
{
    // Create a package with files and dependencies
    var nupkg = TestNupkg.Create("MyPackage", "1.0.0");
    nupkg.AddFile("lib/net8.0/MyPackage.dll");
    nupkg.AddDependency("Newtonsoft.Json", "[13.0.0, )");

    // Save the .nupkg to disk
    var path = nupkg.Save(folder);

    // Use the generated package in your test
    using (var reader = new PackageArchiveReader(path.FullName))
    {
        var identity = reader.GetIdentity();
        // assert, verify, etc.
    }
}
```

### Customizing package metadata

```csharp
var nuspec = new TestNuspec()
{
    Id = "MyPackage",
    Version = "2.0.0-beta.1",
    Authors = "TestAuthor",
    Description = "A test package",
    Tags = "test example"
};

var nupkg = nuspec.CreateNupkg();
nupkg.AddFile("lib/net8.0/MyPackage.dll", fileBytes);
nupkg.Save(outputDir);
```

### Capturing NuGet log output

```csharp
var logger = new TestLogger();

// Pass logger to NuGet APIs...

// Inspect logged messages
var allMessages = logger.GetMessages();
var warnings = logger.GetMessages(LogLevel.Warning);
```

## API overview

| Class | Description |
| --- | --- |
| **TestNupkg** | Creates `.nupkg` package files with configurable files and dependencies |
| **TestNuspec** | Defines package metadata (id, version, authors, dependencies, etc.) |
| **TestNupkgFile** | Represents a file entry inside a `.nupkg` archive |
| **TestFolder** | Creates a temporary directory that auto-cleans on dispose |
| **TestLogger** | In-memory `ILogger` implementation for capturing NuGet log messages |

## Building from source

### Windows

```
build.ps1
```

### Linux / macOS

```
build.sh
```

## Contributing

We welcome contributions. If you are interested in contributing you can report an issue or open a pull request to propose a change.

### License
[MIT License](https://github.com/emgarten/NuGet.Test.Helpers/blob/main/LICENSE)
