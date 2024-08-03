using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ramstack.FileProviders.Internal;

/// <summary>
/// Provides path helper methods.
/// </summary>
internal static class FilePath
{
    /// <summary>
    /// The threshold size in characters for using stack allocation.
    /// </summary>
    private const int StackallocThreshold = 160;

    /// <summary>
    /// Returns the directory portion of a file path.
    /// </summary>
    /// <remarks>
    /// Returns a string consisting of all characters up to but not including the last
    /// forward slash (<c>/</c>) in the file path. The returned value is <see langword="null" />
    /// if the specified path is a root (<c>/</c>).
    /// </remarks>
    /// <param name="path">The path of a file or directory.</param>
    /// <returns>
    /// Directory information for path, or <see langword="null" /> if path denotes a root directory.
    /// </returns>
    public static string? GetDirectoryName(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        var index = path.AsSpan().LastIndexOf('/');
        if (index < 0)
            return "";

        var p = index;
        while (p - 1 >= 0 && path[p - 1] == '/')
            p--;

        return p switch
        {
            0 when index + 1 == path.Length => null,
            0 => "/",
            _ => path[..p]
        };
    }

    /// <summary>
    /// Normalizes the specified path with adding the leading slash and removing the trailing slash.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>
    /// The normalized path.
    /// </returns>
    public static string Normalize(string path)
    {
        if (!IsNormalized(path))
            path = NormalizeImpl(path);

        return path;

        static string NormalizeImpl(string path)
        {
            char[]? rented = null;

            var buffer = path.Length + 1 <= StackallocThreshold
                ? stackalloc char[StackallocThreshold]
                : rented = ArrayPool<char>.Shared.Rent(path.Length + 1);

            buffer[0] = '/';
            var index = 1;
            var slash = true;

            for (var i = 0; i < path.Length; i++)
            {
                var c = path[i];
                if (c == '/' || c == '\\')
                {
                    if (slash)
                        continue;

                    c = '/';
                    slash = true;
                }
                else
                {
                    slash = false;
                }

                buffer[index] = c;
                index++;
            }

            while (index > 1 && buffer[index - 1] == '/')
                index--;

            var result = index > 1
                ? buffer[..index].ToString()
                : "/";

            if (rented is not null)
                ArrayPool<char>.Shared.Return(rented);

            return result;
        }
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
        if (path.Length == 0 || string.IsNullOrWhiteSpace(path))
            return false;

        if (path[0] != '/')
            return false;

        if (path.Length > 1 && path.EndsWith('/'))
            return false;

        if (path.AsSpan().Contains('\\'))
            return false;

        return path.AsSpan().IndexOf("//") < 0;
    }

    /// <summary>
    /// Determines if the specified path in a normalized form.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <returns>
    /// <see langword="true" /> if the path in a normalized form;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsFullyNormalized(string path)
    {
        if (path is ['/', ..])
        {
            var prior = path[0];

            for (var j = 1; j < path.Length; j++)
            {
                var ch = path[j];
                if (ch == '\\' || ch == '/' && prior == '/')
                    return false;

                if (ch == '.' && prior == '/')
                {
                    if ((uint)j + 1 >= path.Length)
                        return false;

                    var nch = path[j + 1];
                    if (nch is '/' or '\\')
                        return false;

                    if (nch == '.')
                    {
                        if ((uint)j + 2 >= path.Length)
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
    /// Returns the absolute path for the specified path string.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain absolute path information.</param>
    /// <returns>
    /// The fully qualified location of <paramref name="path"/>.
    /// </returns>
    public static string GetFullPath(string path)
    {
        if (IsFullyNormalized(path))
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

                // Path.GetFullPath in this case does not throw an exceptiion,
                // it simply clears out the buffer
                if (index < 0)
                    Error_InvalidPath();
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
    /// Combines two strings into a path.
    /// </summary>
    /// <param name="path1">The first path to combine.</param>
    /// <param name="path2">The second path to combine.</param>
    /// <returns>
    /// The combined paths.
    /// If one of the specified paths is a zero-length string, this method returns the other path.
    /// If <paramref name="path2" /> contains an absolute path, this method returns <paramref name="path2" />.
    /// </returns>
    public static string Combine(string path1, string path2)
    {
        if (path2.Length == 0)
            return path1;

        if (path1.Length == 0)
            return path2;

        if (HasLeadingSlash(path2))
            return path2;

        if (HasTrailingSlash(path1))
            return path1 + path2;

        return path1 + "/" + path2;
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
