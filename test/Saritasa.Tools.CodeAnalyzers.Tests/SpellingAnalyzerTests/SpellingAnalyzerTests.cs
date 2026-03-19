using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Analyzers;

namespace Saritasa.Tools.CodeAnalyzers.Tests.SpellingAnalyzerTests;

/// <summary>
/// Tests for <see cref="SpellingAnalyzer"/>.
/// </summary>
[TestClass]
public class SpellingAnalyzerTests
{
    private readonly CSharpAnalyzerTest<SpellingAnalyzer, DefaultVerifier> context;

    /// <summary>
    /// Constructor.
    /// </summary>
    public SpellingAnalyzerTests()
    {
        context = new CSharpAnalyzerTest<SpellingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60
        };
    }

    /// <summary>
    /// Verifies single word string literal produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task StringLiteral_SingleWord_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "[|typoo|]";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies single word string literal does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task StringLiteral_SingleWord_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "typo";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies sentence string literal produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task StringLiteral_Sentence_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "It's string literal with a [|typoo|].";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies sentence string literal does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task StringLiteral_Sentence_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "It's string literal without a typo.";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies interpolated string produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task InterpolatedString_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var testText = "Test text";
                        var test = $"It's interpolated string {testText} with a [|typoo|].";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies interpolated string does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task InterpolatedString_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var testText = "Test text";
                        var test = $"It's interpolated string {testText} without a typo.";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies single raw string produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task SingleRawString_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """"
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test ="""It's raw string with a [|typoo|].""";
                    }
                }
            }
            """";

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies single raw string does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task SingleRawString_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """"
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test ="""It's raw string without a typo.""";
                    }
                }
            }
            """";

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies multiline raw string produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task MultilineRawString_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """"
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test =
                            """
                            It's raw string
                            with a [|typoo|].
                            """;
                    }
                }
            }
            """";

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies multiline raw string does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task MultilineRawString_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """"
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test =
                            """
                            It's raw string
                            without a typo.
                            """;
                    }
                }
            }
            """";

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies identifier produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task Identifier_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var [|typoo|] = string.Empty;
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies identifier does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task Identifier_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var typo = string.Empty;
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies single line comment produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task Comment_SingleLine_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        // It's single line comment with a [|typoo|].
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies single line comment does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task Comment_SingleLine_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        // It's single line comment without a typo.
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies apostrophe is handled correctly.
    /// </summary>
    [TestMethod]
    public async Task Comment_WithApostrophe_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        // It's employees' employee's.
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies multi line comment produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task Comment_MultiLine_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        /*
                        It's multi line comment with a [|typoo|].
                        */
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies multi line comment does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task Comment_MultiLine_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        /*
                        It's multi line comment without a typo.
                        */
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies documentation produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task Documentation_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                /// <summary>
                /// It's multi line documentation comment with a [|typoo|].
                /// </summary>
                class TestClass
                {
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies documentation does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task Documentation_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                /// <summary>
                /// It's multi line documentation comment without a typo.
                /// </summary>
                class TestClass
                {
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies documentation comment (/** ... */ form) produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task Documentation_Classic_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                /**
                 * It's multi line documentation (classic) with a [|typoo|].
                 */
                class TestClass
                {
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies documentation (/** ... */ form) does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task Documentation_Classic_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                /**
                 * It's multi line documentation (classic) without a typo.
                 */
                class TestClass
                {
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies class name produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task Class_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClassWith[|Typoo|]
                {
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies class name does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task Class_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClassWithoutTypo
                {
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies method name produces a warning when it contains a typo.
    /// </summary>
    [TestMethod]
    public async Task Method_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethodWithout[|Typoo|]()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies method name does not produce a warning when it is correct.
    /// </summary>
    [TestMethod]
    public async Task Method_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethodWithoutTypo()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies word in exclusions does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task WordInExclusions_ShouldNotProduceWarning()
    {
        context.TestState.AdditionalFiles.Add(("dictionaries/exclusions.txt", "typoo"));
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "typoo";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies word in general exclusions does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task WordInGeneralExclusions_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "linq";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies <see cref="Guid"/> string does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task Guid_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "1b6cdb5b-8449-4d8e-ad3b-6b3dd8f4158d";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies external method call does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task ExternalMethodCall_WithTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        const int testNumber = 1;
                        var squareRoot = Math.Sqrt(testNumber);
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies user method call produces a warning.
    /// </summary>
    [TestMethod]
    public async Task UserMethodCall_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        MethodWith[|Typoo|]();
                    }

                    private void MethodWith[|Typoo|]()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies string with apostrophe does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task StringWithApostrophe_WithoutTypo_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "'test'";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies string with apostrophe produces a warning.
    /// </summary>
    [TestMethod]
    public async Task StringWithApostrophe_WithTypo_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "'[|typoo|]'";
                    }
                }
            }
            """;

        await context.RunAsync();
    }
}
