using System.IO.Compression;

using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public class ZipFileProviderTests : AbstractFileProviderTests
{
    private readonly TempFileStorage _storage = new TempFileStorage();
    private readonly string _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    [OneTimeSetUp]
    public void Setup()
    {
        ZipFile.CreateFromDirectory(
            sourceDirectoryName: _storage.Root,
            destinationArchiveFileName: _path,
            compressionLevel: CompressionLevel.SmallestSize,
            includeBaseDirectory: false);
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        _storage.Dispose();
        File.Delete(_path);
    }

    protected override IFileProvider GetFileProvider() =>
        new ZipFileProvider(_path);

    protected override DirectoryInfo GetDirectoryInfo() =>
        new DirectoryInfo(_storage.Root);

}
