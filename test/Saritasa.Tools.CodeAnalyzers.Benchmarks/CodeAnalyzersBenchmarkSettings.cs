namespace Saritasa.Tools.CodeAnalyzers.Benchmarks;

/// <summary>
/// Benchmark run settings.
/// </summary>
internal static class CodeAnalyzersBenchmarkSettings
{
    /// <summary>
    /// Path to the test project solution file passed via --testProjectPath argument.
    /// </summary>
    public static string TestProjectPath { get; set; } = string.Empty;
}
