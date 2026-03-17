using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Analyzers;

namespace Saritasa.Tools.CodeAnalyzers.Tests.ExceptionMessageDotAnalyzerTests;

/// <summary>
/// Tests for <see cref="ExceptionMessageDotAnalyzer"/>.
/// </summary>
[TestClass]
public class ExceptionMessageDotAnalyzerTests
{
    private readonly CSharpAnalyzerTest<ExceptionMessageDotAnalyzer, DefaultVerifier> context;

    /// <summary>
    /// Constructor.
    /// </summary>
    public ExceptionMessageDotAnalyzerTests()
    {
        context = new CSharpAnalyzerTest<ExceptionMessageDotAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60
        };
    }

    /// <summary>
    /// Validates that an exception message without a dot produces a warning.
    /// </summary>
    [TestMethod]
    public async Task ExceptionMessage_WithoutDot_ShouldProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentException([|"This is an error message without a dot"|]);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that an interpolated exception message without a dot produces a warning.
    /// </summary>
    [TestMethod]
    public async Task ExceptionMessage_Interpolated_WithoutDot_ShouldProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        const string test = "test";
                        const string test2 = "test2";
                        throw new ArgumentException([|$"{test} {test2}"|]);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that a binary operation in exception message without a dot produces a warning.
    /// </summary>
    [TestMethod]
    public async Task ExceptionMessage_BinaryOperation_WithoutDot_ShouldProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var error = "Error";
                        throw new ArgumentException([|error + "test"|]);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that an exception message wit a dot does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task ExceptionMessage_WithDot_ShouldNotProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentException("This is an error message with a dot.");
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that an interpolated exception message with a dot does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task ExceptionMessage_Interpolated_WithDot_ShouldNotProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        const string test = "test";
                        const string test2 = ".";
                        throw new ArgumentException($"{test} {test2}");
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that a binary operation in exception message with a dot does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task ExceptionMessage_BinaryOperation_WithDot_ShouldNotProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var error = "Error";
                        throw new ArgumentException(error + ".");
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that methods calls do not produce warnings, because we cannot analyze method results.
    /// </summary>
    [TestMethod]
    public async Task ExceptionMessage_ToString_ShouldNotProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var error = "Error";
                        throw new ArgumentException(error.ToString());
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that identifiers do not produce warnings, because we cannot analyze which value identifier contains.
    /// </summary>
    [TestMethod]
    public async Task ExceptionMessage_Identifier_ShouldNotProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var error = "Error";
                        throw new ArgumentException(error);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }
}
