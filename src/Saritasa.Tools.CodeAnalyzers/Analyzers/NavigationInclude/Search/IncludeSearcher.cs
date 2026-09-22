using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.EntityFramework;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

/// <summary>
/// Answers one question: "is the navigation property loaded in this value?".
/// </summary>
/// <remarks>
/// A value never says by itself whether the property is loaded, so the search reads backwards through the
/// values it was made from. A variable is the one case it cannot do alone: it asks the
/// <see cref="WritesWalker"/> and decides what the writes mean. Where several paths meet, all must be loaded.
/// To teach the search a new method, declare it with [PassesIncludes] rather than adding a case here.
/// </remarks>
internal sealed class IncludeSearcher
{
    private readonly string property;
    private readonly WritesWalker walker;

    private IncludeSearcher(string property)
    {
        this.property = property;
        walker = new WritesWalker();
    }

    /// <summary>
    /// Answers whether the navigation property is loaded in the value.
    /// </summary>
    /// <param name="value">Value, e.g. a call argument or a returned expression.</param>
    /// <param name="property">Navigation property name.</param>
    /// <param name="position">Position of the statement that contains the value.</param>
    /// <returns>Answer for the value.</returns>
    public static Answer Check(IOperation value, string property, CodePosition position)
        => new IncludeSearcher(property).Search(Value.Create(value, position));

    /// <summary>
    /// Reads one value, and names it if it is the one that stopped the search. The deepest value wins, since
    /// only the first frame to see Unknown has nothing to name yet.
    /// </summary>
    private Answer Search(Value value)
    {
        var answer = ReadValue(value);

        return answer.IsUnknown && answer.UnreadableValue is null
            ? Answer.Unknown(value.Operation)
            : answer;
    }

    /// <summary>
    /// Reads the value an expression holds, or answers <see cref="Answer.Unknown"/> when there is no
    /// expression to read.
    /// </summary>
    private Answer Search(IOperation? operation, CodePosition position)
        => operation is null ? Answer.Unknown() : Search(Value.Create(operation, position));

    /// <summary>
    /// The answer for one value, from the value alone.
    /// </summary>
    private Answer ReadValue(Value value)
        => value.Operation switch
        {
            // "user", "users", "out var user", "#1": reading a variable is the only expression whose answer
            // depends on what ran before it, so it is the only one the walker has to follow.
            _ when WritesWalker.GetVariable(value.Operation) is { } variable
                => ReadWrites(variable, value.Position),

            // "query.Include(u => u.Profile)", a call of a method declared with [Includes("Profile")].
            IInvocationOperation call when LoadsProperty(call)
                => Answer.Loaded,

            // A move that keeps the same entities: "query.Where(...)", "users.ToList()", "users[0]",
            // "page.Items", "query.Paginate(1)". It says which value they came from.
            _ when BridgeCrosser.FromValue(value) is { } source
                => Search(source, value.Position),

            // A call nobody declared. In our own code that is a real answer: the method promises nothing with
            // [Includes] or [PassesIncludes], so nothing loads the property. In someone else's code we
            // simply cannot see.
            IInvocationOperation call
                => IsOurOwnCode(call.TargetMethod, value.Position) ? Answer.NotLoaded : Answer.Unknown(),

            // "new User()", "new List<User>()": nothing here was sourced from a query, so there was no Include
            // to miss — Include simply does not apply, which this treats as satisfied rather than a violation.
            IObjectCreationOperation
                => Answer.Loaded,

            // "dbContext.Users": the query starts here and no Include was put on it.
            IPropertyReferenceOperation reference when IsEntitySet(reference.Property.Type)
                => Answer.NotLoaded,

            // A field, a call we could not follow, anything else: we cannot tell.
            _ => Answer.Unknown(),
        };

    /// <summary>
    /// The answer for a variable: every write the walker finds must have the property loaded.
    /// </summary>
    private Answer ReadWrites(object variable, CodePosition position)
        => JoinPaths(
            walker.FindWrites(variable, position)
                .Select(ReadWrite)
            );

    /// <summary>
    /// What one write of the walker means for the navigation property.
    /// </summary>
    private Answer ReadWrite(Write write)
        => write switch
        {
            // "user = x": read the value the variable got.
            Write.Written written => Search(written.Value),

            // "dictionary.TryGetValue(id, out var user)".
            Write.OutArgument outArgument => ReadOutArgumentSource(outArgument),

            // The caller of a method that asks for the property with [IncludeRequired] is the one that loads
            // it. A method that does not ask for it is a real answer: nobody loads the property.
            Write.MethodParameter parameter
                => IsRequiredFromCaller(parameter.Parameter) ? Answer.Loaded : Answer.NotLoaded,

            // "users.Select(u => ...)": the parameter holds an element of users.
            Write.LambdaParameter lambda => ReadLambdaSource(lambda),

            // A loop came back to a place the search already read, so this path adds nothing new.
            Write.NothingNew => Answer.Loaded,

            // Nothing ever wrote the variable, so nothing loaded the property either.
            Write.NeverWritten => Answer.NotLoaded,

            _ => Answer.Unknown(),
        };

    /// <summary>
    /// Every path must be loaded, and the first that is not is the answer. An unreadable path therefore
    /// hides a later definite one: a suggestion instead of a warning, which is the safe way round.
    /// </summary>
    private static Answer JoinPaths(IEnumerable<Answer> answers)
    {
        foreach (var answer in answers)
        {
            if (!answer.IsLoaded)
            {
                return answer;
            }
        }

        return Answer.Loaded;
    }

    /// <summary>
    /// "dictionary.TryGetValue(id, out var user)": the bridge says user comes from the dictionary. A method
    /// nobody declared could have put anything there, so we cannot tell.
    /// </summary>
    private Answer ReadOutArgumentSource(Write.OutArgument outArgument)
    {
        var call = outArgument.Call;

        if (call.Operation is not IInvocationOperation invocation)
        {
            return Answer.Unknown();
        }

        return BridgeCrosser.FromOutArgument(
            invocation,
            outArgument.ParameterName,
            call.Position.FlowGraph.Bridges) is { } source
            ? Search(source, call.Position)
            : Answer.Unknown();
    }

    /// <summary>
    /// "users.Select(u =&gt; ...)": the parameter is filled from users. For anything else, such as the index
    /// of "Select((u, i) =&gt; ...)", we cannot tell what it holds.
    /// </summary>
    private Answer ReadLambdaSource(Write.LambdaParameter lambda)
        => Search(
            BridgeCrosser.FromLambdaParameter(lambda.Lambda, lambda.Parameter.Ordinal, lambda.Creation.FlowGraph.Bridges),
            lambda.Creation);

    /// <summary>
    /// True if the method asks for the property with [IncludeRequired], making its caller responsible.
    /// </summary>
    private bool IsRequiredFromCaller(IParameterSymbol parameter)
        => parameter.ContainingSymbol is IMethodSymbol method &&
           AttributeReader.MethodHasIncludeRequiredAttribute(method, parameter.Name, property);

    /// <summary>
    /// True if the call loads the property itself: <c>query.Include(u =&gt; u.Profile)</c> or a call of a method
    /// declared with <c>[Includes("Profile")]</c>.
    /// </summary>
    private bool LoadsProperty(IInvocationOperation call)
        => AttributeReader.MethodHasIncludesAttribute(call.TargetMethod, property) ||
           EfIncludeReader.GetIncludedProperty(call) == property;

    /// <summary>
    /// True if the method is in the assembly being compiled, where it can be read and annotated.
    /// </summary>
    private static bool IsOurOwnCode(IMethodSymbol method, CodePosition position)
        => SymbolEqualityComparer.Default.Equals(
            method.ContainingAssembly,
            position.FlowGraph.Method.ContainingAssembly);

    /// <summary>
    /// True for the "DbSet&lt;User&gt;" of a context property, where a query starts.
    /// </summary>
    private static bool IsEntitySet(ITypeSymbol type)
        => type is INamedTypeSymbol named &&
           named.ConstructedFrom.MetadataName == "DbSet`1" &&
           named.ContainingNamespace?.ToDisplayString() == "Microsoft.EntityFrameworkCore";
}
