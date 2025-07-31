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

    [Test]
    public void ExcludedDirectory_HasNoFileNodes()
    {
        using var storage = new TempFileStorage();
        var fs = new GlobbingFileProvider(new PhysicalFileProvider(storage.Root), "**", exclude: "/project/src/**");

        var directories = new[]
        {
            "/project/src",
            "/project/src/App",
            "/project/src/Modules",
            "/project/src/Modules/Module1",
            "/project/src/Modules/Module1/Submodule",
            "/project/src/Modules/Module2"
        };

        foreach (var path in directories)
        {
            var directory = fs.GetDirectory(path);

            Assert.That(
                directory.Exists,
                Is.False);

            Assert.That(
                directory.EnumerateFileNodes("**").Count(),
                Is.Zero);
        }

        var files = new[]
        {
            "/project/src/App/App.csproj",
            "/project/src/App/Program.cs",
            "/project/src/App/Utils.cs",
            "/project/src/Modules/Module1/Module1.cs",
            "/project/src/Modules/Module1/Module1.csproj",
            "/project/src/Modules/Module1/Submodule/Submodule1.cs",
            "/project/src/Modules/Module1/Submodule/Submodule2.cs",
            "/project/src/Modules/Module1/Submodule/Submodule.csproj",
            "/project/src/Modules/Module2/Module2.cs",
            "/project/src/Modules/Module2/Module2.csproj",
            "/project/src/App.sln"
        };

        foreach (var path in files)
        {
            var file = fs.GetFile(path);
            Assert.That(file.Exists, Is.False);
        }
    }

    protected override IFileProvider GetFileProvider()
    {
        var provider = new PhysicalFileProvider(_storage.Root);
        return new GlobbingFileProvider(provider, "project/docs/**", exclude: "**/*.txt");
    }

    protected override DirectoryInfo GetDirectoryInfo() =>
        new DirectoryInfo(_storage.Root);
}
