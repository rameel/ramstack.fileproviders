using System.Reflection;

using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public sealed class PrefixedFileProviderTests : AbstractFileProviderTests
{
    private static readonly Func<string, string, string?> s_resolveGlobFilter =
        typeof(PrefixedFileProvider)
            .GetMethod("ResolveGlobFilter", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<string, string, string?>>();

    private static readonly Func<string, string, string?> s_resolvePath =
        typeof(PrefixedFileProvider)
            .GetMethod("ResolvePath", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<string, string, string?>>();

    private readonly TempFileStorage _storage = new TempFileStorage();

    protected override IFileProvider GetFileProvider() =>
        new PrefixedFileProvider("/project",
            new PhysicalFileProvider(
                Path.Join(_storage.Root, "project")));

    protected override DirectoryInfo GetDirectoryInfo() =>
        new DirectoryInfo(_storage.Root);

    [TestCase("/modules/profile/assets", "/modules/**", ExpectedResult = "**")]
    [TestCase("/modules/profile/assets", "/modules/**/*.js", ExpectedResult = "**/*.js")]
    [TestCase("/modules/profile/assets", "/modules/profile/*/*.js", ExpectedResult = "*.js")]
    [TestCase("/modules/profile/assets", "/modules/profile/{js,css,assets}/*.js", ExpectedResult = "*.js")]
    [TestCase("/modules/profile/assets", "/modules/{settings,profile}/{js,css,assets}/*.js", ExpectedResult = "*.js")]
    [TestCase("/modules/profile/assets", "/modules/profile/assets/*.js", ExpectedResult = "*.js")]
    [TestCase("/modules/profile/assets", "/modules/profile/assets/js/jquery/*.js", ExpectedResult = "js/jquery/*.js")]
    [TestCase("/modules/profile/assets", "/modules/profiles/assets/*.js", ExpectedResult = null)]
    [TestCase("/modules/profile/assets", "/modules/profile/js/*.js", ExpectedResult = null)]
    [TestCase("/modules/profile/assets", "/module/profile/assets/*.js", ExpectedResult = null)]
    [TestCase("/modules/profile/assets", "/modules/profile/*.js", ExpectedResult = null)]
    [TestCase("/modules/profile/assets", "/modules/*.js", ExpectedResult = null)]
    [TestCase("/modules/profile/assets", "/*.js", ExpectedResult = null)]
    public string? ResolveGlobFilter(string prefix, string filter) =>
        s_resolveGlobFilter(prefix, filter);

    [TestCase("/", "/",            ExpectedResult = "/")]
    [TestCase("/", "/foo",         ExpectedResult = "/foo")]
    [TestCase("/", "/a/b/c",       ExpectedResult = "/a/b/c")]

    [TestCase("/a/b", "/a/b",      ExpectedResult = "/")]

    [TestCase("/a/b", "/a/b/c",    ExpectedResult = "/c")]
    [TestCase("/a/b", "/a/b/c/d",  ExpectedResult = "/c/d")]

    [TestCase("/a/b", "/a/bc",     ExpectedResult = null)]

    [TestCase("/a/b", "/a/c",      ExpectedResult = null)]
    [TestCase("/a/b", "/a",        ExpectedResult = null)]
    public string? ResolvePath(string prefix, string path) =>
        s_resolvePath(prefix, path);

    [Test]
    public void GetFileInfo_RootPrefix_DelegatesToInnerProvider()
    {
        using var provider = new PrefixedFileProvider("/",
            new PhysicalFileProvider(_storage.Root, ExclusionFilters.None));

        var file = provider.GetFileInfo("/project/README.md");
        Assert.That(file.Exists, Is.True);
        Assert.That(file.IsDirectory, Is.False);
    }

    [Test]
    public void GetDirectoryContents_RootPrefix_DelegatesToInnerProvider()
    {
        using var provider = new PrefixedFileProvider("/",
            new PhysicalFileProvider(_storage.Root, ExclusionFilters.None));

        var contents = provider.GetDirectoryContents("/project/docs");
        Assert.That(contents.Exists, Is.True);
        Assert.That(contents.Any(), Is.True);
    }

    [Test]
    public void GetFileInfo_RootPrefix_MissingFile_ReturnsNotFound()
    {
        using var provider = new PrefixedFileProvider("/",
            new PhysicalFileProvider(_storage.Root, ExclusionFilters.None));

        var file = provider.GetFileInfo("/project/nonexistent.txt");
        Assert.That(file.Exists, Is.False);
    }
}
