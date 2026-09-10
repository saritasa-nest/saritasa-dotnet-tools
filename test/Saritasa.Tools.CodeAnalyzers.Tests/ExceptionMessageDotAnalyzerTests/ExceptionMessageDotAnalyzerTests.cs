using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Saritasa.Tools.CodeAnalyzers.Analyzers;
using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.ExceptionMessageDotAnalyzerTests;

/// <summary>
/// Tests for <see cref="ExceptionMessageDotAnalyzer"/>.
/// </summary>
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
    [Fact]
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
                    public void Fact()
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
    /// Validates that an exception message with a dot does not produce a warning.
    /// </summary>
    [Fact]
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
                    public void Fact()
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
    /// Validates that an exception message with trailing whitespace after a dot does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_TrailingWhitespaceAfterDot_ShouldNotProduceWarning()
    {
        // Arrange
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
                        throw new ArgumentException("Error. ");
                    }
                }
            }
            """;

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that an empty exception message produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_EmptyString_ShouldProduceWarning()
    {
        // Arrange
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
                        throw new ArgumentException([|""|]);
                    }
                }
            }
            """;

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that local constant without a dot produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_LocalConstant_WithoutDot_ShouldProduceWarning()
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
                    public void Fact()
                    {
                        const string error = "Error";
                        throw new ArgumentException([|error|]);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that local constant with a dot does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_LocalConstant_WithDot_ShouldNotProduceWarning()
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
                    public void Fact()
                    {
                        const string error = "Error.";
                        throw new ArgumentException(error);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that field constant without a dot produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_FieldConstant_WithoutDot_ShouldProduceWarning()
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
                    private const string Error = "Error";

                    public void Fact()
                    {
                        throw new ArgumentException([|Error|]);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that field constant with a dot does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_FieldConstant_WithDot_ShouldNotProduceWarning()
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
                    private const string Error = "Error.";

                    public void Fact()
                    {
                        throw new ArgumentException(Error);
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
    [Fact]
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
                    public void Fact()
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
    /// Validates that an interpolated exception message with a dot does not produce a warning.
    /// </summary>
    [Fact]
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
                    public void Fact()
                    {
                        const string test = "test";
                        const string test2 = "test2.";
                        throw new ArgumentException($"{test} {test2}");
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that ternary operator without dot produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_TernaryOperator_WithoutDot_ShouldProduceWarning()
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
                    public void Fact(bool isValid)
                    {
                        throw new ArgumentException([|isValid ? "Valid" : "Invalid"|]);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that ternary operator with dot does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_TernaryOperator_WithDot_ShouldNotProduceWarning()
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
                    public void Fact(bool isValid)
                    {
                        throw new ArgumentException(isValid ? "Valid." : "Invalid.");
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that a ternary operator with mixed dot presence produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_TernaryOperator_MixedDot_ShouldProduceWarning()
    {
        // Arrange
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
                    public void TestMethod(bool isValid)
                    {
                        throw new ArgumentException([|isValid ? "Valid." : "Invalid"|]);
                    }
                }
            }
            """;

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that a ternary operator with reversed mixed dot presence produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_TernaryOperator_MixedDotReversed_ShouldProduceWarning()
    {
        // Arrange
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
                    public void TestMethod(bool isValid)
                    {
                        throw new ArgumentException([|isValid ? "Invalid" : "Valid."|]);
                    }
                }
            }
            """;

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that null coalescing operator without dot produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_NullCoalescing_WithoutDot_ShouldProduceWarning()
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
                    public void Fact(string message)
                    {
                        throw new ArgumentException([|message ?? "Default error"|]);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that null coalescing operator with dot does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_NullCoalescing_WithDot_ShouldNotProduceWarning()
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
                    public void Fact(string message)
                    {
                        throw new ArgumentException(message ?? "Default error.");
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that switch expression without dot produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_SwitchExpression_WithoutDot_ShouldProduceWarning()
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
                    public void Fact(int code)
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

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that switch expression with dot does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_SwitchExpression_WithDot_ShouldNotProduceWarning()
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
                    public void Fact(int code)
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

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that a switch expression with mixed dot presence produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_SwitchExpression_MixedDot_ShouldProduceWarning()
    {
        // Arrange
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

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that a switch expression with reversed mixed dot presence produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_SwitchExpression_MixedDotReversed_ShouldProduceWarning()
    {
        // Arrange
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

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that a binary operation in exception message without a dot produces a warning.
    /// </summary>
    [Fact]
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
                    public void Fact()
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
    /// Validates that a binary operation in exception message with a dot does not produce a warning.
    /// </summary>
    [Fact]
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
                    public void Fact()
                    {
                        var error = "Error";
                        throw new ArgumentException(error + "test.");
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that nested binary concatenation without a dot produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_NestedBinaryOperation_WithoutDot_ShouldProduceWarning()
    {
        // Arrange
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
                        throw new ArgumentException([|error + " detail" + " info"|]);
                    }
                }
            }
            """;

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that method calls do not produce warnings, because we cannot analyze method results.
    /// </summary>
    [Fact]
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
                    public void Fact()
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
    /// Validates that local variable do not produce warnings, because we cannot analyze which value it contains.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_LocalVariable_ShouldNotProduceWarning()
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
                    public void Fact()
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

    /// <summary>
    /// Validates that parameter does not produce warnings, because we cannot analyze which value it contains.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_MethodParameter_ShouldNotProduceWarning()
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
                    public void Fact(string errorMessage)
                    {
                        throw new ArgumentException(errorMessage);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that field does not produce warnings, because we cannot analyze which value it contains.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_Field_ShouldNotProduceWarning()
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
                    private string errorMessage = "Error";

                    public void Fact()
                    {
                        throw new ArgumentException(errorMessage);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that property do not produce warnings, because we cannot analyze which value it contains.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_Property_ShouldNotProduceWarning()
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
                    public string ErrorMessage { get; set; } = "Error";

                    public void Fact()
                    {
                        throw new ArgumentException(ErrorMessage);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that base constructor call in exception without a dot produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_BaseConstructor_WithoutDot_ShouldProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

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

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that base constructor call in exception with a dot does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_BaseConstructor_WithDot_ShouldNotProduceWarning()
    {
        const string sourceCode =
            /* lang=c# */
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApplication
            {
                class TestException : Exception
                {
                    public TestException() : base("Error with dot.")
                    {
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that a multi-argument exception constructor with message without dot produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_MultiArgConstructor_WithoutDot_ShouldProduceWarning()
    {
        // Arrange
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
                        throw new ArgumentException([|"Error"|], new Exception("Inner error."));
                    }
                }
            }
            """;

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that exception constructor with non-message parameter name does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_NonMessageParameter_ShouldNotProduceWarning()
    {
        // Arrange
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
                        throw new ArgumentNullException("paramName");
                    }
                }
            }
            """;

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that exception constructor with both paramName and message parameters produces a warning when message lacks a dot.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_ParamNameAndMessage_WithoutDot_ShouldProduceWarning()
    {
        // Arrange
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
                        throw new ArgumentNullException("paramName", [|"Error without dot"|]);
                    }
                }
            }
            """;

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that string.Format without dot produces a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_StringFormat_WithoutDot_ShouldProduceWarning()
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
                    public void Fact(int id)
                    {
                        throw new ArgumentException([|string.Format("Error {0}", id)|]);
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that string.Format with dot does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_StringFormat_WithDot_ShouldNotProduceWarning()
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
                    public void Fact(int id)
                    {
                        throw new ArgumentException(string.Format("Error {0}.", id));
                    }
                }
            }
            """;

        context.TestCode = sourceCode;
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that string.Format with non-constant format argument does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_StringFormat_NonConstantFormat_ShouldNotProduceWarning()
    {
        // Arrange
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
                    public void TestMethod(int id, string format)
                    {
                        throw new ArgumentException(string.Format(format, id));
                    }
                }
            }
            """;

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }

    /// <summary>
    /// Validates that a custom Format method with same signature as string.Format does not produce a warning.
    /// </summary>
    [Fact]
    public async Task ExceptionMessage_CustomFormatMethod_ShouldNotProduceWarning()
    {
        // Arrange
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
                    public void TestMethod(int id)
                    {
                        throw new ArgumentException(Formatter.Format("Error {0}", id));
                    }
                }

                static class Formatter
                {
                    public static string Format(string format, object arg0) => format;
                }
            }
            """;

        // Act
        context.TestCode = sourceCode;

        // Assert
        await context.RunAsync();
    }
}
