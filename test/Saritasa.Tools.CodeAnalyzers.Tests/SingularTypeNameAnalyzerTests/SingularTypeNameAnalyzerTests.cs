using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Analyzers;

namespace Saritasa.Tools.CodeAnalyzers.Tests.SingularTypeNameAnalyzerTests;

/// <summary>
/// Tests for <see cref="SingularTypeNameAnalyzer"/>.
/// </summary>
[TestClass]
public class SingularTypeNameAnalyzerTests
{
    private readonly CSharpAnalyzerTest<SingularTypeNameAnalyzer, DefaultVerifier> context;

    /// <summary>
    /// Constructor.
    /// </summary>
    public SingularTypeNameAnalyzerTests()
    {
        context = new CSharpAnalyzerTest<SingularTypeNameAnalyzer, DefaultVerifier>
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
    /// Validates case when interface name contains plural word.
    /// </summary>
    [TestMethod]
    public async Task Interface_WithPluralWord_ShouldProduceWarning()
    {
        context.TestCode = "interface [|IUsersController|] { }";

        await context.RunAsync();
    }

    /// <summary>
    /// Validates case when class name contains plural word but does not have keyword.
    /// </summary>
    [TestMethod]
    public async Task Class_WithPluralWord_WithoutKeyword_ShouldNotProduceWarning()
    {
        context.TestCode = "class UsersExtensions { }";

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
    public async Task Class_WithMultiplePluralWords_ShouldProduceWarning()
    {
        context.TestCode = "class [|ProjectsExtensionsController|] { }";

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
    public async Task Class_MultipleSingularWords_ShouldNotProduceWarning()
    {
        context.TestCode = "class ProjectQualifiedSpecificationController { }";

        await context.RunAsync();
    }
}
