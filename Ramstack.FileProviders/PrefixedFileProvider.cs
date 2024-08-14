using System.Diagnostics;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Ramstack.FileProviders.Internal;

namespace Ramstack.FileProviders;

/// <summary>
/// Applies a specified prefix to the paths of files managed by an underlying <see cref="IFileProvider"/>.
/// </summary>
/// <remarks>
/// This class wraps another file provider, prepending a prefix to the file paths.
/// </remarks>
[DebuggerDisplay("{_prefix,nq}")]
public sealed class PrefixedFileProvider : IFileProvider, IDisposable
{
    private readonly string _prefix;
    private readonly IFileProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefixedFileProvider"/> class.
    /// </summary>
    /// <param name="prefix">The prefix to be applied to the file paths
    /// managed by this instance.</param>
    /// <param name="provider">The underlying file provider that manages the files
    /// to which the prefix will be applied.</param>
    public PrefixedFileProvider(string prefix, IFileProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        (_prefix, _provider) = (FilePath.GetFullPath(prefix), provider);
    }

    /// <inheritdoc />
    public IFileInfo GetFileInfo(string subpath)
    {
        var path = TryGetPath(subpath, _prefix);
        if (path is not null)
            return _provider.GetFileInfo(path);

        return new NotFoundFileInfo(subpath);
    }

    /// <inheritdoc />
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        var path = TryGetPath(subpath, _prefix);
        if (path is not null)
            return _provider.GetDirectoryContents(path);

        return NotFoundDirectoryContents.Singleton;
    }

    /// <inheritdoc />
    public IChangeToken Watch(string filter)
    {
        var path = TryGetPath(filter, _prefix);
        if (path is not null)
            return _provider.Watch(path);

        return NullChangeToken.Singleton;
    }

    /// <inheritdoc />
    public void Dispose() =>
        (_provider as IDisposable)?.Dispose();

    private static string? TryGetPath(string path, string prefix)
    {
        path = FilePath.GetFullPath(path);
        if (path == prefix)
            return "/";

        if (path.StartsWith(prefix, StringComparison.Ordinal) && path[prefix.Length] == '/')
            return path[prefix.Length..];

        return null;
    }
}
