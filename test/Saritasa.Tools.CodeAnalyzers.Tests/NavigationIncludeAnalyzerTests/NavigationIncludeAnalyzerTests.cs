using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude;
using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// Tests for <see cref="NavigationIncludeAnalyzer"/>.
/// </summary>
public class NavigationIncludeAnalyzerTests
{
    private const string Preamble =
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

            public class Organization
            {
                public string Name { get; set; }
            }

            public class UserProfile
            {
                public string Timezone { get; set; }
            }

            public class User
            {
                public int Id { get; set; }
                public Organization Organization { get; set; }
                [TrackIncludeRequired]
                public UserProfile Profile { get; set; }
            }

            public class SaveUserDto
            {
                public int Id { get; set; }
                public Organization Organization { get; set; }
                public string Timezone { get; set; }
            }
        """;

    private static readonly ReferenceAssemblies References = new ReferenceAssemblies(
            "net10.0",
            new PackageIdentity("Microsoft.NETCore.App.Ref", "10.0.0"),
            Path.Combine("ref", "net10.0")
        )
        .AddPackages([new PackageIdentity("Microsoft.EntityFrameworkCore", "10.0.11")]);

    private static async Task VerifyAnalyzerAsync(string source)
    {
        var test = new CSharpAnalyzerTest<NavigationIncludeAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = References
        };
        test.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(typeof(NavigationIncludeAnalyzer).Assembly.Location));
        test.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(typeof(TrackIncludeRequiredAttribute).Assembly.Location));
        await test.RunAsync(CancellationToken.None);
    }

    #region INCL001

    /// <summary>
    /// INCL001 is reported when a method accesses a [TrackIncludeRequired] property directly without declaring [IncludeRequired].
    /// </summary>
    [Fact]
    public async Task DirectPropertyAccess_WithoutIncludeRequired_ReportsIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    void SetTimezone_NoCheck(User user, string timezone)
                    {
                        // INCL001: navigation property is required, but not checked. Use IncludeRequiredAttribute
                        {|INCL001:user.Profile|}.Timezone = timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No warning is produced when a method declares [IncludeRequired] for the parameter it accesses the navigation property on.
    /// </summary>
    [Fact]
    public async Task DirectPropertyAccess_WithIncludeRequired_NoIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void SetTimezone(User user, string timezone)
                    {
                        // Does not produce INCL001 because the method has IncludeRequired attribute.
                        user.Profile.Timezone = timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL001 is reported when a method passes its own parameter to a callee that has [IncludeRequired], but the caller does not declare the same requirement.
    /// </summary>
    [Fact]
    public async Task ParameterPropagation_WithoutIncludeRequired_ReportsIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    // Uncommenting this line will fix the INCL001 warning.
                    // [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void UpdateUserProfile(User user, SaveUserDto dto)
                    {
                        // Does not produce INCL001 because Organization has no TrackIncludeRequired attribute.
                        user.Organization = dto.Organization;
                        // INCL001: the called method captures the `user` argument and has IncludeRequired attribute.
                        // Use IncludeRequiredAttribute on the current method as well.
                        {|INCL001:SetTimezone(user, dto.Timezone)|};
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void SetTimezone(User user, string timezone)
                    {
                        // Does not produce INCL001 because the method has IncludeRequired attribute.
                        user.Profile.Timezone = timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No warning is produced when both the caller and the callee declare [IncludeRequired] for the propagated parameter.
    /// </summary>
    [Fact]
    public async Task ParameterPropagation_WithIncludeRequired_NoIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void UpdateUserProfile(User user, SaveUserDto dto)
                    {
                        // Does not produce INCL001 because Organization has no TrackIncludeRequired attribute.
                        user.Organization = dto.Organization;
                        SetTimezone(user, dto.Timezone);
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void SetTimezone(User user, string timezone)
                    {
                        // Does not produce INCL001 because the method has IncludeRequired attribute.
                        user.Profile.Timezone = timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    #endregion

    #region INCL002

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

    #endregion

    #region INCL003

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

    #endregion

    #region Flow analysis

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
    /// No INCL002: the include is tracked through an intermediate query variable before the
    /// final local is materialized (multi-hop propagation).
    /// </summary>
    [Fact]
    public async Task MultiHopPropagation_ThroughIntermediateQuery_NoIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var query = dbContext.Users.Include(u => u.Profile);
                        var user = await query.FirstOrDefaultAsync(u => u.Id == dto.Id);

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
    /// INCL002: a query variable gets the include only in one branch, so after the branch
    /// and further reassignments the include is not guaranteed.
    /// </summary>
    [Fact]
    public async Task QueryReassignedSeveralTimes_IncludeOnlyInBranch_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto, bool withProfile)
                    {
                        var query = dbContext.Users.AsQueryable();
                        if (withProfile)
                        {
                            query = query.Include(u => u.Profile);
                        }
                        query = query.Where(u => u.Id > 0);

                        var user = await query.FirstAsync();
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
    /// No INCL002: iterating a collection loaded via .Include() seeds the foreach loop
    /// variable with the same tracked navigation property.
    /// </summary>
    [Fact]
    public async Task ForEach_OverIncludedCollection_NoIncl2()
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

                        foreach (var user in users)
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

    #endregion

    #region Dictionaries

    /// <summary>
    /// Wraps the body of a Handle method that works with a dictionary of users.
    /// </summary>
    private static string DictionarySource(string handleBody) => Preamble +
        $$"""

            class TestClass(AppDbContext dbContext)
            {
                async Task Handle(SaveUserDto dto)
                {
        {{handleBody}}
                }

                [IncludeRequired(nameof(user), nameof(User.Profile))]
                void UpdateUserProfile(User user, SaveUserDto dto)
                {
                    user.Profile.Timezone = dto.Timezone;
                }
            }
        }
        """;

    /// <summary>
    /// No INCL002: an element read by key from a dictionary built from an included query.
    /// </summary>
    [Fact]
    public async Task Dictionary_IndexerOverIncludedQuery_NoIncl2()
    {
        await VerifyAnalyzerAsync(DictionarySource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile).ToDictionaryAsync(u => u.Id);
                        UpdateUserProfile(users[dto.Id], dto);
                        var user = users[dto.Id];
                        UpdateUserProfile(user, dto);
            """));
    }

    /// <summary>
    /// INCL002: the dictionary is built from a query without .Include().
    /// </summary>
    [Fact]
    public async Task Dictionary_IndexerOverNonIncludedQuery_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(DictionarySource(
            """
                        var users = await dbContext.Users.ToDictionaryAsync(u => u.Id);
                        var user = users[dto.Id];
                        {|INCL002:UpdateUserProfile(user, dto)|};
            """));
    }

    /// <summary>
    /// No INCL002: TryGetValue, GetValueOrDefault, Values and foreach over pairs of an included dictionary.
    /// </summary>
    [Fact]
    public async Task Dictionary_AllReadsOverIncludedQuery_NoIncl2()
    {
        await VerifyAnalyzerAsync(DictionarySource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile).ToDictionaryAsync(u => u.Id);
                        if (users.TryGetValue(dto.Id, out var found))
                        {
                            UpdateUserProfile(found, dto);
                        }

                        var user = users.GetValueOrDefault(dto.Id);
                        UpdateUserProfile(user, dto);

                        foreach (var value in users.Values)
                        {
                            UpdateUserProfile(value, dto);
                        }

                        foreach (var pair in users)
                        {
                            UpdateUserProfile(pair.Value, dto);
                        }

                        foreach (var (id, deconstructed) in users)
                        {
                            UpdateUserProfile(deconstructed, dto);
                        }
            """));
    }

    /// <summary>
    /// INCL002: TryGetValue over a dictionary built from a query without .Include().
    /// </summary>
    [Fact]
    public async Task Dictionary_TryGetValueOverNonIncludedQuery_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(DictionarySource(
            """
                        var users = await dbContext.Users.ToDictionaryAsync(u => u.Id);
                        if (users.TryGetValue(dto.Id, out var found))
                        {
                            {|INCL002:UpdateUserProfile(found, dto)|};
                        }
            """));
    }

    /// <summary>
    /// INCL002: an element selector stores other objects in the dictionary, not the included entities.
    /// </summary>
    [Fact]
    public async Task Dictionary_WithElementSelector_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(DictionarySource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile)
                            .ToDictionaryAsync(u => u.Id, u => new User { Id = u.Id });
                        var user = users[dto.Id];
                        {|INCL002:UpdateUserProfile(user, dto)|};
            """));
    }

    #endregion
}
