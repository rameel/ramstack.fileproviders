# Ramstack.FileProviders.Extensions

Represents a lightweight .NET library of useful and convenient extensions for `Microsoft.Extensions.FileProviders`
that enhances file handling capabilities in .NET applications.

## Getting Started

To install the `Ramstack.FileProviders.Extensions` [NuGet package](https://www.nuget.org/packages/Ramstack.FileProviders.Extensions)
in your project, run the following command:
```console
dotnet add package Ramstack.FileProviders.Extensions
```

## Overview

The library provides useful extensions for `IFileProvider`, bringing its capabilities and experience
closer to what's being provided by `DirectoryInfo` and `FileInfo` classes.

Simply stated, a `FileNode` knows which directory it is located in, and a directory represented
by the `DirectoryNode` class can access its parent directory and list all files within it, recursively.

```csharp
FileNode file = provider.GetFile("/docs/README");

// Prints the full path of the given file
Console.WriteLine($"Reading: {file.FullName}");

using StreamReader reader = file.OpenText();
Console.WriteLine(reader.ReadToEnd());
```

```csharp
DirectoryNode directory = provider.GetDirectory("/docs");

foreach (FileNode file in directory.EnumerateFiles())
    Console.WriteLine(file.FullName);
```

Furthermore, the methods for enumerating files (`EnumerateFiles`/`EnumerateDirectories`/`EnumerateFileNodes`)
allow specifying glob patterns to search for the desired files, as well as patterns to exclude files from the resulting list.

It relies on the [Ramstack.Globbing](https://www.nuget.org/packages/Ramstack.Globbing) package for its globbing capabilities.
```csharp
DirectoryNode directory = provider.GetDirectory("/project");

// Finds all *.md files and converts them to HTML
foreach (FileNode file in directory.EnumerateFiles(pattern: "**/*.md"))
    RenderMarkdown(file);

// Excludes files in a specific folder
foreach (FileNode file in directory.EnumerateFiles(pattern: "**/*.md", exclude: "vendors/**"))
    RenderMarkdown(file);
```
For convenience, many methods specific to `DirectoryNode` or `FileNode` are also available for `IFileProvider`.

Thus, if we know the directory in which to look for files or the file to read, there is no need to obtain the
`DirectoryNode` or `FileNode` object.

```csharp
using StreamReader reader = provider.OpenText("/docs/README", Encoding.UTF8);
Console.WriteLine(reader.ReadToEnd());

// Finds all *.md files and converts them to HTML
foreach (FileNode file in provider.EnumerateFiles("/project", pattern: "**/*.md"))
    RenderMarkdown(file);
```

## Related Packages
- [Ramstack.FileProviders](https://www.nuget.org/packages/Ramstack.FileProviders) — Additional file providers.
- [Ramstack.FileProviders.Globbing](https://www.nuget.org/packages/Ramstack.FileProviders.Globbing) — Wraps the file system, filtering files and directories using glob patterns.


## Supported versions

|      | Version |
|------|---------|
| .NET | 6, 7, 8 |

## Contributions

Bug reports and contributions are welcome.

## License
This package is released as open source under the **MIT License**.
See the [LICENSE](https://github.com/rameel/ramstack.fileproviders/blob/main/LICENSE) file for more details.
