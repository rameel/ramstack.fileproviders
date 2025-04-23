using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public class GlobbingFileProviderTests : AbstractFileProviderTests
{
    private readonly TempFileStorage _storage = new TempFileStorage();

    [OneTimeSetUp]
    public void Setup()
    {
        var path = Path.Join(_storage.Root, "project");
        var directory = new DirectoryInfo(path);

        foreach (var di in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
            if (di.Name != "docs")
                di.Delete(recursive: true);

        foreach (var fi in directory.GetFiles("*", SearchOption.TopDirectoryOnly))
            fi.Delete();

        File.Delete(Path.Join(_storage.Root, "project/docs/troubleshooting/common_issues.txt"));
    }

    [OneTimeTearDown]
    public void Cleanup() =>
        _storage.Dispose();

    protected override IFileProvider GetFileProvider()
    {
        var provider = new PhysicalFileProvider(_storage.Root);
        return new GlobbingFileProvider(provider, "project/docs/**", exclude: "**/*.txt");
    }

    protected override DirectoryInfo GetDirectoryInfo() =>
        new DirectoryInfo(_storage.Root);
}
