using Microsoft.Extensions.FileProviders;

namespace Ramstack.FileProviders.Composition;

/// <summary>
/// Provides helper methods for the <see cref="IFileProvider"/>.
/// </summary>
public static class FileProvider
{
    /// <summary>
    /// Tries to flatten the specified <see cref="IFileProvider"/> into a flat list of file providers.
    /// </summary>
    /// <remarks>
    /// If the <paramref name="provider"/> is not a <see cref="CompositeFileProvider"/>,
    /// the same instance of the <paramref name="provider"/> is returned.
    /// </remarks>
    /// <param name="provider">The <see cref="IFileProvider"/> to flatten.</param>
    /// <returns>
    /// A <see cref="IFileProvider"/> that represents the flattened version of the specified <see cref="IFileProvider"/>.
    /// </returns>
    public static IFileProvider FlattenFileProvider(IFileProvider provider)
    {
        if (provider is CompositeFileProvider composite)
            foreach (var p in composite.FileProviders)
                if (p is CompositeFileProvider or NullFileProvider)
                    return CompositeProviders(composite.FileProviders);

        return provider;
    }

    /// <summary>
    /// Returns a provider from the specified list of <see cref="IFileProvider"/> and flattens it into a flat list of file providers.
    /// </summary>
    /// <param name="providers">The list of <see cref="IFileProvider"/> instances to compose and flatten.</param>
    /// <returns>
    /// A <see cref="IFileProvider"/> that represents the flattened version of the specified list of providers.
    /// </returns>
    public static IFileProvider CompositeProviders(params IFileProvider[] providers) =>
        CompositeProviders(providers.AsEnumerable());

    /// <summary>
    /// Creates a provider from the specified list of <see cref="IFileProvider"/> and flattens it into a flat list of file providers.
    /// </summary>
    /// <param name="providers">The list of <see cref="IFileProvider"/> instances to compose and flatten.</param>
    /// <returns>
    /// A <see cref="IFileProvider"/> that represents the flattened version of the specified list of providers.
    /// </returns>
    public static IFileProvider CompositeProviders(IEnumerable<IFileProvider> providers)
    {
        var queue = new Queue<IFileProvider>();
        var collection = new List<IFileProvider>();

        foreach (var provider in providers)
        {
            queue.Enqueue(provider);

            while (queue.TryDequeue(out var current))
            {
                if (current is CompositeFileProvider composite)
                {
                    foreach (var p in composite.FileProviders)
                        queue.Enqueue(p);
                }
                else if (current is not NullFileProvider)
                {
                    collection.Add(current);
                }
            }
        }

        return collection.Count switch
        {
            0 => new NullFileProvider(),
            1 => collection[0],
            _ => new CompositeFileProvider(collection.ToArray())
        };
    }
}
