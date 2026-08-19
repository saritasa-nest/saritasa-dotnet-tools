using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Saritasa.Tools.CodeAnalyzers.Analyzers;
using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.EarlyExitAnalyzerTests;

/// <summary>
/// Tests for <see cref="EarlyExitAnalyzer"/>.
/// </summary>
public class EarlyExitAnalyzerTests
{
    private static async Task VerifyAnalyzerAsync(string source)
    {
        var test = new CSharpAnalyzerTest<EarlyExitAnalyzer, DefaultVerifier>
        {
            TestCode = source,
        };
        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Validates that case produces warning: else have return statement after if with return.
    /// </summary>
    [Fact]
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

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Validates that case produces warning: else if have return statement after if with return.
    /// </summary>
    [Fact]
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

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Validates that case produces warning: else chain have return statement after if with return.
    /// </summary>
    [Fact]
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
        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Validates that case does not produce warning: else does not have only return after if with return.
    /// </summary>
    [Fact]
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

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Validates that case does not produce warning: else after if without return.
    /// </summary>
    [Fact]
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

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Validates that case produces warning: else have return statement after if that throws.
    /// </summary>
    [Fact]
    public async Task IfElse_IfThrows_ShouldWarn()
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
                            throw new System.Exception();
                        }
                        [|else|]
                        {
                            return "false";
                        }
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Validates that case produces warning: else throws after if with return.
    /// </summary>
    [Fact]
    public async Task IfElse_ElseThrows_ShouldWarn()
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
                            throw new System.Exception();
                        }
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Validates that case produces warning: both if and else throw.
    /// </summary>
    [Fact]
    public async Task IfElse_BothThrow_ShouldWarn()
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
                            throw new System.ArgumentException();
                        }
                        [|else|]
                        {
                            throw new System.InvalidOperationException();
                        }
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }
}
