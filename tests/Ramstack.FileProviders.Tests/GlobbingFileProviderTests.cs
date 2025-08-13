using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public class GlobbingFileProviderTests : AbstractFileProviderTests
{
    private readonly TempFileStorage _storage1;
    private readonly TempFileStorage _storage2;

    public GlobbingFileProviderTests()
    {
        _storage1 = new TempFileStorage();
        _storage2 = new TempFileStorage(_storage1);
    }

    [OneTimeSetUp]
    public void Setup()
    {
        var directory = new DirectoryInfo(
            Path.Join(_storage1.Root, "project"));

        foreach (var di in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
            if (di.Name != "assets")
                di.Delete(recursive: true);

        foreach (var fi in directory.GetFiles("*", SearchOption.TopDirectoryOnly))
            if (fi.Name != "README.md")
                fi.Delete();
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        _storage1.Dispose();
        _storage2.Dispose();
    }

    [Test]
    public void Glob_MatchStructures()
    {
        using var provider = CreateFileProvider();

        Assert.That(
            provider
                .EnumerateDirectories("/", "**")
                .OrderBy(f => f.FullName)
                .Select(f => f.FullName)
                .ToArray(),
            Is.EquivalentTo(
            [
                "/project",
                "/project/assets",
                "/project/assets/fonts",
                "/project/assets/images",
                "/project/assets/images/backgrounds",
                "/project/assets/styles"
            ]));

        Assert.That(
            provider
                .EnumerateFiles("/", "**")
                .OrderBy(f => f.FullName)
                .Select(f => f.FullName)
                .ToArray(),
            Is.EquivalentTo(
            [
                "/project/assets/fonts/Arial.ttf",
                "/project/assets/fonts/Roboto.ttf",
                "/project/assets/images/backgrounds/dark.jpeg",
                "/project/assets/images/backgrounds/light.jpg",
                "/project/assets/images/icon.svg",
                "/project/assets/images/logo.png",
                "/project/assets/styles/main.css",
                "/project/assets/styles/print.css",
                "/project/README.md"
            ]));
    }

    [Test]
    public void ExcludedDirectory_HasNoFileNodes()
    {
        var provider = new GlobbingFileProvider(
            new PhysicalFileProvider(_storage2.Root),
            pattern: "**",
            exclude: "/project/src/**");

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
            var directory = provider.GetDirectoryContents(path);

            Assert.That(
                directory.Exists,
                Is.False);

            Assert.That(
                directory.Any(),
                Is.False);
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
            var file = provider.GetFileInfo(path);
            Assert.That(file.Exists, Is.False);
        }
    }

    protected override IFileProvider GetFileProvider()
    {
        var provider = new PhysicalFileProvider(_storage2.Root);
        return new GlobbingFileProvider(provider,
            patterns: ["project/assets/**", "project/README.md", "project/global.json"],
            excludes: ["project/*.json"]);
    }

    protected override DirectoryInfo GetDirectoryInfo() =>
        new DirectoryInfo(_storage1.Root);
}
