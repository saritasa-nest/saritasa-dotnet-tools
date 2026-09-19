using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// INCL001: a method that uses a tracked property of its parameter, or passes the parameter on, must declare
/// [IncludeRequired].
/// </summary>
public class IncludeRequiredTests : NavigationIncludeTestBase
{
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
}
