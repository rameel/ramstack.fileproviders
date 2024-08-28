# Ramstack.FileProviders.Globbing

Represents a .NET library implementing an `IFileProvider` that applies glob-based filtering rules to determine which files to include or exclude.

## Getting Started

To install the `Ramstack.FileProviders.Globbing` [NuGet package](https://www.nuget.org/packages/Ramstack.FileProviders.Globbing)
in your project, run the following command:
```console
dotnet add package Ramstack.FileProviders.Globbing
```

## GlobbingFileProvider
`GlobbingFileProvider` class supports glob pattern matching for file paths, allowing for flexible file selection.
You can specify patterns for both including and excluding files.

It relies on the [Ramstack.Globbing](https://www.nuget.org/packages/Ramstack.Globbing) package for its globbing capabilities.

Example:
```csharp
IFileProvider provider = new GlobbingFileProvider(innerProvider, patterns: ["**/*.txt", "docs/*.md" ], excludes: ["**/README.md"]);
foreach (IFileInfo file in provider.GetDirectoryContents("/"))
    Console.WriteLine(file.Name);
```

## Related Packages
- [Ramstack.FileProviders.Extensions](https://www.nuget.org/packages/Ramstack.FileProviders.Extensions) — Useful and convenient extensions for `IFileProvider`, bringing its capabilities and experience closer to what's provided by the `DirectoryInfo` and `FileInfo` classes.
- [Ramstack.FileProviders](https://www.nuget.org/packages/Ramstack.FileProviders) — Additional file providers, including `ZipFileProvider`, `PrefixedFileProvider`, and `SubFileProvider`.
- [Ramstack.FileProviders.Composition](https://www.nuget.org/packages/Ramstack.FileProviders.Composition) — Provides a helper class for flattening and composing `IFileProvider`.


## Supported versions

|      | Version |
|------|---------|
| .NET | 6, 7, 8 |

## Contributions

Bug reports and contributions are welcome.

## License
This package is released as open source under the **MIT License**.
See the [LICENSE](https://github.com/rameel/ramstack.fileproviders/blob/main/LICENSE) file for more details.
