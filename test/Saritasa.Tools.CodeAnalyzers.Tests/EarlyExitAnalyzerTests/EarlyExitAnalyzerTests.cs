using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Analyzers;

namespace Saritasa.Tools.CodeAnalyzers.Tests.EarlyExitAnalyzerTests;

/// <summary>
/// Tests for <see cref="EarlyExitAnalyzer"/>.
/// </summary>
[TestClass]
public class EarlyExitAnalyzerTests
{
    private readonly CSharpAnalyzerTest<EarlyExitAnalyzer, DefaultVerifier> context;

    /// <summary>
    /// Constructor.
    /// </summary>
    public EarlyExitAnalyzerTests()
    {
        context = new CSharpAnalyzerTest<EarlyExitAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60
        };
    }

    /// <summary>
    /// Validates that case produces warning: else have return statement after if with return.
    /// </summary>
    [TestMethod]
    public async Task IfElse_ElseReturn_ShouldWarn()
    {
        const string sourceCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    string M(bool flag)
                    {
                        if (flag)
                        {
                            return "true";
                        }
                        [|else|]
                        {
                            return "false";
                        }
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that case produces warning: else if have return statement after if with return.
    /// </summary>
    [TestMethod]
    public async Task IfElse_ElseIfReturn_ShouldWarnOnElse()
    {
        const string sourceCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    string M(bool flag, bool other)
                    {
                        if (flag)
                        {
                            return "first";
                        }
                        [|else|] if (other)
                        {
                            return "second";
                        }

                        return "third";
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that case produces warning: else chain have return statement after if with return.
    /// </summary>
    [TestMethod]
    public async Task IfElse_ElseIfAndElseReturn_ShouldWarnOnElse()
    {
        const string sourceCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    string M(bool flag, bool other)
                    {
                        if (flag)
                        {
                            return "first";
                        }
                        [|else|] if (other)
                        {
                            return "second";
                        }
                        [|else|]
                        {
                            return "third";
                        }
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that case does not produce warning: else does not have only return after if with return.
    /// </summary>
    [TestMethod]
    public async Task IfElse_ElseHasNoReturn_ShouldNotWarn()
    {
        const string sourceCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    string M(bool flag)
                    {
                        if (flag)
                        {
                            return "true";
                        }
                        else
                        {
                            System.Console.WriteLine();
                            return "false";
                        }
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that case does not produce warning: else after if without return.
    /// </summary>
    [TestMethod]
    public async Task IfElse_IfDoesNotReturn_ShouldNotWarn()
    {
        const string sourceCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    string M(bool flag)
                    {
                        if (flag)
                        {
                            System.Console.WriteLine();
                        }
                        else
                        {
                            return "false";
                        }
                        return "other";
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }
}
