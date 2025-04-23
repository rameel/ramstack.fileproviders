using Ramstack.Globbing;

namespace Ramstack.FileProviders;

/// <summary>
/// Provides a file provider implementation that filters files based on specified glob patterns.
/// </summary>
/// <remarks>
/// The <see cref="GlobbingFileProvider"/> class wraps around another <see cref="IFileProvider"/> and applies glob-based
/// filtering rules to determine, which files to include or exclude. This allows for flexible and powerful file selection
/// using standard glob patterns.
/// </remarks>
/// <example>
/// <code>
/// var underlyingProvider = new PhysicalFileProvider("C:\\MyDirectory");
/// var provider = new GlobbingFileProvider(underlyingProvider, patterns: ["**/*.txt", "docs/*.md"], excludes: ["**/README.md"]);
/// foreach (var file in provider.GetDirectoryContents("/"))
///     Console.WriteLine(file.Name);
/// </code>
/// </example>
public sealed class GlobbingFileProvider : IFileProvider
{
    private readonly IFileProvider _provider;
    private readonly string[] _patterns;
    private readonly string[] _excludes;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobbingFileProvider"/> class.
    /// </summary>
    /// <param name="provider">The underlying file provider.</param>
    /// <param name="pattern">The pattern to include in the enumeration.</param>
    /// <param name="exclude">The optional pattern to exclude from the enumeration.</param>
    public GlobbingFileProvider(IFileProvider provider, string pattern, string? exclude = null)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(pattern);

        _provider = provider;
        _patterns = [pattern];
        _excludes = exclude is not null ? [exclude] : [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobbingFileProvider"/> class.
    /// </summary>
    /// <param name="provider">The underlying file provider.</param>
    /// <param name="patterns">The patterns to include in the enumeration.</param>
    /// <param name="excludes">The optional patterns to exclude from the enumeration.</param>
    public GlobbingFileProvider(IFileProvider provider, string[] patterns, string[]? excludes = null)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(patterns);

        _provider = provider;
        _patterns = [..patterns];
        _excludes = [..excludes ?? []];
    }

    /// <inheritdoc />
    public IFileInfo GetFileInfo(string subpath)
    {
        subpath = FilePath.Normalize(subpath);
        if (!IsExcluded(subpath) && IsIncluded(subpath))
            return _provider.GetFileInfo(subpath);

        return new NotFoundFileInfo(subpath);
    }

    /// <inheritdoc />
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        subpath = FilePath.Normalize(subpath);

        var directory = _provider.GetDirectoryContents(subpath);
        return new GlobbingDirectoryContents(this, subpath, directory);
    }

    /// <inheritdoc />
    public IChangeToken Watch(string filter) =>
        _provider.Watch(filter);

    private bool IsIncluded(string path)
    {
        foreach (var pattern in _patterns)
            if (Matcher.IsMatch(path, pattern, MatchFlags.Unix))
                return true;

        return false;
    }

    private bool IsExcluded(string path)
    {
        foreach (var pattern in _excludes)
            if (Matcher.IsMatch(path, pattern, MatchFlags.Unix))
                return true;

        return false;
    }

    #region Inner type: GlobbingDirectoryContents

    /// <summary>
    /// Represents the contents of a directory with globbing support for matching file patterns.
    /// </summary>
    private sealed class GlobbingDirectoryContents : IDirectoryContents
    {
        private readonly GlobbingFileProvider _provider;
        private readonly string _directoryPath;
        private readonly IDirectoryContents _directory;

        /// <inheritdoc />
        public bool Exists => !_provider.IsExcluded(_directoryPath) && _directory.Exists;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobbingDirectoryContents"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="GlobbingFileProvider"/> providing the files.</param>
        /// <param name="directoryPath">The path to enumerate.</param>
        /// <param name="directory">The underlying directory contents.</param>
        public GlobbingDirectoryContents(GlobbingFileProvider provider, string directoryPath, IDirectoryContents directory)
        {
            _provider = provider;
            _directoryPath = directoryPath;
            _directory = directory;
        }

        /// <inheritdoc />
        public IEnumerator<IFileInfo> GetEnumerator()
        {
            foreach (var file in _directory)
            {
                var path = FilePath.Join(_directoryPath, file.Name);
                if (!_provider.IsExcluded(path))
                    if (file.IsDirectory || _provider.IsIncluded(path))
                        yield return file;
            }
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    #endregion
}
