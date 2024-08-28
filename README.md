<!-- TOC -->
* [Ramstack.FileProviders](#ramstackfileproviders)
  * [Projects](#projects)
    * [Ramstack.FileProviders.Extensions](#ramstackfileprovidersextensions)
    * [Ramstack.FileProviders](#ramstackfileproviders-1)
    * [Ramstack.FileProviders.Globbing](#ramstackfileprovidersglobbing)
    * [Ramstack.FileProviders.Composition](#ramstackfileproviderscomposition)
  * [Overview](#overview)
    * [Ramstack.FileProviders](#ramstackfileproviders-2)
      * [PrefixedFileProvider](#prefixedfileprovider)
      * [SubFileProvider](#subfileprovider)
      * [ZipFileProvider](#zipfileprovider)
    * [Ramstack.FileProviders.Globbing](#ramstackfileprovidersglobbing-1)
    * [Ramstack.FileProviders.Extensions](#ramstackfileprovidersextensions-1)
    * [Ramstack.FileProviders.Composition](#ramstackfileproviderscomposition-1)
      * [Flattening Providers](#flattening-providers)
      * [Composing Providers](#composing-providers)
  * [NuGet Packages](#nuget-packages)
  * [Supported versions](#supported-versions)
  * [Contributions](#contributions)
  * [License](#license)
<!-- TOC -->

# Ramstack.FileProviders

`Ramstack.FileProviders` is a collection of lightweight .NET libraries that enhance file handling capabilities in .NET applications,
building upon `Microsoft.Extensions.FileProviders`.

## Projects

This repository contains projects:

### Ramstack.FileProviders.Extensions
Offers useful and convenient extensions for `IFileProviders`, bringing its capabilities and experience
closer to what's provided by the `DirectoryInfo` and `FileInfo` standard classes.

To install the `Ramstack.FileProviders.Extensions` [NuGet package](https://www.nuget.org/packages/Ramstack.FileProviders.Extensions) in your project,
run the following command:
```console
dotnet add package Ramstack.FileProviders.Extensions
```

### Ramstack.FileProviders
Provides additional implementations of `IFileProvider` including `PrefixedFileProvider`, `SubFileProvider`, and `ZipFileProvider`.

To install the `Ramstack.FileProviders` [NuGet package](https://www.nuget.org/packages/Ramstack.FileProviders) in your project,
run the following command:
```console
dotnet add package Ramstack.FileProviders
```

### Ramstack.FileProviders.Globbing
Provides an implementation of the `IFileProvider` that filters files using include and/or exclude glob patterns
for flexible file visibility control.

To install the `Ramstack.FileProviders.Globbing` [NuGet package](https://www.nuget.org/packages/Ramstack.FileProviders.Globbing) in your project,
run the following command:
```console
dotnet add package Ramstack.FileProviders.Globbing
```

### Ramstack.FileProviders.Composition
Provides a helper class for flattening and composing `IFileProvider` instances.

To install the `Ramstack.FileProviders.Composition` [NuGet package](https://www.nuget.org/packages/Ramstack.FileProviders.Composition) in your project,
run the following command:
```console
dotnet add package Ramstack.FileProviders.Composition
```

## Overview

### Ramstack.FileProviders

This library offers additional implementations of the [IFileProvider](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.fileproviders.ifileprovider) interface:

- `SubFileProvider`
- `PrefixedFileProvider`
- `ZipFileProvider`

#### PrefixedFileProvider

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

#### SubFileProvider

`SubFileProvider` lets you limit the view of the file system to a specific subdirectory, effectively creating a sandbox.

Example:
```csharp
IFileProvider provider = new SubFileProvider(innerProvider, "/docs");
IFileInfo file = provider.GetFileInfo("/README");
Console.WriteLine(file.Exists);
```

#### ZipFileProvider

`ZipFileProvider` enables access to files within ZIP archives as if they were part of the file system.

Example:
```csharp
IFileProvider provider = new ZipFileProvider("/path/to/archive.zip");
foreach (IFileInfo file in provider.GetDirectoryContents("/"))
    Console.WriteLine(file.Name);
```

### Ramstack.FileProviders.Globbing

`GlobbingFileProvider` class filters files using include and/or exclude glob patterns. Include patterns make only matching files visible,
while exclude patterns hide specific files. Both include and exclude patterns can be combined for flexible file visibility control.

It relies on the [Ramstack.Globbing](https://www.nuget.org/packages/Ramstack.Globbing) package for its globbing capabilities.

Example:
```csharp
IFileProvider provider = new GlobbingFileProvider(innerProvider, patterns: ["**/*.txt", "docs/*.md" ], excludes: ["**/README.md"]);
foreach (IFileInfo file in provider.GetDirectoryContents("/"))
    Console.WriteLine(file.Name);
```

### Ramstack.FileProviders.Extensions

Provides useful extensions for `IFileProvider`, bringing its capabilities and experience closer to what's being
provided by `DirectoryInfo` and `FileInfo` classes.

Simply stated, a `FileNode` knows which directory it is located in, and a directory represented by the `DirectoryNode` class can access
its parent directory and list all files within it, recursively.

```csharp
using Ramstack.FileProviders;

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

Furthermore, the methods for enumerating files (`EnumerateFiles`/`EnumerateDirectories`/`EnumerateFileNodes`) allow specifying glob patterns
to search for the desired files, as well as patterns to exclude files from the resulting list.

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

### Ramstack.FileProviders.Composition

Provides a helper class `FileProviderComposer` for flattening and composing `IFileProvider` instances.

#### Flattening Providers

The `FlattenProvider` method attempts to flatten a given `IFileProvider` into a single list of file providers.

This is especially useful when dealing with nested `CompositeFileProvider` instances, which might have been created during
different stages of a pipeline or configuration. Flattening helps in removing unnecessary indirectness and improving efficiency
by consolidating all file providers into a single level.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Application pipeline configuration
...

builder.Environment.ContentRootFileProvider = FileProviderComposer.FlattenProvider(
    builder.Environment.ContentRootFileProvider);
```

#### Composing Providers

The `ComposeProviders` method combines a list of `IFileProvider` instances into a single `IFileProvider`.
During this process, all encountered `CompositeFileProvider` instances recursively flattened and merged into a single level.
This eliminates unnecessary indirectness and streamline the file provider hierarchy.

```csharp
string packagesPath = Path.Combine(environment.ContentRootPath, "../Packages");
string themesPath   = Path.Combine(environment.ContentRootPath, "../Themes");

environment.ContentRootFileProvider = FileProviderComposer.ComposeProviders(
    // Inject external Modules directory
    new PrefixedFileProvider("/Packages", new PhysicalFileProvider(packagesPath)),

    // Inject external Themes directory
    new PrefixedFileProvider("/Themes", new PhysicalFileProvider(themesPath)),

    // Current provider
    environment.ContentRootFileProvider);
```

In this example, the `ComposeProviders` method handles any unnecessary nesting that might occur, including when the current
`environment.ContentRootFileProvider` is a `CompositeFileProvider`. This ensures that all file providers merged into a single
flat structure, avoiding unnecessary indirectness.


## NuGet Packages
- [Ramstack.FileProviders.Extensions](https://www.nuget.org/packages/Ramstack.FileProviders.Extensions) — Useful and convenient extensions for `IFileProvider`, bringing its capabilities and experience closer to what's provided by the `DirectoryInfo` and `FileInfo` classes.
- [Ramstack.FileProviders](https://www.nuget.org/packages/Ramstack.FileProviders) — Additional file providers, including `ZipFileProvider`, `PrefixedFileProvider`, and `SubFileProvider`.
- [Ramstack.FileProviders.Globbing](https://www.nuget.org/packages/Ramstack.FileProviders.Globbing) — A file provider that filters files using include and/or exclude glob patterns. Include patterns make only matching files visible, while exclude patterns hide specific files. Both include and exclude patterns can be combined for flexible file visibility control.
- [Ramstack.FileProviders.Composition](https://www.nuget.org/packages/Ramstack.FileProviders.Composition) — Provides a helper class for flattening and composing `IFileProvider`.

## Supported versions

|      | Version |
|------|---------|
| .NET | 6, 7, 8 |

## Contributions

Bug reports and contributions are welcome.

## License

This project is released as open source under the **MIT License**.
See the [LICENSE](https://github.com/rameel/ramstack.fileproviders/blob/main/LICENSE) file for more details.
