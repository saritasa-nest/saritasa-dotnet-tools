using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Analyzers;
using Saritasa.Tools.CodeAnalyzers.CodeFixProviders;

namespace Saritasa.Tools.CodeAnalyzers.Tests.SpellingAnalyzerTests;

/// <summary>
/// Tests for <see cref="SpellingCodeFixProvider"/>.
/// </summary>
[TestClass]
public class SpellingAnalyzerCodeFixTests
{
    private const string DefaultExclusionsFile = "dictionaries/spell-checker-exclusions.txt";
    private const string CustomExclusionsFile = "custom/project-terms.txt";

    private CSharpCodeFixTest<SpellingAnalyzer, SpellingCodeFixProvider, DefaultVerifier> CreateTest()
        => new()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
            TestCode = CodeWithTypo,
            FixedCode = CodeWithTypoFixed,
        };

    private const string CodeWithTypo =
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

    private const string CodeWithTypoFixed =
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

    /// <summary>
    /// Verifies that a misspelled word is appended to an empty exclusions file.
    /// </summary>
    [TestMethod]
    public async Task AddWord_EmptyExclusionsFile_WordAppended()
    {
        var test = CreateTest();
        test.TestState.AdditionalFiles.Add((DefaultExclusionsFile, string.Empty));
        test.FixedState.AdditionalFiles.Add((DefaultExclusionsFile, "typoo\n"));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a misspelled word is appended when the exclusions file already ends with a newline.
    /// </summary>
    [TestMethod]
    public async Task AddWord_FileEndsWithNewline_WordAppendedWithoutBlankLine()
    {
        var test = CreateTest();
        test.TestState.AdditionalFiles.Add((DefaultExclusionsFile, "existing\n"));
        test.FixedState.AdditionalFiles.Add((DefaultExclusionsFile, "existing\ntypoo\n"));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a newline is inserted before the word when the exclusions file does not end with one.
    /// </summary>
    [TestMethod]
    public async Task AddWord_FileDoesNotEndWithNewline_NewlineInsertedBeforeWord()
    {
        var test = CreateTest();
        test.TestState.AdditionalFiles.Add((DefaultExclusionsFile, "existing"));
        test.FixedState.AdditionalFiles.Add((DefaultExclusionsFile, "existing\ntypoo\n"));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the word is stored in lowercase even when the diagnostic reports it in uppercase.
    /// </summary>
    [TestMethod]
    public async Task AddWord_WordStoredAsLowercase()
    {
        const string codeWithUpperTypo =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "[|Typoo|]";
                    }
                }
            }
            """;

        const string codeWithUpperTypoFixed =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    public void TestMethod()
                    {
                        var test = "Typoo";
                    }
                }
            }
            """;

        var test = new CSharpCodeFixTest<SpellingAnalyzer, SpellingCodeFixProvider, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
            TestCode = codeWithUpperTypo,
            FixedCode = codeWithUpperTypoFixed,
        };
        test.TestState.AdditionalFiles.Add((DefaultExclusionsFile, string.Empty));
        test.FixedState.AdditionalFiles.Add((DefaultExclusionsFile, "typoo\n"));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that CRLF is used when the existing file content uses CRLF.
    /// </summary>
    [TestMethod]
    public async Task AddWord_FileUsesCrlf_AppendedWithCrlf()
    {
        var test = CreateTest();
        test.TestState.AdditionalFiles.Add((DefaultExclusionsFile, "existing\r\n"));
        test.FixedState.AdditionalFiles.Add((DefaultExclusionsFile, "existing\r\ntypoo\r\n"));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that LF is used when the existing file content uses LF.
    /// </summary>
    [TestMethod]
    public async Task AddWord_FileUsesLf_AppendedWithLf()
    {
        var test = CreateTest();
        test.TestState.AdditionalFiles.Add((DefaultExclusionsFile, "existing\n"));
        test.FixedState.AdditionalFiles.Add((DefaultExclusionsFile, "existing\ntypoo\n"));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that LF is used as a fallback when the file content has no recognizable line separator.
    /// </summary>
    [TestMethod]
    public async Task AddWord_FileHasNoLineSeparator_AppendedWithLfFallback()
    {
        var test = CreateTest();
        test.TestState.AdditionalFiles.Add((DefaultExclusionsFile, string.Empty));
        test.FixedState.AdditionalFiles.Add((DefaultExclusionsFile, "typoo\n"));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a word is appended when a similar but distinct word already exists (substring check).
    /// </summary>
    [TestMethod]
    public async Task AddWord_SimilarWordExists_WordStillAppended()
    {
        var test = CreateTest();
        test.TestState.AdditionalFiles.Add((DefaultExclusionsFile, "typoos\n"));
        test.FixedState.AdditionalFiles.Add((DefaultExclusionsFile, "typoos\ntypoo\n"));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that no code fix is offered when no exclusions file is present in AdditionalFiles.
    /// </summary>
    [TestMethod]
    public async Task NoFix_NoExclusionsFileInAdditionalFiles()
    {
        var test = new CSharpCodeFixTest<SpellingAnalyzer, SpellingCodeFixProvider, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
            TestCode = CodeWithTypo,
            FixedCode = CodeWithTypo,
            NumberOfFixAllIterations = 0,
            NumberOfIncrementalIterations = 0,
        };

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the fix targets the exclusions file configured in .editorconfig (exact path match).
    /// </summary>
    [TestMethod]
    public async Task AddWord_CustomExclusionsFileFromEditorconfig_EqualPaths_WordAppended()
    {
        var test = CreateTest();

        const string editorconfig =
            $"""
             is_global = true
             dotnet_diagnostic.STAN1004.exclusions_file = {CustomExclusionsFile}
             """;
        test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", editorconfig));
        test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", editorconfig));

        test.TestState.AdditionalFiles.Add((CustomExclusionsFile, string.Empty));
        test.FixedState.AdditionalFiles.Add((CustomExclusionsFile, "typoo\n"));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the fix targets the exclusions file configured in .editorconfig
    /// when the file is registered under a longer path that ends with the configured relative path.
    /// </summary>
    [TestMethod]
    public async Task AddWord_CustomExclusionsFileFromEditorconfig_DifferentPaths_WordAppended()
    {
        var test = CreateTest();

        const string editorconfig =
            $"""
             is_global = true
             dotnet_diagnostic.STAN1004.exclusions_file = {CustomExclusionsFile}
             """;
        test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", editorconfig));
        test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", editorconfig));

        const string registeredPath = $"dictionaries/{CustomExclusionsFile}";

        test.TestState.AdditionalFiles.Add((registeredPath, string.Empty));
        test.FixedState.AdditionalFiles.Add((registeredPath, "typoo\n"));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that no code fix is offered when the path in .editorconfig does not match any AdditionalFile.
    /// </summary>
    [TestMethod]
    public async Task NoFix_CustomExclusionsFileConfiguredButNotInAdditionalFiles()
    {
        var test = new CSharpCodeFixTest<SpellingAnalyzer, SpellingCodeFixProvider, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
            TestCode = CodeWithTypo,
            FixedCode = CodeWithTypo,
            NumberOfFixAllIterations = 0,
            NumberOfIncrementalIterations = 0,
        };

        const string editorconfig =
            $"""
             is_global = true
             dotnet_diagnostic.STAN1004.exclusions_file = {CustomExclusionsFile}
             """;
        test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", editorconfig));
        test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", editorconfig));

        await test.RunAsync();
    }
}
