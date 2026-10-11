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
                        {|INCL001:user.Profile|}.Timezone = timezone;
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
                        var timezone = {|INCL001:user?.Profile|}.Timezone;
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
    /// No INCL001: the dotted path "Profile.Timezone" covers Profile, both for the direct access and for the call.
    /// </summary>
    [Fact]
    public async Task PassParameter_DottedPathCoversRequirement_NoIncl1()
    {
        const string sourceCode = Preamble +
            /* lang=c# */
            """
                class TestClass
                {
                    [IncludeRequired(nameof(user), "Profile.Timezone")]
                    void Handle(User user, SaveUserDto dto)
                    {
                        var timezone = user.Profile.Timezone;
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
    /// INCL002: the method asks only for Settings of user, which does not cover Settings.Profile.
    /// </summary>
    [Fact]
    public async Task PassNavigationOfParameter_OnlyPrefixRequired_ReportsIncl2()
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
                        {|INCL002:UpdateProfile(settings)|};
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
}
