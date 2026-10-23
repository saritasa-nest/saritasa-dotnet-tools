using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.EntityFramework;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Roslyn;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

/// <summary>
/// Answers one question: "is the navigation path loaded in this value?".
/// </summary>
/// <remarks>
/// A value never says by itself whether the path is loaded, so the search reads backwards through the
/// values it was made from. A variable is the one case it cannot do alone: it asks the
/// <see cref="WritesWalker"/> and decides what the writes mean. Where several paths meet, all must be loaded.
/// <para>
/// The path asked for changes on the way: a navigation read adds a segment in front ("Profile" of
/// <c>user.Settings</c> is "Settings.Profile" of <c>user</c>), and a bridge keeps it.
/// </para>
/// To teach the search a new method, annotate it with [PassesIncludes] rather than adding a case here.
/// </remarks>
internal sealed class IncludeSearcher
{
    private const string EfDbSet = "Microsoft.EntityFrameworkCore.DbSet`1";

    private readonly WritesWalker walker = new();

    private IncludeSearcher()
    {
    }

    /// <summary>
    /// Answers whether the navigation path is loaded in the value.
    /// </summary>
    /// <param name="value">Value, e.g. a call argument or a returned expression.</param>
    /// <param name="path">Navigation path.</param>
    /// <param name="position">Position of the statement that contains the value.</param>
    /// <returns>Answer for the value.</returns>
    public static Answer Check(IOperation value, NavigationPath path, CodePosition position)
        => new IncludeSearcher().Search(Value.Create(value, position), path);

    /// <summary>
    /// Reads one value, and names it if it is the one that stopped the search. The deepest value wins, since
    /// only the first frame to see Unknown has nothing to name yet.
    /// </summary>
    private Answer Search(Value value, NavigationPath path)
    {
        var answer = ReadValue(value, path);

        return answer is Answer.UnknownAnswer { UnreadableValue: null }
            ? Answer.Unknown(value.Operation)
            : answer;
    }

    /// <summary>
    /// Reads the value an expression holds, or answers <see cref="Answer.Unknown"/> when there is no
    /// expression to read.
    /// </summary>
    private Answer Search(IOperation? operation, CodePosition position, NavigationPath path)
        => operation is null ? Answer.Unknown() : Search(Value.Create(operation, position), path);

    /// <summary>
    /// The answer for one value, from the value alone.
    /// </summary>
    private Answer ReadValue(Value value, NavigationPath path)
        => value.Operation switch
        {
            // "user", "users", "out var user", "#1": reading a variable is the only expression whose answer
            // depends on what ran before it, so it is the only one the walker has to follow.
            _ when WritesWalker.GetVariable(value.Operation) is { } variable
                => SearchAnswerInValuesWrites(variable, value.Position, path),

            // "query.Include(u => u.Profile)", "query.Include(u => u.Settings).ThenInclude(s => s.Profile)", also
            // when "query.Include(...)" sits in a variable.
            _ when ReadIncludeChain(value, path) is { } includeAnswer
                => includeAnswer,

            // A call of a method annotated with [Includes("Profile")].
            IInvocationOperation call when AttributeReader.MethodHasIncludesAttribute(call.TargetMethod, path)
                => Answer.Loaded,

            // An annotated move: "query.Where(...)", "users.ToList()", "users[0]", "pair.Value",
            // "query.Paginate(1)" keep the same entities; "users.Select(u => u.Settings)" moves to their Settings,
            // so users must have "Settings" in front of the path loaded. It says which values they came from.
            _ when BridgeCrosser.FromValue(value) is { Count: > 0 } crossings
                => SearchSources(crossings, value.Position, path),

            // A call nobody annotated. In our own code that is a real answer: the method promises nothing with
            // [Includes] or [PassesIncludes], so nothing loads the path. In someone else's code we
            // simply cannot see.
            IInvocationOperation call
                => IsOurOwnCode(call.TargetMethod, value.Position) ? Answer.NotLoaded(path) : Answer.Unknown(),

            // "new User()", "new List<User>()": nothing here was sourced from a query, so there was no Include
            // to miss — Include simply does not apply, which this treats as satisfied rather than a violation.
            IObjectCreationOperation
                => Answer.Loaded,

            // "dbContext.Users": the query starts here and no Include was put on it.
            IPropertyReferenceOperation reference when IsEntitySet(reference.Property.Type)
                => Answer.NotLoaded(path),

            // "user.Settings": "Profile" of it is loaded when "Settings.Profile" of user is.
            IPropertyReferenceOperation { Instance: { } instance } reference
                => Search(instance, value.Position, path.Prepend(reference.Property.Name)),

            // A field, a call we could not follow, anything else: we cannot tell.
            _ => Answer.Unknown(),
        };

    /// <summary>
    /// The answer for a value of an <c>Include(...).ThenInclude(...)</c> chain; null when the value is not part of
    /// one and the search goes on as usual.
    /// </summary>
    private Answer? ReadIncludeChain(Value value, NavigationPath path)
    {
        var insideMatch = path.Matched > 0;

        // Inside a match only the rest of the include chain can continue it.
        var notIncludeAnswer = insideMatch ? Answer.NotLoaded(path) : null;

        if (value.Operation is not IInvocationOperation call)
        {
            return notIncludeAnswer;
        }

        var thenIncludeChain = EfIncludeReader.ReadThenInclude(call);
        if ((EfIncludeReader.ReadInclude(call) ?? thenIncludeChain) is not { } chain)
        {
            return notIncludeAnswer;
        }

        var continuesChain = thenIncludeChain is not null;
        var receiver = RoslynReader.GetReceiver(call);

        // The step matches the end of what is left of the path with its first "count" properties. Inside a match
        // it must match with all of them. An Include starts at the entity, so nothing may be left in front; a
        // ThenInclude continues its receiver, which must end exactly at what is left in front.
        for (var count = insideMatch ? chain.Count : 1; count <= chain.Count; count++)
        {
            if (!path.RemainingEndsWith(chain.Take(count).ToList()))
            {
                continue;
            }

            var leftInFront = path.Remaining.Length - count;

            if (!continuesChain && leftInFront == 0)
            {
                return Answer.Loaded;
            }

            if (continuesChain && leftInFront > 0 &&
                Search(receiver, value.Position, path.MatchTail(count)) is Answer.LoadedAnswer)
            {
                return Answer.Loaded;
            }
        }

        // Outside a match the step is a query operator like any other: the receiver holds the same entities.
        return insideMatch
            ? Answer.NotLoaded(path)
            : Search(receiver, value.Position, path);
    }

    /// <summary>
    /// The answer for a variable: every write the walker finds must have the path loaded.
    /// </summary>
    private Answer SearchAnswerInValuesWrites(object variable, CodePosition position, NavigationPath path)
        => JoinPaths(
            walker.FindWrites(variable, position, path)
                .Select(write => ReadWrite(write, path))
            );

    /// <summary>
    /// What one write of the walker means for the navigation path.
    /// </summary>
    private Answer ReadWrite(Write write, NavigationPath path)
        => write switch
        {
            // "user = x": read the value the variable got.
            Write.Written written => Search(written.Value, path),

            // "dictionary.TryGetValue(id, out var user)".
            Write.OutArgument outArgument => ReadOutArgumentSource(outArgument, path),

            // The caller of a method that asks for the path with [IncludeRequired] is the one that loads
            // it. A method that does not ask for it is a real answer: nobody loads the path, and the parameter
            // is where to ask for it. The path is already counted from the parameter.
            Write.MethodParameter parameter
                => IsRequiredFromCaller(parameter.Parameter, path)
                    ? Answer.Loaded
                    : Answer.NotRequiredBy(parameter.Parameter, path),

            // "users.Select(u => ...)": the parameter holds an element of users.
            Write.LambdaParameter lambda => ReadLambdaSource(lambda, path),

            // A loop came back to a place the search already read, so this path adds nothing new.
            Write.NothingNew => Answer.Loaded,

            // Nothing ever wrote the variable, so nothing loaded the path either.
            Write.NeverWritten => Answer.NotLoaded(path),

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
            if (answer is not Answer.LoadedAnswer)
            {
                return answer;
            }
        }

        return Answer.Loaded;
    }

    /// <summary>
    /// "dictionary.TryGetValue(id, out var user)": the bridge says user comes from the dictionary. A method
    /// nobody annotated could have put anything there, so we cannot tell.
    /// </summary>
    private Answer ReadOutArgumentSource(Write.OutArgument outArgument, NavigationPath path)
    {
        var call = outArgument.Call;

        if (call.Operation is not IInvocationOperation invocation)
        {
            return Answer.Unknown();
        }

        return SearchSources(
            BridgeCrosser.FromOutArgument(invocation, outArgument.ParameterName, call.Position.FlowGraph.Bridges),
            call.Position,
            path);
    }

    /// <summary>
    /// "users.Select(u =&gt; ...)": the parameter is filled from users. For anything else, such as the index
    /// of "Select((u, i) =&gt; ...)", we cannot tell what it holds.
    /// </summary>
    private Answer ReadLambdaSource(Write.LambdaParameter lambda, NavigationPath path)
    {
        var crossings = BridgeCrosser.FromLambdaParameter(
            lambda.Lambda,
            lambda.Parameter.Ordinal,
            lambda.Creation.FlowGraph.Bridges);

        // The parameter is not a value of its own to name, so name the call that fills it and the callback it
        // arrives through: that is where an annotation would go.
        if (crossings.Count == 0 &&
            BridgeCrosser.GetLambdaArgument(lambda.Lambda) is
                { Parameter: { } callback, Parent: IInvocationOperation call })
        {
            return Answer.UnknownCallback(call, new BridgeEnd.Parameter(callback.Name, lambda.Parameter.Ordinal));
        }

        return SearchSources(crossings, lambda.Creation, path);
    }

    /// <summary>
    /// The answer for the values the bridges landed on, each asked for the path with what its bridge put in front
    /// of it. Several of them, as for <c>first.Concat(second)</c>, must all have the path loaded, like the
    /// branches of an "if". No bridge at all means nothing says where the entities came from, so we cannot tell.
    /// </summary>
    private Answer SearchSources(IReadOnlyList<Crossing> crossings, CodePosition position, NavigationPath path)
        => crossings.Count == 0
            ? Answer.Unknown()
            : JoinPaths(crossings.Select(crossing =>
                Search(crossing.Source, position, path.Prepend([.. crossing.Prefix]))));

    /// <summary>
    /// True if the method asks for the path with [IncludeRequired], making its caller responsible.
    /// </summary>
    private static bool IsRequiredFromCaller(IParameterSymbol parameter, NavigationPath path)
        => parameter.ContainingSymbol is IMethodSymbol method &&
           AttributeReader.MethodHasIncludeRequiredAttribute(method, parameter.Name, path);

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
    private static bool IsEntitySet(ITypeSymbol type) => RoslynReader.IsType(type, EfDbSet);
}
