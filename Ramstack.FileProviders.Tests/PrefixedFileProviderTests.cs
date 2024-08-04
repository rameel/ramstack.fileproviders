using Microsoft.Extensions.FileProviders;

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
            new PlainFileProvider(new IFileInfo[] { new ContentFileInfo("knockout.js", "") }));

        return provider.GetFileInfo(path).Exists;
    }

    [Test]
    public void GetFileInfo_PrefixOnly()
    {
        var provider = new PrefixedFileProvider("/app/assets/js",
            new PlainFileProvider(new IFileInfo[] { new ContentFileInfo("knockout.js", "") }));

        provider.GetFileInfo("/app/assets/js");
        provider.GetFileInfo("/app/assets/js/");
    }
}
