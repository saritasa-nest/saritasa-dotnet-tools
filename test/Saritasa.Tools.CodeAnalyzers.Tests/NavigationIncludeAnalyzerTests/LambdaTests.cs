using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// Lambda parameters that receive elements of a collection, such as u in users.Select(u => ...).
/// </summary>
public class LambdaTests : NavigationIncludeTestBase
{
    /// <summary>
    /// No INCL002: a LINQ .Select() lambda parameter inherits the tracked navigation
    /// property of the source collection it iterates.
    /// </summary>
    [Fact]
    public async Task LinqSelect_OverIncludedCollection_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var users = await dbContext.Users
                            .Include(u => u.Profile)
                            .ToListAsync();

                        var timezones = users.Select(u => GetTimezone(u, dto)).ToList();
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    string GetTimezone(User user, SaveUserDto dto)
                    {
                        return user.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002: a LINQ .Select() lambda parameter over a collection that was not loaded with
    /// .Include() still triggers the diagnostic.
    /// </summary>
    [Fact]
    public async Task LinqSelect_OverNonIncludedCollection_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var users = await dbContext.Users.ToListAsync();

                        // INCL002: users were loaded without .Include(u => u.Profile).
                        var timezones = users.Select(u => {|INCL002:GetTimezone(u, dto)|}).ToList();
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    string GetTimezone(User user, SaveUserDto dto)
                    {
                        return user.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: a project's own extension that hands each element to a callback passes on the includes
    /// of the collection, the same way Select and Where do.
    /// </summary>
    [Fact]
    public async Task CustomCallbackExtension_OverIncludedCollection_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                static class EnumerableExtensions
                {
                    [PassesIncludes(ToCallback = nameof(action))]
                    public static void ForEachItem<T>(this IEnumerable<T> items, Action<T> action)
                    {
                        foreach (var item in items)
                        {
                            action(item);
                        }
                    }
                }

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var users = await dbContext.Users
                            .Include(u => u.Profile)
                            .ToListAsync();

                        users.ForEachItem(u =>
                        {
                            var current = u;
                            UpdateUserProfile(current, dto);
                        });
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
    /// No INCL002: the lambda runs over a whole chain of calls, and every one of them is declared to hand the
    /// entities on, so the search reads the chain back to the Include.
    /// </summary>
    [Fact]
    public async Task LinqChain_BeforeLambda_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var timezones = dbContext.Users
                            .Include(u => u.Profile)
                            .Where(u => u.Id > 0)
                            .OrderBy(u => u.Id)
                            .ToList()
                            .Select(u => GetTimezone(u, dto))
                            .ToList();
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    string GetTimezone(User user, SaveUserDto dto)
                    {
                        return user.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002: one call in the chain before the lambda promises nothing, so the Include before it is not
    /// enough. The method is in the project's own code, where a missing promise is a real answer.
    /// </summary>
    [Fact]
    public async Task UndeclaredCallInChain_BeforeLambda_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                static class UserQueries
                {
                    public static IEnumerable<User> Newest(this IEnumerable<User> users) => users;
                }

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var timezones = dbContext.Users
                            .Include(u => u.Profile)
                            .ToList()
                            .Newest()
                            .Select(u => {|INCL002:GetTimezone(u, dto)|})
                            .ToList();
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    string GetTimezone(User user, SaveUserDto dto)
                    {
                        return user.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002 inside a lambda that becomes an expression tree. The parameter of an IQueryable operator is
    /// declared Expression&lt;Func&lt;TSource, TResult&gt;&gt;, and the search reads through the expression to
    /// the same TSource, so u is filled from the query.
    /// </summary>
    [Fact]
    public async Task QueryableLambda_OverIncludedQuery_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var timezones = await dbContext.Users
                            .Include(u => u.Profile)
                            .Select(u => GetTimezone(u, dto))
                            .ToListAsync();
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    string GetTimezone(User user, SaveUserDto dto)
                    {
                        return user.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002: the same expression tree over a query without the Include.
    /// </summary>
    [Fact]
    public async Task QueryableLambda_OverNonIncludedQuery_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var timezones = await dbContext.Users
                            .Select(u => {|INCL002:GetTimezone(u, dto)|})
                            .ToListAsync();
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    string GetTimezone(User user, SaveUserDto dto)
                    {
                        return user.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: the second parameter of a GroupBy result selector is the group, which is declared
    /// IEnumerable&lt;TSource&gt; and therefore filled from the source collection.
    /// </summary>
    [Fact]
    public async Task GroupByResultSelector_GroupIsFilledFromSource_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var users = await dbContext.Users
                            .Include(u => u.Profile)
                            .ToListAsync();

                        var timezones = users
                            .GroupBy(u => u.Manager, (manager, group) =>
                            {
                                var first = group.First();
                                return GetTimezone(first, dto);
                            })
                            .ToList();
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    string GetTimezone(User user, SaveUserDto dto)
                    {
                        return user.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL004: the first parameter of a GroupBy result selector is the key, not an element of the source.
    /// Including Profile on the source says nothing about the key, even when the key is a User as well. The
    /// search cannot tell what the key holds, so it reports that rather than claiming the property is missing.
    /// </summary>
    [Fact]
    public async Task GroupByResultSelector_KeyIsNotAnElement_ReportsIncl4()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var users = await dbContext.Users
                            .Include(u => u.Profile)
                            .ToListAsync();

                        var timezones = users
                            .GroupBy(u => u.Manager, (manager, group) =>
                            {
                                var boss = manager;
                                return {|INCL004:GetTimezone(boss, dto)|};
                            })
                            .ToList();
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    string GetTimezone(User user, SaveUserDto dto)
                    {
                        return user.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }
}
