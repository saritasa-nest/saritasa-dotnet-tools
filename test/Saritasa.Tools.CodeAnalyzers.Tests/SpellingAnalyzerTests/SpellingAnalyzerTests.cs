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

        var repoRoot = FindRepoRoot(AppContext.BaseDirectory);
        var wordsDir = Path.Combine(repoRoot, "words");

        AddAdditionalFileFromDisk(Path.Combine(wordsDir, "en.dic"), "words/en.dic");
        AddAdditionalFileFromDisk(Path.Combine(wordsDir, "en.aff"), "words/en.aff");
    }

    private static string FindRepoRoot(string startDirectory)
    {
        var directoryInfo = new DirectoryInfo(startDirectory);
        while (directoryInfo != null)
        {
            if (Directory.Exists(Path.Combine(directoryInfo.FullName, "words")))
            {
                return directoryInfo.FullName;
            }

            directoryInfo = directoryInfo.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate repo root containing 'words' folder starting from '{startDirectory}'.");
    }

    private void AddAdditionalFileFromDisk(string physicalPath, string additionalFilePath)
    {
        if (!File.Exists(physicalPath))
        {
            throw new FileNotFoundException(
                $"Required dictionary file was not found: '{physicalPath}'.", physicalPath);
        }

        context.TestState.AdditionalFiles.Add((additionalFilePath, File.ReadAllText(physicalPath)));
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
        context.TestState.AdditionalFiles.Add(("words/exclusions.txt", "typoo"));
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
}
