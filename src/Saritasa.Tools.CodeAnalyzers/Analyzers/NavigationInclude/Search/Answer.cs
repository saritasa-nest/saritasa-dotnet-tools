using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;
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
    /// The answer before the value that stopped the search is known.
    /// </summary>
    private static UnknownAnswer UnnamedUnknown { get; } = new(null, new BridgeEnd.Result());

    /// <summary>
    /// The search followed the entities to the end and the path is not loaded there: "dbContext.Users" with no
    /// Include, a method of our own code that does not promise it.
    /// </summary>
    /// <param name="path">The path that is missing, counted from where the search ended: what the query has to
    /// Include, or the method has to promise.</param>
    /// <returns>Answer.</returns>
    public static Answer NotLoaded(NavigationPath path) => new NotLoadedAnswer(path);

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
        => unreadableValue is null ? UnnamedUnknown : new UnknownAnswer(unreadableValue, new BridgeEnd.Result());

    /// <summary>
    /// The search reached a parameter of a callback that a call nobody annotated was given, as the <c>u</c> of
    /// <c>users.ForEachItem(u =&gt; ...)</c>, so it cannot tell where the call took the entities from.
    /// </summary>
    /// <param name="call">The call that was given the callback.</param>
    /// <param name="callback">The callback parameter of the call, and the position inside it.</param>
    /// <returns>Answer.</returns>
    public static Answer UnknownCallback(IInvocationOperation call, BridgeEnd.Parameter callback)
        => new UnknownAnswer(call, callback);

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
        /// <summary>
        /// Initializes the answer.
        /// </summary>
        /// <param name="path">The path that is missing, counted from where the search ended.</param>
        public NotLoadedAnswer(NavigationPath path)
        {
            Path = path;
        }

        /// <summary>
        /// The path that is missing, counted from where the search ended: "Settings.Profile" for a read of
        /// "user.Settings.Profile" when user comes from "dbContext.Users".
        /// </summary>
        public NavigationPath Path { get; }
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
        /// <param name="from">Where the search found the entities at that value.</param>
        public UnknownAnswer(IOperation? unreadableValue, BridgeEnd from)
        {
            UnreadableValue = unreadableValue;
            From = from;
        }

        /// <summary>
        /// The value the search could not read any further, kept so that INCL004 and the code fix can name it.
        /// Null until the deepest value of the search that sees it names it.
        /// </summary>
        public IOperation? UnreadableValue { get; }

        /// <summary>
        /// Where the search found the entities at <see cref="UnreadableValue"/>: what it handed back, or a
        /// parameter of a callback it was given. This is the <see cref="Bridge.From"/> of the bridge that is
        /// missing, which the code fix offers to write.
        /// </summary>
        public BridgeEnd From { get; }
    }
}
