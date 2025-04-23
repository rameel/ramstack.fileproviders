namespace Ramstack.FileProviders;

/// <summary>
/// Represents a base class for both <see cref="FileNode"/> and <see cref="DirectoryNode"/> objects.
/// </summary>
[DebuggerDisplay("{FullName,nq}")]
public abstract class FileNodeBase
{
    /// <summary>
    /// Gets the file provider associated with this file node.
    /// </summary>
    public IFileProvider Provider { get; }

    /// <summary>
    /// Gets the full path of the directory or file.
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// Gets the name of the file or directory.
    /// </summary>
    public string Name => FilePath.GetFileName(FullName);

    /// <summary>
    /// Gets a value indicating whether the file or directory exists.
    /// </summary>
    public abstract bool Exists { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileNodeBase"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="IFileProvider"/> associated with this file node.</param>
    /// <param name="path">The path of the file node within the provider.</param>
    internal FileNodeBase(IFileProvider provider, string path) =>
        (Provider, FullName) = (provider, path);

    /// <summary>
    /// Returns the <see cref="IFileInfo"/> for the current instance.
    /// </summary>
    /// <returns>
    /// The <see cref="IFileInfo"/> instance.
    /// </returns>
    public abstract IFileInfo ToFileInfo();
}
