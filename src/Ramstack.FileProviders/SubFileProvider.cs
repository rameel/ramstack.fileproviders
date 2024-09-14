using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Ramstack.FileProviders.Internal;

namespace Ramstack.FileProviders;

/// <summary>
/// Represents a file provider that manages a subset of files
/// under a specified path in an underlying <see cref="IFileProvider"/>.
/// </summary>
/// <remarks>
/// This class provides functionality to handle files and directories that are located under
/// a specific path within the root directory of the underlying file provider.
/// </remarks>
[DebuggerDisplay("{_path,nq}")]
public sealed class SubFileProvider : IFileProvider, IDisposable
{
    private readonly IFileProvider _provider;
    private readonly string _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFileProvider"/> class.
    /// </summary>
    /// <param name="path">The path under the root directory of the <paramref name="provider"/>.</param>
    /// <param name="provider">The underlying file provider.</param>
    public SubFileProvider(string path, IFileProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        (_path, _provider) = (FilePath.Normalize(path), provider);
    }

    /// <inheritdoc />
    public IFileInfo GetFileInfo(string subpath) =>
        _provider.GetFileInfo(ResolvePath(subpath));

    /// <inheritdoc />
    public IDirectoryContents GetDirectoryContents(string subpath) =>
        _provider.GetDirectoryContents(ResolvePath(subpath));

    /// <inheritdoc />
    public IChangeToken Watch(string filter)
    {
        if (!string.IsNullOrEmpty(filter))
            return _provider.Watch(ResolvePath(filter));

        return NullChangeToken.Singleton;
    }

    /// <inheritdoc />
    public void Dispose() =>
        (_provider as IDisposable)?.Dispose();

    private string ResolvePath(string subpath)
    {
        if (subpath.Length == 0 || subpath == "/")
            return _path;

        if (FilePath.IsNavigatesAboveRoot(subpath))
            Error_InvalidPath();

        return FilePath.Join(_path, subpath);
    }

    [DoesNotReturn]
    private static void Error_InvalidPath() =>
        throw new ArgumentException("Invalid path.");
}
