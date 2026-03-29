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

        if (prefix == "/")
            return path;

        if (path == prefix)
            return "/";

        if (path.StartsWith(prefix, StringComparison.Ordinal))
            if ((uint)prefix.Length < (uint)path.Length)
                if (path[prefix.Length] == '/')
                    return new string(path.AsSpan(prefix.Length));

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

        while (prefixSegments.MoveNext() && filterSegments.MoveNext())
        {
            var fs = filterSegments.Current;

            if (fs is "**")
            {
                // The globstar '**' matches zero or more path segments.
                // Once we encounter '**', we lose the ability to deterministically align
                // the remaining filter segments with the remaining prefix segments.
                //
                // Why this matters:
                //   We are transforming a filter defined over the 'outer' virtual path
                //   into a filter for the 'inner' provider (mounted at 'prefix').
                //   To do that precisely, we would need to know how many segments '**' consumes.
                //
                // However, this is fundamentally ambiguous:
                //   - '**' may consume 0 segments
                //   - '**' may consume N segments (including prefix tail segments)
                //   - or it may match entirely within the underlying provider
                //
                // Example (false negative if we over-reduce):
                //   prefix: /modules/profile/assets
                //   filter: /modules/**/assets/*.js
                //
                //   Underlying provider may contain:
                //     /src/_build/assets/main.js
                //
                //   Which corresponds to:
                //     /modules/profile/assets/src/_build/assets/main.js
                //
                //   In this case:
                //     '**/assets/*.js' --> MUST match
                //     '*.js'           --> would NOT match
                //
                // Counter-example (false negative if we try to keep prefix tail):
                //   prefix: /modules/profile/assets
                //   filter: /modules/**/assets/*.js
                //
                //   Suppose 'assets' in the filter refers to the *prefix itself*,
                //   and the underlying provider contains only flat files:
                //     /main.js
                //
                //   (i.e. no nested 'assets/' directory inside the provider)
                //
                //   Then:
                //     '*.js'             --> MUST match
                //     '**/assets/*.js'   --> would NOT match
                //
                // Conclusion:
                //   After '**', we cannot know whether subsequent segments belong
                //   to the prefix or to the underlying provider.
                //
                // Therefore, any attempt to:
                //   - consume prefix segments (--> '*.js')
                //   - or preserve intermediate literals (--> '**/assets/*.js')
                //   will break valid scenarios.
                //
                // Strategy:
                //   - Preserve '**' to allow arbitrary depth
                //   - Drop ambiguous intermediate segments
                //   - Keep only the final segment if it is a file pattern (e.g. '*.js')
                //
                // This guarantees:
                //   - No false negatives caused by prefix misalignment
                //   - Possible false positives, which are acceptable for Watch()
                while (filterSegments.MoveNext())
                    fs = filterSegments.Current;

                return fs is not "**"
                    ? "**/" + fs.ToString()
                    : "**";
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
            var list = new List<string>();

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
