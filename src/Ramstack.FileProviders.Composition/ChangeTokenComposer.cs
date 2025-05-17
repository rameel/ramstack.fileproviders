using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Ramstack.FileProviders.Composition;

/// <summary>
/// Provides helper methods for the <see cref="IChangeToken"/>.
/// </summary>
public static class ChangeTokenComposer
{
    /// <summary>
    /// Attempts to flatten the specified <see cref="IChangeToken"/> into a flat list of change tokens.
    /// </summary>
    /// <remarks>
    /// If the <paramref name="changeToken"/> is not a <see cref="CompositeChangeToken"/>,
    /// the same instance of the <paramref name="changeToken"/> is returned.
    /// </remarks>
    /// <param name="changeToken">The <see cref="IChangeToken"/> to flatten.</param>
    /// <returns>
    /// An <see cref="IChangeToken"/> representing the flattened version from the specified <see cref="IChangeToken"/>.
    /// </returns>
    public static IChangeToken Flatten(this IChangeToken changeToken) =>
        FlattenChangeToken(changeToken);

    /// <summary>
    /// Attempts to flatten the specified <see cref="IChangeToken"/> into a flat list of change tokens.
    /// </summary>
    /// <remarks>
    /// If the <paramref name="changeToken"/> is not a <see cref="CompositeChangeToken"/>,
    /// the same instance of the <paramref name="changeToken"/> is returned.
    /// </remarks>
    /// <param name="changeToken">The <see cref="IChangeToken"/> to flatten.</param>
    /// <returns>
    /// An <see cref="IChangeToken"/> representing the flattened version from the specified <see cref="IChangeToken"/>.
    /// </returns>
    public static IChangeToken FlattenChangeToken(IChangeToken changeToken)
    {
        while (changeToken is CompositeChangeToken composite)
        {
            var changeTokens = composite.ChangeTokens;
            if (changeTokens.Count == 0)
                return NullChangeToken.Singleton;

            if (changeTokens.Count == 1)
            {
                changeToken = changeTokens[0];
                continue;
            }

            foreach (var t in changeTokens)
                if (t is CompositeChangeToken or NullChangeToken)
                    return ComposeChangeTokens(changeTokens);

            break;
        }

        return changeToken;
    }

    /// <summary>
    /// Creates a change token from the specified list of <see cref="IChangeToken"/> instances and flattens it into a flat list of change tokens.
    /// </summary>
    /// <remarks>
    /// This method returns a <see cref="CompositeChangeToken"/> if more than one token remains after flattening.
    /// </remarks>
    /// <param name="changeTokens">The list of <see cref="IChangeToken"/> instances to compose and flatten.</param>
    /// <returns>
    /// An <see cref="IChangeToken"/> representing the flattened version from the specified list of tokens.
    /// </returns>
    public static IChangeToken ComposeChangeTokens(params IChangeToken[] changeTokens) =>
        ComposeChangeTokens(changeTokens.AsEnumerable());

    /// <summary>
    /// Creates a change token from the specified list of <see cref="IChangeToken"/> instances and flattens it into a flat list of change tokens.
    /// </summary>
    /// <remarks>
    /// This method returns a <see cref="CompositeChangeToken"/> if more than one token remains after flattening.
    /// </remarks>
    /// <param name="changeTokens">The list of <see cref="IChangeToken"/> instances to compose and flatten.</param>
    /// <returns>
    /// An <see cref="IChangeToken"/> representing the flattened version from the specified list of tokens.
    /// </returns>
    public static IChangeToken ComposeChangeTokens(IEnumerable<IChangeToken> changeTokens)
    {
        var queue = new Queue<IChangeToken>();
        var collection = new List<IChangeToken>();

        foreach (var changeToken in changeTokens)
        {
            queue.Enqueue(changeToken);

            while (queue.TryDequeue(out var current))
            {
                if (current is CompositeChangeToken composite)
                {
                    foreach (var t in composite.ChangeTokens)
                        queue.Enqueue(t);
                }
                else if (current is not NullChangeToken)
                {
                    collection.Add(current);
                }
            }
        }

        return collection.Count switch
        {
            0 => NullChangeToken.Singleton,
            1 => collection[0],
            _ => new CompositeChangeToken(collection.ToArray())
        };
    }
}
