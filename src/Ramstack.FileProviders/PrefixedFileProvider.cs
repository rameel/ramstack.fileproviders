using System.Collections;
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
    private readonly (string Path, string DirectoryName)[] _directories;

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

        prefix = FilePath.Normalize(prefix);
        (_prefix, _provider, _directories) = (prefix, provider, CreateArtificialDirectories(prefix));

        static (string Path, string DirectoryName)[] CreateArtificialDirectories(string path)
        {
            var directories = new List<(string, string)>();

            while (path != "/")
            {
                var directoryName = FilePath.GetFileName(path);
                path = FilePath.GetDirectoryName(path);
                directories.Add((path, directoryName));
            }

            return directories.ToArray();
        }
    }

    /// <inheritdoc />
    public IFileInfo GetFileInfo(string subpath)
    {
        var path = ResolvePath(FilePath.Normalize(subpath), _prefix);
        if (path is not null)
            return _provider.GetFileInfo(path);

        return new NotFoundFileInfo(subpath);
    }

    /// <inheritdoc />
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        subpath = FilePath.Normalize(subpath);

        if (subpath.Length < _prefix.Length)
            foreach (ref var entry in _directories.AsSpan())
                if (entry.Path == subpath)
                    return new ArtificialDirectoryContents(entry.DirectoryName);

        var path = ResolvePath(subpath, _prefix);
        if (path is not null)
            return _provider.GetDirectoryContents(path);

        return NotFoundDirectoryContents.Singleton;
    }

    /// <inheritdoc />
    public IChangeToken Watch(string filter)
    {
        var path = ResolvePath(FilePath.Normalize(filter), _prefix);
        if (path is not null)
            return _provider.Watch(path);

        return NullChangeToken.Singleton;
    }

    /// <inheritdoc />
    public void Dispose() =>
        (_provider as IDisposable)?.Dispose();

    private static string? ResolvePath(string path, string prefix)
    {
        Debug.Assert(path == FilePath.Normalize(path));

        if (path == prefix)
            return "/";

        if (path.StartsWith(prefix, StringComparison.Ordinal) && path[prefix.Length] == '/')
            return path[prefix.Length..];

        return null;
    }

    #region Inner type: ArtificialDirectoryContents

    /// <summary>
    /// Represents an implementation of the <see cref="IDirectoryContents"/>.
    /// </summary>
    /// <param name="name">The name of the child directory.</param>
    private sealed class ArtificialDirectoryContents(string name) : IDirectoryContents
    {
        /// <inheritdoc />
        public bool Exists => true;

        /// <inheritdoc />
        public IEnumerator<IFileInfo> GetEnumerator()
        {
            var result = new IFileInfo[] { new ArtificialDirectoryInfo(name) };
            return result.AsEnumerable().GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    #endregion

    #region Inner type: ArtificialDirectoryInfo

    /// <summary>
    /// Represents an implementation of the <see cref="IFileInfo"/> for the artificial directory.
    /// </summary>
    /// <param name="name">The name of the directory.</param>
    private sealed class ArtificialDirectoryInfo(string name) : IFileInfo
    {
        /// <inheritdoc />
        public bool Exists => true;

        /// <inheritdoc />
        public long Length => -1;

        /// <inheritdoc />
        public string? PhysicalPath => null;

        /// <inheritdoc />
        public string Name => name;

        /// <inheritdoc />
        public DateTimeOffset LastModified => default;

        /// <inheritdoc />
        public bool IsDirectory => true;

        /// <inheritdoc />
        public Stream CreateReadStream() =>
            throw new NotSupportedException("Cannot create a read stream for a directory.");
    }

    #endregion
}
