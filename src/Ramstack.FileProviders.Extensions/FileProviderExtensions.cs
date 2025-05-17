using System.Text;

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
    /// Reads all the text in the file with the specified encoding.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path of the file to read from.</param>
    /// <param name="encoding">The encoding applied to the contents.</param>
    /// <returns>
    /// A string containing all text in the file.
    /// </returns>
    public static string ReadAllText(this IFileProvider provider, string path, Encoding? encoding = null) =>
        provider.GetFileInfo(path).ReadAllText(encoding);

    /// <summary>
    /// Reads all lines of the file with the specified encoding.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path of the file to read from.</param>
    /// <param name="encoding">The encoding applied to the contents.</param>
    /// <returns>
    /// A string array containing all lines of the file.
    /// </returns>
    public static string[] ReadAllLines(this IFileProvider provider, string path, Encoding? encoding = null) =>
        provider.GetFileInfo(path).ReadAllLines(encoding);

    /// <summary>
    /// Reads the entire contents of the current file into a byte array.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path of the file to read from.</param>
    /// <returns>
    /// A byte array containing the contents of the file.
    /// </returns>
    public static byte[] ReadAllBytes(this IFileProvider provider, string path) =>
        provider.GetFileInfo(path).ReadAllBytes();

    /// <summary>
    /// Asynchronously reads all the text in the current file with the specified encoding.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path of the file to read from.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing the full text from the current file.
    /// </returns>
    public static ValueTask<string> ReadAllTextAsync(this IFileProvider provider, string path, CancellationToken cancellationToken = default) =>
        provider.GetFileInfo(path).ReadAllTextAsync(cancellationToken);

    /// <summary>
    /// Asynchronously reads all the text in the current file with the specified encoding.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path of the file to read from.</param>
    /// <param name="encoding">The encoding applied to the contents.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing the full text from the current file.
    /// </returns>
    public static ValueTask<string> ReadAllTextAsync(this IFileProvider provider, string path, Encoding? encoding, CancellationToken cancellationToken = default) =>
        provider.GetFileInfo(path).ReadAllTextAsync(encoding, cancellationToken);

    /// <summary>
    /// Asynchronously reads all lines of the current file.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path of the file to read from.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing an array of all lines in the current file.
    /// </returns>
    public static ValueTask<string[]> ReadAllLinesAsync(this IFileProvider provider, string path, CancellationToken cancellationToken = default) =>
        provider.GetFileInfo(path).ReadAllLinesAsync(cancellationToken);

    /// <summary>
    /// Asynchronously reads all lines of the current file with the specified encoding.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path of the file to read from.</param>
    /// <param name="encoding">The encoding applied to the contents.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing an array of all lines in the current file.
    /// </returns>
    public static ValueTask<string[]> ReadAllLinesAsync(this IFileProvider provider, string path, Encoding? encoding, CancellationToken cancellationToken = default) =>
        provider.GetFileInfo(path).ReadAllLinesAsync(encoding, cancellationToken);

    /// <summary>
    /// Asynchronously reads the entire contents of the current file into a byte array.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path of the file to read from.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing an array of the file's bytes.
    /// </returns>
    public static ValueTask<byte[]> ReadAllBytesAsync(this IFileProvider provider, string path, CancellationToken cancellationToken = default) =>
        provider.GetFileInfo(path).ReadAllBytesAsync(cancellationToken);

    /// <summary>
    /// Returns a directory with the specified path.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path that identifies the directory.</param>
    /// <returns>
    /// The <see cref="DirectoryNode"/> representing the desired directory.
    /// </returns>
    public static DirectoryNode GetDirectory(this IFileProvider provider, string path)
    {
        path = FilePath.Normalize(path);

        var directory = provider.GetDirectoryContents(path);
        return new DirectoryNode(provider, path, directory);
    }

    /// <summary>
    /// Returns a file with the specified path.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/>.</param>
    /// <param name="path">The path that identifies the file.</param>
    /// <returns>
    /// The <see cref="FileNode"/> representing the desired file.
    /// </returns>
    public static FileNode GetFile(this IFileProvider provider, string path)
    {
        path = FilePath.Normalize(path);

        var file = provider.GetFileInfo(path);
        return new FileNode(provider, path, file);
    }
}
