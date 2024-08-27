using System.Collections;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Ramstack.FileProviders.Internal;

namespace Ramstack.FileProviders.Utilities;

public sealed class PlainFileProvider : IFileProvider
{
    private readonly string _path;
    private readonly Dictionary<string, IFileInfo> _files;

    public PlainFileProvider(IEnumerable<IFileInfo> files) : this("/", files)
    {
    }

    public PlainFileProvider(string path, IEnumerable<IFileInfo> files)
    {
        path = FilePath.GetFullPath(path);
        var dictionary = new Dictionary<string, IFileInfo>();

        foreach (var fi in files)
        {
            if (fi.IsDirectory)
                Error_FileExpected(fi);

            var filename = FilePath.Join(path, fi.Name);
            if (!dictionary.TryAdd(filename, fi))
                Error_AlreadyExists(fi);
        }

        (_path, _files) = (path, dictionary);
    }

    /// <inheritdoc />
    public IFileInfo GetFileInfo(string subpath)
    {
        if (_files.TryGetValue(subpath, out var file)
            || _files.TryGetValue(FilePath.GetFullPath(subpath), out file))
            return file;

        return new NotFoundFileInfo(subpath);
    }

    /// <inheritdoc />
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        if (_path == subpath || _path == FilePath.GetFullPath(subpath))
            return new DirectoryContents(_files.Values);

        return NotFoundDirectoryContents.Singleton;
    }

    /// <inheritdoc />
    public IChangeToken Watch(string? filter) =>
        NullChangeToken.Singleton;

    [DoesNotReturn]
    private static void Error_FileExpected(IFileInfo file) =>
        throw new InvalidOperationException($"The '{file.Name}' is not a file");

    [DoesNotReturn]
    private static void Error_AlreadyExists(IFileInfo file) =>
        throw new InvalidOperationException($"The file '{file.Name}' is already exists");

    #region Inner type: DirectoryContents

    /// <summary>
    /// Represents a directory's content that returns the specified list of files.
    /// </summary>
    /// <param name="files">The list of files.</param>
    private sealed class DirectoryContents(IEnumerable<IFileInfo> files) : IDirectoryContents
    {
        /// <inheritdoc />
        public bool Exists => true;

        /// <inheritdoc />
        public IEnumerator<IFileInfo> GetEnumerator() =>
            files.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() =>
            files.GetEnumerator();
    }

    #endregion
}
