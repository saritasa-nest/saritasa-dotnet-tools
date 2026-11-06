using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// INCL002: the basic ways a local variable gets its value: a query, a fresh entity, an object initializer and
/// a method. Also re-initialization of the variable or of a part of it after its first value.
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
    /// No INCL002: a navigation property assigned by hand holds what was put there, so the query that sourced
    /// "user" does not have to Include it.
    /// </summary>
    [Fact]
    public async Task Handle_MemberAssignedByHand_NoIncl2()
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

                        // No INCL002: Profile was set by hand.
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
    /// No INCL002: assigning the start of a nested path covers everything under it, so "user.Settings = ..."
    /// satisfies a requirement for "Settings.Profile".
    /// </summary>
    [Fact]
    public async Task Handle_StartOfPathAssignedByHand_NoIncl2()
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
                        user.Settings = new UserSettings();

                        // No INCL002: Settings, and so Settings.Profile, was set by hand.
                        UpdateTimezone(user, dto);
                    }

                    [IncludeRequired(nameof(user), "Settings.Profile")]
                    void UpdateTimezone(User user, SaveUserDto dto)
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002: assigning a deeper property does not load the one in front of it, so "user.Settings.Profile = ..."
    /// does not satisfy a requirement for "Settings".
    /// </summary>
    [Fact]
    public async Task Handle_DeeperPropertyAssignedByHand_ReportsIncl2()
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
                        // INCL002: reading Settings to set its Profile needs Settings loaded too.
                        user.{|INCL002:Settings|}.Profile = new UserProfile();

                        // INCL002: Settings itself was never loaded nor set.
                        {|INCL002:UpdateSettings(user, dto)|};
                    }

                    [IncludeRequired(nameof(user), nameof(User.Settings))]
                    void UpdateSettings(User user, SaveUserDto dto)
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: "user.Settings.Profile = ..." sets the end of "Settings.Profile", and the search goes on above the
    /// assignment for "Settings", which the query Included.
    /// </summary>
    [Fact]
    public async Task Handle_EndOfPathAssignedOnIncludedProperty_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await dbContext.Users
                            .Include(u => u.Settings)
                            .FirstOrDefaultAsync(u => u.Id == dto.Id);
                        user.Settings.Profile = new UserProfile();

                        // No INCL002: Settings was Included and its Profile was set by hand.
                        UpdateTimezone(user, dto);
                    }

                    [IncludeRequired(nameof(user), "Settings.Profile")]
                    void UpdateTimezone(User user, SaveUserDto dto)
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002: "user.Settings.Profile = ..." sets the end of "Settings.Profile", but the search goes on above the
    /// assignment for "Settings", which nothing loaded.
    /// </summary>
    [Fact]
    public async Task Handle_EndOfPathAssignedOnNotIncludedProperty_ReportsIncl2()
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
                        user.{|INCL002:Settings|}.Profile = new UserProfile();

                        // INCL002: Profile was set by hand, but Settings in front of it was never loaded.
                        {|INCL002:UpdateTimezone(user, dto)|};
                    }

                    [IncludeRequired(nameof(user), "Settings.Profile")]
                    void UpdateTimezone(User user, SaveUserDto dto)
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: "user.Settings = other.Settings" loads Settings and everything under it, whatever the assigned value
    /// holds, so the Profile that "other" was queried without does not matter.
    /// </summary>
    [Fact]
    public async Task Handle_StartOfPathAssignedFromQuery_NoIncl2()
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
                        var other = await dbContext.Users
                            .Include(u => u.Settings)
                            .FirstOrDefaultAsync(u => u.Id != dto.Id);
                        user.Settings = other.Settings;

                        // No INCL002: Settings was assigned, so the Profile it came with is not checked.
                        UpdateTimezone(user, dto);
                    }

                    [IncludeRequired(nameof(user), "Settings.Profile")]
                    void UpdateTimezone(User user, SaveUserDto dto)
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: an object initializer is an assignment too, so "new User { Settings = other.Settings }" loads
    /// Settings and everything under it.
    /// </summary>
    [Fact]
    public async Task Handle_ObjectInitAssignsStartOfPathFromQuery_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var other = await dbContext.Users
                            .Include(u => u.Settings)
                            .FirstOrDefaultAsync(u => u.Id == dto.Id);
                        var user = new User { Settings = other.Settings };

                        // No INCL002: Settings was assigned in the initializer.
                        UpdateTimezone(user, dto);
                    }

                    [IncludeRequired(nameof(user), "Settings.Profile")]
                    void UpdateTimezone(User user, SaveUserDto dto)
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002: a property assigned in only one branch is not loaded on the other one.
    /// </summary>
    [Fact]
    public async Task Handle_MemberAssignedInOneBranch_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto, bool reset)
                    {
                        var user = await dbContext.Users
                            .FirstOrDefaultAsync(u => u.Id == dto.Id);
                        if (reset)
                        {
                            user.Profile = new UserProfile();
                        }

                        // INCL002: when reset is false, Profile comes from a query that never Included it.
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

    /// <summary>
    /// No INCL002: assigning "Profile", which is not on the required path, leaves "Settings.Profile" to the query,
    /// and the query Included it.
    /// </summary>
    [Fact]
    public async Task Reinit_OtherPropertyAssigned_RequiredPathLoadedByQuery_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await dbContext.Users
                            .Include(u => u.Settings).ThenInclude(s => s.Profile)
                            .FirstOrDefaultAsync(u => u.Id == dto.Id);
                        user.Profile = new UserProfile();

                        // No INCL002: Settings.Profile is still the one the query loaded.
                        UpdateTimezone(user, dto);
                    }

                    [IncludeRequired(nameof(user), "Settings.Profile")]
                    void UpdateTimezone(User user, SaveUserDto dto)
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: a write below the required path ("Settings.Profile.Timezone") does not change what is loaded,
    /// and the query Included "Settings.Profile".
    /// </summary>
    [Fact]
    public async Task Reinit_WriteBelowRequiredPath_RequiredPathLoadedByQuery_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await dbContext.Users
                            .Include(u => u.Settings).ThenInclude(s => s.Profile)
                            .FirstOrDefaultAsync(u => u.Id == dto.Id);
                        user.Settings.Profile.Timezone = dto.Timezone;

                        // No INCL002: the write was below the required path, which the query loaded.
                        UpdateTimezone(user, dto);
                    }

                    [IncludeRequired(nameof(user), "Settings.Profile")]
                    void UpdateTimezone(User user, SaveUserDto dto)
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }
}
