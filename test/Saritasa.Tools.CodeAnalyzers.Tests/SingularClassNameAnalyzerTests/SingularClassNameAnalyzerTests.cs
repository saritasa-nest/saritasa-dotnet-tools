using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Analyzers;

namespace Saritasa.Tools.CodeAnalyzers.Tests.SingularClassNameAnalyzerTests;

/// <summary>
/// Tests for <see cref="SingularClassNameAnalyzer"/>.
/// </summary>
[TestClass]
public class SingularClassNameAnalyzerTests
{
    private readonly CSharpAnalyzerTest<SingularClassNameAnalyzer, DefaultVerifier> context;

    /// <summary>
    /// Constructor.
    /// </summary>
    public SingularClassNameAnalyzerTests()
    {
        context = new CSharpAnalyzerTest<SingularClassNameAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60
        };
    }

    /// <summary>
    /// Validates case when class name contains plural word.
    /// </summary>
    [TestMethod]
    public async Task Class_WithPluralWord_ShouldProduceWarning()
    {
        context.TestCode = "class [|UsersController|] { }";

        await context.RunAsync();
    }

    /// <summary>
    /// Validates case when class name contains multiple words including plural word.
    /// </summary>
    [TestMethod]
    public async Task Class_MultipleWords_WithPluralWord_ShouldProduceWarning()
    {
        context.TestCode = "class [|ProjectQualifiedSpecificationsController|] { }";

        await context.RunAsync();
    }

    /// <summary>
    /// Validates case when class name starts and ends with plural word.
    /// </summary>
    [TestMethod]
    public async Task Class_WithPluralFirstAndLastWord_ShouldProduceWarning()
    {
        context.TestCode = "class [|ProjectsExtensions|] { }";

        await context.RunAsync();
    }

    /// <summary>
    /// Validates case when class name contains allowed plural word.
    /// </summary>
    [TestMethod]
    public async Task Class_WithAllowedPluralWord_ShouldNotProduceWarning()
    {
        context.TestCode = "class NewsController { }";

        await context.RunAsync();
    }

    /// <summary>
    /// Validates case when class name contains singular word.
    /// </summary>
    [TestMethod]
    public async Task Class_WithSingularWord_ShouldNotProduceWarning()
    {
        context.TestCode = "class UserController { }";

        await context.RunAsync();
    }

    /// <summary>
    /// Validates case when class name contains multiple words without plural ones.
    /// </summary>
    [TestMethod]
    public async Task Class_MultipleWords_WithSingularWord_ShouldNotProduceWarning()
    {
        context.TestCode = "class ProjectQualifiedSpecificationController { }";

        await context.RunAsync();
    }

    /// <summary>
    /// Validates case when class name ends with plural word.
    /// </summary>
    [TestMethod]
    public async Task Class_WithPluralLastWord_ShouldNotProduceWarning()
    {
        context.TestCode = "class IEnumerableExtensions { }";

        await context.RunAsync();
    }

    /// <summary>
    /// Validates case when class name contains single plural word.
    /// </summary>
    [TestMethod]
    public async Task Class_WithPluralSingleWord_ShouldNotProduceWarning()
    {
        context.TestCode = "class Extensions { }";

        await context.RunAsync();
    }
}
