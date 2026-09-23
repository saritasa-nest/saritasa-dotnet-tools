using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Roslyn;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

/// <summary>
/// A place where entities sit: an expression, together with the position in the code it is read at.
/// </summary>
/// <remarks>
/// A value is what the search looks at, never what it decided, see <see cref="Answer"/>. The same expression
/// read at two positions is two values: a variable holds different things at different lines.
/// </remarks>
internal sealed class Value
{
    private Value(IOperation operation, CodePosition position)
    {
        Operation = operation;
        Position = position;
    }

    /// <summary>
    /// The expression, without the wrappers that do not change the entities in it.
    /// </summary>
    public IOperation Operation { get; }

    /// <summary>
    /// The position in the code the expression is read at.
    /// </summary>
    public CodePosition Position { get; }

    /// <summary>
    /// Creates the value of an expression read at a position.
    /// </summary>
    /// <param name="operation">Expression.</param>
    /// <param name="position">Position the expression is read at.</param>
    /// <returns>Value.</returns>
    public static Value Create(IOperation operation, CodePosition position)
        => new(RemoveWrappers(operation), position);

    /// <summary>
    /// Removes everything wrapped around an expression that does not change the entities in it, so that the
    /// call or the property below it is recognized: implicit conversions ("IEnumerable&lt;User&gt; users =
    /// query.ToList()") and "await".
    /// </summary>
    private static IOperation RemoveWrappers(IOperation operation)
        => RoslynReader.SkipWrappers(operation) switch
        {
            // "await GetUserAsync()": the entities are the ones of the awaited value.
            IAwaitOperation awaited => RemoveWrappers(awaited.Operation),
            var unwrapped => unwrapped,
        };
}
