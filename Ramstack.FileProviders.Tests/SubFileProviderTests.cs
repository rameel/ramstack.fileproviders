using Microsoft.Extensions.FileProviders;

using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public sealed class SubFileProviderTests
{
    [TestCase("/js/knockout.js", ExpectedResult = true)]
    [TestCase("js/knockout.js", ExpectedResult = true)]
    [TestCase("js/knockout.min.js", ExpectedResult = false)]
    [TestCase("js/./knockout.js", ExpectedResult = true)]
    [TestCase("js/../js/knockout.js", ExpectedResult = true)]
    [TestCase("js/legacy/../knockout.js", ExpectedResult = true)]
    public bool GetFileInfo(string path)
    {
        var provider = new SubFileProvider(
            "/app/assets",
            new PrefixedFileProvider(
                "/app/assets/js",
                new PlainFileProvider(
                    new IFileInfo[] { new ContentFileInfo("knockout.js", "") })));

        return provider.GetFileInfo(path).Exists;
    }

    [Test]
    public void GetFileInfo_InvalidPath()
    {
        var provider = new SubFileProvider(
            "/app/assets",
            new PrefixedFileProvider(
                "/app/assets/js",
                new PlainFileProvider(
                    new IFileInfo[] { new ContentFileInfo("knockout.js", "") })));

        var exception = Assert.Throws<ArgumentException>(
            () => provider.GetFileInfo("js/../../knockout.js"));

        Assert.That(exception!.Message, Is.EqualTo("Invalid path"));
    }
}
