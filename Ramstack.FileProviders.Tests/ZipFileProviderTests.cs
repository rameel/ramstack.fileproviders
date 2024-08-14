using System.IO.Compression;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public class ZipFileProviderTests
{
    private readonly TempFileStorage _storage = new();
    private readonly string _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    [OneTimeTearDown]
    public void Cleanup()
    {
        _storage.Dispose();
        File.Delete(_path);
    }

    [Test]
    public void CheckZipFileProvider()
    {
        ZipFile.CreateFromDirectory(_storage.Root, _path, CompressionLevel.SmallestSize, includeBaseDirectory: false);

        using var zip = new ZipFileProvider(_path);

        var physical = new PhysicalFileProvider(_storage.Root, ExclusionFilters.None);
        CompareDirectories(zip.GetDirectory("/"), physical.GetDirectory("/"));
    }

    private static void CompareDirectories(DirectoryNode zip, DirectoryNode dir)
    {
        if (zip.Exists != dir.Exists)
            Assert.Fail($"Directory '{zip.FullName}.Exists ({zip.Exists})' != '{dir.FullName}.Exists ({dir.Exists})'");

        var files1 = zip.EnumerateFiles().OrderBy(f => f.Name).ToArray();
        var files2 = dir.EnumerateFiles().OrderBy(f => f.Name).ToArray();

        Assert.That(
            files1.Select(f => f.Name),
            Is.EquivalentTo(files2.Select(f => f.Name)),
            zip.FullName);

        for (var i = 0; i < files1.Length; i++)
            FileEquals(files1[i], files2[i]);

        var dirs1 = zip.EnumerateDirectories().OrderBy(d => d.Name).ToArray();
        var dirs2 = dir.EnumerateDirectories().OrderBy(d => d.Name).ToArray();

        Assert.That(
            dirs1.Select(f => f.Name),
            Is.EquivalentTo(dirs2.Select(f => f.Name)),
            zip.FullName);

        for (var i = 0; i < dirs1.Length; i++)
            CompareDirectories(dirs1[i], dirs2[i]);
    }

    private static void FileEquals(FileNode zip, FileNode file)
    {
        using var sr1 = zip.OpenText();
        using var sr2 = file.OpenText();

        Assert.That(
            sr1.ReadToEnd(),
            Is.EqualTo(sr2.ReadToEnd()),
            zip.FullName);
    }
}
