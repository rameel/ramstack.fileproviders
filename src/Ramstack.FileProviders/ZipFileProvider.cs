using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace Ramstack.FileProviders;

/// <summary>
/// Provides access to files within a ZIP archive.
/// </summary>
public sealed class ZipFileProvider : IFileProvider, IDisposable
{
    private readonly ZipArchive _archive;
    private readonly Dictionary<string, IFileInfo> _directories =
        new() { ["/"] = new ZipDirectoryInfo("/") };

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipFileProvider"/> class
    /// using a ZIP archive located at the specified file path.
    /// </summary>
    /// <param name="path">The path to the ZIP archive file.</param>
    public ZipFileProvider(string path)
        : this(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipFileProvider"/> class
    /// using a stream containing a ZIP archive.
    /// </summary>
    /// <param name="stream">The stream containing the ZIP archive.</param>
    /// <param name="leaveOpen"><see langword="true" /> to leave the stream open
    /// after the <see cref="ZipFileProvider"/> object is disposed; otherwise, <see langword="false" />.</param>
    public ZipFileProvider(Stream stream, bool leaveOpen = false)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream does not support seeking.", nameof(stream));

        _archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen);
        Initialize(_archive, _directories);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipFileProvider"/> class
    /// using an existing <see cref="ZipArchive"/>.
    /// </summary>
    /// <param name="archive">The <see cref="ZipArchive"/> instance
    /// to use for providing access to ZIP archive content.</param>
    public ZipFileProvider(ZipArchive archive)
    {
        if (archive.Mode != ZipArchiveMode.Read)
            throw new ArgumentException(
                "Archive must be opened in read mode (ZipArchiveMode.Read).",
                nameof(archive));

        _archive = archive;
        Initialize(archive, _directories);
    }

    /// <inheritdoc />
    public IFileInfo GetFileInfo(string subpath) =>
        Find(subpath) ?? new NotFoundFileInfo(FilePath.GetFileName(subpath));

    /// <inheritdoc />
    public IDirectoryContents GetDirectoryContents(string subpath) =>
        Find(subpath) as IDirectoryContents ?? NotFoundDirectoryContents.Singleton;

    /// <inheritdoc />
    public IChangeToken Watch(string filter) =>
        NullChangeToken.Singleton;

    /// <inheritdoc />
    public void Dispose() =>
        _archive.Dispose();

    private IFileInfo? Find(string path) =>
        _directories.GetValueOrDefault(FilePath.Normalize(path));

    /// <summary>
    /// Initializes the current provider by populating it with entries from the underlying ZIP archive.
    /// </summary>
    private static void Initialize(ZipArchive archive, Dictionary<string, IFileInfo> cache)
    {
        foreach (var entry in archive.Entries)
        {
            //
            // Strip common path prefixes from zip entries to handle archives
            // saved with absolute paths.
            //
            var path = FilePath.Normalize(
                entry.FullName[GetPrefixLength(entry.FullName)..]);

            if (FilePath.HasTrailingSlash(entry.FullName))
            {
                GetDirectory(path);
                continue;
            }

            var directory = GetDirectory(FilePath.GetDirectoryName(path));
            var file = new ZipFileInfo(FilePath.GetFileName(path), entry);

            //
            // Archives legitimately may contain entries with identical names,
            // so skip if a file with this name has already been added,
            // avoiding duplicates in the directory file list.
            //
            if (cache.TryAdd(path, file))
                directory.RegisterFile(file);
        }

        ZipDirectoryInfo GetDirectory(string path)
        {
            if (cache.TryGetValue(path, out var di))
                return (ZipDirectoryInfo)di;

            di = new ZipDirectoryInfo(FilePath.GetFileName(path));
            var parent = GetDirectory(FilePath.GetDirectoryName(path));
            parent.RegisterFile(di);
            cache.Add(path, di);

            return (ZipDirectoryInfo)di;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int GetPrefixLength(string path)
    {
        //
        // Check only well-known prefixes.
        // Note: Since entry names can be arbitrary,
        // we specifically target only common absolute path patterns.
        //

        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(@"\\.\UNC\", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("//?/UNC/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("//./UNC/", StringComparison.OrdinalIgnoreCase))
            return 8;

        if (path.StartsWith(@"\\?\", StringComparison.Ordinal)
            || path.StartsWith(@"\\.\", StringComparison.Ordinal)
            || path.StartsWith("//?/", StringComparison.Ordinal)
            || path.StartsWith("//./", StringComparison.Ordinal))
            return path.Length >= 6 && IsAsciiLetter(path[4]) && path[5] == ':' ? 6 : 4;

        if (path.Length >= 2
            && IsAsciiLetter(path[0]) && path[1] == ':')
            return 2;

        return 0;

        static bool IsAsciiLetter(char ch) =>
            (uint)((ch | 0x20) - 'a') <= 'z' - 'a';
    }

    #region Inner type: ZipDirectoryInfo

    /// <summary>
    /// Represents directory contents and file information within a ZIP archive for the specified path.
    /// This class is used to provide both <see cref="IDirectoryContents"/> and <see cref="IFileInfo"/> interfaces for directory entries in the ZIP archive.
    /// </summary>
    /// <param name="name">The name of the directory within the ZIP archive.</param>
    [DebuggerDisplay("{Name,nq}")]
    [DebuggerTypeProxy(typeof(ZipDirectoryInfoDebuggerProxy))]
    private sealed class ZipDirectoryInfo(string name) : IDirectoryContents, IFileInfo
    {
        /// <summary>
        /// The list of the <see cref="IFileInfo"/> within this directory.
        /// </summary>
        private readonly List<IFileInfo> _files = [];

        /// <inheritdoc cref="IDirectoryContents.Exists" />
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

        /// <inheritdoc />
        public IEnumerator<IFileInfo> GetEnumerator() =>
            _files.AsEnumerable().GetEnumerator();

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            GetEnumerator();

        /// <summary>
        /// Register a file associated with this directory.
        /// </summary>
        /// <param name="file">The file associated with this directory.</param>
        public void RegisterFile(IFileInfo file) =>
            _files.Add(file);
    }

    #endregion

    #region Inner type: ZipFileInfo

    /// <summary>
    /// Represents a file within a ZIP archive as an implementation of the <see cref="IFileInfo"/> interface.
    /// </summary>
    /// <param name="entry">The ZIP archive entry representing the file.</param>
    [DebuggerDisplay("{ToStringDebugger(),nq}")]
    private sealed class ZipFileInfo(string name, ZipArchiveEntry entry) : IFileInfo
    {
        /// <inheritdoc />
        public bool Exists => true;

        /// <inheritdoc />
        public bool IsDirectory => false;

        /// <inheritdoc />
        public DateTimeOffset LastModified => entry.LastWriteTime;

        /// <inheritdoc />
        public long Length => entry.Length;

        /// <inheritdoc />
        public string? PhysicalPath => null;

        /// <inheritdoc />
        public string Name => name;

        /// <inheritdoc />
        public Stream CreateReadStream() =>
            entry.Open();

        private string ToStringDebugger() =>
            entry.FullName;
    }

    #endregion

    #region Inner type: ZipDirectoryInfoDebuggerProxy

    /// <summary>
    /// Represents a debugger proxy for viewing the contents of a <see cref="ZipDirectoryInfo"/> instance in a more user-friendly way during debugging.
    /// </summary>
    /// <param name="directoryInfo">The <see cref="ZipDirectoryInfo"/> instance to provide debugging information for.</param>
    private sealed class ZipDirectoryInfoDebuggerProxy(ZipDirectoryInfo directoryInfo)
    {
        /// <summary>
        /// Gets an array of <see cref="IFileInfo"/> instances representing the files within the associated <see cref="ZipDirectoryInfo"/>.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IFileInfo[] Files { get; } = directoryInfo.ToArray();
    }

    #endregion
}
