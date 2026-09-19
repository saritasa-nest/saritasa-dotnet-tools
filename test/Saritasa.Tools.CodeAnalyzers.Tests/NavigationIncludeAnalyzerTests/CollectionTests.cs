using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// Reading entities out of collections: foreach, arrays, async streams and dictionaries.
/// </summary>
public class CollectionTests : NavigationIncludeTestBase
{
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
    /// No INCL002: "foreach" over an array keeps the includes, however the compiler rewrites it.
    /// </summary>
    [Fact]
    public async Task ForEach_OverIncludedArray_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile).ToArrayAsync();
                        foreach (var user in users)
                        {
                            UpdateUserProfile(user, dto);
                        }
            """));
    }

    /// <summary>
    /// No INCL002: an array element comes from the array. Array indexing is not a property in Roslyn, so it has a
    /// rule of its own rather than a declaration.
    /// </summary>
    [Fact]
    public async Task ArrayElement_OverIncludedQuery_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile).ToArrayAsync();
                        var user = users[0];

                        UpdateUserProfile(user, dto);
            """));
    }

    /// <summary>
    /// No INCL002: "await foreach" over an async stream keeps the includes of the query it came from.
    /// IAsyncEnumerable is not an IEnumerable, so it has to be known as a type that holds entities.
    /// </summary>
    [Fact]
    public async Task AwaitForeach_OverIncludedAsyncStream_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = dbContext.Users.Include(u => u.Profile).AsAsyncEnumerable();
                        await foreach (var user in users)
                        {
                            UpdateUserProfile(user, dto);
                        }
            """));
    }

    /// <summary>
    /// INCL002: the same async stream without the include is still reported.
    /// </summary>
    [Fact]
    public async Task AwaitForeach_OverNonIncludedAsyncStream_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = dbContext.Users.AsAsyncEnumerable();
                        await foreach (var user in users)
                        {
                            {|INCL002:UpdateUserProfile(user, dto)|};
                        }
            """));
    }

    /// <summary>
    /// No INCL002: every way of reading an included dictionary: the indexer, TryGetValue, GetValueOrDefault,
    /// Values, pairs and deconstruction.
    /// </summary>
    [Fact]
    public async Task Dictionary_AllReadsOverIncludedQuery_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users.Include(u => u.Profile).ToDictionaryAsync(u => u.Id);
                        var byKey = users[dto.Id];
                        UpdateUserProfile(byKey, dto);

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
    /// INCL002: the dictionary is built from a query without .Include().
    /// </summary>
    [Fact]
    public async Task Dictionary_IndexerOverNonIncludedQuery_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users.ToDictionaryAsync(u => u.Id);
                        var user = users[dto.Id];
                        {|INCL002:UpdateUserProfile(user, dto)|};
            """));
    }

    /// <summary>
    /// INCL002: TryGetValue over a dictionary built from a query without .Include().
    /// </summary>
    [Fact]
    public async Task Dictionary_TryGetValueOverNonIncludedQuery_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var users = await dbContext.Users.ToDictionaryAsync(u => u.Id);
                        if (users.TryGetValue(dto.Id, out var found))
                        {
                            {|INCL002:UpdateUserProfile(found, dto)|};
                        }
            """));
    }
}
