using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public sealed class PrefixedFileProviderTests : FileProviderBaseTests
{
    private const string Prefix = "solution/app";

    private readonly TempFileStorage _storage = new TempFileStorage(Prefix);

    protected override IFileProvider GetFileProvider() =>
        new PrefixedFileProvider(Prefix, new PhysicalFileProvider(_storage.PrefixedPath, ExclusionFilters.None));

    protected override DirectoryInfo GetDirectoryInfo() =>
        new DirectoryInfo(_storage.Root);
}
