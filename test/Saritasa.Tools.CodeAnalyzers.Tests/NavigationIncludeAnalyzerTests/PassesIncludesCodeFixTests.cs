using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.CodeFixes;
using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// Tests for <see cref="PassesIncludesCodeFixProvider"/>.
/// </summary>
/// <remarks>
/// The method that stops the search lives in a second project, "SomeLib", because a method of the project
/// being analyzed that promises nothing is INCL002, not INCL004. Whether SomeLib references the attributes
/// decides where the annotation goes: on the method itself when it can, into NavigationIncludes.cs otherwise.
/// Every fixed state carries no markup, so each test also checks that the annotation answers INCL004.
/// </remarks>
public class PassesIncludesCodeFixTests : NavigationIncludeTestBase
{
    private const string LibraryName = "SomeLib";

    private const string BridgesFileName = "NavigationIncludes.cs";

    private const string UsingAttributes =
        "using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;";

    private const string ChunkSource =
        /* lang=c# */
        """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Threading.Tasks;
        using Microsoft.EntityFrameworkCore;
        using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

        namespace TestApplication
        {
            public class AppDbContext : DbContext
            {
                public DbSet<User> Users { get; set; }
            }

            public class UserProfile
            {
                public string Timezone { get; set; }
            }

            public class User
            {
                public int Id { get; set; }
                [TrackIncludeRequired]
                public UserProfile Profile { get; set; }
            }

            class TestClass(AppDbContext dbContext)
            {
                void Handle()
                {
                    // Enumerable.Chunk is not in the built-in list, so the search cannot read past it.
                    var batches = dbContext.Users.Include(u => u.Profile).ToList().Chunk(10);
                    var user = batches.First()[0];
                    {|INCL004:UpdateUserProfile(user)|};
                }

                [IncludeRequired(nameof(user), nameof(User.Profile))]
                void UpdateUserProfile(User user)
                {
                    user.Profile.Timezone = "UTC";
                }
            }
        }
        """;

    private const string GetAsyncLibrary =
        /* lang=c# */
        """
        using System.Linq;
        using System.Linq.Expressions;
        using System.Threading;
        using System.Threading.Tasks;

        namespace SomeLib
        {
            public static class QueryExtensions
            {
                public static Task<TEntity> GetAsync<TEntity>(
                    this IQueryable<TEntity> entities,
                    Expression<System.Func<TEntity, bool>> predicate,
                    CancellationToken cancellationToken = default)
                    => Task.FromResult(default(TEntity));
            }
        }
        """;

    private const string GetAsyncCall =
        /* lang=c# */
        """
                var user = await dbContext.Users
                    .Include(u => u.Profile)
                    .GetAsync(u => u.Id == dto.Id);

                {|INCL004:UpdateUserProfile(user, dto)|};
        """;

    private const string OnlyActiveLibrary =
        /* lang=c# */
        """
        using System.Linq;

        namespace SomeLib
        {
            public static class QueryExtensions
            {
                /// <summary>
                /// Keeps the active ones.
                /// </summary>
                public static IQueryable<T> OnlyActive<T>(this IQueryable<T> query) => query;
            }
        }
        """;

    private const string OnlyActiveCall =
        /* lang=c# */
        """
                var user = dbContext.Users.Include(u => u.Profile).OnlyActive().First();
                {|INCL004:UpdateUserProfile(user, dto)|};
        """;

    private const string TakePageLibrary =
        /* lang=c# */
        """
        using System.Collections.Generic;
        using System.Linq;

        namespace SomeLib
        {
            public static class Paging
            {
                public static List<T> TakePage<T>(int pageNumber, IQueryable<T> source) => source.ToList();
            }
        }
        """;

    private const string TakePageCall =
        /* lang=c# */
        """
                var users = Paging.TakePage(pageNumber: 1, source: dbContext.Users.Include(u => u.Profile));
                {|INCL004:UpdateUserProfile(users[0], dto)|};
        """;

    private const string CollectionLibrary =
        /* lang=c# */
        """
        using System;
        using System.Collections.Generic;

        namespace SomeLib
        {
            public static class CollectionExtensions
            {
                public static void ForEachItem<T>(this IEnumerable<T> items, Action<T> action)
                {
                }

                public static void ForEachIndexed<T>(this IEnumerable<T> items, Action<int, T> action)
                {
                }

                public static void ForEachOfSecond<T>(IEnumerable<T> first, IEnumerable<T> second, Action<T> action)
                {
                }
            }
        }
        """;

    private const string ForEachItemCall =
        /* lang=c# */
        """
                dbContext.Users.Include(u => u.Profile).ToList()
                    .ForEachItem(u => {|INCL004:UpdateUserProfile(u, dto)|});
        """;

    /// <summary>
    /// Case 1. A library extension method that hands an entity back through a Task, the shape a repository
    /// helper takes. Only "entities" holds a User: the predicate is an expression and the token is not an entity.
    /// SomeLib does not reference the attributes, so the annotation goes into a new NavigationIncludes.cs.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverLibraryTaskExtension_AddsAnnotationFile()
    {
        var test = CreateTest(GetAsyncCall);
        AddLibrary(test, GetAsyncLibrary);
        AddBridgesFile(
            test,
            """[assembly: PassesIncludes(typeof(SomeLib.QueryExtensions), "GetAsync", "entities")]""");

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// A method of the framework has no source in the solution, so the annotation goes into a new
    /// NavigationIncludes.cs, the way Visual Studio keeps suppressions in a file of their own.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverFrameworkMethod_AddsAnnotationFile()
    {
        var test = new CSharpCodeFixTest<NavigationIncludeAnalyzer, PassesIncludesCodeFixProvider, DefaultVerifier>
        {
            ReferenceAssemblies = References,
            TestCode = ChunkSource,
            FixedCode = WithoutMarkup(ChunkSource),
        };

        test.TestState.AdditionalReferences.AddRange(AnalyzerReferences);
        test.FixedState.AdditionalReferences.AddRange(AnalyzerReferences);
        AddBridgesFile(
            test,
            """[assembly: PassesIncludes(typeof(System.Linq.Enumerable), "Chunk", "source")]""");

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Case 2. The method's source is in the solution and its project can see the attribute, so the
    /// annotation goes on the method itself, after its documentation, with the using it needs.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverOwnExtension_AnnotatesMethod()
    {
        const string fixedLibrary =
            /* lang=c# */
            """
            using System.Linq;
            using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

            namespace SomeLib
            {
                public static class QueryExtensions
                {
                    /// <summary>
                    /// Keeps the active ones.
                    /// </summary>
                    [PassesIncludes(nameof(query))]
                    public static IQueryable<T> OnlyActive<T>(this IQueryable<T> query) => query;
                }
            }
            """;

        var test = CreateTest(OnlyActiveCall);
        AddLibrary(test, OnlyActiveLibrary, fixedLibrary, seesAttributes: true);

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Case 2, where the file already has the using and the method another attribute: no second using, and the
    /// annotation goes in front of the attribute at the same indentation.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverOwnAttributedExtension_KeepsUsingAndAttributes()
    {
        const string library =
            /* lang=c# */
            """
            using System.Diagnostics;
            using System.Linq;
            using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

            namespace SomeLib
            {
                public static class QueryExtensions
                {
                    [DebuggerStepThrough]
                    public static IQueryable<T> OnlyActive<T>(this IQueryable<T> query) => query;
                }
            }
            """;

        var test = CreateTest(OnlyActiveCall);
        AddLibrary(
            test,
            library,
            library.Replace(
                "        [DebuggerStepThrough]",
                "        [PassesIncludes(nameof(query))]\n        [DebuggerStepThrough]"),
            seesAttributes: true);

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Case 3. A static method that is not an extension: the entities come from a named parameter, never from
    /// an instance it does not have. "pageNumber" holds no User, so it is not offered.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverStaticMethod_NamesTheParameterHoldingTheEntity()
    {
        var test = CreateTest(TakePageCall);
        AddLibrary(test, TakePageLibrary);
        AddBridgesFile(
            test,
            """[assembly: PassesIncludes(typeof(SomeLib.Paging), "TakePage", "source")]""");

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Case 4. The search stops at a lambda parameter of a library method; the callback and its position go into
    /// the annotation.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverLibraryCallback_AddsCallbackAnnotation()
    {
        var test = CreateTest(ForEachItemCall);
        AddLibrary(test, CollectionLibrary);
        AddBridgesFile(
            test,
            """[assembly: PassesIncludes(typeof(SomeLib.CollectionExtensions), "ForEachItem", "items", ToCallback = "action")]""");

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Case 4, own method: the callback annotation goes on the method itself.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverOwnCallback_AnnotatesMethod()
    {
        var test = CreateTest(ForEachItemCall);
        AddLibrary(
            test,
            CollectionLibrary,
            CollectionLibrary
                .Replace(
                    "using System.Collections.Generic;",
                    "using System.Collections.Generic;\n" + UsingAttributes)
                .Replace(
                    "        public static void ForEachItem<T>",
                    "        [PassesIncludes(nameof(items), ToCallback = nameof(action))]\n" +
                    "        public static void ForEachItem<T>"),
            seesAttributes: true);

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// The entity arrives at the second parameter of the callback, so the position is written down.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverCallbackSecondParameter_WritesPosition()
    {
        var test = CreateTest(
            """
                    dbContext.Users.Include(u => u.Profile).ToList()
                        .ForEachIndexed((i, u) => {|INCL004:UpdateUserProfile(u, dto)|});
            """);
        AddLibrary(test, CollectionLibrary);
        AddBridgesFile(
            test,
            """[assembly: PassesIncludes(typeof(SomeLib.CollectionExtensions), "ForEachIndexed", "items", ToCallback = "action", ToCallbackParameter = 1)]""");

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Case 4 with two collections: both hold a User, so the fix is a submenu and the user picks one.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverMethodWithTwoSources_OffersEachParameter()
    {
        var test = CreateTest(
            """
                    var users = dbContext.Users.Include(u => u.Profile).ToList();
                    SomeLib.CollectionExtensions.ForEachOfSecond(
                        new List<User>(),
                        users,
                        u => {|INCL004:UpdateUserProfile(u, dto)|});
            """);
        AddLibrary(test, CollectionLibrary);
        AddBridgesFile(
            test,
            """[assembly: PassesIncludes(typeof(SomeLib.CollectionExtensions), "ForEachOfSecond", "second", ToCallback = "action")]""");
        test.CodeActionEquivalenceKey = "PassesIncludes:M:SomeLib.CollectionExtensions.ForEachOfSecond``1(" +
                                        "System.Collections.Generic.IEnumerable{``0}," +
                                        "System.Collections.Generic.IEnumerable{``0}," +
                                        "System.Action{``0}):second:action";

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Case 5. NavigationIncludes.cs already exists, so the line is appended to it.
    /// </summary>
    [Fact]
    public async Task CannotCheck_WithExistingAnnotationFile_AppendsLine()
    {
        const string existingLine =
            """[assembly: PassesIncludes(typeof(System.Linq.Enumerable), "Chunk", "source")]""";

        var test = CreateTest(TakePageCall);
        AddLibrary(test, TakePageLibrary);
        test.TestState.Sources.Add((BridgesFileName, UsingAttributes + "\n\n" + existingLine));
        AddBridgesFile(
            test,
            existingLine + "\n" + """[assembly: PassesIncludes(typeof(SomeLib.Paging), "TakePage", "source")]""");

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// An instance method can change what it was called on, so INCL006 would reject the annotation and none
    /// is offered.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverInstanceMethod_OffersNothing()
    {
        await VerifyNoFixAsync(
            """
            using System.Linq;

            namespace SomeLib
            {
                public class Filter
                {
                    public IQueryable<T> Apply<T>(IQueryable<T> query) => query;
                }
            }
            """,
            """
                    var user = new SomeLib.Filter().Apply(dbContext.Users.Include(u => u.Profile)).First();
                    {|INCL004:UpdateUserProfile(user, dto)|};
            """);
    }

    /// <summary>
    /// No parameter holds a User, so there is nothing valid to name: a mapping makes new objects, not a bridge.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverEntityTypeChange_OffersNothing()
    {
        await VerifyNoFixAsync(
            """
            using System.Linq;

            namespace SomeLib
            {
                public static class Mapping
                {
                    public static T Make<T>(object source) where T : new() => new T();
                }
            }
            """,
            """
                    var user = SomeLib.Mapping.Make<User>(dbContext.Users.Include(u => u.Profile));
                    {|INCL004:UpdateUserProfile(user, dto)|};
            """);
    }

    /// <summary>
    /// A container of its own that is not enumerable cannot be told apart from an unrelated type, so the rules
    /// would reject the annotation and none is offered. Implementing IEnumerable&lt;T&gt; is the fix.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverOwnContainer_OffersNothing()
    {
        await VerifyNoFixAsync(
            """
            using System.Linq;

            namespace SomeLib
            {
                public class Page<T>
                {
                    public T First { get; set; }
                }

                public static class Paging
                {
                    public static Page<T> Of<T>(IQueryable<T> query) => new Page<T> { First = query.First() };
                }
            }
            """,
            """
                    var page = SomeLib.Paging.Of(dbContext.Users.Include(u => u.Profile));
                    {|INCL004:UpdateUserProfile(page.First, dto)|};
            """);
    }

    /// <summary>
    /// A fix test over the shared model. When a fix is expected, the INCL004 markup is gone from the fixed state.
    /// </summary>
    private static CSharpCodeFixTest<NavigationIncludeAnalyzer, PassesIncludesCodeFixProvider, DefaultVerifier>
        CreateTest(string handleBody, bool expectFix = true)
    {
        var source = HandleSource(handleBody).Replace("using System;", "using System;\nusing SomeLib;");

        var test = new CSharpCodeFixTest<NavigationIncludeAnalyzer, PassesIncludesCodeFixProvider, DefaultVerifier>
        {
            ReferenceAssemblies = References,
            TestCode = source,
            FixedCode = expectFix ? WithoutMarkup(source) : source,
        };

        // By default the fixed state ignores the diagnostics the fix is for, which would hide one left behind.
        test.FixedState.MarkupHandling = MarkupMode.Allow;
        test.TestState.AdditionalReferences.AddRange(AnalyzerReferences);
        test.FixedState.AdditionalReferences.AddRange(AnalyzerReferences);

        return test;
    }

    /// <summary>
    /// Adds SomeLib, referenced by the project under test.
    /// </summary>
    /// <param name="test">Test.</param>
    /// <param name="source">SomeLib before the fix.</param>
    /// <param name="fixedSource">SomeLib after the fix, when the fix changes it.</param>
    /// <param name="seesAttributes">Whether SomeLib references the attributes.</param>
    private static void AddLibrary(
        CodeFixTest<DefaultVerifier> test,
        string source,
        string fixedSource = null,
        bool seesAttributes = false)
    {
        var states = new[] { (test.TestState, source), (test.FixedState, fixedSource ?? source) };

        foreach (var (state, librarySource) in states)
        {
            var library = new ProjectState(LibraryName, LanguageNames.CSharp, "/" + LibraryName + "/", "cs");
            library.Sources.Add(("Library.cs", librarySource));

            if (seesAttributes)
            {
                library.AdditionalReferences.AddRange(AnalyzerReferences);
            }

            state.AdditionalProjects.Add(LibraryName, library);
            state.AdditionalProjectReferences.Add(LibraryName);
        }
    }

    /// <summary>
    /// Expects NavigationIncludes.cs with these lines after the fix.
    /// </summary>
    private static void AddBridgesFile(CodeFixTest<DefaultVerifier> test, string lines)
        => test.FixedState.Sources.Add((BridgesFileName, UsingAttributes + "\n\n" + lines));

    /// <summary>
    /// INCL004 is reported and no fix changes anything.
    /// </summary>
    private static async Task VerifyNoFixAsync(string library, string handleBody)
    {
        var test = CreateTest(handleBody, expectFix: false);
        AddLibrary(test, library);

        await test.RunAsync(CancellationToken.None);
    }

    private static string WithoutMarkup(string source)
        => source.Replace("{|INCL004:", string.Empty).Replace("|}", string.Empty);
}
