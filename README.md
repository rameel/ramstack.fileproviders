<!-- TOC -->
* [Ramstack.FileProviders](#ramstackfileproviders)
  * [Getting Started](#getting-started)
  * [Overview](#overview)
    * [PrefixedFileProvider](#prefixedfileprovider)
    * [SubFileProvider](#subfileprovider)
    * [GlobbingFileProvider](#globbingfileprovider)
    * [ZipFileProvider](#zipfileprovider)
  * [Supported versions](#supported-versions)
  * [Contributions](#contributions)
  * [License](#license)
<!-- TOC -->

# Ramstack.FileProviders

`Ramstack.FileProviders` is a .NET library that provides useful and convenient extensions for `Microsoft.Extensions.FileProviders`,
enhancing file handling capabilities in .NET applications.

## Getting Started

To install the `Ramstack.FileProviders` [NuGet package](https://www.nuget.org/packages/Ramstack.FileProviders)
in your project, run the following command:
```console
dotnet add package Ramstack.FileProviders
```

## Overview

The library offers a set of additional implementations of the [IFileProvider](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.fileproviders.ifileprovider)
interface:
- `SubFileProvider`
- `PrefixedFileProvider`
- `ZipFileProvider`
- `GlobbingFileProvider`

Additionally, the library provides useful extensions for `IFileProvider`, bringing its capabilities and experience
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
allow specifying glob patterns to search for the desired files, as well as patterns to exclude files
from the resulting list.
```csharp
DirectoryNode directory = provider.GetDirectory("/project");

// Finds all *.md files and converts them to HTML
foreach (FileNode file in directory.EnumerateFiles(pattern: "**/*.md"))
    RenderMarkdown(file);

// Exclude files in a specific folder
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

### PrefixedFileProvider

`PrefixedFileProvider` allows you to apply a prefix to the paths of files and directories.
This is useful when you need to organize files in a virtual hierarchy.

Example:
```csharp
IFileProvider provider = new PrefixedFileProvider(innerProvider, "/project/app");
IFileInfo file = provider.GetFileInfo("/project/app/docs/README");
Console.WriteLine(file.Exists);
```

This is how you can add virtual directories to your project that are external to the project root:
```csharp
string packagesPath = Path.Combine(environment.ContentRootPath, "../Packages");
string themesPath   = Path.Combine(environment.ContentRootPath, "../Themes");

environment.ContentRootFileProvider = new CompositeFileProvider(
    new PrefixedFileProvider("/Packages", new PhysicalFileProvider(packagesPath)),
    new PrefixedFileProvider("/Themes",   new PhysicalFileProvider(themesPath)),
    environment.ContentRootFileProvider);
```
The `Packages` and `Themes` directories are now available to the ASP.NET infrastructure under their respective names,
as if they were originally defined within your project.

**Before:**
```
/App
├── Controllers
├── Models
├── Views
└── wwwroot

/Packages
├── package-1
└── package-2

/Themes
├── theme-1
└── theme-2
```

**After:**
```
/App
├── Controllers
├── Models
├── Views
├── Packages         <-- (virtual)
│   ├── package1
│   └── package2
├── Themes           <-- (virtual)
│   ├── theme1
│   └── theme2
└── wwwroot
```

### SubFileProvider
`SubFileProvider` lets you limit the view of the file system to a specific subdirectory, effectively creating a sandbox.

Example:
```csharp
IFileProvider provider = new SubFileProvider(innerProvider, "/docs");
IFileInfo file = provider.GetFileInfo("/README");
Console.WriteLine(file.Exists);
```

### GlobbingFileProvider
`GlobbingFileProvider` supports glob pattern matching for file paths, allowing for flexible file selection.
You can specify patterns for both including and excluding files.
It relies on the [Ramstack.Globbing](https://www.nuget.org/packages/Ramstack.Globbing) project for its globbing capabilities.

Example:
```csharp
IFileProvider provider = new GlobbingFileProvider(innerProvider, patterns: ["**/*.txt", "docs/*.md" ], excludes: ["**/README.md"]);
foreach (FileNode file in provider.EnumerateFiles("/"))
    Console.WriteLine(file.Name);
```

### ZipFileProvider
`ZipFileProvider` enables access to files within ZIP archives as if they were part of the file system.

Example:
```csharp
IFileProvider provider = new ZipFileProvider("/path/to/archive.zip");
foreach (FileNode file in provider.EnumerateFiles("/", "**/*.md"))
    Console.WriteLine(file.FullName);
```

## Supported versions

|      | Version |
|------|---------|
| .NET | 6, 7, 8 |

## Contributions

Bug reports and contributions are welcome.

## License
This package is released as open source under the **MIT License**. See the [LICENSE](LICENSE) file for more details.
