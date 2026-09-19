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
