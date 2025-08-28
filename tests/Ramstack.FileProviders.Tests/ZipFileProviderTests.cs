using System.IO.Compression;

using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public class ZipFileProviderTests : AbstractFileProviderTests
{
    private readonly TempFileStorage _storage = new TempFileStorage();
    private readonly string _path =
        Path.Combine(
            Path.GetTempPath(),
            Path.GetRandomFileName()
            ) + ".zip";

    [OneTimeSetUp]
    public void Setup()
    {
        ZipFile.CreateFromDirectory(
            sourceDirectoryName: _storage.Root,
            destinationArchiveFileName: _path,
            compressionLevel: CompressionLevel.SmallestSize,
            includeBaseDirectory: false);
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        _storage.Dispose();
        File.Delete(_path);
    }

    // [Test]
    public void ZipArchive_WithIdenticalNameEntries()
    {
        using var provider = new ZipFileProvider(CreateArchive());

        var list = provider
            .EnumerateFiles("/1")
            .ToArray();

        Assert.That(
            list.Length,
            Is.EqualTo(1));

        Assert.That(
            list[0].ReadAllBytes(),
            Is.EquivalentTo("Hello, World!"u8.ToArray()));

        static MemoryStream CreateArchive()
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var a = archive.CreateEntry("1/text.txt");
                using (var writer = a.Open())
                    writer.Write("Hello, World!"u8);

                archive.CreateEntry("1/text.txt");
                archive.CreateEntry(@"1\text.txt");
            }

            stream.Position = 0;
            return stream;
        }
    }

    [Test]
    public void ZipArchive_PrefixedEntries()
    {
        var archive = new ZipArchive(CreateArchive(), ZipArchiveMode.Read, leaveOpen: true);
        using var provider = new ZipFileProvider(archive);

        var directories = provider
            .EnumerateDirectories("/", "**")
            .Select(f =>
                f.FullName)
            .OrderBy(f => f)
            .ToArray();

        var files = provider
            .EnumerateFiles("/", "**")
            .Select(f =>
                f.FullName)
            .OrderBy(f => f)
            .ToArray();

        Console.Error.WriteLine("--- files ---");
        foreach (var name in files) Console.Error.WriteLine(name);
        Console.Error.WriteLine("--- directories ---");
        foreach (var name in directories) Console.Error.WriteLine(name);

        Console.Error.WriteLine("--- archive ---");
        foreach (var entry in archive.Entries) Console.Error.WriteLine(entry.FullName);

        // Assert.That(files, Is.EquivalentTo(
        // [
        //     "/1/text.txt",
        //     "/2/text.txt",
        //     "/3/text.txt",
        //     "/4/text.txt",
        //     "/5/text.txt",
        //     "/localhost/backup/text.txt",
        //     "/localhost/share/text.txt",
        //     "/server/backup/text.txt",
        //     "/server/share/text.txt",
        //     "/text.txt",
        //     "/text.xml"
        // ]));
        //
        // Assert.That(directories, Is.EquivalentTo(
        // [
        //     "/1",
        //     "/2",
        //     "/3",
        //     "/4",
        //     "/5",
        //     "/localhost",
        //     "/localhost/backup",
        //     "/localhost/share",
        //     "/server",
        //     "/server/backup",
        //     "/server/share"
        // ]));

        static MemoryStream CreateArchive()
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                archive.CreateEntry(@"D:\1/text.txt");
                archive.CreateEntry(@"D:2\text.txt");

                archive.CreateEntry(@"\\?\D:\text.txt");
                archive.CreateEntry(@"\\?\D:text.xml");
                archive.CreateEntry(@"\\.\D:\3\text.txt");
                archive.CreateEntry(@"//?/D:/4\text.txt");
                archive.CreateEntry(@"//./D:\5/text.txt");

                archive.CreateEntry(@"\\?\UNC\localhost\share\text.txt");
                archive.CreateEntry(@"\\.\unc\server\share\text.txt");
                archive.CreateEntry(@"//?/UNC/localhost/backup\text.txt");
                archive.CreateEntry(@"//./unc/server/backup\text.txt");
            }

            stream.Position = 0;
            return stream;
        }
    }

    // [Test]
    public void ZipArchive_Directories()
    {
        using var provider = new ZipFileProvider(CreateArchive());

        var directories = provider
            .EnumerateDirectories("/", "**")
            .Select(f =>
                f.FullName)
            .OrderBy(f => f)
            .ToArray();

        Assert.That(directories, Is.EquivalentTo(
        [
            "/1",
            "/2",
            "/2/3",
            "/4",
            "/4/5",
            "/4/5/6"
        ]));

        static MemoryStream CreateArchive()
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                archive.CreateEntry(@"\1/");
                archive.CreateEntry(@"\2/");
                archive.CreateEntry(@"/2\");
                archive.CreateEntry(@"/2\");
                archive.CreateEntry(@"/2\");
                archive.CreateEntry(@"/2\3/");
                archive.CreateEntry(@"/2\3/");
                archive.CreateEntry(@"/2\3/");
                archive.CreateEntry(@"4\5/6\");
            }

            stream.Position = 0;
            return stream;
        }
    }

    protected override IFileProvider GetFileProvider() =>
        new ZipFileProvider(_path);

    protected override DirectoryInfo GetDirectoryInfo() =>
        new DirectoryInfo(_storage.Root);

}
