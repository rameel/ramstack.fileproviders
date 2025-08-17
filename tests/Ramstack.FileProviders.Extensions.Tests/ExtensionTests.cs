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
                FilePath.Normalize(
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
            Assert.That(node.Exists, Is.True);

            if (info.IsDirectory)
            {
                Assert.That(info.Length, Is.EqualTo(-1));
            }
            else
            {
                var file = (FileNode)node;

                Assert.That(info.Length, Is.GreaterThanOrEqualTo(0));
                Assert.That(file.Length, Is.EqualTo(info.Length));
            }
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

            Assert.That(file.Length, Is.EqualTo(info.Length));
            Assert.That(file.Exists, Is.True);
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
            Assert.That(info.Exists, Is.True);
            Assert.That(info.Length, Is.EqualTo(-1));

            Assert.That(directory.Exists, Is.True);
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
            Assert.That(dir.Exists, Is.True);

            var infos = directory.OrderBy(f => f.Name).ToArray();
            var nodes = dir.EnumerateFileNodes().OrderBy(f => f.Name).ToArray();

            Assert.That(
                infos.Select(f => f.Name),
                Is.EquivalentTo(nodes.Select(f => f.Name)));

            foreach (var (info, node) in infos.Zip(nodes))
            {
                Assert.That(node is DirectoryNode, Is.EqualTo(info.IsDirectory));
                Assert.That(node is FileNode, Is.Not.EqualTo(info.IsDirectory));
            }
        }
    }
}
