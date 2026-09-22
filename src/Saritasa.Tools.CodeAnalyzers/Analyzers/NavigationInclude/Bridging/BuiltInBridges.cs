using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// The bridges that ship with the analyzer, written out one per line.
/// </summary>
/// <remarks>
/// Each line means exactly what an <c>[assembly: PassesIncludes(...)]</c> means and goes through the same
/// lookup, so nothing about Microsoft's methods is special. Types are named by metadata name, so the analyzer
/// needs no reference to EF Core. Nothing is inferred: a member not on this list is not crossed, and to see
/// what the analyzer knows about a method you search for its name.
/// Lines are written the way the search walks, backwards: the factory names the end the bridge runs from —
/// <c>Result</c>, <c>Lambda</c>, <c>Out</c> — and the rest names where it lands.
/// Enumerable and Queryable declare most operators identically, so those are written once in
/// <see cref="QueryOperators"/> and read for both. What only one of them has is written out in
/// <see cref="All"/>; keeping two hand-written copies in step is how Queryable came to claim a ToDictionary it
/// does not have.
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
        "Concat",
        "Union",
        "UnionBy",
        "Intersect",
        "IntersectBy",
        "Except",
        "ExceptBy",
        "Append",
        "Prepend",
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
        ("UnionBy", "keySelector"),
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
    public static IEnumerable<(string DeclaringType, Bridge Bridge)> All =>
    [
        // ----- What Enumerable and Queryable both declare -----
        .. QueryOperators(LinqEnumerable),
        .. QueryOperators(LinqQueryable),

        // ----- Only on Enumerable, which is the one that materializes -----
        Result(LinqEnumerable, "AsEnumerable"),
        Result(LinqEnumerable, "ToList"),
        Result(LinqEnumerable, "ToArray"),
        Result(LinqEnumerable, "ToHashSet"),
        ResultExceptOverloadWith(LinqEnumerable, "ToDictionary", "elementSelector"),
        Lambda(LinqEnumerable, "ToDictionary", "keySelector"),
        Lambda(LinqEnumerable, "ToDictionary", "elementSelector"),
        Lambda(LinqEnumerable, "ToLookup", "keySelector"),
        Lambda(LinqEnumerable, "ToLookup", "elementSelector"),

        // Zip is handed a second collection, and the two types name that parameter differently.
        Lambda(LinqEnumerable, "Zip", "resultSelector", lambdaParameter: 1, source: "second"),

        // ----- Only on Queryable -----
        Result(LinqQueryable, "AsQueryable"),
        Lambda(LinqQueryable, "Zip", "resultSelector", lambdaParameter: 1, source: "source2"),

        // ----- EF Core query operators: the result holds the entities it was given -----
        Result(EfQueryable, "Include"),
        Result(EfQueryable, "ThenInclude"),
        Result(EfQueryable, "AsNoTracking"),
        Result(EfQueryable, "AsNoTrackingWithIdentityResolution"),
        Result(EfQueryable, "AsTracking"),
        Result(EfQueryable, "IgnoreQueryFilters"),
        Result(EfQueryable, "IgnoreAutoIncludes"),
        Result(EfQueryable, "TagWith"),
        Result(EfQueryable, "TagWithCallSite"),
        Result(EfQueryable, "AsAsyncEnumerable"),
        Result(EfQueryable, "FirstAsync"),
        Result(EfQueryable, "FirstOrDefaultAsync"),
        Result(EfQueryable, "SingleAsync"),
        Result(EfQueryable, "SingleOrDefaultAsync"),
        Result(EfQueryable, "LastAsync"),
        Result(EfQueryable, "LastOrDefaultAsync"),
        Result(EfQueryable, "ElementAtAsync"),
        Result(EfQueryable, "ElementAtOrDefaultAsync"),
        Result(EfQueryable, "ToListAsync"),
        Result(EfQueryable, "ToArrayAsync"),
        Result(EfQueryable, "ToHashSetAsync"),
        ResultExceptOverloadWith(EfQueryable, "MinAsync", "selector"),
        ResultExceptOverloadWith(EfQueryable, "MaxAsync", "selector"),
        ResultExceptOverloadWith(EfQueryable, "ToDictionaryAsync", "elementSelector"),

        Result(EfDbSet, "AsAsyncEnumerable"),
        Result(EfDbSet, "AsQueryable"),

        // Split queries are a relational feature and live in their own assembly and their own type.
        Result(EfRelationalQueryable, "AsSplitQuery"),
        Result(EfRelationalQueryable, "AsSingleQuery"),

        // ----- List has its own methods, which win over the Enumerable ones of the same name -----
        Result(GenericList, "ToArray"),
        Result(GenericList, "Find"),
        Result(GenericList, "FindLast"),
        Result(GenericList, "FindAll"),
        Result(GenericList, "GetRange"),
        Result(GenericList, "AsReadOnly"),

        // ----- "foreach" is rewritten by the compiler into GetEnumerator and Current -----
        Result("System.Collections.Generic.IEnumerable`1", "GetEnumerator"),
        Result("System.Collections.Generic.IAsyncEnumerable`1", "GetAsyncEnumerator"),
        Result("System.Collections.Generic.IEnumerator`1", "Current"),
        Result("System.Collections.Generic.IAsyncEnumerator`1", "Current"),

        // "foreach" over an array goes through the old non-generic interfaces.
        Result("System.Collections.IEnumerable", "GetEnumerator"),
        Result("System.Collections.IEnumerator", "Current"),

        // ----- Reading one entity back out of a collection -----
        Result("System.Collections.Generic.IList`1", "this[]"),
        Result("System.Collections.Generic.IReadOnlyList`1", "this[]"),
        Result("System.Collections.Generic.IDictionary`2", "this[]"),
        Result("System.Collections.Generic.IDictionary`2", "Values"),
        Result("System.Collections.Generic.IReadOnlyDictionary`2", "this[]"),
        Result("System.Collections.Generic.IReadOnlyDictionary`2", "Values"),
        Result("System.Collections.Generic.KeyValuePair`2", "Value"),
        Result("System.Collections.Generic.CollectionExtensions", "GetValueOrDefault"),
        Result("System.Threading.Tasks.Task`1", "Result"),

        // TryGetValue hands the entity back through its out argument; its result is a bool and holds nothing.
        Out("System.Collections.Generic.IDictionary`2", "TryGetValue", "value"),
        Out("System.Collections.Generic.IReadOnlyDictionary`2", "TryGetValue", "value"),

        // ----- EF Core: a callback is handed an element -----
        Lambda(EfQueryable, "AnyAsync", "predicate"),
        Lambda(EfQueryable, "AllAsync", "predicate"),
        Lambda(EfQueryable, "CountAsync", "predicate"),
        Lambda(EfQueryable, "LongCountAsync", "predicate"),
        Lambda(EfQueryable, "FirstAsync", "predicate"),
        Lambda(EfQueryable, "FirstOrDefaultAsync", "predicate"),
        Lambda(EfQueryable, "LastAsync", "predicate"),
        Lambda(EfQueryable, "LastOrDefaultAsync", "predicate"),
        Lambda(EfQueryable, "SingleAsync", "predicate"),
        Lambda(EfQueryable, "SingleOrDefaultAsync", "predicate"),
        Lambda(EfQueryable, "MinAsync", "selector"),
        Lambda(EfQueryable, "MaxAsync", "selector"),
        Lambda(EfQueryable, "SumAsync", "selector"),
        Lambda(EfQueryable, "AverageAsync", "selector"),
        Lambda(EfQueryable, "ToDictionaryAsync", "keySelector"),
        Lambda(EfQueryable, "ToDictionaryAsync", "elementSelector"),
        Lambda(EfQueryable, "ForEachAsync", "action"),

        // ----- Collections that call you back with an element -----
        Lambda(GenericList, "ForEach", "action"),
        Lambda(Array, "ForEach", "action", source: "array"),
    ];

    /// <summary>
    /// The operators Enumerable and Queryable declare the same way, read for one of them. Both types spell
    /// these members and their parameters identically, so a line that is right for one is right for the other.
    /// </summary>
    private static IEnumerable<(string DeclaringType, Bridge Bridge)> QueryOperators(string type) =>
    [
        .. SameEntitiesInResult.Select(member => Result(type, member)),

        // "Min(selector)" hands back what the selector returned, not an element. The name of that parameter is
        // the only thing separating it from "Min(comparer)", which takes just as many arguments.
        ResultExceptOverloadWith(type, "Min", "selector"),
        ResultExceptOverloadWith(type, "Max", "selector"),

        .. ElementInCallback.Select(entry => Lambda(type, entry.Member, entry.Callback)),

        // "GroupBy(keySelector, resultSelector)": the group is the second parameter, the key is not an element.
        Lambda(type, "GroupBy", "resultSelector", lambdaParameter: 1),

        // "Aggregate(seed, func)": the element is the second parameter, the accumulator is not.
        Lambda(type, "Aggregate", "func", lambdaParameter: 1),

        // Zip and the join operators are given a second collection of their own. Zip's is named differently on
        // each type, so those two lines are in All rather than here.
        Lambda(type, "Zip", "resultSelector"),
        Lambda(type, "Join", "outerKeySelector"),
        Lambda(type, "Join", "innerKeySelector", source: "inner"),
        Lambda(type, "Join", "resultSelector"),
        Lambda(type, "Join", "resultSelector", lambdaParameter: 1, source: "inner"),
        Lambda(type, "GroupJoin", "outerKeySelector"),
        Lambda(type, "GroupJoin", "innerKeySelector", source: "inner"),
        Lambda(type, "GroupJoin", "resultSelector"),
        Lambda(type, "GroupJoin", "resultSelector", lambdaParameter: 1, source: "inner"),
    ];

    /// <summary>
    /// From the value the member hands back, the bridge lands on the value it was used on.
    /// </summary>
    private static (string DeclaringType, Bridge Bridge) Result(string declaringType, string memberName)
        => (declaringType, new Bridge.FromResult(memberName, string.Empty));

    /// <summary>
    /// The same as <see cref="Result"/>, except in the overload that takes the named parameter, where the
    /// bridge does not exist because the result holds something else.
    /// </summary>
    private static (string DeclaringType, Bridge Bridge) ResultExceptOverloadWith(
        string declaringType,
        string memberName,
        string parameterName)
        => (declaringType, new Bridge.FromResult(memberName, string.Empty, parameterName));

    /// <summary>
    /// From a parameter of the callback the member is given, the bridge lands on the collection behind it.
    /// </summary>
    private static (string DeclaringType, Bridge Bridge) Lambda(
        string declaringType,
        string memberName,
        string callbackParameter,
        int lambdaParameter = 0,
        string source = "")
        => (declaringType, new Bridge.FromLambdaParameter(memberName, source, callbackParameter, lambdaParameter));

    /// <summary>
    /// From an out argument the member wrote, the bridge lands on the collection behind it.
    /// </summary>
    private static (string DeclaringType, Bridge Bridge) Out(
        string declaringType,
        string memberName,
        string outParameter)
        => (declaringType, new Bridge.FromOutArgument(memberName, string.Empty, outParameter));
}
