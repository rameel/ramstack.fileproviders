using System.Text;

using Microsoft.Extensions.FileProviders;

namespace Ramstack.FileProviders;

/// <summary>
/// Provides extension methods for the <see cref="IFileInfo"/>.
/// </summary>
public static class FileInfoExtensions
{
    /// <summary>
    /// Returns file contents as readonly stream.
    /// </summary>
    /// <param name="file">The <see cref="IFileInfo"/>.</param>
    /// <returns>
    /// The file contents as readonly stream.
    /// </returns>
    public static Stream OpenRead(this IFileInfo file) =>
        file.CreateReadStream();

    /// <summary>
    /// Returns a <see cref="StreamReader"/> with the specified character encoding that reads from the text file.
    /// </summary>
    /// <param name="file">The <see cref="IFileInfo"/>.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <returns>
    /// A new <see cref="StreamReader"/> with the specified character encoding.
    /// </returns>
    public static StreamReader OpenText(this IFileInfo file, Encoding? encoding = null) =>
        new(file.CreateReadStream(), encoding, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: false);
}
