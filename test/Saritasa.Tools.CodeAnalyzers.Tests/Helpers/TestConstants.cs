using Microsoft.CodeAnalysis.CSharp.Testing;

namespace Saritasa.Tools.CodeAnalyzers.Tests.Helpers;

/// <summary>
/// Constants for tests.
/// </summary>
public static class TestConstants
{
    /// <summary>
    /// File path for test file.
    /// We might need it instead of <see cref="CSharpAnalyzerTest{TAnalyzer,TVerifier}.TestCode"/> when .editorconfig is involved.
    /// Because .editorconfig and test file must be in the same directory.
    /// </summary>
    public const string TestSourceFilePath = "/src/Test0.cs";

    /// <summary>
    /// File path for .editorconfig.
    /// </summary>
    public const string EditorConfigFilePath = "/src/.editorconfig";
}
