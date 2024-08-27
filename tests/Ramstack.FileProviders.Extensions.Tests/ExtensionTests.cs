using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

using Ramstack.FileProviders.Internal;
using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

[TestFixture]
public class ExtensionTests
{
    private readonly TempFileStorage _storage = new TempFileStorage();
    private readonly PhysicalFileProvider _provider;

    public ExtensionTests() =>
        _provider = new PhysicalFileProvider(root: _storage.Root, ExclusionFilters.None);

    [OneTimeTearDown]
    public void Cleanup() =>
        _storage.Dispose();

    [Test]
    public void Structures_ShouldMatch()
    {
        var list = _provider
            .EnumerateFileNodes("/", "**")
            .Select(f => f.FullName)
            .OrderBy(f => f)
            .ToArray();

        var expected = Directory
            .GetFileSystemEntries(_storage.Root, "*", SearchOption.AllDirectories)
            .Select(p =>
                FilePath.GetFullPath(
                    Path.GetRelativePath(_storage.Root, p)))
            .OrderBy(f => f)
            .ToArray();

        Assert.That(list, Is.EquivalentTo(expected));
    }

    [Test]
    public void FileNodeBase_Returns_Valid_FileInfo()
    {
        foreach (var node in _provider.EnumerateFileNodes("/", "**"))
        {
            var info = node.ToFileInfo();

            Assert.That(info, Is.Not.Null);
            Assert.That(info.IsDirectory, Is.EqualTo(node is DirectoryNode));
            Assert.That(info.Name, Is.EqualTo(node.Name));
            Assert.That(info.Exists, Is.True);

            if (info.IsDirectory)
                Assert.That(info.Length, Is.EqualTo(-1));
            else
                Assert.That(info.Length, Is.GreaterThanOrEqualTo(0));
        }
    }

    [Test]
    public void File_Returns_Valid_FileInfo()
    {
        foreach (var file in _provider.EnumerateFiles("/", "**"))
        {
            var info = file.ToFileInfo();

            Assert.That(info, Is.Not.Null);
            Assert.That(info.IsDirectory, Is.False);
            Assert.That(info.Name, Is.EqualTo(file.Name));
            Assert.That(info.Length, Is.GreaterThanOrEqualTo(0));
            Assert.That(info.Exists, Is.True);
        }
    }

    [Test]
    public void Directory_Returns_Valid_FileInfo()
    {
        foreach (var directory in _provider.EnumerateDirectories("/", "**"))
        {
            var info = directory.ToFileInfo();

            Assert.That(info, Is.Not.Null);
            Assert.That(info.IsDirectory, Is.True);
            Assert.That(info.Name, Is.EqualTo(directory.Name));
            Assert.That(directory.Exists, Is.True);
            Assert.That(info.Length, Is.EqualTo(-1));
        }
    }

    [Test]
    public void Directory_Returns_Valid_DirectoryContents()
    {
        foreach (var dir in _provider.EnumerateDirectories("/", "**"))
        {
            var directory = dir.ToDirectoryContents();

            Assert.That(directory, Is.Not.Null);
            Assert.That(directory.Exists, Is.True);

            var list = directory.OrderBy(f => f.Name).ToArray();
            var expected = dir.EnumerateFileNodes().OrderBy(f => f.Name).ToArray();

            Assert.That(
                list.Select(f => f.Name),
                Is.EquivalentTo(expected.Select(f => f.Name)));
        }
    }
}
