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

    private const string Prefix = "solution/app";

    private readonly TempFileStorage _storage = new TempFileStorage(Prefix);

    protected override IFileProvider GetFileProvider() =>
        new PrefixedFileProvider(Prefix, new PhysicalFileProvider(_storage.PrefixedPath, ExclusionFilters.None));

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
}
