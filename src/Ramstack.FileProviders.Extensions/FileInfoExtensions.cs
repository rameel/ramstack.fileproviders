using System.Buffers;
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
    public static StreamReader OpenText(this IFileInfo file, Encoding? encoding) =>
        new StreamReader(file.CreateReadStream(), encoding, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: false);

    /// <summary>
    /// Reads all the text in the file with the specified encoding.
    /// </summary>
    /// <param name="file">The file from which to read the entire text content.</param>
    /// <param name="encoding">The encoding applied to the contents.</param>
    /// <returns>
    /// A string containing all text in the file.
    /// </returns>
    public static string ReadAllText(this IFileInfo file, Encoding? encoding = null)
    {
        using var reader = file.OpenText(encoding);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Reads all lines of the file with the specified encoding.
    /// </summary>
    /// <param name="file">The file to read from.</param>
    /// <param name="encoding">The encoding applied to the contents.</param>
    /// <returns>
    /// A string array containing all lines of the file.
    /// </returns>
    public static string[] ReadAllLines(this IFileInfo file, Encoding? encoding = null)
    {
        var stream = file.OpenRead();
        using var reader = new StreamReader(stream, encoding!);

        var list = new List<string>();
        while (reader.ReadLine() is { } line)
            list.Add(line);

        return list.ToArray();
    }

    /// <summary>
    /// Reads the entire contents of the current file into a byte array.
    /// </summary>
    /// <param name="file">The file to read from.</param>
    /// <returns>
    /// A byte array containing the contents of the file.
    /// </returns>
    public static byte[] ReadAllBytes(this IFileInfo file)
    {
        // ReSharper disable once UseAwaitUsing
        using var stream = file.OpenRead();

        var length = GetStreamLength(stream);
        if (length > Array.MaxLength)
            throw new IOException("The file is too large.");

        // https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Private.CoreLib/src/System/IO/File.cs#L660
        // Some file systems (e.g. procfs on Linux) return 0 for length even when there's content
        // Thus we need to assume 0 doesn't mean empty.
        return length <= 0
            ? ReadAllBytesUnknownLengthImpl(stream)
            : ReadAllBytesImpl(stream);

        static byte[] ReadAllBytesImpl(Stream stream)
        {
            var bytes = new byte[stream.Length];
            var index = 0;
            do
            {
                var count = stream.Read(bytes.AsSpan(index));
                if (count == 0)
                    Error_EndOfStream();

                index += count;
            }
            while (index < bytes.Length);

            return bytes;
        }

        static byte[] ReadAllBytesUnknownLengthImpl(Stream stream)
        {
            var bytes = ArrayPool<byte>.Shared.Rent(512);
            var index = 0;

            try
            {
                while (true)
                {
                    if (index == bytes.Length)
                        bytes = ResizeBuffer(bytes);

                    var count = stream.Read(bytes.AsSpan(index));
                    if (count == 0)
                        return bytes.AsSpan(0, index).ToArray();

                    index += count;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }
    }

    /// <summary>
    /// Asynchronously reads all the text in the current file with the specified encoding.
    /// </summary>
    /// <param name="file">The file from which to read the entire text content.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing the full text from the current file.
    /// </returns>
    public static ValueTask<string> ReadAllTextAsync(this IFileInfo file, CancellationToken cancellationToken = default) =>
        ReadAllTextAsync(file, Encoding.UTF8, cancellationToken);

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
    public static async ValueTask<string> ReadAllTextAsync(this IFileInfo file, Encoding? encoding, CancellationToken cancellationToken = default)
    {
        const int BufferSize = 4096;

        var stream = file.OpenRead();
        var reader = new StreamReader(stream, encoding ??= Encoding.UTF8);
        var buffer = (char[]?)null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            buffer = ArrayPool<char>.Shared.Rent(encoding.GetMaxCharCount(BufferSize));
            var sb = new StringBuilder();

            while (true)
            {
                var count = await reader
                    .ReadAsync(new Memory<char>(buffer), cancellationToken)
                    .ConfigureAwait(false);

                if (count == 0)
                    return sb.ToString();

                sb.Append(buffer.AsSpan(0, count));
            }
        }
        finally
        {
            reader.Dispose();

            if (buffer is not null)
                ArrayPool<char>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Asynchronously reads all lines of the current file.
    /// </summary>
    /// <param name="file">The file to read from.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing an array of all lines in the current file.
    /// </returns>
    public static ValueTask<string[]> ReadAllLinesAsync(this IFileInfo file, CancellationToken cancellationToken = default) =>
        ReadAllLinesAsync(file, Encoding.UTF8, cancellationToken);

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
    public static async ValueTask<string[]> ReadAllLinesAsync(this IFileInfo file, Encoding? encoding, CancellationToken cancellationToken = default)
    {
        var stream = file.OpenRead();
        using var reader = new StreamReader(stream, encoding!);

        var list = new List<string>();
        while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();
            list.Add(line);
        }

        return list.ToArray();
    }

    /// <summary>
    /// Asynchronously reads the entire contents of the current file into a byte array.
    /// </summary>
    /// <param name="file">The file to read from.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing an array of the file's bytes.
    /// </returns>
    public static async ValueTask<byte[]> ReadAllBytesAsync(this IFileInfo file, CancellationToken cancellationToken = default)
    {
        // ReSharper disable once UseAwaitUsing
        using var stream = file.OpenRead();

        var length = GetStreamLength(stream);
        if (length > Array.MaxLength)
            throw new IOException("The file is too large.");

        // https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Private.CoreLib/src/System/IO/File.cs#L660
        // Some file systems (e.g. procfs on Linux) return 0 for length even when there's content.
        // Thus we need to assume 0 doesn't mean empty.
        var task = length <= 0
            ? ReadAllBytesUnknownLengthImplAsync(stream, cancellationToken)
            : ReadAllBytesImplAsync(stream, cancellationToken);

        return await task.ConfigureAwait(false);

        static async ValueTask<byte[]> ReadAllBytesImplAsync(Stream stream, CancellationToken cancellationToken)
        {
            var bytes = new byte[stream.Length];
            var index = 0;
            do
            {
                var count = await stream.ReadAsync(bytes.AsMemory(index), cancellationToken).ConfigureAwait(false);
                if (count == 0)
                    Error_EndOfStream();

                index += count;
            }
            while (index < bytes.Length);

            return bytes;
        }

        static async ValueTask<byte[]> ReadAllBytesUnknownLengthImplAsync(Stream stream, CancellationToken cancellationToken)
        {
            var bytes = ArrayPool<byte>.Shared.Rent(512);
            var total = 0;

            try
            {
                while (true)
                {
                    if (total == bytes.Length)
                        bytes = ResizeBuffer(bytes);

                    var count = await stream
                        .ReadAsync(bytes.AsMemory(total), cancellationToken)
                        .ConfigureAwait(false);

                    if (count == 0)
                        return bytes.AsSpan(0, total).ToArray();

                    total += count;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }
    }

    private static byte[] ResizeBuffer(byte[] bytes)
    {
        var length = (uint)bytes.Length * 2;
        if (length > (uint)Array.MaxLength)
            length = (uint)Math.Max(Array.MaxLength, bytes.Length + 1);

        var tmp = ArrayPool<byte>.Shared.Rent((int)length);
        Buffer.BlockCopy(bytes, 0, tmp, 0, bytes.Length);

        var rented = bytes;
        bytes = tmp;

        ArrayPool<byte>.Shared.Return(rented);
        return bytes;
    }

    private static long GetStreamLength(Stream stream)
    {
        try
        {
            if (stream.CanSeek)
                return stream.Length;
        }
        catch
        {
            // skip
        }

        return 0;
    }

    private static void Error_EndOfStream() =>
        throw new EndOfStreamException();
}
