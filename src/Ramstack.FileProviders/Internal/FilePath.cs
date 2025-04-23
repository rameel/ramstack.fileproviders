using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.FileProviders;

namespace Ramstack.FileProviders.Internal;

/// <summary>
/// Provides utility methods for working with virtual paths.
/// </summary>
/// <remarks>
/// <para>
///   For compatibility across different implementations of <see cref="IFileProvider"/>
///   and operating systems, directory separators are unified to use both
///   backslashes and forward slashes ("/" and "\").
///   <strong>This approach will be reviewed once a better solution is found.</strong>
/// </para>
/// <para>
///   When normalizing paths by using the method <see cref="Normalize" />,
///   backslashes ("\") will be replaced with forward slashes ("/") forcibly.
/// </para>
/// </remarks>
internal static class FilePath
{
    /// <summary>
    /// The threshold size in characters for using stack allocation.
    /// </summary>
    private const int StackallocThreshold = 256;

    /// <summary>
    /// Returns an extension (including the period ".") of the specified path string.
    /// </summary>
    /// <param name="path">The path string from which to get the extension.</param>
    /// <returns>
    /// The extension of the specified path (including the period "."),
    /// or an empty string if no extension is present.
    /// </returns>
    /// <remarks>
    /// <see cref="Path.GetExtension(string)"/> returns an empty string ("")
    /// if the extension consists solely of a period (e.g., "file."), which differs from
    /// <see cref="FileSystemInfo.Extension"/>, which returns "." in this case.
    /// This method follows the behavior of <see cref="Path.GetExtension(string)"/>.
    /// </remarks>
    public static string GetExtension(string path)
    {
        for (var i = path.Length - 1; i >= 0; i--)
        {
            if (path[i] == '.')
            {
                if (i == path.Length - 1)
                    break;

                return path[i..];
            }

            if (path[i] == '/' || path[i] == '\\')
                break;
        }

        return "";
    }

    /// <summary>
    /// Returns the file name and extension for the specified path.
    /// </summary>
    /// <param name="path">The path from which to get the file name and extension.</param>
    /// <returns>
    /// The file name and extension for the <paramref name="path"/>.
    /// </returns>
    public static string GetFileName(string path)
    {
        _ = path.Length;
        var p = path.AsSpan();

        var start = p.LastIndexOfAny('/', '\\');
        return start >= 0
            ? p.Slice(start + 1).ToString()
            : path;
    }

    /// <summary>
    /// Returns the directory portion for the specified path.
    /// </summary>
    /// <param name="path">The path to retrieve the directory portion from.</param>
    /// <returns>
    /// Directory portion for <paramref name="path"/>, or an empty string if path denotes a root directory.
    /// </returns>
    public static string GetDirectoryName(string path)
    {
        _ = path.Length;

        var lastIndex = path.AsSpan().LastIndexOfAny('/', '\\');
        var index = lastIndex;

        // Process consecutive separators
        while ((uint)index - 1 < (uint)path.Length && (path[index - 1] == '/' || path[index - 1] == '\\'))
            index--;

        // Path consists of separators only
        if (index != 0 && (uint)index < (uint)path.Length)
            return path[..index];

        if (lastIndex + 1 == path.Length)
            return "";

        return "/";
    }

    /// <summary>
    /// Determines if the specified path in a normalized form.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <returns>
    /// <see langword="true" /> if the path in a normalized form;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsNormalized(string path)
    {
        _ = path.Length;

        if (path is ['/', ..])
        {
            var prior = path[0];

            for (var j = 1; (uint)j < (uint)path.Length; j++)
            {
                var ch = path[j];
                if (ch == '\\' || ch == '/' && prior == '/')
                    return false;

                if (ch == '.' && prior == '/')
                {
                    if ((uint)j + 1 >= (uint)path.Length)
                        return false;

                    var nch = path[j + 1];
                    if (nch is '/' or '\\')
                        return false;

                    if (nch == '.')
                    {
                        if ((uint)j + 2 >= (uint)path.Length)
                            return false;

                        var sch = path[j + 2];
                        if (sch is '/' or '\\')
                            return false;
                    }
                }

                prior = ch;
            }

            if (prior != '/' || path.Length == 1)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Normalizes the specified path by resolving relative segments and applying formatting.
    /// </summary>
    /// <param name="path">The file or directory path to normalize.</param>
    /// <returns>
    /// The fully normalized and absolute form of <paramref name="path"/>.
    /// </returns>
    /// <remarks>
    /// The normalization process includes the following steps:
    /// <list type="bullet">
    ///   <item><description>Resolves relative segments (e.g., ".", "..").</description></item>
    ///   <item><description>Removes consecutive slashes.</description></item>
    ///   <item><description>Replaces backslashes with forward slashes.</description></item>
    ///   <item><description>Ensures the path starts with a leading slash.</description></item>
    ///   <item><description>Removes any trailing slash.</description></item>
    /// </list>
    /// </remarks>
    public static string Normalize(string path)
    {
        if (IsNormalized(path))
            return path;

        char[]? rented = null;

        var buffer = path.Length + 1 <= StackallocThreshold
            ? stackalloc char[StackallocThreshold]
            : rented = ArrayPool<char>.Shared.Rent(path.Length + 1);

        var index = 0;

        foreach (var s in PathTokenizer.Tokenize(path))
        {
            if (s.Length == 0 || s is ['.'])
                continue;

            if (s is ['.', '.'])
            {
                // Unwind back to the last separator
                index = buffer[..index].LastIndexOf('/');

                // If no separator is found, reset to start
                // (mimics Path.GetFullPath behavior)
                if (index < 0)
                    index = 0;
            }
            else
            {
                buffer[index] = '/';
                s.CopyTo(buffer.Slice(index + 1));
                index += s.Length + 1;
            }
        }

        var result = index != 0
            ? buffer[..index].ToString()
            : "/";

        if (rented is not null)
            ArrayPool<char>.Shared.Return(rented);

        return result;
    }

    /// <summary>
    /// Determines whether the path navigates above the root.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <returns>
    /// <see langword="true" /> if path navigates above the root;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsNavigatesAboveRoot(string path)
    {
        var depth = 0;

        if (path.Length != 0)
        {
            foreach (var s in PathTokenizer.Tokenize(path))
            {
                // ReSharper disable once RedundantIfElseBlock
                // ReSharper disable once RedundantJumpStatement

                if (s.Length == 0 || s is ['.'])
                    continue;
                else if (s is not ['.', '.'])
                    depth++;
                else if (--depth < 0)
                    break;
            }
        }

        return depth < 0;
    }

    /// <summary>
    /// Concatenates two paths into a single path.
    /// </summary>
    /// <param name="path1">The path to join.</param>
    /// <param name="path2">The path to join.</param>
    /// <returns>
    /// The concatenated path.
    /// </returns>
    public static string Join(string path1, string path2)
    {
        if (path1.Length == 0)
            return path2;

        if (path2.Length == 0)
            return path1;

        if (HasTrailingSlash(path1) || HasLeadingSlash(path2))
            return string.Concat(path1, path2);

        return string.Concat(path1, "/", path2);
    }

    /// <summary>
    /// Determines whether the specified path string starts with a directory separator.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <returns>
    /// <see langword="true" /> if the path has a leading directory separator;
    /// otherwise, <see langword="false" />.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasLeadingSlash(string path) =>
        path.StartsWith('/') || path.StartsWith('\\');

    /// <summary>
    /// Determines whether the specified path string ends in a directory separator.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <returns>
    /// <see langword="true" /> if the path has a trailing directory separator;
    /// otherwise, <see langword="false" />.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasTrailingSlash(string path) =>
        path.EndsWith('/') || path.EndsWith('\\');

    [DoesNotReturn]
    private static void Error_InvalidPath() =>
        throw new ArgumentException("Invalid path");
}
