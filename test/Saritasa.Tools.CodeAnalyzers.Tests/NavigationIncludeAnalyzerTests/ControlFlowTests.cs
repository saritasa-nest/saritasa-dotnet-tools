using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// The search reads the code backward through assignments, branches and loops. The property must be loaded on
/// every path that leads to the place.
/// </summary>
public class ControlFlowTests : NavigationIncludeTestBase
{
    /// <summary>
    /// INCL002: a local reassigned to a query without .Include() after an initial included
    /// declaration loses its tracked navigation property.
    /// </summary>
    [Fact]
    public async Task Reassignment_ToQueryMissingInclude_ReportsIncl2()
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

                        // Reassigning drops the previously tracked Profile include.
                        user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == dto.Id + 1);

                        // INCL002: user was reassigned to a value that does not load Profile.
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
    /// No INCL002: a local first declared without the required include is reassigned to a
    /// query that does include it.
    /// </summary>
    [Fact]
    public async Task Reassignment_ToQueryWithInclude_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);

                        // Reassigning to a query with Include now satisfies the requirement.
                        user = await dbContext.Users
                            .Include(u => u.Profile)
                            .FirstOrDefaultAsync(u => u.Id == dto.Id + 1);

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
    /// No INCL002: both branches of an if/else assign a value that loads the required
    /// property, so the merged state after the if still has it.
    /// </summary>
    [Fact]
    public async Task IfElse_BothBranchesInclude_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto, bool flag)
                    {
                        User user;
                        if (flag)
                        {
                            user = await dbContext.Users
                                .Include(u => u.Profile)
                                .FirstOrDefaultAsync(u => u.Id == dto.Id);
                        }
                        else
                        {
                            user = await dbContext.Users
                                .Include(u => u.Profile)
                                .FirstOrDefaultAsync(u => u.Id == dto.Id + 1);
                        }

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
    /// INCL002: only one branch of an if/else loads the required property, so the merged
    /// state after the if cannot guarantee it.
    /// </summary>
    [Fact]
    public async Task IfElse_OnlyOneBranchIncludes_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto, bool flag)
                    {
                        User user;
                        if (flag)
                        {
                            user = await dbContext.Users
                                .Include(u => u.Profile)
                                .FirstOrDefaultAsync(u => u.Id == dto.Id);
                        }
                        else
                        {
                            user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == dto.Id + 1);
                        }

                        // INCL002: Profile is only guaranteed loaded on one of the two branches.
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
    /// No INCL002: a query variable reassigned several times keeps the include added earlier.
    /// </summary>
    [Fact]
    public async Task QueryReassignedSeveralTimes_IncludeKept_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto, bool onlyActive)
                    {
                        var query = dbContext.Users.AsQueryable();
                        query = query.Include(u => u.Profile);
                        if (onlyActive)
                        {
                            query = query.Where(u => u.Id > 0);
                        }
                        query = query.OrderBy(u => u.Id);

                        var user = await query.FirstAsync();
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
    /// No INCL002: a variable loaded with .Include() before a loop keeps it inside the loop.
    /// </summary>
    [Fact]
    public async Task Loop_IncludedBeforeLoop_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto, int count)
                    {
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        for (var i = 0; i < count; i++)
                        {
                            UpdateUserProfile(user, dto);
                        }
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
    /// INCL002: the variable is reassigned at the end of the loop body, so the next iteration
    /// calls the method with a value that does not load Profile.
    /// </summary>
    [Fact]
    public async Task Loop_ReassignedAtEndOfLoopBody_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto, int count)
                    {
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        for (var i = 0; i < count; i++)
                        {
                            {|INCL002:UpdateUserProfile(user, dto)|};
                            user = await dbContext.Users.FirstAsync();
                        }
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
