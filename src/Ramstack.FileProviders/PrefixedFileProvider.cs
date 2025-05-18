using Ramstack.Globbing;

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
        ArgumentNullException.ThrowIfNull(prefix);
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
        subpath = FilePath.Normalize(subpath);

        var path = ResolvePath(_prefix, subpath);
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

        var path = ResolvePath(_prefix, subpath);
        if (path is not null)
            return _provider.GetDirectoryContents(path);

        return NotFoundDirectoryContents.Singleton;
    }

    /// <inheritdoc />
    public IChangeToken Watch(string filter)
    {
        filter = FilePath.Normalize(filter);

        var path = ResolvePath(_prefix, filter);
        if (path is not null)
            return _provider.Watch(path);

        var pattern = ResolveGlobFilter(_prefix, filter);
        if (pattern is not null)
            return _provider.Watch(pattern);

        return NullChangeToken.Singleton;
    }

    /// <inheritdoc />
    public void Dispose() =>
        (_provider as IDisposable)?.Dispose();

    private static string? ResolvePath(string prefix, string path)
    {
        Debug.Assert(path == FilePath.Normalize(path));

        if (path == prefix)
            return "/";

        if (path.StartsWith(prefix, StringComparison.Ordinal))
            if ((uint)prefix.Length < (uint)path.Length)
                if (path[prefix.Length] == '/')
                    return path[prefix.Length..];

        return null;
    }

    /// <summary>
    /// Attempts to resolve a glob filter relative to a virtual path prefix,
    /// removing any prefix segments that match corresponding parts of the filter.
    /// </summary>
    /// <param name="prefix">The virtual path prefix representing the base of the current provider.</param>
    /// <param name="filter">The incoming glob filter that may include glob patterns.</param>
    /// <returns>
    /// A normalized filter value that can be safely passed to the wrapped file provider
    /// or <see langword="null" /> if the filter cannot be applied.
    /// </returns>
    /// <remarks>
    /// The goal is to determine whether a specified glob filter
    /// (e.g., "/modules/*/{assets,css,js}/**/*.{css,js}") applies to this provider, which is
    /// virtually mounted at a specific prefix path (e.g., "/modules/profile/assets").
    /// </remarks>
    private static string? ResolveGlobFilter(string prefix, string filter)
    {
        Debug.Assert(prefix == FilePath.Normalize(prefix));
        Debug.Assert(filter == FilePath.Normalize(filter));

        var prefixSegments = new PathTokenizer(prefix).GetEnumerator();
        var filterSegments = new PathTokenizer(filter).GetEnumerator();

        var list = new List<string>();

        while (prefixSegments.MoveNext() && filterSegments.MoveNext())
        {
            var fs = filterSegments.Current;

            // The globstar '**' matches any number of remaining segments, including none
            if (fs is "**")
            {
                // Add '**' and all remaining filter segments to the result.
                do
                {
                    var segment = filterSegments.Current.ToString();
                    list.Add(segment);
                }
                while (filterSegments.MoveNext());

                return string.Join("/", list);
            }

            if (fs is "*")
            {
                // '*' matches any prefix segment, continue matching.
                continue;
            }

            if (Matcher.IsMatch(prefixSegments.Current, fs, MatchFlags.Unix))
            {
                // Segment matches the prefix segment, continue matching.
                continue;
            }

            // Segment doesn't match the prefix at all.
            // This means the filter cannot be applied to the underlying provider.
            return null;
        }

        if (!prefixSegments.MoveNext())
        {
            // All prefix segments have been matched and consumed successfully.
            // Append all remaining filter segments.
            while (filterSegments.MoveNext())
                list.Add(filterSegments.Current.ToString());

            return string.Join("/", list);
        }

        // Not all prefix segments were matched.
        // This means the filter cannot be applied to the underlying provider.
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
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
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
