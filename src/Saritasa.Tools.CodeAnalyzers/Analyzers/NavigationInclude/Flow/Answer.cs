using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// What the search decided about a value: the navigation property is loaded, it is not, or we cannot tell.
/// </summary>
/// <remarks>
/// An answer ends the search, while a <see cref="Value"/> continues it. Keeping the two apart is the point:
/// a reader always knows whether a thing is a question or a decision.
/// <see cref="NotLoaded"/> and <see cref="Unknown"/> both mean the property is not there, but they are very
/// different to a developer: one is a mistake in their code, the other is a gap in what the analyzer can read.
/// </remarks>
internal sealed class Answer
{
    private Answer(bool isLoaded, bool isUnknown, IOperation? unreadableValue)
    {
        IsLoaded = isLoaded;
        IsUnknown = isUnknown;
        UnreadableValue = unreadableValue;
    }

    /// <summary>
    /// The property is loaded on every path: the value comes from ".Include(...)", from a method declared with
    /// [Includes], someone assigned the property by hand, or the method asks for it with [IncludeRequired].
    /// </summary>
    public static Answer Loaded { get; } = new(isLoaded: true, isUnknown: false, unreadableValue: null);

    /// <summary>
    /// The search followed the entities to the end and the property is not loaded there: "new User()",
    /// "dbContext.Users" with no Include, a parameter of a method that does not ask for it.
    /// </summary>
    public static Answer NotLoaded { get; } = new(isLoaded: false, isUnknown: false, unreadableValue: null);

    /// <summary>
    /// True when the property is loaded on every path.
    /// </summary>
    public bool IsLoaded { get; }

    /// <summary>
    /// True when the search ran into something it cannot read, so it cannot tell.
    /// </summary>
    public bool IsUnknown { get; }

    /// <summary>
    /// The value the search could not read any further, kept so that INCL004 and the code fix can name it.
    /// Null unless <see cref="IsUnknown"/>.
    /// </summary>
    public IOperation? UnreadableValue { get; }

    /// <summary>
    /// The search cannot follow the entities any further, so it cannot tell: a method it does not know,
    /// a field, an out argument of an arbitrary method, unreachable code.
    /// </summary>
    /// <param name="unreadableValue">The value that stopped the search, when it is already known.</param>
    /// <returns>Answer.</returns>
    public static Answer Unknown(IOperation? unreadableValue = null)
        => new(isLoaded: false, isUnknown: true, unreadableValue);
}
