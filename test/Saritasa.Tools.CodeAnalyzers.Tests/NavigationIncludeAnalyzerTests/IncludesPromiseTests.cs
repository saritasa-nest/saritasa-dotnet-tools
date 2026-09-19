using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// INCL003: a method declared with [Includes] must load the property in the value it returns.
/// </summary>
public class IncludesPromiseTests : NavigationIncludeTestBase
{
    /// <summary>
    /// INCL003: method declares [Includes(nameof(User.Profile))] but returns a query result
    /// without .Include(u => u.Profile).
    /// </summary>
    [Fact]
    public async Task GetUser_QueryMissingInclude_ReportsIncl3()
    {
        var sourceCode = Preamble +
             /* lang=c# */
             """

                 class TestClass(AppDbContext dbContext)
                 {
                     [Includes(nameof(User.Profile))]
                     async Task<User> GetUser(int id)
                     {
                         // INCL003: the method has Includes attribute for User.Profile, but Profile is not Included in the query.
                         {|INCL003:return await dbContext.Users
                             //.Include(u => u.Profile) // Uncommenting this line would fix the INCL003 warning.
                             .FirstOrDefaultAsync(u => u.Id == id);|}
                     }
                 }
             }
             """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL003: method declares [Includes(nameof(User.Profile))] and the query includes
    /// .Include(u => u.Profile).
    /// </summary>
    [Fact]
    public async Task GetUser_QueryWithInclude_NoIncl3()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    [Includes(nameof(User.Profile))]
                    async Task<User> GetUser(int id)
                    {
                        // No INCL003: Profile is loaded via .Include().
                        return await dbContext.Users
                            .Include(u => u.Profile)
                            .FirstOrDefaultAsync(u => u.Id == id);
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL003: method declares [Includes(nameof(User.Profile))] but the created object does not
    /// set Profile in the object initializer.
    /// </summary>
    [Fact]
    public async Task CreateUser_ObjectInitMissingProfile_ReportsIncl3()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass
                {
                    [Includes(nameof(User.Profile))]
                    User CreateUser(SaveUserDto dto)
                    {
                        // INCL003: the method has Includes attribute for User.Profile, but Profile is not set.
                        var user = new User
                        {
                            Id = dto.Id,
                            Organization = dto.Organization,
                            //Profile = new UserProfile{ Timezone = dto.Timezone } // Uncommenting this line would fix the INCL003 warning.
                        };
                        {|INCL003:return user;|}
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL003: method declares [Includes(nameof(User.Profile))] and the created object sets
    /// Profile in the object initializer.
    /// </summary>
    [Fact]
    public async Task CreateUser_ObjectInitWithProfile_NoIncl3()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass
                {
                    [Includes(nameof(User.Profile))]
                    User CreateUser(SaveUserDto dto)
                    {
                        // No INCL003: Profile is set in the object initializer.
                        var user = new User
                        {
                            Id = dto.Id,
                            Organization = dto.Organization,
                            Profile = new UserProfile { Timezone = dto.Timezone },
                        };
                        return user;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL003: verification of the [Includes] promise is disabled with Verify = false.
    /// </summary>
    [Fact]
    public async Task GetUser_VerifyDisabled_NoIncl3()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    [Includes(nameof(User.Profile), Verify = false)]
                    async Task<User> GetUser(int id)
                    {
                        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL003: Verify = true (the default) keeps the verification enabled.
    /// </summary>
    [Fact]
    public async Task GetUser_VerifyEnabledExplicitly_ReportsIncl3()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    [Includes(nameof(User.Profile), Verify = true)]
                    async Task<User> GetUser(int id)
                    {
                        {|INCL003:return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);|}
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: callers still rely on an [Includes] promise whose verification is disabled.
    /// </summary>
    [Fact]
    public async Task Handle_SourceMethodWithUnverifiedIncludes_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await GetUser(dto.Id);
                        UpdateUserProfile(user, dto);
                    }

                    [Includes(nameof(User.Profile), Verify = false)]
                    async Task<User> GetUser(int id)
                    {
                        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
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
}
