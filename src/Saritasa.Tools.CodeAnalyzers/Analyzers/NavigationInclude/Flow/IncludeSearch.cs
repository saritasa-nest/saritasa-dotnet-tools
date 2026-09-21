using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Answers one question: "is the navigation property loaded in this value?".
/// </summary>
/// <remarks>
/// A value never says by itself whether the property is loaded, so the search reads it, and where it was made
/// from another value it reads that one too, the way a person reads code backwards. Reading a variable is the
/// only case it cannot do alone: it asks the <see cref="Walker"/> and decides what the writes mean. When
/// several paths lead to the same place, every one of them must be loaded.
/// To teach the search a new method, declare it with [PassesIncludes] rather than adding a case here.
/// </remarks>
internal sealed class IncludeSearch
{
    private readonly string property;
    private readonly Walker walker;

    private IncludeSearch(string property)
    {
        this.property = property;
        walker = new Walker(property);
    }

    /// <summary>
    /// Answers whether the navigation property is loaded in the value.
    /// </summary>
    /// <param name="value">Value, e.g. a call argument or a returned expression.</param>
    /// <param name="property">Navigation property name.</param>
    /// <param name="position">Position of the statement that contains the value.</param>
    /// <returns>Answer for the value.</returns>
    public static Answer Check(IOperation value, string property, CodePosition position)
        => new IncludeSearch(property).Search(Value.Create(value, position));

    /// <summary>
    /// Reads one value, and names it when it turns out to be the one that stopped the search. The value named
    /// is the deepest one, because only the first frame that sees Unknown has nothing to name yet.
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
            _ when Walker.GetVariable(value.Operation) is { } variable
                => ReadWrites(variable, value.Position),

            // "query.Include(u => u.Profile)", a call of a method declared with [Includes("Profile")].
            IInvocationOperation call when LoadsProperty(call)
                => Answer.Loaded,

            // A move that keeps the same entities: "query.Where(...)", "users.ToList()", "users[0]",
            // "page.Items", "query.Paginate(1)". It says which value they came from.
            _ when Bridge.FindSource(value) is { } source
                => Search(source, value.Position),

            // A call nobody declared. In our own code that is a real answer: the method promises nothing with
            // [Includes] or [PassesIncludes], so nothing loads the property. In someone else's code we
            // simply cannot see.
            IInvocationOperation call
                => IsOurOwnCode(call.TargetMethod, value.Position) ? Answer.NotLoaded : Answer.Unknown(),

            // "new User()": a fresh entity definitely has nothing loaded on it. "new List<User>()" is a
            // different claim: a fresh container says nothing about what is put into it later, and the analyzer
            // does not follow mutation, so the honest answer there is that we cannot tell.
            IObjectCreationOperation creation
                => IsContainer(creation.Type) ? Answer.Unknown() : Answer.NotLoaded,

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
        => JoinPaths(walker.FindWrites(variable, position).Select(ReadWrite));

    /// <summary>
    /// What one write of the walker means for the navigation property.
    /// </summary>
    private Answer ReadWrite(Write write)
        => write switch
        {
            // "user = x": read the value the variable got.
            Write.Written written => Search(written.Value),

            // "user.Profile = x": somebody loaded the property by hand.
            Write.MemberWritten => Answer.Loaded,

            // "dictionary.TryGetValue(id, out var user)".
            Write.OutArgument outArgument => ReadOutArgumentCall(outArgument.Call),

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
    /// Every path must have the property loaded. The first path that is not loaded is the answer, so the
    /// search reads no more code than it has to, and a path it cannot read hides a later one it could have
    /// answered: a suggestion instead of a warning, which is the safe way round.
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
    /// "dictionary.TryGetValue(id, out var user)": user is a value of the dictionary, because the declaration
    /// of TryGetValue says the entities come from the dictionary. A method nobody declared could have put
    /// anything into the out argument, so we cannot tell.
    /// </summary>
    private Answer ReadOutArgumentCall(Value call)
        => Bridge.FindSource(call) is { } source
            ? Search(source, call.Position)
            : Answer.Unknown();

    /// <summary>
    /// "users.Select(u =&gt; ...)": the parameter is filled from users. For a parameter filled from something
    /// else ("Select((u, i) =&gt; ...)") or a lambda of a method we cannot read, we cannot tell what it holds.
    /// </summary>
    private Answer ReadLambdaSource(Write.LambdaParameter lambda)
        => Search(
            Bridge.FindSource(lambda.Lambda, lambda.Parameter.Ordinal, lambda.Creation.FlowGraph.Declarations),
            lambda.Creation);

    /// <summary>
    /// True if the parameter's method asks for the property with [IncludeRequired], which makes the caller of
    /// that method responsible for loading it.
    /// </summary>
    private bool IsRequiredFromCaller(IParameterSymbol parameter)
        => parameter.ContainingSymbol is IMethodSymbol method &&
           AttributeHelper.MethodHasIncludeRequiredAttribute(method, parameter.Name, property);

    /// <summary>
    /// True if the call loads the property itself: <c>query.Include(u =&gt; u.Profile)</c> or a call of a method
    /// declared with <c>[Includes("Profile")]</c>.
    /// </summary>
    private bool LoadsProperty(IInvocationOperation call)
        => AttributeHelper.MethodHasIncludesAttribute(call.TargetMethod, property) ||
           EfIncludes.GetIncludedProperty(call) == property;

    /// <summary>
    /// True if the method is declared in the assembly being compiled, where the developer can read it and put
    /// [Includes] on it. A method from anywhere else cannot be read or annotated.
    /// </summary>
    private static bool IsOurOwnCode(IMethodSymbol method, CodePosition position)
        => SymbolEqualityComparer.Default.Equals(
            method.ContainingAssembly,
            position.FlowGraph.Method.ContainingAssembly);

    /// <summary>
    /// True for a type that holds other objects, such as "List&lt;User&gt;". A fresh one says nothing about
    /// what is put into it afterwards, while a fresh entity definitely has nothing loaded.
    /// </summary>
    private static bool IsContainer(ITypeSymbol? type)
        => type is not null &&
           type.SpecialType != SpecialType.System_String &&
           (type.SpecialType == SpecialType.System_Collections_IEnumerable ||
            type.AllInterfaces.Any(implemented =>
                implemented.SpecialType == SpecialType.System_Collections_IEnumerable));

    /// <summary>
    /// True for the "DbSet&lt;User&gt;" of a context property, where a query starts.
    /// </summary>
    private static bool IsEntitySet(ITypeSymbol type)
        => type is INamedTypeSymbol named &&
           named.ConstructedFrom.MetadataName == "DbSet`1" &&
           named.ContainingNamespace?.ToDisplayString() == "Microsoft.EntityFrameworkCore";
}
