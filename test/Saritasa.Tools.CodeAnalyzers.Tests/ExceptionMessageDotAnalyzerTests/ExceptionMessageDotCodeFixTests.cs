using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Analyzers;
using Saritasa.Tools.CodeAnalyzers.CodeFixProviders;

namespace Saritasa.Tools.CodeAnalyzers.Tests.ExceptionMessageDotAnalyzerTests;

/// <summary>
/// Tests for <see cref="ExceptionMessageDotCodeFixProvider"/>.
/// </summary>
[TestClass]
public class ExceptionMessageDotCodeFixTests
{
    private CSharpCodeFixTest<ExceptionMessageDotAnalyzer, ExceptionMessageDotCodeFixProvider, DefaultVerifier> CreateTest(
        string sourceCode,
        string fixedCode)
    {
        return new CSharpCodeFixTest<ExceptionMessageDotAnalyzer, ExceptionMessageDotCodeFixProvider, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
            TestCode = sourceCode,
            FixedCode = fixedCode,
        };
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to a simple string literal exception message.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_StringLiteral_AppendsDot()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

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

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentException("This is an error message without a dot.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix inserts a dot before trailing whitespace in the exception message.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_StringLiteral_TrailingWhitespace_InsertsDotBeforeWhitespace()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentException([|"Error "|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentException("Error. ");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to a raw string literal exception message.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_RawStringLiteral_AppendsDot()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """"
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentException([|"""Error without dot"""|]);
                    }
                }
            }
            """";

        const string fixedCode =
            /* lang=c# */
            """"
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentException("""Error without dot.""");
                    }
                }
            }
            """";

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to an interpolated string exception message.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_InterpolatedString_AppendsDot()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        const string test = "test";
                        throw new ArgumentException([|$"Error: {test}"|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        const string test = "test";
                        throw new ArgumentException($"Error: {test}.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to the last text part of an interpolated string.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_InterpolatedString_EndsWithText_AppendsDotToText()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        const string test = "test";
                        throw new ArgumentException([|$"{test} error"|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        const string test = "test";
                        throw new ArgumentException($"{test} error.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot when an interpolated string ends with an interpolation.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_InterpolatedString_EndsWithInterpolation_AppendsDot()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

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

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        const string test = "test";
                        const string test2 = "test2";
                        throw new ArgumentException($"{test} {test2}.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to the format string in a string.Format call.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_StringFormat_AppendsDotToFormatString()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(int id)
                    {
                        throw new ArgumentException([|string.Format("Error {0}", id)|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(int id)
                    {
                        throw new ArgumentException(string.Format("Error {0}.", id));
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to both branches of a ternary operator.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_TernaryOperator_AppendsDotToBothBranches()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(bool isValid)
                    {
                        throw new ArgumentException([|isValid ? "Valid" : "Invalid"|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(bool isValid)
                    {
                        throw new ArgumentException(isValid ? "Valid." : "Invalid.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot only to the branch missing a dot in a ternary operator.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_TernaryOperator_MixedDot_AppendsDotOnlyToMissingBranch()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(bool isValid)
                    {
                        throw new ArgumentException([|isValid ? "Valid." : "Invalid"|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(bool isValid)
                    {
                        throw new ArgumentException(isValid ? "Valid." : "Invalid.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot only to the branch missing a dot in a reversed mixed ternary operator.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_TernaryOperator_MixedDotReversed_AppendsDotOnlyToMissingBranch()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(bool isValid)
                    {
                        throw new ArgumentException([|isValid ? "Invalid" : "Valid."|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(bool isValid)
                    {
                        throw new ArgumentException(isValid ? "Invalid." : "Valid.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to the right side of a null coalescing operator.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_NullCoalescing_AppendsDotToRightSide()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(string message)
                    {
                        throw new ArgumentException([|message ?? "Default error"|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(string message)
                    {
                        throw new ArgumentException(message ?? "Default error.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to all arms of a switch expression.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_SwitchExpression_AppendsDotToAllArms()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(int code)
                    {
                        throw new ArgumentException([|code switch
                        {
                            1 => "One",
                            _ => "Two"
                        }|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(int code)
                    {
                        throw new ArgumentException(code switch
                        {
                            1 => "One.",
                            _ => "Two."
                        });
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot only to the arm missing a dot in a switch expression.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_SwitchExpression_MixedDot_AppendsDotOnlyToMissingArm()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(int code)
                    {
                        throw new ArgumentException([|code switch
                        {
                            1 => "One.",
                            _ => "Two"
                        }|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(int code)
                    {
                        throw new ArgumentException(code switch
                        {
                            1 => "One.",
                            _ => "Two."
                        });
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot only to the arm missing a dot in a reversed mixed switch expression.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_SwitchExpression_MixedDotReversed_AppendsDotOnlyToMissingArm()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(int code)
                    {
                        throw new ArgumentException([|code switch
                        {
                            1 => "One",
                            _ => "Two."
                        }|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod(int code)
                    {
                        throw new ArgumentException(code switch
                        {
                            1 => "One.",
                            _ => "Two."
                        });
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to the rightmost string literal in a binary concatenation.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_BinaryOperation_AppendsDotToRightmostLiteral()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var error = "Error";
                        throw new ArgumentException([|error + " test"|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var error = "Error";
                        throw new ArgumentException(error + " test.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to the rightmost literal in a nested binary concatenation.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_NestedBinaryOperation_AppendsDotToRightmostLiteral()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var error = "Error";
                        throw new ArgumentException([|error + " detail" + " info"|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var error = "Error";
                        throw new ArgumentException(error + " detail" + " info.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot in a base constructor call.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_BaseConstructor_AppendsDot()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestException : Exception
                {
                    public TestException() : base([|"Error without dot"|])
                    {
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestException : Exception
                {
                    public TestException() : base("Error without dot.")
                    {
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to the message in a multi-argument exception constructor.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_MultiArgConstructor_AppendsDot()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentException([|"Error"|], new Exception("Inner error."));
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentException("Error.", new Exception("Inner error."));
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the code fix appends a dot to the message in an exception constructor with paramName and message.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_ParamNameAndMessage_AppendsDot()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentNullException("paramName", [|"Error without dot"|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        throw new ArgumentNullException("paramName", "Error without dot.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a base constructor call with both a message and an inner exception appends a dot to the message.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_BaseConstructorMultiArg_AppendsDotToMessage()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestException : Exception
                {
                    public TestException(Exception inner) : base([|"Error without dot"|], inner)
                    {
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestException : Exception
                {
                    public TestException(Exception inner) : base("Error without dot.", inner)
                    {
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that fix all applies the code fix to every diagnostic in the document at once.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_FixAll_MultipleDiagnosticsInDocument_AppendsDotToAll()
    {
        // Arrange
        const string sourceCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void FirstMethod()
                    {
                        throw new ArgumentException([|"First error without dot"|]);
                    }

                    public void SecondMethod()
                    {
                        throw new ArgumentException([|"Second error without dot"|]);
                    }
                }
            }
            """;

        const string fixedCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void FirstMethod()
                    {
                        throw new ArgumentException("First error without dot.");
                    }

                    public void SecondMethod()
                    {
                        throw new ArgumentException("Second error without dot.");
                    }
                }
            }
            """;

        // Act
        var test = CreateTest(sourceCode, fixedCode);

        // Assert
        await test.RunAsync();
    }
}
