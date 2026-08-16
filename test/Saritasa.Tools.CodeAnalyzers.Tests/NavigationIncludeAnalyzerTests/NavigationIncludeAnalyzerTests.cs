using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude;
using VerifyCS = Saritasa.Tools.CodeAnalyzers.Tests.Verifiers.CSharpAnalyzerVerifier<
    Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.NavigationIncludeAnalyzer>;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// Tests for <see cref="NavigationIncludeAnalyzer"/>.
/// </summary>
[TestClass]
public class NavigationIncludeAnalyzerTests
{
    private const string Preamble =
        /* lang=c# */
        """
        using System;
        using System.Threading.Tasks;

        namespace TestApplication
        {
            [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
            public class IncludeRequiredAttribute : Attribute
            {
                public IncludeRequiredAttribute(string paramName, string propertyName)
                {
                }
            }

            [AttributeUsage(AttributeTargets.Property)]
            public class TrackIncludeRequiredAttribute : Attribute
            {
                public TrackIncludeRequiredAttribute()
                {
                }
            }

            [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
            public class IncludesAttribute : Attribute
            {
                public string IncludedProperty { get; }

                public IncludesAttribute(string includedProperty)
                {
                    IncludedProperty = includedProperty;
                }
            }

            class DbQuery<T>
            {
                public DbQuery<T> Include<TProperty>(Func<T, TProperty> nav) => this;
                public T FirstOrDefault(Func<T, bool> predicate) => default;
                public Task<T> FirstOrDefaultAsync(Func<T, bool> predicate) => Task.FromResult(default(T));
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

    #region INCL001

    /// <summary>
    /// INCL001 is reported when a method accesses a [TrackIncludeRequired] property directly without declaring [IncludeRequired].
    /// </summary>
    [TestMethod]
    public async Task DirectPropertyAccess_WithoutIncludeRequired_ReportsIncl001()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    void SetTimezone_Nocheck(User user, string timezone)
                    {
                        // INCL001: navigation property is required, but not checked. Use IncludeRequiredAttribute
                        {|INCL001:user.Profile|}.Timezone = timezone;
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No warning is produced when a method declares [IncludeRequired] for the parameter it accesses the navigation property on.
    /// </summary>
    [TestMethod]
    public async Task DirectPropertyAccess_WithIncludeRequired_NoWarning()
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL001 is reported when a method passes its own parameter to a callee that has [IncludeRequired], but the caller does not declare the same requirement.
    /// </summary>
    [TestMethod]
    public async Task ParameterPropagation_WithoutIncludeRequired_ReportsIncl001()
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No warning is produced when both the caller and the callee declare [IncludeRequired] for the propagated parameter.
    /// </summary>
    [TestMethod]
    public async Task ParameterPropagation_WithIncludeRequired_NoWarning()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    // Uncommenting this line will fix the INCL001 warning.
                    [IncludeRequired(nameof(user), nameof(@User.Profile))]
                    void UpdateUserProfile(User user, SaveUserDto dto)
                    {
                        // Does not produce INCL001 because Organization has no TrackIncludeRequired attribute.
                        user.Organization = dto.Organization;
                        SetTimezone(user, dto.Timezone);
                    }

                    [IncludeRequired(nameof(user), nameof(@User.Profile))]
                    void SetTimezone(User user, string timezone)
                    {
                        // Does not produce INCL001 because the method has IncludeRequired attribute.
                        user.Profile.Timezone = timezone;
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    #endregion

    #region INCL002

    /// <summary>
    /// INCL002: local variable obtained from a query without .Include() triggers the warning.
    /// </summary>
    [TestMethod]
    public async Task Handle_QueryMissingInclude_Warns()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    private DbQuery<User> _users;

                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await _users
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: local variable obtained from a query that includes .Include(u => u.Profile).
    /// </summary>
    [TestMethod]
    public async Task Handle_QueryWithInclude_NoWarn()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass
                {
                    private DbQuery<User> _users;

                    async Task Handle(SaveUserDto dto)
                    {
                        var user = await _users
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002: local variable created via object initializer without the required property triggers the warning.
    /// </summary>
    [TestMethod]
    public async Task Handle2_ObjectInitMissingProperty_Warns()
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: local variable created via object initializer that sets the required property.
    /// </summary>
    [TestMethod]
    public async Task Handle2_ObjectInitWithProperty_NoWarn()
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL002: local variable obtained from a method that does not carry [Includes] triggers the warning.
    /// </summary>
    [TestMethod]
    public async Task Handle3_SourceMethodMissingIncludesAttr_Warns()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass
                {
                    private DbQuery<User> _users;

                    async Task Handle3(SaveUserDto dto)
                    {
                        // GetUser has no [Includes(nameof(User.Profile))] – INCL002 expected.
                        var user = await GetUser(dto.Id);
                        // INCL002: the called method requires User.Profile, but it is not checked.
                        {|INCL002:UpdateUserProfile(user, dto)|};
                    }

                    // [Includes(nameof(User.Profile))] // Uncommenting would fix the warning.
                    async Task<User> GetUser(int id)
                    {
                        return await _users
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: local variable obtained from a method decorated with [Includes(nameof(User.Profile)].
    /// </summary>
    [TestMethod]
    public async Task Handle3_SourceMethodWithIncludesAttr_NoWarn()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass
                {
                    private DbQuery<User> _users;

                    async Task Handle3(SaveUserDto dto)
                    {
                        var user = await GetUser(dto.Id);
                        // No INCL002: GetUser carries [Includes(nameof(User.Profile))].
                        UpdateUserProfile(user, dto);
                    }

                    [Includes(nameof(User.Profile))]
                    async Task<User> GetUser(int id)
                    {
                        return await _users
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    #endregion
}
