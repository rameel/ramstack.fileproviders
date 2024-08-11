using System.Diagnostics.CodeAnalysis;
using System.Text;

using Microsoft.Extensions.FileProviders;

using Ramstack.FileProviders.Internal;

namespace Ramstack.FileProviders;

/// <summary>
/// Represents a file in the specified file provider.
/// </summary>
public sealed class FileNode : FileNodeBase
{
    private readonly IFileInfo _file;

    /// <inheritdoc />
    public override bool Exists => _file.Exists;

    /// <summary>
    /// Gets the string representing the extension part of the file.
    /// </summary>
    public string Extension => FilePath.GetExtension(FullName);

    /// <summary>
    /// Gets the full path of the directory containing the file.
    /// </summary>
    public string DirectoryName => FilePath.GetDirectoryName(FullName);

    /// <summary>
    /// Gets the last write time of the current file.
    /// </summary>
    public DateTimeOffset LastWriteTime => _file.LastModified;

    /// <summary>
    /// Gets an <see cref="DirectoryNode"/> instance representing the parent directory.
    /// </summary>
    public DirectoryNode Directory
    {
        get
        {
            var path = FilePath.GetDirectoryName(FullName);
            var directory = Provider.GetDirectoryContents(path);
            return new DirectoryNode(Provider, path, directory);
        }
    }

    /// <summary>
    /// Gets the length of the file in bytes, or <c>-1</c> for non-existing files.
    /// </summary>
    public long Length => _file.Length;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileNode"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/> associated with this file.</param>
    /// <param name="path">The full path of the file within the provider.</param>
    /// <param name="file">The <see cref="IFileInfo"/> associated with this file.</param>
    internal FileNode(IFileProvider provider, string path, IFileInfo file) : base(provider, path)
    {
        if (file.IsDirectory)
            Error_FileExpected();

        _file = file;
    }

    /// <summary>
    /// Returns a read-only stream of the current file.
    /// </summary>
    /// <returns>
    /// A read-only stream.
    /// </returns>
    public Stream OpenRead() =>
        _file.CreateReadStream();

    /// <summary>
    /// Returns a <see cref="StreamReader" /> with the specified character encoding that reads from the current text file.
    /// </summary>
    /// <param name="encoding">The character encoding to use.</param>
    /// <returns>
    /// A new <see cref="StreamReader"/> with the specified character encoding.
    /// </returns>
    public StreamReader OpenText(Encoding? encoding = null) =>
        new(_file.CreateReadStream(), encoding, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: false);

    /// <summary>
    /// Returns the <see cref="IFileInfo"/> from the current instance.
    /// </summary>
    /// <returns>
    /// The <see cref="IFileInfo"/> instance.
    /// </returns>
    public IFileInfo GetFileInfo() =>
        _file;

    [DoesNotReturn]
    private static void Error_FileExpected() =>
        throw new ArgumentException("The specified path refers to a directory, not a file.");
}
