using Microsoft.Extensions.FileProviders;

namespace Ramstack.FileProviders;

partial class FileProviderExtensions
{
    /// <summary>
    /// Returns an enumerable collection of files in the directory at the specified path.
    /// </summary>
    /// <param name="provider">The file provider to use.</param>
    /// <param name="path">The path to the directory to enumerate.</param>
    /// <returns>
    /// An enumerable collection of files in the specified directory.
    /// </returns>
    public static IEnumerable<FileNode> EnumerateFiles(this IFileProvider provider, string path) =>
        provider.GetDirectory(path).EnumerateFiles();

    /// <summary>
    /// Returns an enumerable collection of directories in the directory at the specified path.
    /// </summary>
    /// <param name="provider">The file provider to use.</param>
    /// <param name="path">The path to the directory to enumerate.</param>
    /// <returns>
    /// An enumerable collection of directories in the specified directory.
    /// </returns>
    public static IEnumerable<DirectoryNode> EnumerateDirectories(this IFileProvider provider, string path) =>
        provider.GetDirectory(path).EnumerateDirectories();

    /// <summary>
    /// Returns an enumerable collection of file nodes in the directory at the specified path.
    /// </summary>
    /// <param name="provider">The file provider to use.</param>
    /// <param name="path">The path to the directory to enumerate.</param>
    /// <returns>
    /// An enumerable collection of file nodes in the specified directory.
    /// </returns>
    public static IEnumerable<FileNodeBase> EnumerateFileNodes(this IFileProvider provider, string path) =>
        provider.GetDirectory(path).EnumerateFileNodes();

    /// <summary>
    /// Returns an enumerable collection of files in the directory at the specified path that match the given glob pattern.
    /// </summary>
    /// <param name="provider">The file provider to use.</param>
    /// <param name="path">The path to the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of files.</param>
    /// <param name="exclude">Optional glob pattern to exclude files.</param>
    /// <returns>
    /// An enumerable collection of files in the directory that match the given glob pattern.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Supported meta-characters include <c>'*'</c>, <c>'?'</c>, <c>'\'</c> and <c>'['</c>, <c>']'</c>.
    ///       And inside character classes <c>'-'</c>, <c>'!'</c> and <c>']'</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'.'</c> and <c>'..'</c> symbols do not have any special treatment and are processed
    ///       as regular characters for matching.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Character classes can be negated by prefixing them with  <c>'!'</c>, such as <c>[!0-9]</c>,
    ///       which matches all characters except digits.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Brace patterns are supported, including nested brace pattern:
    ///       <c>{file,dir,name}</c>, <c>{file-1.{c,cpp},file-2.{cs,f}}</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       An empty pattern in brace expansion  <c>{}</c> is allowed, as well as variations
    ///       like <c>{.cs,}</c>, <c>{name,,file}</c>, or  <c>{,.cs}</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>Leading and trailing separators are ignored.</description>
    ///   </item>
    ///   <item>
    ///     <description>Consecutive separators are counted as one.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'**'</c> sequence in the glob pattern can be used to match zero or more directories and subdirectories.
    ///       It can be used at the beginning, middle, or end of a pattern, for example,
    ///       <c>"**/file.txt"</c>, <c>"dir/**/*.txt"</c>, <c>"dir/**"</c>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static IEnumerable<FileNode> EnumerateFiles(this IFileProvider provider, string path, string pattern, string? exclude = null) =>
        provider.GetDirectory(path).EnumerateFiles(pattern, exclude);

    /// <summary>
    /// Returns an enumerable collection of files in the directory at the specified path that match any of the given glob patterns.
    /// </summary>
    /// <param name="provider">The file provider to use.</param>
    /// <param name="path">The path to the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of files.</param>
    /// <param name="excludes">An optional array of glob patterns to exclude files.</param>
    /// <returns>
    /// An enumerable collection of files in the directory that match any of the given glob patterns.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Supported meta-characters include <c>'*'</c>, <c>'?'</c>, <c>'\'</c> and <c>'['</c>, <c>']'</c>.
    ///       And inside character classes <c>'-'</c>, <c>'!'</c> and <c>']'</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'.'</c> and <c>'..'</c> symbols do not have any special treatment and are processed
    ///       as regular characters for matching.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Character classes can be negated by prefixing them with  <c>'!'</c>, such as <c>[!0-9]</c>,
    ///       which matches all characters except digits.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Brace patterns are supported, including nested brace pattern:
    ///       <c>{file,dir,name}</c>, <c>{file-1.{c,cpp},file-2.{cs,f}}</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       An empty pattern in brace expansion  <c>{}</c> is allowed, as well as variations
    ///       like <c>{.cs,}</c>, <c>{name,,file}</c>, or  <c>{,.cs}</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>Leading and trailing separators are ignored.</description>
    ///   </item>
    ///   <item>
    ///     <description>Consecutive separators are counted as one.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'**'</c> sequence in the glob pattern can be used to match zero or more directories and subdirectories.
    ///       It can be used at the beginning, middle, or end of a pattern, for example,
    ///       <c>"**/file.txt"</c>, <c>"dir/**/*.txt"</c>, <c>"dir/**"</c>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static IEnumerable<FileNode> EnumerateFiles(this IFileProvider provider, string path, string[] patterns, string[]? excludes = null) =>
        provider.GetDirectory(path).EnumerateFiles(patterns, excludes);

    /// <summary>
    /// Returns an enumerable collection of directories in the directory at the specified path that match the given glob pattern.
    /// </summary>
    /// <param name="provider">The file provider to use.</param>
    /// <param name="path">The path to the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of directories.</param>
    /// <param name="exclude">Optional glob pattern to exclude directories.</param>
    /// <returns>
    /// An enumerable collection of directories in the directory that match the given glob pattern.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Supported meta-characters include <c>'*'</c>, <c>'?'</c>, <c>'\'</c> and <c>'['</c>, <c>']'</c>.
    ///       And inside character classes <c>'-'</c>, <c>'!'</c> and <c>']'</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'.'</c> and <c>'..'</c> symbols do not have any special treatment and are processed
    ///       as regular characters for matching.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Character classes can be negated by prefixing them with  <c>'!'</c>, such as <c>[!0-9]</c>,
    ///       which matches all characters except digits.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Brace patterns are supported, including nested brace pattern:
    ///       <c>{file,dir,name}</c>, <c>{file-1.{c,cpp},file-2.{cs,f}}</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       An empty pattern in brace expansion  <c>{}</c> is allowed, as well as variations
    ///       like <c>{.cs,}</c>, <c>{name,,file}</c>, or  <c>{,.cs}</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>Leading and trailing separators are ignored.</description>
    ///   </item>
    ///   <item>
    ///     <description>Consecutive separators are counted as one.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'**'</c> sequence in the glob pattern can be used to match zero or more directories and subdirectories.
    ///       It can be used at the beginning, middle, or end of a pattern, for example,
    ///       <c>"**/file.txt"</c>, <c>"dir/**/*.txt"</c>, <c>"dir/**"</c>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static IEnumerable<DirectoryNode> EnumerateDirectories(this IFileProvider provider, string path, string pattern, string? exclude = null) =>
        provider.GetDirectory(path).EnumerateDirectories(pattern, exclude);

    /// <summary>
    /// Returns an enumerable collection of directories in the directory at the specified path that match any of the given glob patterns.
    /// </summary>
    /// <param name="provider">The file provider to use.</param>
    /// <param name="path">The path to the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of directories.</param>
    /// <param name="excludes">An optional array of glob patterns to exclude directories.</param>
    /// <returns>
    /// An enumerable collection of directories in the directory that match any of the given glob patterns.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Supported meta-characters include <c>'*'</c>, <c>'?'</c>, <c>'\'</c> and <c>'['</c>, <c>']'</c>.
    ///       And inside character classes <c>'-'</c>, <c>'!'</c> and <c>']'</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'.'</c> and <c>'..'</c> symbols do not have any special treatment and are processed
    ///       as regular characters for matching.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Character classes can be negated by prefixing them with  <c>'!'</c>, such as <c>[!0-9]</c>,
    ///       which matches all characters except digits.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Brace patterns are supported, including nested brace pattern:
    ///       <c>{file,dir,name}</c>, <c>{file-1.{c,cpp},file-2.{cs,f}}</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       An empty pattern in brace expansion  <c>{}</c> is allowed, as well as variations
    ///       like <c>{.cs,}</c>, <c>{name,,file}</c>, or  <c>{,.cs}</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>Leading and trailing separators are ignored.</description>
    ///   </item>
    ///   <item>
    ///     <description>Consecutive separators are counted as one.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'**'</c> sequence in the glob pattern can be used to match zero or more directories and subdirectories.
    ///       It can be used at the beginning, middle, or end of a pattern, for example,
    ///       <c>"**/file.txt"</c>, <c>"dir/**/*.txt"</c>, <c>"dir/**"</c>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static IEnumerable<DirectoryNode> EnumerateDirectories(this IFileProvider provider, string path, string[] patterns, string[]? excludes = null) =>
        provider.GetDirectory(path).EnumerateDirectories(patterns, excludes);

    /// <summary>
    /// Returns an enumerable collection of file nodes in the directory at the specified path that match the given glob pattern.
    /// </summary>
    /// <param name="provider">The file provider to use.</param>
    /// <param name="path">The path to the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of file nodes.</param>
    /// <param name="exclude">Optional glob pattern to exclude file nodes.</param>
    /// <returns>
    /// An enumerable collection of file nodes in the directory that match the given glob pattern.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Supported meta-characters include <c>'*'</c>, <c>'?'</c>, <c>'\'</c> and <c>'['</c>, <c>']'</c>.
    ///       And inside character classes <c>'-'</c>, <c>'!'</c> and <c>']'</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'.'</c> and <c>'..'</c> symbols do not have any special treatment and are processed
    ///       as regular characters for matching.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Character classes can be negated by prefixing them with  <c>'!'</c>, such as <c>[!0-9]</c>,
    ///       which matches all characters except digits.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Brace patterns are supported, including nested brace pattern:
    ///       <c>{file,dir,name}</c>, <c>{file-1.{c,cpp},file-2.{cs,f}}</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       An empty pattern in brace expansion  <c>{}</c> is allowed, as well as variations
    ///       like <c>{.cs,}</c>, <c>{name,,file}</c>, or  <c>{,.cs}</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>Leading and trailing separators are ignored.</description>
    ///   </item>
    ///   <item>
    ///     <description>Consecutive separators are counted as one.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'**'</c> sequence in the glob pattern can be used to match zero or more directories and subdirectories.
    ///       It can be used at the beginning, middle, or end of a pattern, for example,
    ///       <c>"**/file.txt"</c>, <c>"dir/**/*.txt"</c>, <c>"dir/**"</c>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static IEnumerable<FileNodeBase> EnumerateFileNodes(this IFileProvider provider, string path, string pattern, string? exclude = null) =>
        provider.GetDirectory(path).EnumerateFileNodes(pattern, exclude);

    /// <summary>
    /// Returns an enumerable collection of file nodes in the directory at the specified path that match any of the given glob patterns.
    /// </summary>
    /// <param name="provider">The file provider to use.</param>
    /// <param name="path">The path of the directory from which to retrieve file nodes.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of file nodes.</param>
    /// <param name="excludes">An optional array of glob patterns to exclude file nodes.</param>
    /// <returns>
    /// An enumerable collection of file nodes in the directory that match any of the given glob patterns.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Supported meta-characters include <c>'*'</c>, <c>'?'</c>, <c>'\'</c> and <c>'['</c>, <c>']'</c>.
    ///       And inside character classes <c>'-'</c>, <c>'!'</c> and <c>']'</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'.'</c> and <c>'..'</c> symbols do not have any special treatment and are processed
    ///       as regular characters for matching.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Character classes can be negated by prefixing them with  <c>'!'</c>, such as <c>[!0-9]</c>,
    ///       which matches all characters except digits.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Brace patterns are supported, including nested brace pattern:
    ///       <c>{file,dir,name}</c>, <c>{file-1.{c,cpp},file-2.{cs,f}}</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       An empty pattern in brace expansion  <c>{}</c> is allowed, as well as variations
    ///       like <c>{.cs,}</c>, <c>{name,,file}</c>, or  <c>{,.cs}</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>Leading and trailing separators are ignored.</description>
    ///   </item>
    ///   <item>
    ///     <description>Consecutive separators are counted as one.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The <c>'**'</c> sequence in the glob pattern can be used to match zero or more directories and subdirectories.
    ///       It can be used at the beginning, middle, or end of a pattern, for example,
    ///       <c>"**/file.txt"</c>, <c>"dir/**/*.txt"</c>, <c>"dir/**"</c>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static IEnumerable<FileNodeBase> EnumerateFileNodes(this IFileProvider provider, string path, string[] patterns, string[]? excludes = null) =>
        provider.GetDirectory(path).EnumerateFileNodes(patterns, excludes);
}
