using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

/// <summary>
/// What the search decided about a value: the navigation property is loaded, it is not, or we cannot tell.
/// </summary>
/// <remarks>
/// An answer ends the search, a <see cref="Value"/> continues it. <see cref="NotLoaded"/> and
/// <see cref="Unknown"/> both mean the property is not there, but one is a mistake in the code and the other
/// a gap in what the analyzer can read, so they must not be mixed.
/// <para>
/// Each kind of answer is a type of its own, so the code that reports a diagnostic decides by the type alone and
/// only the answers that carry something have it to read. Answers are made through the members of this class,
/// which share the ones that carry nothing.
/// </para>
/// </remarks>
internal abstract class Answer
{
    private Answer()
    {
    }

    /// <summary>
    /// The property is loaded on every path: the value comes from ".Include(...)", from a method annotated with
    /// [Includes], someone assigned the property by hand, the entity was freshly constructed, or the method
    /// asks for it with [IncludeRequired].
    /// </summary>
    public static Answer Loaded { get; } = new LoadedAnswer();

    /// <summary>
    /// The search followed the entities to the end and the property is not loaded there: "dbContext.Users"
    /// with no Include, a method of our own code that does not promise it.
    /// </summary>
    public static Answer NotLoaded { get; } = new NotLoadedAnswer();

    /// <summary>
    /// The answer before the value that stopped the search is known.
    /// </summary>
    private static UnknownAnswer UnnamedUnknown { get; } = new(null);

    /// <summary>
    /// The search reached a parameter of a method that does not ask for the path with [IncludeRequired], so
    /// nobody loads it.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="path">The path the parameter misses, counted from the parameter.</param>
    /// <returns>Answer.</returns>
    public static Answer NotRequiredBy(IParameterSymbol parameter, NavigationPath path)
        => new NotRequiredByAnswer(parameter, path);

    /// <summary>
    /// The search cannot follow the entities any further, so it cannot tell: a method it does not know,
    /// a field, an out argument of an arbitrary method, unreachable code.
    /// </summary>
    /// <param name="unreadableValue">The value that stopped the search, when it is already known.</param>
    /// <returns>Answer.</returns>
    public static Answer Unknown(IOperation? unreadableValue = null)
        => unreadableValue is null ? UnnamedUnknown : new UnknownAnswer(unreadableValue);

    /// <summary>
    /// See <see cref="Loaded"/>.
    /// </summary>
    public sealed class LoadedAnswer : Answer
    {
    }

    /// <summary>
    /// See <see cref="NotLoaded"/>.
    /// </summary>
    public sealed class NotLoadedAnswer : Answer
    {
    }

    /// <summary>
    /// See <see cref="NotRequiredBy"/>.
    /// </summary>
    public sealed class NotRequiredByAnswer : Answer
    {
        /// <summary>
        /// Initializes the answer.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="path">The path the parameter misses, counted from the parameter.</param>
        public NotRequiredByAnswer(IParameterSymbol parameter, NavigationPath path)
        {
            Parameter = parameter;
            Path = path;
        }

        /// <summary>
        /// The parameter, which the method has to ask for the path on.
        /// </summary>
        public IParameterSymbol Parameter { get; }

        /// <summary>
        /// The path the parameter misses, counted from the parameter: "Settings.Profile" for a read of
        /// "user.Settings.Profile".
        /// </summary>
        public NavigationPath Path { get; }
    }

    /// <summary>
    /// See <see cref="Unknown"/>.
    /// </summary>
    public sealed class UnknownAnswer : Answer
    {
        /// <summary>
        /// Initializes the answer.
        /// </summary>
        /// <param name="unreadableValue">The value that stopped the search, when it is already known.</param>
        public UnknownAnswer(IOperation? unreadableValue)
        {
            UnreadableValue = unreadableValue;
        }

        /// <summary>
        /// The value the search could not read any further, kept so that INCL004 and the code fix can name it.
        /// Null until the deepest value of the search that sees it names it.
        /// </summary>
        public IOperation? UnreadableValue { get; }
    }
}
