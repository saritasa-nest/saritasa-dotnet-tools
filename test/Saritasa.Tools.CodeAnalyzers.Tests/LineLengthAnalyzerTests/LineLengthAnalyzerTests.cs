using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Analyzers;

namespace Saritasa.Tools.CodeAnalyzers.Tests.LineLengthAnalyzerTests;

/// <summary>
/// Tests for <see cref="LineLengthAnalyzer"/>.
/// </summary>
[TestClass]
public class LineLengthAnalyzerTests
{
    private readonly CSharpAnalyzerTest<LineLengthAnalyzer, DefaultVerifier> context;

    /// <summary>
    /// Constructor.
    /// </summary>
    public LineLengthAnalyzerTests()
    {
        context = new CSharpAnalyzerTest<LineLengthAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60
        };
    }

    /// <summary>
    /// Validates that long line produces warning.
    /// </summary>
    [TestMethod]
    public async Task Line_IsTooLong_ShouldProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
            [|  class ClassWithVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongName|]
                {}
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that short line does not produce warning.
    /// </summary>
    [TestMethod]
    public async Task Line_Short_ShouldNotProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
                class ClassWithShortName
                {}
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }
}
