using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public sealed class SubFileProviderTests : AbstractFileProviderTests
{
    private readonly TempFileStorage _storage = new TempFileStorage();

    [OneTimeTearDown]
    public void Cleanup() =>
        _storage.Dispose();

    protected override IFileProvider GetFileProvider() =>
        new SubFileProvider("/project/docs", new PhysicalFileProvider(_storage.Root));

    protected override DirectoryInfo GetDirectoryInfo() =>
        new DirectoryInfo(Path.Join(_storage.Root, "project", "docs"));
}
