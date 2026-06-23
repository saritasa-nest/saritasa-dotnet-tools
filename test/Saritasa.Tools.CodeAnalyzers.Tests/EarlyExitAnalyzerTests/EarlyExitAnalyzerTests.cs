using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Analyzers;
using VerifyCS = Saritasa.Tools.CodeAnalyzers.Tests.Verifiers.CSharpAnalyzerVerifier<
    Saritasa.Tools.CodeAnalyzers.Analyzers.EarlyExitAnalyzer>;

namespace Saritasa.Tools.CodeAnalyzers.Tests.EarlyExitAnalyzerTests;

/// <summary>
/// Tests for <see cref="EarlyExitAnalyzer"/>.
/// </summary>
[TestClass]
public class EarlyExitAnalyzerTests
{
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
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
        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Validates that case produces warning: else have return statement after if that throws.
    /// </summary>
    [TestMethod]
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Validates that case produces warning: else throws after if with return.
    /// </summary>
    [TestMethod]
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Validates that case produces warning: both if and else throw.
    /// </summary>
    [TestMethod]
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }
}
