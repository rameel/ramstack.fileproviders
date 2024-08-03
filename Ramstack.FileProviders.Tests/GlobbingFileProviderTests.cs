using Microsoft.Extensions.FileProviders;

using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public class GlobbingFileProviderTests
{
    private readonly TempFileStorage _storage = new();

    [OneTimeTearDown]
    public void Cleanup() =>
        _storage.Dispose();

    [TestCase("project/tests/TestMain.cs", ExpectedResult = true)]
    [TestCase("project/src/Modules/Module1/Submodule/Submodule2.cs", ExpectedResult = true)]
    [TestCase("project/src/App.sln", ExpectedResult = false)]
    [TestCase("project/src/App/App.csproj", ExpectedResult = false)]
    public bool GetFileInfo_Includes(string path)
    {
        var provider = new GlobbingFileProvider(
            new PhysicalFileProvider(_storage.Root),
            "**/*.cs");

        var file = provider.GetFileInfo(path);
        if (file.Exists is false)
            return false;

        Assert.That(file.Name, Is.EqualTo(Path.GetFileName(path)));
        return true;
    }

    [TestCase("project/tests/TestMain.cs", ExpectedResult = true)]
    [TestCase("project/src/Modules/Module1/Submodule/Submodule2.cs", ExpectedResult = true)]
    [TestCase("project/src/App.sln", ExpectedResult = true)]
    [TestCase("project/src/App/App.csproj", ExpectedResult = true)]
    [TestCase("project/logs/archive/2019/01/app_2019-01.log", ExpectedResult = true)]
    [TestCase("project/data/temp/temp_file1.tmp", ExpectedResult = false)]
    [TestCase("project/data/temp/ac/b2/34/2d/7e/temp_file2.tmp", ExpectedResult = false)]
    [TestCase("project/scripts/setup.p1", ExpectedResult = false)]
    [TestCase("project/scripts/build.sh", ExpectedResult = false)]
    public bool GetFileInfo_Excluded(string path)
    {
        var provider = new GlobbingFileProvider(
            new PhysicalFileProvider(_storage.Root),
            ["**/*"], ["**/*.tmp", "project/scripts/**"]);

        var file = provider.GetFileInfo(path);
        if (file.Exists is false)
            return false;

        Assert.That(file.Name, Is.EqualTo(Path.GetFileName(path)));
        return true;
    }

    [TestCase("project/src/App/Program.cs", ExpectedResult = true)]
    [TestCase("project/src/Modules/Module1/Submodule/Submodule2.cs", ExpectedResult = true)]
    [TestCase("project/tests/TestMain.cs", ExpectedResult = false)]
    [TestCase("project/src/App/App.csproj", ExpectedResult = false)]
    [TestCase("project/logs/archive/2019/01/app_2019-01.log", ExpectedResult = false)]
    [TestCase("project/scripts/build.sh", ExpectedResult = false)]
    public bool GetFileInfo_Both(string path)
    {
        var provider = new GlobbingFileProvider(
            new PhysicalFileProvider(_storage.Root),
            "**/*.cs", "**/tests/**");

        var file = provider.GetFileInfo(path);
        if (file.Exists is false)
            return false;

        Assert.That(file.Name, Is.EqualTo(Path.GetFileName(path)));
        return true;
    }

    [TestCase("project/src/App", 2)]
    [TestCase("project/src/Modules/Module1/Submodule", 2)]
    [TestCase("project/tests", 0)]
    [TestCase("project/scripts", 0)]
    public void GetDirectoryContents(string path, int count)
    {
        var provider = new GlobbingFileProvider(
            new PhysicalFileProvider(_storage.Root),
            "**/*.cs", "**/tests/**");

        var directory = provider.GetDirectoryContents(path);
        Assert.That(directory.Count(), Is.EqualTo(count));

        foreach (var fi in directory)
            Assert.That(fi.Name.EndsWith(".cs"), Is.True);
    }

    [Test]
    public void Traversal()
    {
        var provider = new GlobbingFileProvider(
            new PhysicalFileProvider(_storage.Root),
            "**/*.cs", "**/tests");

        var list = provider.EnumerateFiles("/", "**").ToList();

        Assert.That(list.Count, Is.EqualTo(6));
        Assert.That(list.Count(p => p.FullName.Contains("tests")), Is.Zero);
    }
}
