# Ramstack.FileProviders.Composition

Represents a .NET library that provides a helper class for flattening and composing `IFileProvider` instances.

## Getting Started

To install the `Ramstack.FileProviders.Composition` [NuGet package](https://www.nuget.org/packages/Ramstack.FileProviders.Composition)
in your project, run the following command:
```console
dotnet add package Ramstack.FileProviders.Composition
```

## Flattening Providers
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

## Composing Providers
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

## Supported versions

|      | Version |
|------|---------|
| .NET | 6, 7, 8 |

## Contributions

Bug reports and contributions are welcome.

## License
This package is released as open source under the **MIT License**.
See the [LICENSE](https://github.com/rameel/ramstack.fileproviders/blob/main/LICENSE) file for more details.
