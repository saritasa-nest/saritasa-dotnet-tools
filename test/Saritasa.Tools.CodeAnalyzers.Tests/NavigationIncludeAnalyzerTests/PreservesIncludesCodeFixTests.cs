using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.CodeFixes;
using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// Tests for <see cref="PreservesIncludesCodeFixProvider"/>.
/// </summary>
public class PreservesIncludesCodeFixTests : NavigationIncludeTestBase
{
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

            public class PagedResult<T>
            {
                public List<T> Items { get; set; }
            }

            public static class QueryableExtensions
            {
                // Declared already, so the only thing left for the fix to add is the property.
                [PreservesIncludes(nameof(query))]
                public static PagedResult<User> Paginate(this IQueryable<User> query, int page) => null;
            }

            class TestClass(AppDbContext dbContext)
            {
                void Handle()
                {
                    var page = dbContext.Users.Include(u => u.Profile).Paginate(1);
                    var user = page.Items[0];
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
    /// The fix writes the declaration for the member that stopped the search into a new file, the way Visual
    /// Studio keeps suppressions in a file of their own.
    /// </summary>
    [Fact]
    public async Task CannotCheck_OverUnreadableProperty_AddsDeclarationFile()
    {
        const string expectedFile =
            /* lang=c# */
            """
            using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;

            [assembly: PreservesIncludes(typeof(TestApplication.PagedResult<>), "Items")]
            """;

        var test = new CSharpCodeFixTest<NavigationIncludeAnalyzer, PreservesIncludesCodeFixProvider, DefaultVerifier>
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
}
