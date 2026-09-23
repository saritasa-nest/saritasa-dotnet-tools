using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.CodeFixes;
using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// Tests for <see cref="PassesIncludesCodeFixProvider"/>.
/// </summary>
public class PassesIncludesCodeFixTests : NavigationIncludeTestBase
{
    private const string ConcatSource =
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

            public class UserProfile
            {
                public string Timezone { get; set; }
            }

            public class User
            {
                public int Id { get; set; }
                [TrackIncludeRequired]
                public UserProfile Profile { get; set; }
            }

            class TestClass(AppDbContext dbContext)
            {
                void Handle()
                {
                    // Concat holds the entities of two collections, so it is left off the built-in list.
                    var loaded = dbContext.Users.Include(u => u.Profile).ToList();
                    var user = loaded.Concat(dbContext.Users.ToList()).First();
                    {|INCL004:UpdateUserProfile(user)|};
                }

                [IncludeRequired(nameof(user), nameof(User.Profile))]
                void UpdateUserProfile(User user)
                {
                    user.Profile.Timezone = "UTC";
                }
            }
        }
        """;

    private const string Source =
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

            public class UserProfile
            {
                public string Timezone { get; set; }
            }

            public class User
            {
                public int Id { get; set; }
                [TrackIncludeRequired]
                public UserProfile Profile { get; set; }
            }

            class TestClass(AppDbContext dbContext)
            {
                void Handle()
                {
                    // Enumerable.Chunk is not in the built-in list, so the search cannot read past it.
                    var batches = dbContext.Users.Include(u => u.Profile).ToList().Chunk(10);
                    var user = batches.First()[0];
                    {|INCL004:UpdateUserProfile(user)|};
                }

                [IncludeRequired(nameof(user), nameof(User.Profile))]
                void UpdateUserProfile(User user)
                {
                    user.Profile.Timezone = "UTC";
                }
            }
        }
        """;

    /// <summary>
    /// The fix writes the declaration for the method that stopped the search into a new file, the way Visual
    /// Studio keeps suppressions in a file of their own.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverUndeclaredMethod_AddsDeclarationFile()
    {
        const string expectedFile =
            /* lang=c# */
            """
            using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

            [assembly: PassesIncludes(typeof(System.Linq.Enumerable), "Chunk", "source")]
            """;

        var test = new CSharpCodeFixTest<NavigationIncludeAnalyzer, PassesIncludesCodeFixProvider, DefaultVerifier>
        {
            ReferenceAssemblies = References,
            TestCode = Source,
            FixedCode = Source,
        };

        test.FixedState.Sources.Add(("NavigationIncludes.cs", expectedFile));

        test.TestState.AdditionalReferences.AddRange(AnalyzerReferences);
        test.FixedState.AdditionalReferences.AddRange(AnalyzerReferences);

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// No fix is offered for a member the built-in list leaves off on purpose. Writing the declaration would
    /// be believed and would answer for the second collection, which nobody looked at.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverMemberLeftOutOnPurpose_OffersNoFix()
    {
        var test = new CSharpCodeFixTest<NavigationIncludeAnalyzer, PassesIncludesCodeFixProvider, DefaultVerifier>
        {
            ReferenceAssemblies = References,
            TestCode = ConcatSource,
            FixedCode = ConcatSource,
        };

        // Nothing is fixed, so the warning is still there afterwards and the markup describes both states.
        test.FixedState.MarkupHandling = MarkupMode.Allow;
        test.TestState.AdditionalReferences.AddRange(AnalyzerReferences);
        test.FixedState.AdditionalReferences.AddRange(AnalyzerReferences);

        await test.RunAsync(CancellationToken.None);
    }
}
