using Ramstack.FileProviders.Internal;

namespace Ramstack.FileProviders;

[TestFixture]
public class FilePathTests
{
    [TestCase("", ExpectedResult = "")]
    [TestCase(".", ExpectedResult = ".")]
    [TestCase("/", ExpectedResult = "")]
    [TestCase("/.", ExpectedResult = ".")]
    [TestCase("file.txt", ExpectedResult = ".txt")]
    [TestCase("/path/to/file.txt", ExpectedResult = ".txt")]
    [TestCase("/path/to/.hidden", ExpectedResult = ".hidden")]
    [TestCase("/path/to/file", ExpectedResult = "")]
    [TestCase("/path.with.dots/to/file.txt", ExpectedResult = ".txt")]
    [TestCase("/path/with.dots/file.", ExpectedResult = ".")]
    [TestCase("/path.with.dots/to/.hidden.ext", ExpectedResult = ".ext")]
    [TestCase("file.with.multiple.dots.ext", ExpectedResult = ".ext")]
    [TestCase("/path/to/file.with.multiple.dots.ext", ExpectedResult = ".ext")]
    [TestCase("/.hidden", ExpectedResult = ".hidden")]
    public string GetExtension(string path) =>
        FilePath.GetExtension(path);

    [TestCase("/", ExpectedResult = true)]
    [TestCase("/a/b/c", ExpectedResult = true)]
    [TestCase("/a/./b/c", ExpectedResult = true)]
    [TestCase("/a/../b/c", ExpectedResult = true)]
    [TestCase("/a/./../b/c", ExpectedResult = true)]
    [TestCase("/a / /c", ExpectedResult = true)]
    [TestCase("/a/ ", ExpectedResult = true)]
    [TestCase("/a/", ExpectedResult = false)]
    [TestCase("/a//b", ExpectedResult = false)]
    [TestCase("/a\\b", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(" ", ExpectedResult = false)]
    [TestCase(" /", ExpectedResult = false)]
    public bool IsPartiallyNormalized(string path) =>
        FilePath.IsNormalized(path);

    [TestCase("/", ExpectedResult = true)]
    [TestCase("/a/b/c", ExpectedResult = true)]
    [TestCase("/a/ /c", ExpectedResult = true)]
    [TestCase("/a/ ", ExpectedResult = true)]
    [TestCase("/a/ /c", ExpectedResult = true)]
    [TestCase("/a/./b/c", ExpectedResult = false)]
    [TestCase("/a/../b/c", ExpectedResult = false)]
    [TestCase("/a/./../b/c", ExpectedResult = false)]
    [TestCase("/a/", ExpectedResult = false)]
    [TestCase("/a//b", ExpectedResult = false)]
    [TestCase("a/b", ExpectedResult = false)]
    [TestCase("a/b/", ExpectedResult = false)]
    [TestCase("/a\\b", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(" ", ExpectedResult = false)]
    [TestCase(" /", ExpectedResult = false)]
    public bool IsFullyNormalized(string path) =>
        FilePath.IsFullyNormalized(path);

    [TestCase("", ExpectedResult = "/")]
    [TestCase(".", ExpectedResult = "/")]
    [TestCase(".", ExpectedResult = "/")]
    [TestCase("/home/", ExpectedResult = "/home")]
    [TestCase("/home/..folder1/.folder2/file", ExpectedResult = "/home/..folder1/.folder2/file")]
    [TestCase("/home/././", ExpectedResult = "/home")]
    [TestCase("/././././/home/user/documents", ExpectedResult = "/home/user/documents")]
    [TestCase("/home/./user/./././/documents", ExpectedResult = "/home/user/documents")]
    [TestCase("/home/../home/user//documents", ExpectedResult = "/home/user/documents")]
    [TestCase("/home/../home/user/../../home/config/documents", ExpectedResult = "/home/config/documents")]
    [TestCase("/home/../home/user/./.././.././home/config/documents", ExpectedResult = "/home/config/documents")]
    public string GetFullPath(string path) =>
        FilePath.GetFullPath(path);

    [TestCase("..")]
    [TestCase("/home/../..")]
    public void GetFullPath_Error(string path) =>
        Assert.Throws<ArgumentException>(() => FilePath.GetFullPath(path));

    [TestCase("/home/user/documents", ExpectedResult = false)]
    [TestCase("/././././home/user/documents", ExpectedResult = false)]
    [TestCase("/home/../documents", ExpectedResult = false)]
    [TestCase("/home/.././././././documents", ExpectedResult = false)]
    [TestCase("/home/../../documents", ExpectedResult = true)]
    [TestCase("/home/../..", ExpectedResult = true)]
    [TestCase("/../documents", ExpectedResult = true)]
    [TestCase("/home/user/documents/..", ExpectedResult = false)]
    [TestCase("/home/user/documents/../..", ExpectedResult = false)]
    [TestCase("/home/user/documents/../../..", ExpectedResult = false)]
    [TestCase("/home/user/documents/../../../..", ExpectedResult = true)]
    [TestCase("//home//user//documents//..//..////..///..", ExpectedResult = true)]
    [TestCase("/..", ExpectedResult = true)]
    [TestCase("/../", ExpectedResult = true)]
    [TestCase("/", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    public bool IsNavigatesAboveRoot(string path) =>
        FilePath.IsNavigatesAboveRoot(path);

    [TestCase("/", ExpectedResult = "")]
    [TestCase("/dir", ExpectedResult = "/")]
    [TestCase("/dir/file", ExpectedResult = "/dir")]
    [TestCase("/dir/dir/", ExpectedResult = "/dir/dir")]
    [TestCase("dir/dir", ExpectedResult = "dir")]
    [TestCase("dir/dir/", ExpectedResult = "dir/dir")]

    [TestCase("//", ExpectedResult = "")]
    [TestCase("///", ExpectedResult = "")]
    [TestCase("//dir", ExpectedResult = "/")]
    [TestCase("///dir", ExpectedResult = "/")]
    [TestCase("////dir", ExpectedResult = "/")]
    [TestCase("/dir///dir", ExpectedResult = "/dir")]
    [TestCase("/dir///dir///", ExpectedResult = "/dir///dir")]
    [TestCase("//dir///dir///", ExpectedResult = "//dir///dir")]
    [TestCase("dir///dir", ExpectedResult = "dir")]
    public string? GetDirectoryName(string path) =>
        FilePath.GetDirectoryName(path);

    [TestCase("", ExpectedResult = "/")]
    [TestCase("/", ExpectedResult = "/")]
    [TestCase("///", ExpectedResult = "/")]
    [TestCase("///a///b///", ExpectedResult = "/a/b")]
    [TestCase("\\", ExpectedResult = "/")]
    [TestCase("a", ExpectedResult = "/a")]
    [TestCase("a/", ExpectedResult = "/a")]
    [TestCase("a/b/c", ExpectedResult = "/a/b/c")]
    [TestCase("a/b/c/", ExpectedResult = "/a/b/c")]
    [TestCase("a//b//c//", ExpectedResult = "/a/b/c")]
    [TestCase("a\\//b\\//c\\//", ExpectedResult = "/a/b/c")]
    [TestCase("a//b\\c//", ExpectedResult = "/a/b/c")]
    [TestCase("/a/b/c/", ExpectedResult = "/a/b/c")]
    [TestCase("\\a\\b\\c\\", ExpectedResult = "/a/b/c")]
    [TestCase("\\\\a\\\\b\\\\c\\\\", ExpectedResult = "/a/b/c")]
    [TestCase("/a/./b/c/", ExpectedResult = "/a/./b/c")]
    [TestCase("/a/../b/c/", ExpectedResult = "/a/../b/c")]
    [TestCase("/a/./../b/c/", ExpectedResult = "/a/./../b/c")]
    public string Normalize(string path) =>
        FilePath.Normalize(path);
}
