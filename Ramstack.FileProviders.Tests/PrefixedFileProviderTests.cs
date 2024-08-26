using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public sealed class PrefixedFileProviderTests
{
    [TestCase("/app/assets/js/knockout.js", ExpectedResult = true)]
    [TestCase("app/assets/js/knockout.js", ExpectedResult = true)]
    [TestCase("app/assets/js/knockout.min.js", ExpectedResult = false)]
    [TestCase("app/assets/./js/knockout.js", ExpectedResult = true)]
    [TestCase("app/assets/../assets/js/knockout.js", ExpectedResult = true)]
    [TestCase("app/assets/../js/knockout.js", ExpectedResult = false)]
    [TestCase("app/assets/knockout.js", ExpectedResult = false)]
    [TestCase("app/knockout.js", ExpectedResult = false)]
    public bool GetFileInfo(string path)
    {
        var provider = new PrefixedFileProvider("/app/assets/js",
            new PlainFileProvider([new ContentFileInfo("knockout.js", "")]));

        return provider.GetFileInfo(path).Exists;
    }

    [Test]
    public void GetFileInfo_PrefixOnly()
    {
        var provider = new PrefixedFileProvider("/app/assets/js",
            new PlainFileProvider([new ContentFileInfo("knockout.js", "")]));

        provider.GetFileInfo("/app/assets/js");
        provider.GetFileInfo("/app/assets/js/");
    }

    [Test]
    public void CheckArtificialDirectoriesAvailability()
    {
        var provider = new PrefixedFileProvider("/public/assets/js",
            new PlainFileProvider([new ContentFileInfo("knockout.js", "")]));

        var nodes = provider.EnumerateFileNodes("/", "**").Select(n => n.FullName);
        var files = provider.EnumerateFiles("/", "**").Select(n => n.FullName);
        var directories = provider.EnumerateDirectories("/", "**").Select(n => n.FullName);

        Assert.That(nodes, Is.EquivalentTo(new[] {
            "/public",
            "/public/assets",
            "/public/assets/js",
            "/public/assets/js/knockout.js"
        }));

        Assert.That(files, Is.EquivalentTo(new[] {
            "/public/assets/js/knockout.js"
        }));

        Assert.That(directories, Is.EquivalentTo(new[] {
            "/public",
            "/public/assets",
            "/public/assets/js"
        }));
    }
}
