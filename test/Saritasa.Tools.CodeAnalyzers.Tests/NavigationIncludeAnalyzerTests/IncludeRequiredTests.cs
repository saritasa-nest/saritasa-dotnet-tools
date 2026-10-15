using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// INCL001: a method that uses a tracked property of its parameter, or passes the parameter on, must be annotated with
/// [IncludeRequired].
/// </summary>
public class IncludeRequiredTests : NavigationIncludeTestBase
{
    /// <summary>
    /// INCL001 is reported when a method accesses a [TrackIncludeRequired] property directly without [IncludeRequired].
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
                        user.{|INCL001:Profile|}.Timezone = timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL001 is reported when a method accesses a [TrackIncludeRequired] property directly without [IncludeRequired].
    /// </summary>
    [Fact]
    public async Task ConditionalPropertyAccess_WithoutIncludeRequired_ReportsIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    void ReadTimezone_NoCheck(User user)
                    {
                        // INCL001: conditional navigation property is required, but not checked. Use IncludeRequiredAttribute
                        var timezone = user?.{|INCL001:Profile|}.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No warning is produced when a method is annotated with [IncludeRequired] for the parameter it accesses the navigation property on.
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
    /// INCL001 is reported when a method passes its own parameter to a callee that has [IncludeRequired], but the caller is not annotated with the same requirement.
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
    /// No warning is produced when both the caller and the callee are annotated with [IncludeRequired] for the propagated parameter.
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

    /// <summary>
    /// No INCL001: the path given one segment per argument starts with Profile, so it covers what the callee asks for.
    /// </summary>
    [Fact]
    public async Task PassParameter_SegmentPathCoversRequirement_NoIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), nameof(User.Profile), nameof(UserProfile.Timezone))]
                    void Handle(User user, SaveUserDto dto)
                    {
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
    /// INCL001: the caller asks only for Profile, which does not cover the longer path the callee asks for.
    /// </summary>
    [Fact]
    public async Task PassParameter_ShorterPathDoesNotCoverRequirement_ReportsIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void Handle(User user, SaveUserDto dto)
                    {
                        {|INCL001:UpdateTimezone(user, dto)|};
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile), nameof(UserProfile.Timezone))]
                    void UpdateTimezone(User user, SaveUserDto dto)
                    {
                        user.Profile.Timezone = dto.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL002: settings is user.Settings, so its Profile is Settings.Profile of user, which the method asks for.
    /// </summary>
    [Fact]
    public async Task PassNavigationOfParameter_NestedPathRequired_NoDiagnostic()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), nameof(User.Settings), nameof(UserSettings.Profile))]
                    void Handle(User user)
                    {
                        var settings = user.Settings;
                        UpdateProfile(settings);
                    }

                    [IncludeRequired(nameof(settings), nameof(UserSettings.Profile))]
                    void UpdateProfile(UserSettings settings)
                    {
                        settings.Profile.Timezone = "UTC";
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL001: settings is user.Settings, and the method asks only for Settings of user, which does not cover
    /// Settings.Profile. The search ends at user, so the attribute belongs on this method.
    /// </summary>
    [Fact]
    public async Task PassNavigationOfParameter_OnlyPrefixRequired_ReportsIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), nameof(User.Settings))]
                    void Handle(User user)
                    {
                        var settings = user.Settings;
                        {|INCL001:UpdateProfile(settings)|};
                    }

                    [IncludeRequired(nameof(settings), nameof(UserSettings.Profile))]
                    void UpdateProfile(UserSettings settings)
                    {
                        settings.Profile.Timezone = "UTC";
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL001 twice: the call needs Settings.Profile of user, and the argument reads Settings of user. Neither is
    /// asked for; [IncludeRequired("user", "Settings.Profile")] fixes both.
    /// </summary>
    [Fact]
    public async Task PassNavigationOfParameterDirectly_NothingRequired_ReportsIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    void Handle(User user)
                    {
                        {|INCL001:UpdateProfile(user.{|INCL001:Settings|})|};
                    }

                    [IncludeRequired(nameof(settings), nameof(UserSettings.Profile))]
                    void UpdateProfile(UserSettings settings)
                    {
                        settings.Profile.Timezone = "UTC";
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL001: the argument is Settings of user, and the method asks for Settings.Profile of user.
    /// </summary>
    [Fact]
    public async Task PassNavigationOfParameterDirectly_FullPathRequired_NoIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), "Settings.Profile")]
                    void Handle(User user)
                    {
                        UpdateProfile(user.Settings);
                    }

                    [IncludeRequired(nameof(settings), nameof(UserSettings.Profile))]
                    void UpdateProfile(UserSettings settings)
                    {
                        settings.Profile.Timezone = "UTC";
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL004: the argument is a field, which the analyzer cannot follow.
    /// </summary>
    [Fact]
    public async Task PassField_ReportsIncl4()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    private User user;

                    void Handle()
                    {
                        {|INCL004:UpdateProfile(user)|};
                    }

                    [IncludeRequired(nameof(user), nameof(User.Profile))]
                    void UpdateProfile(User user)
                    {
                        user.Profile.Timezone = "UTC";
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL001 on Profile only: the method asks for Settings of user, which does not cover Settings.Profile.
    /// </summary>
    [Fact]
    public async Task NestedRead_OnlyPrefixRequired_ReportsIncl1OnUncoveredProperty()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), nameof(User.Settings))]
                    string GetTimezone(User user)
                    {
                        return user.Settings.{|INCL001:Profile|}.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL001 on each property: every read is checked on its own. [IncludeRequired("user", "Settings.Profile")]
    /// fixes both.
    /// </summary>
    [Fact]
    public async Task NestedRead_NothingRequired_ReportsIncl1OnEachProperty()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    string GetTimezone(User user)
                    {
                        return user.{|INCL001:Settings|}.{|INCL001:Profile|}.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL001: Settings.Profile covers the whole chain.
    /// </summary>
    [Fact]
    public async Task NestedRead_FullPathRequired_NoIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), "Settings.Profile")]
                    string GetTimezone(User user)
                    {
                        return user.Settings.Profile.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL001 on each property: "?." is checked the same way as ".".
    /// </summary>
    [Fact]
    public async Task ConditionalNestedRead_NothingRequired_ReportsIncl1OnEachProperty()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    string GetTimezone(User user)
                    {
                        return user?.{|INCL001:Settings|}?.{|INCL001:Profile|}?.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No INCL001: "?." with the whole chain asked for.
    /// </summary>
    [Fact]
    public async Task ConditionalNestedRead_FullPathRequired_NoIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), "Settings.Profile")]
                    string GetTimezone(User user)
                    {
                        return user?.Settings?.Profile?.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// INCL001 on Profile of a local: settings is user.Settings, so the search ends at user missing
    /// Settings.Profile. The read of Settings is covered by Settings.
    /// </summary>
    [Fact]
    public async Task ReadThroughAlias_OnlyPrefixRequired_ReportsIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), nameof(User.Settings))]
                    string GetTimezone(User user)
                    {
                        var settings = user.Settings;
                        return settings.{|INCL001:Profile|}.Timezone;
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// No diagnostic: writes of a tracked property, in an assignment or an object initializer, read nothing,
    /// and the entity reading its own property has no query behind it.
    /// </summary>
    [Fact]
    public async Task WritesAndOwnProperty_NotReads_NoDiagnostic()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    User Reset(User user)
                    {
                        user.Profile = new UserProfile { Timezone = "UTC" };
                        return new User { Profile = new UserProfile(), Settings = new UserSettings() };
                    }
                }

                class Account
                {
                    [TrackIncludeRequired]
                    public UserProfile Profile { get; set; }

                    public string GetTimezone() => Profile.Timezone + this.Profile.Timezone;
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }
}
