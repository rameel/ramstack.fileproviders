using System.Diagnostics.CodeAnalysis;

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
    public async Task ReadAllText()
    {
        var provider = new PhysicalFileProvider(_path);

        var text1 = await provider.ReadAllTextAsync(FileName);
        var text2 = provider.ReadAllText(FileName);
        var expected = await File.ReadAllTextAsync(Path.Join(_path, FileName));

        Assert.That(text1, Is.EqualTo(expected));
        Assert.That(text2, Is.EqualTo(expected));
    }

    [Test]
    public async Task ReadAllLines()
    {
        var provider = new PhysicalFileProvider(_path);

        var list1 = await provider.ReadAllLinesAsync(FileName);
        var list2 = provider.ReadAllLines(FileName);
        var expected = await File.ReadAllLinesAsync(Path.Join(_path, FileName));

        Assert.That(list1, Is.EquivalentTo(expected));
        Assert.That(list2, Is.EquivalentTo(expected));
    }

    [Test]
    [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
    public async Task ReadAllBytes()
    {
        var provider = new PhysicalFileProvider(_path);

        var data1 = await provider.ReadAllBytesAsync(FileName);
        var data2 = provider.ReadAllBytes(FileName);
        var expected = await File.ReadAllBytesAsync(Path.Join(_path, FileName));

        Assert.That(data1.SequenceEqual(expected), Is.True);
        Assert.That(data2.SequenceEqual(expected), Is.True);
    }
}
