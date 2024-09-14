using System.Text;

namespace Ramstack.FileProviders;

/// <summary>
/// Provides extension methods for the <see cref="FileNode"/>.
/// </summary>
public static class FileNodeExtensions
{
    /// <summary>
    /// Asynchronously reads all the text in the current file with the specified encoding.
    /// </summary>
    /// <param name="file">The file from which to read the entire text content.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing the full text from the current file.
    /// </returns>
    public static ValueTask<string> ReadAllTextAsync(this FileNode file, CancellationToken cancellationToken = default) =>
        file.ToFileInfo().ReadAllTextAsync(cancellationToken);

    /// <summary>
    /// Asynchronously reads all the text in the current file with the specified encoding.
    /// </summary>
    /// <param name="file">The file from which to read the entire text content.</param>
    /// <param name="encoding">The encoding applied to the contents.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing the full text from the current file.
    /// </returns>
    public static ValueTask<string> ReadAllTextAsync(this FileNode file, Encoding? encoding, CancellationToken cancellationToken = default) =>
        file.ToFileInfo().ReadAllTextAsync(encoding, cancellationToken);

    /// <summary>
    /// Asynchronously reads all lines of the current file.
    /// </summary>
    /// <param name="file">The file to read from.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing an array of all lines in the current file.
    /// </returns>
    public static ValueTask<string[]> ReadAllLinesAsync(this FileNode file, CancellationToken cancellationToken = default) =>
        file.ToFileInfo().ReadAllLinesAsync(cancellationToken);

    /// <summary>
    /// Asynchronously reads all lines of the current file with the specified encoding.
    /// </summary>
    /// <param name="file">The file to read from.</param>
    /// <param name="encoding">The encoding applied to the contents.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing an array of all lines in the current file.
    /// </returns>
    public static ValueTask<string[]> ReadAllLinesAsync(this FileNode file, Encoding encoding, CancellationToken cancellationToken = default) =>
        file.ToFileInfo().ReadAllLinesAsync(encoding, cancellationToken);

    /// <summary>
    /// Asynchronously reads the entire contents of the current file into a byte array.
    /// </summary>
    /// <param name="file">The file to read from.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing an array of the file's bytes.
    /// </returns>
    public static ValueTask<byte[]> ReadAllBytesAsync(this FileNode file, CancellationToken cancellationToken = default) =>
        file.ToFileInfo().ReadAllBytesAsync(cancellationToken);
}
