using Microsoft.Extensions.FileProviders;

namespace Ramstack.FileProviders;

partial class FileProviderExtensions
{
    /// <summary>
    /// Enumerates files in a directory.
    /// </summary>
    /// <param name="provider">The file provider to retrieve files from.</param>
    /// <param name="path">The path for which to retrieve files.</param>
    /// <returns>
    /// An enumerable collection of <see cref="FileNode"/> objects.
    /// </returns>
    public static IEnumerable<FileNode> EnumerateFiles(this IFileProvider provider, string path) =>
        provider.GetDirectory(path).EnumerateFiles();

    /// <summary>
    /// Enumerates directories in a directory.
    /// </summary>
    /// <param name="provider">The file provider to retrieve directories from.</param>
    /// <param name="path">The path for which to retrieve directories.</param>
    /// <returns>
    /// An enumerable collection of <see cref="DirectoryNode"/> objects.
    /// </returns>
    public static IEnumerable<DirectoryNode> EnumerateDirectories(this IFileProvider provider, string path) =>
        provider.GetDirectory(path).EnumerateDirectories();

    /// <summary>
    /// Enumerates file nodes in a directory.
    /// </summary>
    /// <param name="provider">The file provider to retrieve files from.</param>
    /// <param name="path">The path for which to retrieve file nodes.</param>
    /// <returns>
    /// An enumerable collection of <see cref="FileNodeBase"/> objects.
    /// </returns>
    public static IEnumerable<FileNodeBase> EnumerateFileNodes(this IFileProvider provider, string path) =>
        provider.GetDirectory(path).EnumerateFileNodes();

    /// <summary>
    /// Enumerates files in a directory that match the specified glob pattern.
    /// </summary>
    /// <param name="provider">The file provider to retrieve files from.</param>
    /// <param name="path">The path for which to retrieve files.</param>
    /// <param name="pattern">The glob pattern to match against the names of files.</param>
    /// <param name="exclude">Optional glob pattern to exclude files.</param>
    /// <returns>
    /// An enumerable collection of files in the directory specified by <paramref name="path"/>
    /// that match the specified glob pattern.
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
    /// Enumerates files in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="provider">The file provider to retrieve files from.</param>
    /// <param name="path">The path for which to retrieve files.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of files.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude files.</param>
    /// <returns>
    /// An enumerable collection of files in the directory specified by <paramref name="path"/>
    /// that match the specified glob patterns.
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
    /// Enumerates directories in a directory that match the specified glob pattern.
    /// </summary>
    /// <param name="provider">The file provider to retrieve directories from.</param>
    /// <param name="path">The path for which to retrieve directories.</param>
    /// <param name="pattern">The glob pattern to match against the names of directories.</param>
    /// <param name="exclude">Optional glob pattern to exclude directories.</param>
    /// <returns>
    /// An enumerable collection of directories in the directory specified by <paramref name="path"/>
    /// that match the specified glob pattern.
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
    /// Enumerates directories in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="provider">The file provider to retrieve directories from.</param>
    /// <param name="path">The path for which to retrieve directories.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of directories.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude directories.</param>
    /// <returns>
    /// An enumerable collection of directories in the directory specified by <paramref name="path"/>
    /// that match the specified glob patterns.
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
    /// Enumerates file nodes in a directory that match any of the specified glob pattern.
    /// </summary>
    /// <param name="provider">The file provider to retrieve file nodes from.</param>
    /// <param name="path">The path for which to retrieve file nodes.</param>
    /// <param name="pattern">The glob pattern to match against the names of file nodes in path.</param>
    /// <param name="exclude">Optional glob pattern to exclude file nodes.</param>
    /// <returns>
    /// An enumerable collection of file nodes in the directory specified by <paramref name="path"/>
    /// that match the specified glob pattern.
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
    /// Enumerates file nodes in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="provider">The file provider to retrieve file nodes from.</param>
    /// <param name="path">The path for which to retrieve file nodes.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of file nodes.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude file nodes.</param>
    /// <returns>
    /// An enumerable collection of file nodes in the directory specified by <paramref name="path"/>
    /// that match the specified glob patterns.
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
