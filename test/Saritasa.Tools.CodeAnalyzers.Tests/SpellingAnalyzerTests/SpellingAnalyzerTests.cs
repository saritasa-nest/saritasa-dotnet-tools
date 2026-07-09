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
                    /// <summary>
                    /// Example of use case:
                    /// Environments.
                    /// </summary>
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
    /// Verifies word in user exclusions does not produce a warning when using the default file is used.
    /// </summary>
    [TestMethod]
    public async Task WordInUserExclusions_DefaultFile_ShouldNotProduceWarning()
    {
        context.TestState.AdditionalFiles.Add(("dictionaries/spell-checker-exclusions.txt", "typoo"));

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
    /// Verifies word in user exclusions does not produce a warning when the same paths are used for file in .editorconfig.
    /// </summary>
    [TestMethod]
    public async Task WordInUserExclusions_FileFromEditorconfig_EqualPaths_ShouldNotProduceWarning()
    {
        const string userExclusionsPath = "custom/my-exclusions.txt";
        context.TestState.AdditionalFiles.Add((userExclusionsPath, "typoo"));

        const string editorconfig =
            $"""
            is_global = true
            dotnet_diagnostic.STAN1004.exclusions_file = {userExclusionsPath}
            """;
        context.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", editorconfig));

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
    /// Verifies word in user exclusions does not produce a warning when the different paths are used for file in .editorconfig.
    /// </summary>
    [TestMethod]
    public async Task WordInUserExclusions_FileFromEditorconfig_DifferentPaths_ShouldNotProduceWarning()
    {
        const string userExclusionsPath = "custom/my-exclusions.txt";
        context.TestState.AdditionalFiles.Add(($"dictionaries/{userExclusionsPath}", "typoo"));

        const string editorconfig =
            $"""
            is_global = true
            dotnet_diagnostic.STAN1004.exclusions_file = {userExclusionsPath}
            """;
        context.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", editorconfig));

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
    /// Verifies word in user exclusions does not produce a warning
    /// when inconsistent separators are used for paths.
    /// </summary>
    [TestMethod]
    public async Task WordInUserExclusions_FileFromEditorconfig_InconsistentSeparators_ShouldNotProduceWarning()
    {
        context.TestState.AdditionalFiles.Add(("custom/my-exclusions.txt", "typoo"));

        const string editorconfig =
            """
            is_global = true
            dotnet_diagnostic.STAN1004.exclusions_file = custom\my-exclusions.txt
            """;
        context.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", editorconfig));

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
    public async Task Guid_String_ShouldNotProduceWarning()
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
                        var test = "This GUID 1b6cdb5b-8449-4d8e-ad3b-6b3dd8f4158d should not be spellchecked";
                        var guid = new Guid("1b6cdb5b-8449-4d8e-ad3b-6b3dd8f4158d");
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies <see cref="Guid"/> comment does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task Guid_Comment_ShouldNotProduceWarning()
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
                        // This GUID 1b6cdb5b-8449-4d8e-ad3b-6b3dd8f4158d should not be spellchecked
                        // This GUID [1b6cdb5b-8449-4d8e-ad3b-6b3dd8f4158d] should not be spellchecked
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies external method call is excluded from analysis.
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
    /// Verifies word without typo but with possession apostrophe does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task Comment_WithApostrophe_WithoutTypo_ShouldNotProduceWarning()
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
    /// Verifies string with typo and apostrophe produces a warning.
    /// </summary>
    [TestMethod]
    public async Task String_WithApostrophe_WithTypo_ShouldProduceWarning()
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

    /// <summary>
    /// Verifies string without typo but with apostrophe does not produce a warning.
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
    /// Verifies URL in sentence is excluded from analysis.
    /// </summary>
    [TestMethod]
    public async Task String_WithUrl_ShouldNotProduceWarning()
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
                        var test = "Visit https://fghfgh.com/bnm/vbn for more information.";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies URL in comment is excluded from analysis.
    /// </summary>
    [TestMethod]
    public async Task Comment_WithUrl_ShouldNotProduceWarning()
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
                        // See more details at https://asdasd.com/qwe/rty?foo=bar
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies URL in documentation is excluded from analysis.
    /// </summary>
    [TestMethod]
    public async Task Documentation_WithUrl_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                /// <summary>
                /// See more details at [https://zxczxc.com/dfg/hjk]
                /// See more details at https://zxczxc.com/dfg/hjk
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
    /// Verifies format string is excluded from analysis.
    /// </summary>
    [TestMethod]
    public async Task Comment_Format_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    // yyyy-MM-dd
                    // yyyy-MM-dd_HH-mm-ss
                    // HH:mm:ss
                    // dd/MM/yyyy
                    // yyyyMMddHHmmss
                    // yyyy-MM-dd_HH-mm-ss_fff
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies file path is excluded from analysis.
    /// </summary>
    [TestMethod]
    public async Task Comment_FilePath_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    // C:\Program Files\MyApp\asd.exe
                    // D:\Work\project\asd.json
                    // \\Server\Shared\folder\asd.txt
                    // /home/user/project/asd.cs
                    // ./relative/path/asd.txt
                    // ../up/one/level/asd.yaml
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies that file path typo after produces a warning.
    /// </summary>
    [TestMethod]
    public async Task Comment_FilePath_TypoAfterFilePath_ShouldProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    // See C:\Users\foo\bar.txt [|typoo|]
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies hex is excluded from analysis.
    /// </summary>
    [TestMethod]
    public async Task Comment_Hex_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            using System;

            namespace TestApplication
            {
                class TestClass
                {
                    // 0xFF
                    // 0x1A3B
                    // #FFAABB
                    // #fff
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies that name in camelCase does not produce a warning.
    /// </summary>
    [TestMethod]
    public async Task Name_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                /// <summary>
                /// MediatRModule.
                /// </summary>
                class MediatRModule
                {
                    public void MediatRBehavior()
                    {
                        // MediatRRequest
                        var mediatRRequest = "MediatRRequest";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies that name in regular words does not produce a warning.
    /// We have name "Suse" and "devopsUsers" has it as "sUse". We check that in this case we do not mask the name.
    /// </summary>
    [TestMethod]
    public async Task Name_ContainedInRegularWord_ShouldNotProduceWarning()
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
                        var devopsUsers = "";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies that name with different first letter case does not produce a warning.
    /// </summary>
    /// <remarks>
    /// We have name "Hangfire" in the dictionary, so when we use "hangfire" in identifier it should not produce warning.
    /// </remarks>
    [TestMethod]
    public async Task Name_WithDifferentFirstLetterCase_ShouldNotProduceWarning()
    {
        context.TestCode =
            /* lang=c# */
            """
            namespace TestApplication
            {
                class TestClass
                {
                    /// <summary>
                    /// hangfire.
                    /// </summary>
                    public void TestMethod()
                    {
                        // hangfire
                        var hangfire = "hangfire";
                    }
                }
            }
            """;

        await context.RunAsync();
    }

    /// <summary>
    /// Verifies that word stored in dictionary with upper-case first letter does not produce warnings
    /// when used with lower-case first letter.
    /// </summary>
    /// <remarks>
    /// Word "Monday" stored with upper-case first letter.
    /// When split from "mondayDate", the segment "monday" must still pass the check.
    /// </remarks>
    [TestMethod]
    public async Task Identifier_DictionaryWordStartWithUpperCase_ShouldNotProduceWarning()
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
                        var mondayDate = System.DateTime.Now;
                        var januaryReport = System.DateTime.Now;
                    }
                }
            }
            """;

        await context.RunAsync();
    }
}
