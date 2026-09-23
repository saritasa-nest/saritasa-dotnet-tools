using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// [PassesIncludes]: declaring on a member or on the assembly which members hand back the entities they were
/// given, and INCL005 for a declaration that names nothing.
/// </summary>
public class PassesIncludesTests : NavigationIncludeTestBase
{
    /// <summary>
    /// No INCL002: a project's own IQueryable extension keeps the includes once it is declared with
    /// [PassesIncludes], exactly like the built-in declarations of System.Linq and EF Core.
    /// </summary>
    [Fact]
    public async Task PassesIncludes_WithoutParameterName_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                static class QueryableExtensions
                {
                    [PassesIncludes]
                    public static IQueryable<User> OnlyActive(this IQueryable<User> query) => query;
                }

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await dbContext.Users
                            .Include(u => u.Profile)
                            .OnlyActive()
                            .FirstOrDefaultAsync(u => u.Id == dto.Id);

                        UpdateUserProfile(user, dto);
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void UpdateUserProfile(User user, SaveUserDto dto)
                    {
                        user.Profile.Timezone = dto.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: a container that is not a collection cannot be read by the analyzer, but the method and the
    /// property say where their entities come from, so the includes are followed through both.
    /// </summary>
    [Fact]
    public async Task PassesIncludes_OnOwnMethod_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                public class PagedResult<T> : IEnumerable<T>
                {
                    public List<T> Items { get; set; }

                    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

                    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                        => GetEnumerator();
                }

                static class QueryableExtensions
                {
                    [PassesIncludes(nameof(query))]
                    public static PagedResult<User> Paginate(this IQueryable<User> query, int page) => null;
                }

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var page = dbContext.Users
                            .Include(u => u.Profile)
                            .Paginate(1);

                        var user = page.First();
                        UpdateUserProfile(user, dto);
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void UpdateUserProfile(User user, SaveUserDto dto)
                    {
                        user.Profile.Timezone = dto.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL006: an instance method cannot be declared with [PassesIncludes]. It could change what it was
    /// called on, or hand back something it built from a field, and the declaration would look the same, so
    /// the promise is one the analyzer has no way to hold it to.
    /// </summary>
    [Fact]
    public async Task PassesIncludes_OnInstanceMethod_ReportsIncl6()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                public class UserBatch
                {
                    private readonly List<User> items = new();

                    [{|INCL006:PassesIncludes|}]
                    public List<User> GetItems() => items;
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL006: a bridge may carry entities into another container, but not turn them into something else.
    /// Once entities change type along the way, nothing says any more which query a value came from.
    /// </summary>
    [Fact]
    public async Task PassesIncludes_BetweenDifferentEntities_ReportsIncl6()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                static class QueryableExtensions
                {
                    [{|INCL006:PassesIncludes(nameof(users))|}]
                    public static IEnumerable<Organization> GetOrganizations(IEnumerable<User> users) => null;
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL006: a container of one's own cannot be read, so it looks no different from an unrelated entity
    /// and a bridge into it is not allowed. Making it enumerable is what tells the two apart.
    /// </summary>
    [Fact]
    public async Task PassesIncludes_IntoUnreadableContainer_ReportsIncl6()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                public class UserBatch
                {
                    public List<User> Items { get; set; }
                }

                static class QueryableExtensions
                {
                    [{|INCL006:PassesIncludes(nameof(query))|}]
                    public static UserBatch ToBatch(this IQueryable<User> query) => null;
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL006: the same container, once it is enumerable, is one the analyzer can read, so the bridge
    /// into it is allowed.
    /// </summary>
    [Fact]
    public async Task PassesIncludes_IntoEnumerableContainer_NoIncl6()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                public class UserBatch : IEnumerable<User>
                {
                    public List<User> Items { get; set; }

                    public IEnumerator<User> GetEnumerator() => Items.GetEnumerator();

                    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                        => GetEnumerator();
                }

                static class QueryableExtensions
                {
                    [PassesIncludes(nameof(query))]
                    public static UserBatch ToBatch(this IQueryable<User> query) => null;
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002: the declarations only say where the entities come from, so a query without the include is
    /// still reported through them.
    /// </summary>
    [Fact]
    public async Task PassesIncludes_OverNonIncludedQuery_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                public class PagedResult<T> : IEnumerable<T>
                {
                    public List<T> Items { get; set; }

                    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

                    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                        => GetEnumerator();
                }

                static class QueryableExtensions
                {
                    [PassesIncludes(nameof(query))]
                    public static PagedResult<User> Paginate(this IQueryable<User> query, int page) => null;
                }

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var page = dbContext.Users.Paginate(1);

                        var user = page.First();
                        {|INCL002:UpdateUserProfile(user, dto)|};
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void UpdateUserProfile(User user, SaveUserDto dto)
                    {
                        user.Profile.Timezone = dto.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: the same thing declared on the assembly instead of the members, which is the only way to
    /// describe a library the project does not own.
    /// </summary>
    [Fact]
    public async Task PassesIncludes_DeclaredOnAssembly_NoIncl2()
    {
        var sourceCode = PreambleWithAssemblyAttributes(
            """
            [assembly: PassesIncludes(typeof(TestApplication.QueryableExtensions), "Paginate", "query")]
            """) +
            /* lang=c# */
            """

                public class PagedResult<T> : IEnumerable<T>
                {
                    public List<T> Items { get; set; }

                    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

                    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                        => GetEnumerator();
                }

                static class QueryableExtensions
                {
                    public static PagedResult<User> Paginate(this IQueryable<User> query, int page) => null;
                }

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var page = dbContext.Users
                            .Include(u => u.Profile)
                            .Paginate(1);

                        var user = page.First();
                        UpdateUserProfile(user, dto);
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void UpdateUserProfile(User user, SaveUserDto dto)
                    {
                        user.Profile.Timezone = dto.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002: a project's own extension that nobody declared is not followed. It is our own code, so the answer
    /// is a real one: nothing promises the property is loaded.
    /// </summary>
    [Fact]
    public async Task UndeclaredOwnExtension_NotFollowed_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                static class QueryableExtensions
                {
                    public static IQueryable<User> OnlyActive(this IQueryable<User> query) => query;
                }

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await dbContext.Users
                            .Include(u => u.Profile)
                            .OnlyActive()
                            .FirstOrDefaultAsync(u => u.Id == dto.Id);

                        {|INCL002:UpdateUserProfile(user, dto)|};
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void UpdateUserProfile(User user, SaveUserDto dto)
                    {
                        user.Profile.Timezone = dto.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL005: a declaration naming a method that does not exist, e.g. after the library renamed it, is reported
    /// instead of silently doing nothing.
    /// </summary>
    [Fact]
    public async Task Declaration_OfMissingMember_ReportsIncl5()
    {
        var sourceCode = PreambleWithAssemblyAttributes(
            """
            [assembly: {|INCL005:PassesIncludes(typeof(TestApplication.QueryableExtensions), "Paginated")|}]
            """) +
            /* lang=c# */
            """

                static class QueryableExtensions
                {
                    public static IQueryable<User> Paginate(this IQueryable<User> query, int page) => query;
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL006: the assembly form names a member by string, so it speaks for every overload at once. When not
    /// one of them is a method the rules allow, the declaration cannot do anything and is reported.
    /// </summary>
    [Fact]
    public async Task PassesIncludes_DeclaredOnAssemblyForInstanceMethod_ReportsIncl6()
    {
        var sourceCode = PreambleWithAssemblyAttributes(
            """
            [assembly: {|INCL006:PassesIncludes(typeof(TestApplication.UserBatch), "GetItems")|}]
            """) +
            /* lang=c# */
            """

                public class UserBatch
                {
                    private readonly List<User> items = new();

                    public List<User> GetItems() => items;
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL005: the member exists, but it has no parameter with the declared name.
    /// </summary>
    [Fact]
    public async Task Declaration_OfMissingParameter_ReportsIncl5()
    {
        var sourceCode = PreambleWithAssemblyAttributes(
            """
            [assembly: {|INCL005:PassesIncludes(typeof(TestApplication.QueryableExtensions), "Paginate", "source")|}]
            """) +
            /* lang=c# */
            """

                static class QueryableExtensions
                {
                    public static IQueryable<User> Paginate(this IQueryable<User> query, int page) => query;
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }
}
