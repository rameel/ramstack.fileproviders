using Microsoft.Extensions.FileProviders;

namespace Ramstack.FileProviders;

[TestFixture]
public class FileInfoExtensionsTests
{
    private const string FileName = "file.txt";

    private readonly string _path = Path.Join(
        Path.GetTempPath(), Path.GetRandomFileName());

    [OneTimeSetUp]
    public void Setup()
    {
        var list = new List<string>();
        for (var i = 0; i < 10240; i++)
            list.Add($"Hello, 世界! Unicode test: café, weiß, Привет, ёжик! こんにちは! {Guid.NewGuid()}");

        Directory.CreateDirectory(_path);
        File.WriteAllLines(Path.Join(_path, FileName), list);
    }

    [OneTimeTearDown]
    public void Cleanup() =>
        Directory.Delete(_path, recursive: true);

    [Test]
    public async Task File_ReadAllText()
    {
        var provider = new PhysicalFileProvider(_path);

        var text = await provider.ReadAllTextAsync(FileName);
        var expected = await File.ReadAllTextAsync(Path.Join(_path, FileName));

        Assert.That(text, Is.EqualTo(expected));
    }

    [Test]
    public async Task File_ReadAllLines()
    {
        var provider = new PhysicalFileProvider(_path);

        var list = await provider.ReadAllLinesAsync(FileName);
        var expected = await File.ReadAllLinesAsync(Path.Join(_path, FileName));
        Assert.That(list, Is.EquivalentTo(expected));
    }

    [Test]
    public async Task File_ReadAllBytes()
    {
        var provider = new PhysicalFileProvider(_path);

        var data = await provider.ReadAllBytesAsync(FileName);
        var expected = await File.ReadAllBytesAsync(Path.Join(_path, FileName));

        Assert.That(data.SequenceEqual(expected), Is.True);
    }
}
