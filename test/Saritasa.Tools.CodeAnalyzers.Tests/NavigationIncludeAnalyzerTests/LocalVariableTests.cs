using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// INCL002: the basic ways a local variable gets its value: a query, an object initializer and a method.
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
    /// INCL002: local variable created via object initializer without the required property triggers the warning.
    /// </summary>
    [Fact]
    public async Task Handle2_ObjectInitMissingProperty_ReportsIncl2()
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
                            //Profile = new UserProfile { Timezone = dto.Timezone } // Uncommenting this line would fix the INCL002 warning
                        };

                        // INCL002: the called method requires User.Profile, but it is not set.
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
    /// No INCL002: local variable created via object initializer that sets the required property.
    /// </summary>
    [Fact]
    public async Task Handle2_ObjectInitWithProperty_NoIncl2()
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
                            Profile = new UserProfile { Timezone = dto.Timezone },
                        };
                        // No INCL002: Profile is set in the object initializer.
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
}
