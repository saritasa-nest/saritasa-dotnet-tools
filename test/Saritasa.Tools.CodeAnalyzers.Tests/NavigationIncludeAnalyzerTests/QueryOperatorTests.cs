using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// The built-in annotations of System.Linq and EF Core: which operators keep the entities, and which
/// operators or overloads make new objects instead.
/// </summary>
public class QueryOperatorTests : NavigationIncludeTestBase
{
    /// <summary>
    /// No INCL002: a long chain of EF and LINQ query operators keeps the entities, with nothing annotated
    /// by name anywhere in the analyzer.
    /// </summary>
    [Fact]
    public async Task QueryOperatorChain_KeepsIncludes_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users
                            .Include(u => u.Profile)
                            .AsNoTracking()
                            .Where(u => u.Id == dto.Id)
                            .OrderBy(u => u.Id)
                            .ThenBy(u => u.Id)
                            .Skip(0)
                            .Take(10)
                            .Distinct()
                            .FirstOrDefaultAsync();

                        UpdateUserProfile(user, dto);
            """));
    }

    /// <summary>
    /// No INCL002: the entities survive being materialized into every common collection shape.
    /// </summary>
    [Fact]
    public async Task MaterializingOperators_KeepIncludes_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var list = await dbContext.Users.Include(u => u.Profile).ToListAsync();
                        var array = list.ToArray();
                        var set = array.ToHashSet();
                        var ordered = set.OrderByDescending(u => u.Id).ToList();
                        var user = ordered.Last();

                        UpdateUserProfile(user, dto);
            """));
    }

    /// <summary>
    /// INCL004: Select hands back whatever its lambda made, not the entities it was given. A lambda that is not a
    /// chain of properties cannot be read, so the analyzer says it cannot tell rather than claiming the property
    /// is missing.
    /// </summary>
    [Fact]
    public async Task SelectProjection_NotPropertyChain_ReportsIncl4()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile).ToListAsync();
                        var managers = users.Select(u => u.Manager ?? u).ToList();
                        var manager = managers[0];

                        {|INCL004:UpdateUserProfile(manager, dto)|};
            """));
    }

    /// <summary>
    /// INCL002: Select(u => u.Manager) holds the managers of users, so their Profile needs Manager.Profile included
    /// on users. Profile alone is the users' own Profile.
    /// </summary>
    [Fact]
    public async Task SelectNavigation_NestedPathNotIncluded_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile).ToListAsync();
                        var managers = users.Select(u => u.Manager).ToList();
                        var manager = managers[0];

                        {|INCL002:UpdateUserProfile(manager, dto)|};
            """));
    }

    /// <summary>
    /// No INCL002: Manager.Profile is included on users, which covers Profile of the managers Select hands back.
    /// </summary>
    [Fact]
    public async Task SelectNavigation_NestedPathIncluded_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users
                            .Include(u => u.Manager)
                            .ThenInclude(m => m.Profile)
                            .ToListAsync();
                        var managers = users.Select(u => u.Manager).ToList();
                        var manager = managers[0];

                        UpdateUserProfile(manager, dto);
            """));
    }

    /// <summary>
    /// INCL004: an element selector stores other objects in the dictionary, not the included entities. The
    /// search does not read the selector, so it cannot say the property is missing, only that it cannot tell.
    /// </summary>
    [Fact]
    public async Task Dictionary_WithElementSelector_ReportsIncl4()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile)
                            .ToDictionaryAsync(u => u.Id, u => new User { Id = u.Id });
                        var user = users[dto.Id];
                        {|INCL004:UpdateUserProfile(user, dto)|};
            """));
    }

    /// <summary>
    /// INCL004: "Max(selector)" hands back whatever the selector returned, not an element, so the entities of
    /// the source say nothing about it. The overload is excluded by the name of its selector parameter, which
    /// is the only thing that separates it from "Max(comparer)" — both take two arguments.
    /// </summary>
    [Fact]
    public async Task MaxWithSelector_DoesNotKeepIncludes_ReportsIncl4()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile).ToListAsync();
                        var user = users.Max(u => u.Manager);
                        {|INCL004:UpdateUserProfile(user, dto)|};
            """));
    }

    /// <summary>
    /// No INCL002: "MaxBy(keySelector)" hands back an element, so it keeps the entities even though it takes
    /// a selector of its own.
    /// </summary>
    [Fact]
    public async Task MaxByKeySelector_KeepsIncludes_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile).ToListAsync();
                        var user = users.MaxBy(u => u.Id);
                        UpdateUserProfile(user, dto);
            """));
    }

    /// <summary>
    /// INCL004: "Concat" hands back the entities of two collections, and the analyzer can follow only one
    /// value at a time. Saying the property is loaded would mean answering for the second collection without
    /// having looked at it, so it says it cannot tell.
    /// </summary>
    [Fact]
    public async Task Concat_HasTwoSources_ReportsIncl4()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var loaded = await dbContext.Users.Include(u => u.Profile).ToListAsync();
                        var others = await dbContext.Users.ToListAsync();
                        var user = loaded.Concat(others).First();

                        {|INCL004:UpdateUserProfile(user, dto)|};
            """));
    }

    /// <summary>
    /// INCL004: the same for "Append", whose second source is one entity rather than a collection.
    /// </summary>
    [Fact]
    public async Task Append_HasTwoSources_ReportsIncl4()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var loaded = await dbContext.Users.Include(u => u.Profile).ToListAsync();
                        var user = loaded.Append(new User()).First();

                        {|INCL004:UpdateUserProfile(user, dto)|};
            """));
    }

    /// <summary>
    /// No INCL002: "Except" is matched against the collection it is given, but the entities that come out are
    /// the ones it was used on, so there is only one place to look.
    /// </summary>
    [Fact]
    public async Task Except_HasOneSource_KeepsIncludes_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var loaded = await dbContext.Users.Include(u => u.Profile).ToListAsync();
                        var others = await dbContext.Users.ToListAsync();
                        var user = loaded.Except(others).First();

                        UpdateUserProfile(user, dto);
            """));
    }

    /// <summary>
    /// No INCL002: "DefaultIfEmpty()" hands back the entities it was given.
    /// </summary>
    [Fact]
    public async Task DefaultIfEmpty_KeepsIncludes_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var loaded = await dbContext.Users.Include(u => u.Profile).ToListAsync();
                        var user = loaded.DefaultIfEmpty().First();

                        UpdateUserProfile(user, dto);
            """));
    }

    /// <summary>
    /// No INCL002: "DefaultIfEmpty(defaultValue)" is crossed whichever overload is used. The value argument
    /// is a second place the entities could come from, and it is not followed — the accepted hole of leaving
    /// this member on one line, rather than anything the analyzer worked out about the value.
    /// </summary>
    [Fact]
    public async Task DefaultIfEmptyWithValue_ValueArgumentNotFollowed_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var loaded = await dbContext.Users.Include(u => u.Profile).ToListAsync();
                        var user = loaded.DefaultIfEmpty(new User()).First();

                        UpdateUserProfile(user, dto);
            """));
    }

    /// <summary>
    /// INCL003: Include of Settings alone does not load its Profile.
    /// </summary>
    [Fact]
    public async Task IncludeWithoutThenInclude_DoesNotLoadNestedPath_ReportsIncl3()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    [Includes(nameof(User.Settings), nameof(UserSettings.Profile))]
                    async Task<User> GetUser(int id)
                    {
                        {|INCL003:return await dbContext.Users
                            .Include(u => u.Settings)
                            .FirstOrDefaultAsync(u => u.Id == id);|}
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL003: a member chain in the Include lambda and a dotted Include string both load the whole path,
    /// and a loaded path covers its prefix.
    /// </summary>
    [Fact]
    public async Task IncludeChainAndDottedString_LoadNestedPath_NoIncl3()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    [Includes(nameof(User.Settings), nameof(UserSettings.Profile))]
                    [Includes(nameof(User.Settings))]
                    async Task<User> GetUserByChain(int id)
                    {
                        return await dbContext.Users
                            .Include(u => u.Settings.Profile)
                            .FirstOrDefaultAsync(u => u.Id == id);
                    }

                    [Includes("Settings.Profile")]
                    async Task<User> GetUserByString(int id)
                    {
                        return await dbContext.Users
                            .Include("Settings.Profile")
                            .FirstOrDefaultAsync(u => u.Id == id);
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL003: ThenInclude continues the path of the Include it is called on, also after a collection Include,
    /// where it loads a property of each element. Each Include starts a new path, so the search finds both nested
    /// paths in one chain, whatever comes after them.
    /// </summary>
    [Fact]
    public async Task SeveralIncludePaths_InOneChain_NoIncl3()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    [Includes(nameof(User.Settings), nameof(UserSettings.Profile))]
                    [Includes(nameof(User.Applications), nameof(Application.Stats))]
                    async Task<User> GetUser(int id)
                    {
                        return await dbContext.Users
                            .Include(u => u.Settings)
                            .ThenInclude(s => s.Profile)
                            .Include(u => u.Applications)
                            .ThenInclude(a => a.Stats)
                            .Include(u => u.Organization)
                            .FirstOrDefaultAsync(u => u.Id == id);
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL003: a filtered include loads some of the elements but fills the collection, whatever operators
    /// filter it, and a ThenInclude continues from it.
    /// </summary>
    [Fact]
    public async Task FilteredInclude_LoadsCollection_NoIncl3()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    [Includes(nameof(User.Applications))]
                    async Task<User> GetUserWithSeveralOperators(int id)
                    {
                        return await dbContext.Users
                            .Include(u => u.Applications
                                .Where(a => a.Stats.Downloads > 10)
                                .OrderByDescending(a => a.Stats.Downloads)
                                .Take(5))
                            .FirstOrDefaultAsync(u => u.Id == id);
                    }

                    [Includes(nameof(User.Applications), nameof(Application.Stats))]
                    async Task<User> GetUserWithThenInclude(int id)
                    {
                        return await dbContext.Users
                            .Include(u => u.Applications.Where(a => a.Stats.Downloads > 10))
                            .ThenInclude(a => a.Stats)
                            .FirstOrDefaultAsync(u => u.Id == id);
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL003: the filter can also stand in the lambda of a ThenInclude.
    /// </summary>
    [Fact]
    public async Task FilteredThenInclude_LoadsCollection_NoIncl3()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    [Includes("Manager.Applications")]
                    async Task<User> GetUser(int id)
                    {
                        return await dbContext.Users
                            .Include(u => u.Manager)
                            .ThenInclude(m => m.Applications.Where(a => a.Stats.Downloads > 10))
                            .FirstOrDefaultAsync(u => u.Id == id);
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL003: the ThenInclude belongs to another Include, so it does not load Settings.Profile.
    /// </summary>
    [Fact]
    public async Task ThenIncludeOfOtherInclude_DoesNotLoadPath_ReportsIncl3()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    [Includes(nameof(User.Settings), nameof(UserSettings.Profile))]
                    async Task<User> GetUser(int id)
                    {
                        {|INCL003:return await dbContext.Users
                            .Include(u => u.Settings)
                            .Include(u => u.Manager)
                            .ThenInclude(m => m.Profile)
                            .FirstOrDefaultAsync(u => u.Id == id);|}
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL003: Select and SelectMany inside the query move from users to their Settings and Applications, whose
    /// Profile and Stats the query includes.
    /// </summary>
    [Fact]
    public async Task QueryableSelectAndSelectMany_NestedPathIncluded_NoIncl3()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    [Includes(nameof(UserSettings.Profile))]
                    async Task<UserSettings> GetSettings(int id)
                    {
                        return await dbContext.Users
                            .Include(u => u.Settings)
                            .ThenInclude(s => s.Profile)
                            .Where(u => u.Id == id)
                            .Select(u => u.Settings)
                            .FirstAsync();
                    }

                    [Includes(nameof(Application.Stats))]
                    async Task<List<Application>> GetApplications()
                    {
                        return await dbContext.Users
                            .Include(u => u.Applications)
                            .ThenInclude(a => a.Stats)
                            .SelectMany(u => u.Applications)
                            .ToListAsync();
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL003: SelectMany hands back the applications, but the query never included their Stats.
    /// </summary>
    [Fact]
    public async Task QueryableSelectMany_NestedPathNotIncluded_ReportsIncl3()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    [Includes(nameof(Application.Stats))]
                    async Task<List<Application>> GetApplications()
                    {
                        {|INCL003:return await dbContext.Users
                            .Include(u => u.Applications)
                            .SelectMany(u => u.Applications)
                            .ToListAsync();|}
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }
}
