using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.NavigationIncludeAnalyzerTests;

/// <summary>
/// Try, catch and finally. The graph has no jumps for exceptions, so a handler can be entered from any place
/// inside the try block.
/// </summary>
public class ExceptionFlowTests : NavigationIncludeTestBase
{
    /// <summary>
    /// INCL002: inside a catch block the collection may hold the value from before the try
    /// or from the try; neither query includes Profile.
    /// </summary>
    [Fact]
    public async Task TryCatch_ForEachInCatchOverNonIncludedCollection_ReportsIncl2()
    {
        var sourceCode = Preamble +
            /* lang=c# */
            """

                class TestClass(AppDbContext dbContext)
                {
                    async Task Handle(SaveUserDto dto)
                    {
                        var users = await dbContext.Users
                            .Where(u => u.Id == dto.Id)
                            .ToListAsync();

                        try
                        {
                            users = await dbContext.Users
                                .Where(u => u.Id == dto.Id)
                                .ToListAsync();
                        }
                        catch (Exception)
                        {
                            foreach (var user in users)
                            {
                                {|INCL002:UpdateUserProfile(user, dto)|};
                            }
                        }

                        await dbContext.SaveChangesAsync();
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
    /// No INCL002: loaded before the try block and not changed in it.
    /// </summary>
    [Fact]
    public async Task TryCatch_IncludedBeforeTry_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        try
                        {
                            await dbContext.SaveChangesAsync();
                        }
                        catch (Exception)
                        {
                            UpdateUserProfile(user, dto);
                        }
            """));
    }

    /// <summary>
    /// INCL002: loaded before the try block, but the try block may reassign it without .Include() before throwing.
    /// </summary>
    [Fact]
    public async Task TryCatch_ReassignedInTryWithoutInclude_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        try
                        {
                            user = await dbContext.Users.FirstAsync();
                            await dbContext.SaveChangesAsync();
                        }
                        catch (Exception)
                        {
                            {|INCL002:UpdateUserProfile(user, dto)|};
                        }
            """));
    }

    /// <summary>
    /// INCL002: the try block loads the property, but an exception can happen before the assignment.
    /// </summary>
    [Fact]
    public async Task TryCatch_IncludedOnlyInTry_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.FirstAsync();
                        try
                        {
                            user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        }
                        catch (Exception)
                        {
                            {|INCL002:UpdateUserProfile(user, dto)|};
                        }
            """));
    }

    /// <summary>
    /// No INCL002: an exception filter ("catch when") gets the same values as a catch block.
    /// </summary>
    [Fact]
    public async Task TryCatch_FilteredCatchIncludedBeforeTry_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        try
                        {
                            user = await dbContext.Users.Include(u => u.Profile).LastAsync();
                        }
                        catch (Exception exception) when (exception is InvalidOperationException)
                        {
                            UpdateUserProfile(user, dto);
                        }
            """));
    }

    /// <summary>
    /// No INCL002: every value the finally block can see loads the property.
    /// </summary>
    [Fact]
    public async Task TryFinally_IncludedBeforeAndInTry_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        try
                        {
                            user = await dbContext.Users.Include(u => u.Profile).LastAsync();
                        }
                        finally
                        {
                            UpdateUserProfile(user, dto);
                        }
            """));
    }

    /// <summary>
    /// INCL002: the finally block runs after the catch block, which reassigns the value without .Include().
    /// </summary>
    [Fact]
    public async Task TryCatchFinally_ReassignedInCatch_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        try
                        {
                            await dbContext.SaveChangesAsync();
                        }
                        catch (Exception)
                        {
                            user = await dbContext.Users.FirstAsync();
                        }
                        finally
                        {
                            {|INCL002:UpdateUserProfile(user, dto)|};
                        }
            """));
    }

    /// <summary>
    /// No INCL002: a try/catch inside a loop; the search must stop when it comes back around the loop.
    /// </summary>
    [Fact]
    public async Task TryCatch_InsideLoop_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        for (var i = 0; i < 3; i++)
                        {
                            try
                            {
                                await dbContext.SaveChangesAsync();
                            }
                            catch (Exception)
                            {
                                UpdateUserProfile(user, dto);
                                user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                            }
                        }
            """));
    }

    /// <summary>
    /// INCL002: nested try blocks; the outer catch can see the value assigned in the inner catch.
    /// </summary>
    [Fact]
    public async Task TryCatch_NestedReassignedInInnerCatch_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        try
                        {
                            try
                            {
                                await dbContext.SaveChangesAsync();
                            }
                            catch (InvalidOperationException)
                            {
                                user = await dbContext.Users.FirstAsync();
                                throw;
                            }
                        }
                        catch (Exception)
                        {
                            {|INCL002:UpdateUserProfile(user, dto)|};
                        }
            """));
    }

    /// <summary>
    /// INCL002: the catch block starts with a loop, so its first block has a jump from the end of the loop;
    /// the try block must still be searched.
    /// </summary>
    [Fact]
    public async Task TryCatch_LoopAtCatchStartReassignedInTry_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        try
                        {
                            user = await dbContext.Users.FirstAsync();
                            await dbContext.SaveChangesAsync();
                        }
                        catch
                        {
                            while (dto.Id > 0)
                            {
                                {|INCL002:UpdateUserProfile(user, dto)|};
                            }
                        }
            """));
    }

    /// <summary>
    /// INCL002: the finally block starts with a loop, so its first block has a jump from the end of the loop;
    /// the try block must still be searched.
    /// </summary>
    [Fact]
    public async Task TryFinally_LoopAtFinallyStartReassignedInTry_ReportsIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        try
                        {
                            user = await dbContext.Users.FirstAsync();
                        }
                        finally
                        {
                            do
                            {
                                {|INCL002:UpdateUserProfile(user, dto)|};
                            }
                            while (dto.Id > 0);
                        }
            """));
    }

    /// <summary>
    /// No INCL002: the catch block starts with a loop; loaded before the try block and not changed in it.
    /// </summary>
    [Fact]
    public async Task TryCatch_LoopAtCatchStartIncludedBeforeTry_NoIncl2()
    {
        await VerifyAnalyzerAsync(HandleSource(
            """
                        var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
                        try
                        {
                            await dbContext.SaveChangesAsync();
                        }
                        catch
                        {
                            while (dto.Id > 0)
                            {
                                UpdateUserProfile(user, dto);
                            }
                        }
            """));
    }
}
