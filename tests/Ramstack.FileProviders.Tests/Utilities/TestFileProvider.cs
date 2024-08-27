using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Ramstack.FileProviders.Utilities;

public sealed class TestFileProvider : IFileProvider
{
    public IFileInfo GetFileInfo(string subpath) =>
        new NotFoundFileInfo(subpath);

    public IDirectoryContents GetDirectoryContents(string subpath) =>
        NotFoundDirectoryContents.Singleton;

    public IChangeToken Watch(string? filter) =>
        NullChangeToken.Singleton;
}
