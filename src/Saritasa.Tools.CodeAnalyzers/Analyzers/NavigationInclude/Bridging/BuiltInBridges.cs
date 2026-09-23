namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// The bridges that ship with the analyzer, written out one per line.
/// </summary>
/// <remarks>
/// Each line means exactly what an <c>[assembly: PassesIncludes(...)]</c> means and goes through the same
/// lookup, so nothing about Microsoft's methods is special. Types are named by metadata name, so the analyzer
/// needs no reference to EF Core. Nothing is inferred: a member not on this list is not crossed.
/// A line names both ends and reads backwards, the way the search walks: <c>from: Result, to: Instance</c>
/// says <c>users.ToList()</c> hands back what <c>users</c> held.
/// A member holding the entities of two places has no line at all, see <see cref="IsLeftOutOnPurpose"/>.
/// Enumerable and Queryable declare most operators identically, so those are written once in
/// <see cref="QueryOperators"/> and read for both; keeping two hand-written copies in step is how Queryable
/// came to claim a ToDictionary it does not have.
/// </remarks>
internal static class BuiltInBridges
{
    private const string LinqEnumerable = "System.Linq.Enumerable";
    private const string LinqQueryable = "System.Linq.Queryable";
    private const string EfQueryable = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions";
    private const string EfRelationalQueryable = "Microsoft.EntityFrameworkCore.RelationalQueryableExtensions";
    private const string EfDbSet = "Microsoft.EntityFrameworkCore.DbSet`1";
    private const string GenericList = "System.Collections.Generic.List`1";
    private const string Array = "System.Array";
    private const string Enumerable = "System.Collections.Generic.IEnumerable`1";
    private const string AsyncEnumerable = "System.Collections.Generic.IAsyncEnumerable`1";
    private const string Enumerator = "System.Collections.Generic.IEnumerator`1";
    private const string AsyncEnumerator = "System.Collections.Generic.IAsyncEnumerator`1";
    private const string OldEnumerable = "System.Collections.IEnumerable";
    private const string OldEnumerator = "System.Collections.IEnumerator";
    private const string GenericIList = "System.Collections.Generic.IList`1";
    private const string ReadOnlyList = "System.Collections.Generic.IReadOnlyList`1";
    private const string Dictionary = "System.Collections.Generic.IDictionary`2";
    private const string ReadOnlyDictionary = "System.Collections.Generic.IReadOnlyDictionary`2";
    private const string KeyValuePair = "System.Collections.Generic.KeyValuePair`2";
    private const string CollectionExtensions = "System.Collections.Generic.CollectionExtensions";
    private const string Task = "System.Threading.Tasks.Task`1";

    /// <summary>
    /// The value the member handed back: <c>users.ToList()</c>, <c>page.Items</c>.
    /// </summary>
    private static readonly BridgeEnd Result = new BridgeEnd.Result();

    /// <summary>
    /// The value the member was used on: the <c>users</c> of <c>users.ToList()</c>.
    /// </summary>
    private static readonly BridgeEnd Instance = new BridgeEnd.Instance();

    /// <summary>
    /// Operators whose result holds the entities of two collections at once, which is why they have no line.
    /// </summary>
    private static readonly HashSet<string> leftOutOnPurpose =
        new(["Concat", "Union", "UnionBy", "Append", "Prepend"], StringComparer.Ordinal);

    /// <summary>
    /// Operators whose result holds the entities it was given, on both Enumerable and Queryable.
    /// </summary>
    private static readonly string[] SameEntitiesInResult =
    [
        "Where",
        "OrderBy",
        "OrderByDescending",
        "ThenBy",
        "ThenByDescending",
        "Order",
        "OrderDescending",
        "Skip",
        "SkipLast",
        "SkipWhile",
        "Take",
        "TakeLast",
        "TakeWhile",
        "Distinct",
        "DistinctBy",
        "Reverse",
        "Intersect",
        "IntersectBy",
        "Except",
        "ExceptBy",
        "DefaultIfEmpty",
        "First",
        "FirstOrDefault",
        "Single",
        "SingleOrDefault",
        "Last",
        "LastOrDefault",
        "ElementAt",
        "ElementAtOrDefault",
        "MinBy",
        "MaxBy",
    ];

    /// <summary>
    /// Operators that hand an element to the first parameter of a callback, on both Enumerable and Queryable.
    /// The ones that hand it somewhere else are written out in <see cref="QueryOperators"/>.
    /// </summary>
    private static readonly (string Member, string Callback)[] ElementInCallback =
    [
        ("Where", "predicate"),
        ("Any", "predicate"),
        ("All", "predicate"),
        ("Count", "predicate"),
        ("LongCount", "predicate"),
        ("First", "predicate"),
        ("FirstOrDefault", "predicate"),
        ("Last", "predicate"),
        ("LastOrDefault", "predicate"),
        ("Single", "predicate"),
        ("SingleOrDefault", "predicate"),
        ("TakeWhile", "predicate"),
        ("SkipWhile", "predicate"),
        ("Select", "selector"),
        ("SelectMany", "selector"),
        ("Sum", "selector"),
        ("Min", "selector"),
        ("Max", "selector"),
        ("Average", "selector"),
        ("OrderBy", "keySelector"),
        ("OrderByDescending", "keySelector"),
        ("ThenBy", "keySelector"),
        ("ThenByDescending", "keySelector"),
        ("GroupBy", "keySelector"),
        ("MinBy", "keySelector"),
        ("MaxBy", "keySelector"),
        ("DistinctBy", "keySelector"),
        ("IntersectBy", "keySelector"),
        ("ExceptBy", "keySelector"),
        ("GroupBy", "elementSelector"),
        ("SelectMany", "collectionSelector"),

        // "SelectMany(collectionSelector, resultSelector)": the element is the first parameter. The second
        // comes from what collectionSelector returned, which is not followed.
        ("SelectMany", "resultSelector"),
    ];

    /// <summary>
    /// Every built-in line.
    /// </summary>
    public static IEnumerable<BuiltInBridge> All =>
    [
        // ----- What Enumerable and Queryable both declare -----
        .. QueryOperators(LinqEnumerable),
        .. QueryOperators(LinqQueryable),

        // ----- Only on Enumerable, which is the one that materializes -----
        Declare(LinqEnumerable, member: "AsEnumerable", from: Result, to: Instance),
        Declare(LinqEnumerable, member: "ToList", from: Result, to: Instance),
        Declare(LinqEnumerable, member: "ToArray", from: Result, to: Instance),
        Declare(LinqEnumerable, member: "ToHashSet", from: Result, to: Instance),
        Declare(
            LinqEnumerable,
            member: "ToDictionary",
            from: Result,
            to: Instance,
            excludeOverloadsWithParameters: ["elementSelector"]),
        Declare(LinqEnumerable, member: "ToDictionary", from: Callback("keySelector"), to: Instance),
        Declare(LinqEnumerable, member: "ToDictionary", from: Callback("elementSelector"), to: Instance),
        Declare(LinqEnumerable, member: "ToLookup", from: Callback("keySelector"), to: Instance),
        Declare(LinqEnumerable, member: "ToLookup", from: Callback("elementSelector"), to: Instance),

        // Zip is handed a second collection, and the two types name that parameter differently.
        Declare(
            LinqEnumerable,
            member: "Zip",
            from: Callback("resultSelector", position: 1),
            to: Parameter("second")),

        // ----- Only on Queryable -----
        Declare(LinqQueryable, member: "AsQueryable", from: Result, to: Instance),
        Declare(
            LinqQueryable,
            member: "Zip",
            from: Callback("resultSelector", position: 1),
            to: Parameter("source2")),

        // ----- EF Core query operators: the result holds the entities it was given -----
        Declare(EfQueryable, member: "Include", from: Result, to: Instance),
        Declare(EfQueryable, member: "ThenInclude", from: Result, to: Instance),
        Declare(EfQueryable, member: "AsNoTracking", from: Result, to: Instance),
        Declare(EfQueryable, member: "AsNoTrackingWithIdentityResolution", from: Result, to: Instance),
        Declare(EfQueryable, member: "AsTracking", from: Result, to: Instance),
        Declare(EfQueryable, member: "IgnoreQueryFilters", from: Result, to: Instance),
        Declare(EfQueryable, member: "IgnoreAutoIncludes", from: Result, to: Instance),
        Declare(EfQueryable, member: "TagWith", from: Result, to: Instance),
        Declare(EfQueryable, member: "TagWithCallSite", from: Result, to: Instance),
        Declare(EfQueryable, member: "AsAsyncEnumerable", from: Result, to: Instance),
        Declare(EfQueryable, member: "FirstAsync", from: Result, to: Instance),
        Declare(EfQueryable, member: "FirstOrDefaultAsync", from: Result, to: Instance),
        Declare(EfQueryable, member: "SingleAsync", from: Result, to: Instance),
        Declare(EfQueryable, member: "SingleOrDefaultAsync", from: Result, to: Instance),
        Declare(EfQueryable, member: "LastAsync", from: Result, to: Instance),
        Declare(EfQueryable, member: "LastOrDefaultAsync", from: Result, to: Instance),
        Declare(EfQueryable, member: "ElementAtAsync", from: Result, to: Instance),
        Declare(EfQueryable, member: "ElementAtOrDefaultAsync", from: Result, to: Instance),
        Declare(EfQueryable, member: "ToListAsync", from: Result, to: Instance),
        Declare(EfQueryable, member: "ToArrayAsync", from: Result, to: Instance),
        Declare(EfQueryable, member: "ToHashSetAsync", from: Result, to: Instance),
        Declare(
            EfQueryable,
            member: "MinAsync",
            from: Result,
            to: Instance,
            excludeOverloadsWithParameters: ["selector"]),
        Declare(
            EfQueryable,
            member: "MaxAsync",
            from: Result,
            to: Instance,
            excludeOverloadsWithParameters: ["selector"]),
        Declare(
            EfQueryable,
            member: "ToDictionaryAsync",
            from: Result,
            to: Instance,
            excludeOverloadsWithParameters: ["elementSelector"]),

        Declare(EfDbSet, member: "AsAsyncEnumerable", from: Result, to: Instance),
        Declare(EfDbSet, member: "AsQueryable", from: Result, to: Instance),

        // Split queries are a relational feature and live in their own assembly and their own type.
        Declare(EfRelationalQueryable, member: "AsSplitQuery", from: Result, to: Instance),
        Declare(EfRelationalQueryable, member: "AsSingleQuery", from: Result, to: Instance),

        // ----- List has its own methods, which win over the Enumerable ones of the same name -----
        Declare(GenericList, member: "ToArray", from: Result, to: Instance),
        Declare(GenericList, member: "Find", from: Result, to: Instance),
        Declare(GenericList, member: "FindLast", from: Result, to: Instance),
        Declare(GenericList, member: "FindAll", from: Result, to: Instance),
        Declare(GenericList, member: "GetRange", from: Result, to: Instance),
        Declare(GenericList, member: "AsReadOnly", from: Result, to: Instance),

        // ----- "foreach" is rewritten by the compiler into GetEnumerator and Current -----
        Declare(Enumerable, member: "GetEnumerator", from: Result, to: Instance),
        Declare(AsyncEnumerable, member: "GetAsyncEnumerator", from: Result, to: Instance),
        Declare(Enumerator, member: "Current", from: Result, to: Instance),
        Declare(AsyncEnumerator, member: "Current", from: Result, to: Instance),

        // "foreach" over an array goes through the old non-generic interfaces.
        Declare(OldEnumerable, member: "GetEnumerator", from: Result, to: Instance),
        Declare(OldEnumerator, member: "Current", from: Result, to: Instance),

        // ----- Reading one entity back out of a collection -----
        Declare(GenericIList, member: "this[]", from: Result, to: Instance),
        Declare(ReadOnlyList, member: "this[]", from: Result, to: Instance),
        Declare(Dictionary, member: "this[]", from: Result, to: Instance),
        Declare(Dictionary, member: "Values", from: Result, to: Instance),
        Declare(ReadOnlyDictionary, member: "this[]", from: Result, to: Instance),
        Declare(ReadOnlyDictionary, member: "Values", from: Result, to: Instance),
        Declare(KeyValuePair, member: "Value", from: Result, to: Instance),
        Declare(CollectionExtensions, member: "GetValueOrDefault", from: Result, to: Instance),
        Declare(Task, member: "Result", from: Result, to: Instance),

        // TryGetValue hands the entity back through its out argument; its result is a bool and holds nothing.
        Declare(Dictionary, member: "TryGetValue", from: OutArgument("value"), to: Instance),
        Declare(ReadOnlyDictionary, member: "TryGetValue", from: OutArgument("value"), to: Instance),

        // ----- EF Core: a callback is handed an element -----
        Declare(EfQueryable, member: "AnyAsync", from: Callback("predicate"), to: Instance),
        Declare(EfQueryable, member: "AllAsync", from: Callback("predicate"), to: Instance),
        Declare(EfQueryable, member: "CountAsync", from: Callback("predicate"), to: Instance),
        Declare(EfQueryable, member: "LongCountAsync", from: Callback("predicate"), to: Instance),
        Declare(EfQueryable, member: "FirstAsync", from: Callback("predicate"), to: Instance),
        Declare(EfQueryable, member: "FirstOrDefaultAsync", from: Callback("predicate"), to: Instance),
        Declare(EfQueryable, member: "LastAsync", from: Callback("predicate"), to: Instance),
        Declare(EfQueryable, member: "LastOrDefaultAsync", from: Callback("predicate"), to: Instance),
        Declare(EfQueryable, member: "SingleAsync", from: Callback("predicate"), to: Instance),
        Declare(EfQueryable, member: "SingleOrDefaultAsync", from: Callback("predicate"), to: Instance),
        Declare(EfQueryable, member: "MinAsync", from: Callback("selector"), to: Instance),
        Declare(EfQueryable, member: "MaxAsync", from: Callback("selector"), to: Instance),
        Declare(EfQueryable, member: "SumAsync", from: Callback("selector"), to: Instance),
        Declare(EfQueryable, member: "AverageAsync", from: Callback("selector"), to: Instance),
        Declare(EfQueryable, member: "ToDictionaryAsync", from: Callback("keySelector"), to: Instance),
        Declare(EfQueryable, member: "ToDictionaryAsync", from: Callback("elementSelector"), to: Instance),
        Declare(EfQueryable, member: "ForEachAsync", from: Callback("action"), to: Instance),

        // ----- Collections that call you back with an element -----
        Declare(GenericList, member: "ForEach", from: Callback("action"), to: Instance),
        Declare(Array, member: "ForEach", from: Callback("action"), to: Parameter("array")),
    ];

    /// <summary>
    /// True for a member left off the table on purpose, rather than one simply not reached yet.
    /// </summary>
    /// <remarks>
    /// These hand back the entities of two places at once and one line can name only one. A declaration
    /// would be believed and would answer for entities nobody looked at, so nothing offers to write one.
    /// </remarks>
    /// <param name="declaringType">Metadata name of the type that declares the member.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <returns>True when it is one of them.</returns>
    public static bool IsLeftOutOnPurpose(string declaringType, string memberName)
        => (declaringType == LinqEnumerable || declaringType == LinqQueryable) &&
           leftOutOnPurpose.Contains(memberName);

    /// <summary>
    /// The operators Enumerable and Queryable declare the same way, read for one of them. Both types spell
    /// these members and their parameters identically, so a line that is right for one is right for the other.
    /// </summary>
    private static IEnumerable<BuiltInBridge> QueryOperators(string type) =>
    [
        .. SameEntitiesInResult.Select(member => Declare(type, member, from: Result, to: Instance)),

        // "Min(selector)" hands back what the selector returned, not an element. The name of that parameter is
        // the only thing separating it from "Min(comparer)", which takes just as many arguments.
        Declare(type, member: "Min", from: Result, to: Instance, excludeOverloadsWithParameters: ["selector"]),
        Declare(type, member: "Max", from: Result, to: Instance, excludeOverloadsWithParameters: ["selector"]),

        .. ElementInCallback.Select(entry =>
            Declare(type, entry.Member, from: Callback(entry.Callback), to: Instance)),

        // "GroupBy(keySelector, resultSelector)": the group is the second parameter, the key is not an element.
        Declare(type, member: "GroupBy", from: Callback("resultSelector", position: 1), to: Instance),

        // "Aggregate(seed, func)": the element is the second parameter, the accumulator is not.
        Declare(type, member: "Aggregate", from: Callback("func", position: 1), to: Instance),

        // Zip and the join operators are given a second collection of their own. Zip's is named differently on
        // each type, so those two lines are in All rather than here.
        Declare(type, member: "Zip", from: Callback("resultSelector"), to: Instance),
        Declare(type, member: "Join", from: Callback("outerKeySelector"), to: Instance),
        Declare(type, member: "Join", from: Callback("innerKeySelector"), to: Parameter("inner")),
        Declare(type, member: "Join", from: Callback("resultSelector"), to: Instance),
        Declare(type, member: "Join", from: Callback("resultSelector", position: 1), to: Parameter("inner")),
        Declare(type, member: "GroupJoin", from: Callback("outerKeySelector"), to: Instance),
        Declare(type, member: "GroupJoin", from: Callback("innerKeySelector"), to: Parameter("inner")),
        Declare(type, member: "GroupJoin", from: Callback("resultSelector"), to: Instance),
        Declare(type, member: "GroupJoin", from: Callback("resultSelector", position: 1), to: Parameter("inner")),
    ];

    /// <summary>
    /// One line of the table.
    /// </summary>
    /// <param name="declaringType">Metadata name of the type that declares the member.</param>
    /// <param name="member">Name of the method or property.</param>
    /// <param name="from">Where the search is standing when it reaches this member.</param>
    /// <param name="to">Where the same entities came from.</param>
    /// <param name="excludeOverloadsWithParameters">Parameters of the overloads this line does not describe.</param>
    /// <returns>The record.</returns>
    private static BuiltInBridge Declare(
        string declaringType,
        string member,
        BridgeEnd from,
        BridgeEnd to,
        string[]? excludeOverloadsWithParameters = null)
        => new(declaringType, member, new Bridge(from, to), excludeOverloadsWithParameters ?? []);

    /// <summary>
    /// A parameter of the call, by name: the <c>inner</c> of <c>Join(outer, inner, ...)</c>.
    /// </summary>
    /// <param name="name">Name of the parameter.</param>
    /// <returns>The end.</returns>
    private static BridgeEnd Parameter(string name)
        => new BridgeEnd.Parameter(name);

    /// <summary>
    /// A parameter of a callback the member is given: the <c>u</c> of <c>users.Select(u =&gt; ...)</c>.
    /// </summary>
    /// <param name="name">Name of the parameter that takes the callback.</param>
    /// <param name="position">Position of the callback's own parameter.</param>
    /// <returns>The end.</returns>
    private static BridgeEnd Callback(string name, int position = 0)
        => new BridgeEnd.Parameter(name, position);

    /// <summary>
    /// An out argument the member wrote: the <c>user</c> of <c>dictionary.TryGetValue(id, out var user)</c>.
    /// </summary>
    /// <param name="name">Name of the out parameter.</param>
    /// <returns>The end.</returns>
    private static BridgeEnd OutArgument(string name)
        => new BridgeEnd.Parameter(name);
}
