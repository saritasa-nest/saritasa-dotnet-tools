using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// A place the entities of a value come from: the value itself and the position in the code it is read at.
/// </summary>
/// <remarks>
/// The search traces an origin back to the origins it comes from, until every path ends at <see cref="Loaded"/>
/// or at <see cref="NotFound"/>. Those two are answers rather than places in the code, so they carry no value:
/// the search recognizes them before it looks at <see cref="Value"/> or <see cref="Position"/>.
/// </remarks>
internal sealed class Origin
{
    private Origin(IOperation value, CodePosition position)
    {
        Value = value;
        Position = position;
    }

    /// <summary>
    /// The navigation property is loaded here and the search can stop: the value comes from ".Include(...)"
    /// or from a method declared with [Includes], someone assigned the property by hand, the method asks for it
    /// with [IncludeRequired], or a loop came back to a place the search already read.
    /// </summary>
    public static Origin Loaded { get; } = new(null!, null!);

    /// <summary>
    /// The entities come from somewhere the search cannot follow, so the property counts as not loaded:
    /// "new User()", "dbContext.Users", a field, an out argument of an arbitrary method, unreachable code.
    /// </summary>
    public static Origin NotFound { get; } = new(null!, null!);

    /// <summary>
    /// The value, without the wrappers that do not change the entities in it.
    /// </summary>
    public IOperation Value { get; }

    /// <summary>
    /// The position in the code the value is read at.
    /// </summary>
    public CodePosition Position { get; }

    /// <summary>
    /// Creates the origin of a value read at a position, or returns <see cref="NotFound"/> when the value
    /// is not known.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="position">Position the value is read at.</param>
    /// <returns>Origin.</returns>
    public static Origin Create(IOperation? value, CodePosition position)
        => value is null ? NotFound : new Origin(RemoveWrappers(value), position);

    /// <summary>
    /// Removes everything wrapped around a value that does not change the entities in it, so that the call or
    /// the property below it is recognized.
    /// </summary>
    /// <remarks>
    /// Values come with implicit conversions: "IEnumerable&lt;User&gt; users = query.ToList()",
    /// "list.Where(...)" (the list is converted to IEnumerable&lt;User&gt;).
    /// </remarks>
    private static IOperation RemoveWrappers(IOperation value)
        => RoslynHelper.SkipWrappers(value) switch
        {
            // "await GetUserAsync()": the entities are the ones of the awaited value.
            IAwaitOperation awaited => RemoveWrappers(awaited.Operation),
            var unwrapped => unwrapped,
        };
}
