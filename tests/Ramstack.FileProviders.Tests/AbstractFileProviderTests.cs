using Ramstack.FileProviders.Utilities;

namespace Ramstack.FileProviders;

/// <summary>
/// Represents a base class for the <see cref="IFileProvider"/> tests.
/// </summary>
public abstract class AbstractFileProviderTests
{
    [Test]
    public void Structures_ShouldMatch()
    {
        using var pa = CreateFileProvider();
        using var pb = new PhysicalFileProvider(GetDirectoryInfo().FullName, ExclusionFilters.None);

        CompareDirectories("/");

        void CompareDirectories(string path)
        {
            var a = pa.GetDirectoryContents(path);
            var b = pb.GetDirectoryContents(path);

            Assert.That(a.Exists, Is.EqualTo(b.Exists), $"Directory existence mismatch for '{path}'");

            var listA = a.OrderBy(f => f.Name).ToArray();
            var listB = b.OrderBy(f => f.Name).ToArray();

            Assert.That(
                listA.Select(f => f.Name),
                Is.EquivalentTo(listB.Select(f => f.Name)),
                $"Directory contents mismatch for '{path}'");

            for (var i = 0; i < listA.Length; i++)
                CompareFiles(FilePath.Join(path, listA[i].Name), listA[i], listB[i]);
        }

        void CompareFiles(string path, IFileInfo a, IFileInfo b)
        {
            Assert.That(a.Name, Is.EqualTo(b.Name), $"File name mismatch for '{path}'");
            Assert.That(a.Exists, Is.EqualTo(b.Exists), $"File existence mismatch for '{path}'");
            Assert.That(a.Length, Is.EqualTo(b.Length), $"File length mismatch for '{path}'");
            Assert.That(a.IsDirectory, Is.EqualTo(b.IsDirectory), $"Type mismatch: '{path}' is a directory in one provider but not in the other.");

            if (a.IsDirectory)
            {
                Assert.That(a.Length, Is.EqualTo(-1), $"Directory '{path}' should return -1");
                CompareDirectories(FilePath.Join(path, a.Name));
            }
            else
            {
                using var ra = new StreamReader(a.CreateReadStream());
                using var rb = new StreamReader(b.CreateReadStream());

                Assert.That(ra.ReadToEnd(), Is.EqualTo(rb.ReadToEnd()), $"File content mismatch for '{path}'");
            }
        }
    }

    [Test]
    public void GetFileInfo_NonExistingFile()
    {
        using var provider = CreateFileProvider();

        var name = $"{Guid.NewGuid()}.txt";
        var info = provider.GetFileInfo(name);

        Assert.That(
            FilePath.GetFileName(info.Name),
            Is.EqualTo(name));
        Assert.That(info.IsDirectory, Is.False);
        Assert.That(info.Exists, Is.False);
    }

    [Test]
    public void GetDirectoryContents_NonExistingDirectory()
    {
        using var provider = CreateFileProvider();

        var name = Guid.NewGuid().ToString();
        var info = provider.GetDirectoryContents($"/{name}");

        Assert.That(info.Exists, Is.False);
    }

    /// <summary>
    /// Returns an instance of the file provider.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="IFileProvider"/>.
    /// </returns>
    protected abstract IFileProvider GetFileProvider();

    /// <summary>
    /// Returns the <see cref="DirectoryInfo"/> object representing the root of the test directory.
    /// </summary>
    /// <returns>
    /// A <see cref="DirectoryInfo"/> object that points to the root of the test directory.
    /// </returns>
    protected abstract DirectoryInfo GetDirectoryInfo();

    /// <summary>
    /// Returns a disposable instance of the file provider.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="IFileProvider"/>.
    /// </returns>
    protected DisposableFileProvider CreateFileProvider() =>
        new DisposableFileProvider(GetFileProvider());
}
