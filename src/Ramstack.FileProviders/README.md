# Ramstack.FileProviders

Represents a .NET library that provides additional implementations for `Microsoft.Extensions.FileProviders` including:
- `PrefixedFileProvider`
- `SubFileProvider`
- `ZipFileProvider`

## Getting Started

To install the `Ramstack.FileProviders` [NuGet package](https://www.nuget.org/packages/Ramstack.FileProviders)
in your project, run the following command:

```console
dotnet add package Ramstack.FileProviders
```

## PrefixedFileProvider

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

## SubFileProvider
`SubFileProvider` lets you limit the view of the file system to a specific subdirectory, effectively creating a sandbox.

Example:
```csharp
IFileProvider provider = new SubFileProvider(innerProvider, "/docs");
IFileInfo file = provider.GetFileInfo("/README");
Console.WriteLine(file.Exists);
```

## ZipFileProvider
`ZipFileProvider` enables access to files within ZIP archives as if they were part of the file system.

Example:
```csharp
IFileProvider provider = new ZipFileProvider("/path/to/archive.zip");
foreach (IFileInfo file in provider.GetDirectoryContents("/"))
    Console.WriteLine(file.Name);
```

## Related Packages
- [Ramstack.FileProviders.Extensions](https://www.nuget.org/packages/Ramstack.FileProviders.Extensions) — Useful and convenient extensions for `IFileProvider`, bringing its capabilities and experience closer to what's provided by the `DirectoryInfo` and `FileInfo` classes.
- [Ramstack.FileProviders.Globbing](https://www.nuget.org/packages/Ramstack.FileProviders.Globbing) — A file provider that filters files using include and/or exclude glob patterns. Include patterns make only matching files visible, while exclude patterns hide specific files. Both include and exclude patterns can be combined for flexible file visibility control.
- [Ramstack.FileProviders.Composition](https://www.nuget.org/packages/Ramstack.FileProviders.Composition) — Provides a helper class for flattening and composing `IFileProvider`.

## Supported versions

|      | Version    |
|------|------------|
| .NET | 6, 7, 8, 9 |

## Contributions

Bug reports and contributions are welcome.

## License
This package is released as open source under the **MIT License**.
See the [LICENSE](https://github.com/rameel/ramstack.fileproviders/blob/main/LICENSE) file for more details.
