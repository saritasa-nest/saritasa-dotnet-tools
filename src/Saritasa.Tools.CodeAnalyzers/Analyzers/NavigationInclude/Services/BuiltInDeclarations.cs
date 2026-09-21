namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

/// <summary>
/// The declarations that ship with the analyzer, written out one per line.
/// </summary>
/// <remarks>
/// Each line means exactly what an <c>[assembly: PassesIncludes(...)]</c> means and goes through the same
/// lookup, so nothing about Microsoft's methods is special. Types are named by metadata name, so the analyzer
/// needs no reference to EF Core, and a line whose type the project does not use simply matches nothing.
/// Nothing here is a pattern or a cross product: if a member is not on this list it is not crossed, and to see
/// what the analyzer knows about a method you search for its name.
/// </remarks>
internal static class BuiltInDeclarations
{
    private const string LinqEnumerable = "System.Linq.Enumerable";
    private const string LinqQueryable = "System.Linq.Queryable";
    private const string EfQueryable = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions";
    private const string EfDbSet = "Microsoft.EntityFrameworkCore.DbSet`1";
    private const string GenericList = "System.Collections.Generic.List`1";
    private const string Array = "System.Array";

    /// <summary>
    /// Every built-in line.
    /// </summary>
    public static IEnumerable<BuiltInDeclaration> All =>
    [
        // ----- System.Linq.Enumerable: the result holds the entities it was given -----
        Result(LinqEnumerable, "Where"),
        Result(LinqEnumerable, "OrderBy"),
        Result(LinqEnumerable, "OrderByDescending"),
        Result(LinqEnumerable, "ThenBy"),
        Result(LinqEnumerable, "ThenByDescending"),
        Result(LinqEnumerable, "Order"),
        Result(LinqEnumerable, "OrderDescending"),
        Result(LinqEnumerable, "Skip"),
        Result(LinqEnumerable, "SkipLast"),
        Result(LinqEnumerable, "SkipWhile"),
        Result(LinqEnumerable, "Take"),
        Result(LinqEnumerable, "TakeLast"),
        Result(LinqEnumerable, "TakeWhile"),
        Result(LinqEnumerable, "Distinct"),
        Result(LinqEnumerable, "DistinctBy"),
        Result(LinqEnumerable, "Reverse"),
        Result(LinqEnumerable, "Concat"),
        Result(LinqEnumerable, "Union"),
        Result(LinqEnumerable, "UnionBy"),
        Result(LinqEnumerable, "Intersect"),
        Result(LinqEnumerable, "IntersectBy"),
        Result(LinqEnumerable, "Except"),
        Result(LinqEnumerable, "ExceptBy"),
        Result(LinqEnumerable, "Append"),
        Result(LinqEnumerable, "Prepend"),
        Result(LinqEnumerable, "DefaultIfEmpty"),
        Result(LinqEnumerable, "First"),
        Result(LinqEnumerable, "FirstOrDefault"),
        Result(LinqEnumerable, "Single"),
        Result(LinqEnumerable, "SingleOrDefault"),
        Result(LinqEnumerable, "Last"),
        Result(LinqEnumerable, "LastOrDefault"),
        Result(LinqEnumerable, "ElementAt"),
        Result(LinqEnumerable, "ElementAtOrDefault"),
        Result(LinqEnumerable, "MinBy"),
        Result(LinqEnumerable, "MaxBy"),
        Result(LinqEnumerable, "AsEnumerable"),
        Result(LinqEnumerable, "ToList"),
        Result(LinqEnumerable, "ToArray"),
        Result(LinqEnumerable, "ToHashSet"),

        // "Min(selector)" hands back what the selector returned, not an element. The name of that parameter is
        // the only thing separating it from "Min(comparer)", which takes just as many arguments.
        ResultUnless(LinqEnumerable, "Min", "selector"),
        ResultUnless(LinqEnumerable, "Max", "selector"),
        ResultUnless(LinqEnumerable, "ToDictionary", "elementSelector"),

        // ----- System.Linq.Queryable: the result holds the entities it was given -----
        Result(LinqQueryable, "Where"),
        Result(LinqQueryable, "OrderBy"),
        Result(LinqQueryable, "OrderByDescending"),
        Result(LinqQueryable, "ThenBy"),
        Result(LinqQueryable, "ThenByDescending"),
        Result(LinqQueryable, "Order"),
        Result(LinqQueryable, "OrderDescending"),
        Result(LinqQueryable, "Skip"),
        Result(LinqQueryable, "SkipLast"),
        Result(LinqQueryable, "SkipWhile"),
        Result(LinqQueryable, "Take"),
        Result(LinqQueryable, "TakeLast"),
        Result(LinqQueryable, "TakeWhile"),
        Result(LinqQueryable, "Distinct"),
        Result(LinqQueryable, "DistinctBy"),
        Result(LinqQueryable, "Reverse"),
        Result(LinqQueryable, "Concat"),
        Result(LinqQueryable, "Union"),
        Result(LinqQueryable, "UnionBy"),
        Result(LinqQueryable, "Intersect"),
        Result(LinqQueryable, "IntersectBy"),
        Result(LinqQueryable, "Except"),
        Result(LinqQueryable, "ExceptBy"),
        Result(LinqQueryable, "Append"),
        Result(LinqQueryable, "Prepend"),
        Result(LinqQueryable, "DefaultIfEmpty"),
        Result(LinqQueryable, "First"),
        Result(LinqQueryable, "FirstOrDefault"),
        Result(LinqQueryable, "Single"),
        Result(LinqQueryable, "SingleOrDefault"),
        Result(LinqQueryable, "Last"),
        Result(LinqQueryable, "LastOrDefault"),
        Result(LinqQueryable, "ElementAt"),
        Result(LinqQueryable, "ElementAtOrDefault"),
        Result(LinqQueryable, "MinBy"),
        Result(LinqQueryable, "MaxBy"),
        Result(LinqQueryable, "AsQueryable"),
        ResultUnless(LinqQueryable, "Min", "selector"),
        ResultUnless(LinqQueryable, "Max", "selector"),

        // ----- EF Core query operators: the result holds the entities it was given -----
        Result(EfQueryable, "Include"),
        Result(EfQueryable, "ThenInclude"),
        Result(EfQueryable, "AsNoTracking"),
        Result(EfQueryable, "AsNoTrackingWithIdentityResolution"),
        Result(EfQueryable, "AsTracking"),
        Result(EfQueryable, "AsSplitQuery"),
        Result(EfQueryable, "AsSingleQuery"),
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
        ResultUnless(EfQueryable, "MinAsync", "selector"),
        ResultUnless(EfQueryable, "MaxAsync", "selector"),
        ResultUnless(EfQueryable, "ToDictionaryAsync", "elementSelector"),

        Result(EfDbSet, "AsAsyncEnumerable"),
        Result(EfDbSet, "AsQueryable"),

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
        Result("System.Collections.Generic.IDictionary`2", "TryGetValue"),
        Result("System.Collections.Generic.IReadOnlyDictionary`2", "this[]"),
        Result("System.Collections.Generic.IReadOnlyDictionary`2", "Values"),
        Result("System.Collections.Generic.IReadOnlyDictionary`2", "TryGetValue"),
        Result("System.Collections.Generic.KeyValuePair`2", "Value"),
        Result("System.Collections.Generic.CollectionExtensions", "GetValueOrDefault"),
        Result("System.Threading.Tasks.Task`1", "Result"),

        // ----- System.Linq.Enumerable: a callback is handed an element -----
        Lambda(LinqEnumerable, "Where", "predicate"),
        Lambda(LinqEnumerable, "Any", "predicate"),
        Lambda(LinqEnumerable, "All", "predicate"),
        Lambda(LinqEnumerable, "Count", "predicate"),
        Lambda(LinqEnumerable, "LongCount", "predicate"),
        Lambda(LinqEnumerable, "First", "predicate"),
        Lambda(LinqEnumerable, "FirstOrDefault", "predicate"),
        Lambda(LinqEnumerable, "Last", "predicate"),
        Lambda(LinqEnumerable, "LastOrDefault", "predicate"),
        Lambda(LinqEnumerable, "Single", "predicate"),
        Lambda(LinqEnumerable, "SingleOrDefault", "predicate"),
        Lambda(LinqEnumerable, "TakeWhile", "predicate"),
        Lambda(LinqEnumerable, "SkipWhile", "predicate"),
        Lambda(LinqEnumerable, "Select", "selector"),
        Lambda(LinqEnumerable, "SelectMany", "selector"),
        Lambda(LinqEnumerable, "Sum", "selector"),
        Lambda(LinqEnumerable, "Min", "selector"),
        Lambda(LinqEnumerable, "Max", "selector"),
        Lambda(LinqEnumerable, "Average", "selector"),
        Lambda(LinqEnumerable, "OrderBy", "keySelector"),
        Lambda(LinqEnumerable, "OrderByDescending", "keySelector"),
        Lambda(LinqEnumerable, "ThenBy", "keySelector"),
        Lambda(LinqEnumerable, "ThenByDescending", "keySelector"),
        Lambda(LinqEnumerable, "GroupBy", "keySelector"),
        Lambda(LinqEnumerable, "ToDictionary", "keySelector"),
        Lambda(LinqEnumerable, "ToLookup", "keySelector"),
        Lambda(LinqEnumerable, "MinBy", "keySelector"),
        Lambda(LinqEnumerable, "MaxBy", "keySelector"),
        Lambda(LinqEnumerable, "DistinctBy", "keySelector"),
        Lambda(LinqEnumerable, "UnionBy", "keySelector"),
        Lambda(LinqEnumerable, "IntersectBy", "keySelector"),
        Lambda(LinqEnumerable, "ExceptBy", "keySelector"),
        Lambda(LinqEnumerable, "GroupBy", "elementSelector"),
        Lambda(LinqEnumerable, "ToDictionary", "elementSelector"),
        Lambda(LinqEnumerable, "ToLookup", "elementSelector"),
        Lambda(LinqEnumerable, "SelectMany", "collectionSelector"),

        // "SelectMany(collectionSelector, resultSelector)": the element is the first parameter. The second
        // comes from what collectionSelector returned, which is not followed.
        Lambda(LinqEnumerable, "SelectMany", "resultSelector"),

        // "GroupBy(keySelector, resultSelector)": the group is the second parameter, the key is not an element.
        Lambda(LinqEnumerable, "GroupBy", "resultSelector", ordinal: 1),

        // "Aggregate(seed, func)": the element is the second parameter, the accumulator is not.
        Lambda(LinqEnumerable, "Aggregate", "func", ordinal: 1),

        // Zip and the join operators are given a second collection of their own.
        Lambda(LinqEnumerable, "Zip", "resultSelector"),
        Lambda(LinqEnumerable, "Zip", "resultSelector", ordinal: 1, from: "second"),
        Lambda(LinqEnumerable, "Join", "outerKeySelector"),
        Lambda(LinqEnumerable, "Join", "innerKeySelector", from: "inner"),
        Lambda(LinqEnumerable, "Join", "resultSelector"),
        Lambda(LinqEnumerable, "Join", "resultSelector", ordinal: 1, from: "inner"),
        Lambda(LinqEnumerable, "GroupJoin", "outerKeySelector"),
        Lambda(LinqEnumerable, "GroupJoin", "innerKeySelector", from: "inner"),
        Lambda(LinqEnumerable, "GroupJoin", "resultSelector"),
        Lambda(LinqEnumerable, "GroupJoin", "resultSelector", ordinal: 1, from: "inner"),

        // ----- System.Linq.Queryable: a callback is handed an element -----
        Lambda(LinqQueryable, "Where", "predicate"),
        Lambda(LinqQueryable, "Any", "predicate"),
        Lambda(LinqQueryable, "All", "predicate"),
        Lambda(LinqQueryable, "Count", "predicate"),
        Lambda(LinqQueryable, "LongCount", "predicate"),
        Lambda(LinqQueryable, "First", "predicate"),
        Lambda(LinqQueryable, "FirstOrDefault", "predicate"),
        Lambda(LinqQueryable, "Last", "predicate"),
        Lambda(LinqQueryable, "LastOrDefault", "predicate"),
        Lambda(LinqQueryable, "Single", "predicate"),
        Lambda(LinqQueryable, "SingleOrDefault", "predicate"),
        Lambda(LinqQueryable, "TakeWhile", "predicate"),
        Lambda(LinqQueryable, "SkipWhile", "predicate"),
        Lambda(LinqQueryable, "Select", "selector"),
        Lambda(LinqQueryable, "SelectMany", "selector"),
        Lambda(LinqQueryable, "Sum", "selector"),
        Lambda(LinqQueryable, "Min", "selector"),
        Lambda(LinqQueryable, "Max", "selector"),
        Lambda(LinqQueryable, "Average", "selector"),
        Lambda(LinqQueryable, "OrderBy", "keySelector"),
        Lambda(LinqQueryable, "OrderByDescending", "keySelector"),
        Lambda(LinqQueryable, "ThenBy", "keySelector"),
        Lambda(LinqQueryable, "ThenByDescending", "keySelector"),
        Lambda(LinqQueryable, "GroupBy", "keySelector"),
        Lambda(LinqQueryable, "ToDictionary", "keySelector"),
        Lambda(LinqQueryable, "ToLookup", "keySelector"),
        Lambda(LinqQueryable, "MinBy", "keySelector"),
        Lambda(LinqQueryable, "MaxBy", "keySelector"),
        Lambda(LinqQueryable, "DistinctBy", "keySelector"),
        Lambda(LinqQueryable, "UnionBy", "keySelector"),
        Lambda(LinqQueryable, "IntersectBy", "keySelector"),
        Lambda(LinqQueryable, "ExceptBy", "keySelector"),
        Lambda(LinqQueryable, "GroupBy", "elementSelector"),
        Lambda(LinqQueryable, "SelectMany", "collectionSelector"),
        Lambda(LinqQueryable, "SelectMany", "resultSelector"),
        Lambda(LinqQueryable, "GroupBy", "resultSelector", ordinal: 1),
        Lambda(LinqQueryable, "Aggregate", "func", ordinal: 1),
        Lambda(LinqQueryable, "Zip", "resultSelector"),
        Lambda(LinqQueryable, "Zip", "resultSelector", ordinal: 1, from: "second"),
        Lambda(LinqQueryable, "Join", "outerKeySelector"),
        Lambda(LinqQueryable, "Join", "innerKeySelector", from: "inner"),
        Lambda(LinqQueryable, "Join", "resultSelector"),
        Lambda(LinqQueryable, "Join", "resultSelector", ordinal: 1, from: "inner"),
        Lambda(LinqQueryable, "GroupJoin", "outerKeySelector"),
        Lambda(LinqQueryable, "GroupJoin", "innerKeySelector", from: "inner"),
        Lambda(LinqQueryable, "GroupJoin", "resultSelector"),
        Lambda(LinqQueryable, "GroupJoin", "resultSelector", ordinal: 1, from: "inner"),

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
        Lambda(Array, "ForEach", "action", from: "array"),
    ];

    /// <summary>
    /// A method, or one of the analyzer's own property bridges, whose result holds the entities it was given.
    /// </summary>
    private static BuiltInDeclaration Result(string type, string name)
        => new(type, name, string.Empty, null, 0, null);

    /// <summary>
    /// The result holds the entities, except in the overload that takes the named parameter.
    /// </summary>
    /// <remarks>
    /// TODO SN-993: this is the weakest line in the table and should be replaced.
    /// A declaration names a method, and a method can be several overloads that disagree. Counting parameters
    /// cannot separate them — "Min(source, comparer)" hands back an element and "Min(source, selector)" does
    /// not, and both take two — so the overload is excluded by the name of the parameter that makes the
    /// difference. That leans on three things we do not control:
    /// the parameter names of the BCL stay as they are; no new overload arrives that hands back something else
    /// under a different parameter name; and the name written here is spelled the way the framework spells it.
    /// Every one of those fails silently and in the dangerous direction — the exclusion stops matching, the
    /// broad declaration applies again, and the search reports Loaded for entities that were never there.
    /// Nothing checks it today: INCL005 only validates declarations somebody wrote, not these.
    /// The cheap next step is a test over this table that resolves each line against a real compilation and
    /// asserts the NotWhen parameter exists on some overload of that method, which turns a rename into a build
    /// failure instead of a wrong answer. The real fix is to name the overload by its signature rather than
    /// punching a hole in a method name.
    /// </remarks>
    private static BuiltInDeclaration ResultUnless(string type, string name, string notWhen)
        => new(type, name, string.Empty, null, 0, notWhen);

    /// <summary>
    /// A parameter of the named callback is handed an element.
    /// </summary>
    private static BuiltInDeclaration Lambda(
        string type,
        string name,
        string toLambda,
        int ordinal = 0,
        string from = "")
        => new(type, name, from, toLambda, ordinal, null);
}
