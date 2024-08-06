using System.Diagnostics;

using Microsoft.Extensions.FileProviders;

using Ramstack.FileProviders.Internal;

namespace Ramstack.FileProviders;

/// <summary>
/// Represents a directory in the specified file provider.
/// </summary>
public sealed class DirectoryNode : FileNodeBase
{
    private readonly IFileInfo? _file;
    private IDirectoryContents? _directory;

    /// <summary>
    /// Gets a value indicating whether the <see cref="FileNodeBase.FullName"/> is a root.
    /// </summary>
    public bool IsRoot => FullName == "/";

    /// <summary>
    /// Gets an <see cref="DirectoryNode"/> instance representing the root directory.
    /// </summary>
    public DirectoryNode Root
    {
        get
        {
            if (IsRoot)
                return this;

            var directory = Provider.GetDirectoryContents("/");
            return new DirectoryNode(Provider, "/", directory);
        }
    }

    /// <summary>
    /// Gets an <see cref="DirectoryNode"/> instance representing the parent directory.
    /// </summary>
    public DirectoryNode? Parent
    {
        get
        {
            if (IsRoot)
                return null;

            var parent = FilePath.GetDirectoryName(FullName);
            var directory = Provider.GetDirectoryContents(parent);
            return new DirectoryNode(Provider, parent, directory);
        }
    }

    /// <inheritdoc />
    public override bool Exists => _file?.Exists ?? _directory!.Exists;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryNode"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/> associated with this directory.</param>
    /// <param name="path">The full path of the directory within the provider.</param>
    /// <param name="file">The <see cref="IFileInfo"/> associated with this directory.</param>
    internal DirectoryNode(IFileProvider provider, string path, IFileInfo file) : base(provider, path)
    {
        Debug.Assert(file.IsDirectory);

        _file = file;
        // https://github.com/dotnet/aspnetcore/pull/50586
        _directory = _file as IDirectoryContents;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryNode"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/> associated with this directory.</param>
    /// <param name="path">The full path of the directory within the provider.</param>
    /// <param name="directory">The <see cref="IDirectoryContents"/> associated with this directory.</param>
    internal DirectoryNode(IFileProvider provider, string path, IDirectoryContents directory) : base(provider, path) =>
        _directory = directory;

    /// <summary>
    /// Returns an enumerable collection of files in the current directory.
    /// </summary>
    /// <returns>
    /// An enumerable collection of files in the current directory.
    /// </returns>
    public IEnumerable<FileNode> EnumerateFiles()
    {
        foreach (var fi in _directory ??= Provider.GetDirectoryContents(FullName))
        {
            if (fi.IsDirectory)
                continue;

            var path = FilePath.Join(FullName, fi.Name);
            yield return new FileNode(Provider, path, fi);
        }
    }

    /// <summary>
    /// Returns an enumerable collection of files in the current directory.
    /// </summary>
    /// <returns>
    /// An enumerable collection of directories in the current directory.
    /// </returns>
    public IEnumerable<DirectoryNode> EnumerateDirectories()
    {
        foreach (var fi in _directory ??= Provider.GetDirectoryContents(FullName))
        {
            if (fi.IsDirectory)
            {
                var path = FilePath.Join(FullName, fi.Name);
                yield return new DirectoryNode(Provider, path, fi);
            }
        }
    }

    /// <summary>
    /// Returns an enumerable collection of file nodes in the current directory.
    /// </summary>
    /// <returns>
    /// An enumerable collection of file nodes in the current directory.
    /// </returns>
    public IEnumerable<FileNodeBase> EnumerateFileNodes()
    {
        foreach (var fi in _directory ??= Provider.GetDirectoryContents(FullName))
        {
            var path = FilePath.Join(FullName, fi.Name);
            yield return fi.IsDirectory
                ? new DirectoryNode(Provider, path, fi)
                : new FileNode(Provider, path, fi);
        }
    }
}
