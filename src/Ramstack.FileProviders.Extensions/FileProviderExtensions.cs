using System.Text;

using Microsoft.Extensions.FileProviders;

using Ramstack.FileProviders.Internal;

namespace Ramstack.FileProviders;

/// <summary>
/// Provides extension methods for the <see cref="IFileProvider"/>.
/// </summary>
public static partial class FileProviderExtensions
{
    /// <summary>
    /// Returns file contents as readonly stream.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path of the file to open.</param>
    /// <returns>
    /// The file contents as readonly stream.
    /// </returns>
    public static Stream OpenRead(this IFileProvider provider, string path) =>
        provider.GetFileInfo(path).OpenRead();

    /// <summary>
    /// Returns a <see cref="StreamReader"/> with the specified character encoding that reads from the current text file.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path of the file to open.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <returns>
    /// A new <see cref="StreamReader"/> with the specified character encoding.
    /// </returns>
    public static StreamReader OpenText(this IFileProvider provider, string path, Encoding? encoding = null) =>
        provider.GetFileInfo(path).OpenText(encoding);

    /// <summary>
    /// Returns a directory with the specified path.
    /// </summary>
    /// <param name="provider">The file provider to retrieve the directory from.</param>
    /// <param name="path">The local path that identifies the directory.</param>
    /// <returns>
    /// The <see cref="DirectoryNode"/> representing the desired directory.
    /// </returns>
    public static DirectoryNode GetDirectory(this IFileProvider provider, string path)
    {
        path = FilePath.GetFullPath(path);

        var directory = provider.GetDirectoryContents(path);
        return new DirectoryNode(provider, path, directory);
    }

    /// <summary>
    /// Returns a file with the specified path.
    /// </summary>
    /// <param name="provider">The file provider to retrieve the file from.</param>
    /// <param name="path">The local path that identifies the file.</param>
    /// <returns>
    /// The <see cref="FileNode"/> representing the desired file.
    /// </returns>
    public static FileNode GetFile(this IFileProvider provider, string path)
    {
        path = FilePath.GetFullPath(path);

        var file = provider.GetFileInfo(path);
        return new FileNode(provider, path, file);
    }
}
