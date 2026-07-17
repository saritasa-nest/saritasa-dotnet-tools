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
    /// <summary>
    /// INCL001 is reported when a method accesses a [TrackIncludeRequired] property directly without declaring [IncludeRequired].
    /// </summary>
    [TestMethod]
    public async Task DirectPropertyAccess_WithoutIncludeRequired_ReportsIncl001()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                [AttributeUsage(AttributeTargets.Property)]
                public class TrackIncludeRequiredAttribute : Attribute
                {
                    public TrackIncludeRequiredAttribute()
                    {
                    }
                }

                class Organization
                {
                    public string Name { get; set; }
                }

                class UserProfile
                {
                    public string Timezone { get; set; }
                }

                class User
                {
                    public Organization Organization { get; set; }
                    [TrackIncludeRequired]
                    public UserProfile Profile { get; set; }
                }

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
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                [AttributeUsage(AttributeTargets.Method)]
                public class IncludeRequired : Attribute
                {
                    public string Description { get; }
                    public string Name { get; }

                    public IncludeRequired(string description, string Name)
                    {
                        Description = description;
                        Name = Name;
                    }
                }

                [AttributeUsage(AttributeTargets.Property)]
                public class TrackIncludeRequiredAttribute : Attribute
                {
                    public TrackIncludeRequiredAttribute()
                    {
                    }
                }

                class Organization
                {
                    public string Name { get; set; }
                }

                class UserProfile
                {
                    public string Timezone { get; set; }
                }

                class User
                {
                    public Organization Organization { get; set; }
                    [TrackIncludeRequired]
                    public UserProfile Profile { get; set; }
                }

                class TestClass
                {
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

    /// <summary>
    /// INCL001 is reported when a method passes its own parameter to a callee that has [IncludeRequired], but the caller does not declare the same requirement.
    /// </summary>
    [TestMethod]
    public async Task ParameterPropagation_WithoutIncludeRequired_ReportsIncl001()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                [AttributeUsage(AttributeTargets.Method)]
                public class IncludeRequired : Attribute
                {
                    public string Description { get; }
                    public string Name { get; }

                    public IncludeRequired(string description, string Name)
                    {
                        Description = description;
                        Name = Name;
                    }
                }

                [AttributeUsage(AttributeTargets.Property)]
                public class TrackIncludeRequiredAttribute : Attribute
                {
                    public TrackIncludeRequiredAttribute()
                    {
                    }
                }

                class Organization
                {
                    public string Name { get; set; }
                }

                class UserProfile
                {
                    public string Timezone { get; set; }
                }

                class User
                {
                    public Organization Organization { get; set; }
                    [TrackIncludeRequired]
                    public UserProfile Profile { get; set; }
                }

                class SaveUserDto
                {
                  public Organization Organization { get; set; }
                  public string Timezone { get; set; }
                }

                class TestClass
                {
                    // Uncommenting this line will fix the INCL001 warning.
                    // [IncludeRequired(nameof(entity), nameof(@User.Profile))]
                    void UpdateUserProfile(User entity, SaveUserDto dto)
                    {
                        // Does not produce INCL001 because Organization has no TrackIncludeRequired attribute.
                        entity.Organization = dto.Organization;
                        // INCL001: the called method captures the `user` argument and has IncludeRequired attribute.
                        // Use IncludeRequiredAttribute on the current method as well.
                        {|INCL001:SetTimezone(entity, dto.Timezone)|};
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

    /// <summary>
    /// No warning is produced when both the caller and the callee declare [IncludeRequired] for the propagated parameter.
    /// </summary>
    [TestMethod]
    public async Task ParameterPropagation_WithIncludeRequired_NoWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                [AttributeUsage(AttributeTargets.Method)]
                public class IncludeRequired : Attribute
                {
                    public string Description { get; }
                    public string Name { get; }

                    public IncludeRequired(string description, string Name)
                    {
                        Description = description;
                        Name = Name;
                    }
                }

                [AttributeUsage(AttributeTargets.Property)]
                public class TrackIncludeRequiredAttribute : Attribute
                {
                    public TrackIncludeRequiredAttribute()
                    {
                    }
                }

                class Organization
                {
                    public string Name { get; set; }
                }

                class UserProfile
                {
                    public string Timezone { get; set; }
                }

                class User
                {
                    public Organization Organization { get; set; }
                    [TrackIncludeRequired]
                    public UserProfile Profile { get; set; }
                }

                class SaveUserDto
                {
                  public Organization Organization { get; set; }
                  public string Timezone { get; set; }
                }

                class TestClass
                {
                    // Uncommenting this line will fix the INCL001 warning.
                    [IncludeRequired(nameof(entity), nameof(@User.Profile))]
                    void UpdateUserProfile(User entity, SaveUserDto dto)
                    {
                        // Does not produce INCL001 because Organization has no TrackIncludeRequired attribute.
                        entity.Organization = dto.Organization;
                        SetTimezone(entity, dto.Timezone);
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
}
