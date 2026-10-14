using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// INCL002: the basic ways a local variable gets its value: a query, a fresh entity, an object initializer and
/// a method.
/// </summary>
public class LocalVariableTests : NavigationIncludeTestBase
{
    /// <summary>
    /// INCL002: local variable obtained from a query without .Include() triggers the warning.
    /// </summary>
    [Fact]
    public async Task Handle_QueryMissingInclude_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await dbContext.Users
                            //.Include(u => u.Profile) // Uncommenting this line would fix the INCL002 warning.
                            .FirstOrDefaultAsync(u => u.Id == dto.Id);

                        // INCL002: the called method requires User.Profile, but it is not Included in the query.
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
    /// INCL002: a navigation property assigned by hand is not a write the search follows. Finding whether an
    /// Include happened is the analyzer's whole concern, not whether the property currently holds a value, so
    /// even assigning something that would itself count as loaded — a freshly constructed object — does not
    /// satisfy the requirement: the walk skips the assignment and keeps looking for how "user" was sourced,
    /// which here is a query without Include.
    /// </summary>
    [Fact]
    public async Task Handle_MemberAssignedByHand_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await dbContext.Users
                            .FirstOrDefaultAsync(u => u.Id == dto.Id);
                        user.Profile = new UserProfile { Timezone = dto.Timezone };

                        // INCL002: the assignment is not followed; the query never Included Profile.
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
    /// No INCL002: local variable obtained from a query that includes .Include(u => u.Profile).
    /// </summary>
    [Fact]
    public async Task Handle_QueryWithInclude_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await dbContext.Users
                            .Include(u => u.Profile)
                            .FirstOrDefaultAsync(u => u.Id == dto.Id);
                        // No INCL002: Profile is loaded via .Include().
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
    /// No INCL002: local variable created via object initializer that leaves the required property unset. The
    /// entity was not sourced from a query, so there was no Include to miss for any property, including this
    /// one — the same reasoning as a bare <c>new User()</c>, regardless of which other properties are set.
    /// </summary>
    [Fact]
    public async Task Handle2_ObjectInitLeavesPropertyUnset_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass
                {
                    async Task Handle2(SaveUserDto dto)
                    {
                        var user = new User
                        {
                            Id = dto.Id,
                            Organization = dto.Organization,
                        };

                        // No INCL002: the entity was not sourced from a query.
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
    /// No INCL002: a freshly constructed entity was not sourced from a query, so there is no Include to have
    /// missed. The analyzer does not follow a property set afterward by an opaque mapper or builder; that is
    /// the accepted blind spot of treating construction itself as satisfying the requirement.
    /// </summary>
    [Fact]
    public async Task Handle2_FreshEntity_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass
                {
                    async Task Handle2(SaveUserDto dto)
                    {
                        var user = new User();
                        // No INCL002: the entity was not sourced from a query.
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
    /// INCL002: local variable obtained from a method that does not carry [Includes] triggers the warning.
    /// </summary>
    [Fact]
    public async Task Handle3_SourceMethodMissingIncludesAttr_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle3(SaveUserDto dto)
                    {
                        // GetUser has no [Includes(nameof(User.Profile))].
                        var user = await GetUser(dto.Id);
                        // INCL002: the called method requires User.Profile, but it is not checked.
                        {|INCL002:UpdateUserProfile(user, dto)|};
                    }

                    // [Includes(nameof(User.Profile))] // Uncommenting would fix the warning.
                    async Task<User> GetUser(int id)
                    {
                        return await dbContext.Users
                            .Include(u => u.Profile)
                            .FirstOrDefaultAsync(u => u.Id == id);
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
    /// No INCL002: local variable obtained from a method decorated with [Includes(nameof(User.Profile)].
    /// </summary>
    [Fact]
    public async Task Handle3_SourceMethodWithIncludesAttr_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle3(SaveUserDto dto)
                    {
                        var user = await GetUser(dto.Id);
                        // No INCL002: GetUser carries [Includes(nameof(User.Profile))].
                        UpdateUserProfile(user, dto);
                    }

                    [Includes(nameof(User.Profile))]
                    async Task<User> GetUser(int id)
                    {
                        return await dbContext.Users
                            .Include(u => u.Profile)
                            .FirstOrDefaultAsync(u => u.Id == id);
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
    /// INCL002 on the read: the local comes from a query that never Included Profile.
    /// </summary>
    [Fact]
    public async Task ReadOfLocal_QueryWithoutInclude_ReportsIncl2()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task<string> GetTimezone(int id)
                    {
                        var user = await dbContext.Users.FirstAsync(u => u.Id == id);
                        return user.{|INCL002:Profile|}.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002 on Profile: the query Includes Settings, but not Settings.Profile.
    /// </summary>
    [Fact]
    public async Task NestedReadOfLocal_OnlyPrefixIncluded_ReportsIncl2()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task<string> GetTimezone(int id)
                    {
                        var user = await dbContext.Users
                            .Include(u => u.Settings)
                            .FirstAsync(u => u.Id == id);
                        return user.Settings.{|INCL002:Profile|}.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: Include and ThenInclude load the whole chain.
    /// </summary>
    [Fact]
    public async Task NestedReadOfLocal_ThenIncluded_NoIncl2()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task<string> GetTimezone(int id)
                    {
                        var user = await dbContext.Users
                            .Include(u => u.Settings)
                            .ThenInclude(s => s.Profile)
                            .FirstAsync(u => u.Id == id);
                        return user.Settings.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: a ThenInclude continues another ThenInclude held in a variable, which continues an Include held
    /// in another one.
    /// </summary>
    [Fact]
    public async Task ThenIncludeOnThenIncludeInVariables_NoIncl2()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task<string> GetTimezone(int id)
                    {
                        var managers = dbContext.Users.Include(u => u.Manager);
                        var managerSettings = managers.ThenInclude(m => m.Settings);
                        var user = await managerSettings.ThenInclude(s => s.Profile).FirstAsync(u => u.Id == id);
                        return user.Manager.Settings.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002 on Settings and on Profile: one branch Includes the manager's Settings, so the ThenInclude continues
    /// from there and loads Manager.Settings.Profile, not Settings.Profile of the user.
    /// </summary>
    [Fact]
    public async Task ThenIncludeOnVariable_OneBranchIncludesOtherPrefix_ReportsIncl2()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task<string> GetTimezone(int id, bool ofManager)
                    {
                        Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<User, UserSettings> query;
                        if (ofManager)
                        {
                            query = dbContext.Users.Include(u => u.Manager.Settings);
                        }
                        else
                        {
                            query = dbContext.Users.Include(u => u.Settings);
                        }

                        var user = await query.ThenInclude(s => s.Profile).FirstAsync(u => u.Id == id);
                        return user.{|INCL002:Settings|}.{|INCL002:Profile|}.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: the ThenInclude's Profile matches the end of Settings.Profile, but its chain belongs to Manager;
    /// an earlier Include loads Settings.Profile on its own.
    /// </summary>
    [Fact]
    public async Task ThenIncludeOfOtherChain_EarlierIncludeLoadsPath_NoIncl2()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task<string> GetTimezone(int id)
                    {
                        var query = dbContext.Users.Include(u => u.Settings.Profile).Include(u => u.Manager);
                        var user = await query.ThenInclude(m => m.Profile).FirstAsync(u => u.Id == id);
                        return user.Settings.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL001: Manager.Manager.Manager.Manager is loaded, and the two-segment lambda matches the end of each
    /// required path in a different way: one Manager for the first method, two for the second.
    /// </summary>
    [Fact]
    public async Task ThenIncludeWithRepeatedNames_EveryWayOfMatchingTried_NoIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(int id)
                    {
                        var query = dbContext.Users.Include(u => u.Manager.Manager);
                        var user = await query.ThenInclude(m => m.Manager.Manager).FirstAsync(u => u.Id == id);
                        UseThreeManagers(user);
                        UseFourManagers(user);
                    }

                    [IncludeRequired(nameof(user), "Manager.Manager.Manager")]
                    void UseThreeManagers(User user)
                    {
                    }

                    [IncludeRequired(nameof(user), "Manager.Manager.Manager.Manager")]
                    void UseFourManagers(User user)
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002 on the read: GetUser is our own method and promises nothing with [Includes].
    /// </summary>
    [Fact]
    public async Task ReadOfCallResult_UnannotatedMethod_ReportsIncl2()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task<string> GetTimezone(int id)
                    {
                        return (await GetUser(id)).{|INCL002:Profile|}.Timezone;
                    }

                    Task<User> GetUser(int id) => dbContext.Users.FirstAsync(u => u.Id == id);
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: a new entity has no query behind it.
    /// </summary>
    [Fact]
    public async Task ReadOfNewEntity_NoIncl2()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    string GetTimezone()
                    {
                        var user = new User { Profile = new UserProfile() };
                        return user.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }
}
