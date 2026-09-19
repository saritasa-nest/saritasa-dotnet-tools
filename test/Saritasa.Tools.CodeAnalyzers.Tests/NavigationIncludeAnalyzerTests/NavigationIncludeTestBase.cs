using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// Shared code of the <see cref="NavigationIncludeAnalyzer"/> tests: the model every test compiles against and the
/// way to run the analyzer on it.
/// </summary>
public abstract class NavigationIncludeTestBase
{
    /// <summary>
    /// The model every test uses: a context with users, and a user whose Profile is tracked. The namespace is left
    /// open, so a test appends its own classes and closes it.
    /// </summary>
    protected const string Preamble =
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
                public User Manager { get; set; }
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

    /// <summary>
    /// .NET and EF Core, which every test compilation references.
    /// </summary>
    protected static readonly ReferenceAssemblies References = new ReferenceAssemblies(
            "net10.0",
            new PackageIdentity("Microsoft.NETCore.App.Ref", "10.0.0"),
            Path.Combine("ref", "net10.0")
        )
        .AddPackages([new PackageIdentity("Microsoft.EntityFrameworkCore", "10.0.11")]);

    /// <summary>
    /// The analyzer and its attributes, which every test compilation references.
    /// </summary>
    protected static MetadataReference[] AnalyzerReferences =>
    [
        MetadataReference.CreateFromFile(typeof(NavigationIncludeAnalyzer).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(TrackIncludeRequiredAttribute).Assembly.Location),
    ];

    /// <summary>
    /// Runs the analyzer on the source and checks the diagnostics marked in it.
    /// </summary>
    /// <param name="source">Source with diagnostic markup.</param>
    /// <returns>Task.</returns>
    protected static async Task VerifyAnalyzerAsync(string source)
    {
        var test = new CSharpAnalyzerTest<NavigationIncludeAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = References,
        };

        foreach (var reference in AnalyzerReferences)
        {
            test.TestState.AdditionalReferences.Add(reference);
        }

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Wraps the body of a Handle method that calls UpdateUserProfile.
    /// </summary>
    /// <param name="handleBody">Statements of the Handle method.</param>
    /// <returns>Source.</returns>
    protected static string HandleSource(string handleBody) => Preamble +
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
    /// The preamble with assembly attributes in front of it. Assembly attributes have to stand before the namespace,
    /// so they cannot be appended like the rest of a test.
    /// </summary>
    /// <param name="attributes">Assembly attributes, one per line.</param>
    /// <returns>Preamble.</returns>
    protected static string PreambleWithAssemblyAttributes(string attributes)
        => Preamble.Replace(
            "namespace TestApplication",
            attributes + Environment.NewLine + Environment.NewLine + "namespace TestApplication");
}
