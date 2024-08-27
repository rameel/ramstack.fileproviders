using System.Text;

using Microsoft.Extensions.FileProviders;

namespace Ramstack.FileProviders.Utilities;

public sealed class ContentFileInfo(string name, string content, DateTimeOffset lastModified = default) : IFileInfo
{
    private readonly byte[] _content = Encoding.UTF8.GetBytes(content);

    public string Name => name;
    public long Length => _content.Length;
    public string? PhysicalPath => null;
    public bool IsDirectory => false;
    public bool Exists => true;
    public DateTimeOffset LastModified => lastModified;

    /// <inheritdoc />
    public Stream CreateReadStream() =>
        new MemoryStream(_content);
}
