using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Saritasa.Tools.CodeAnalyzers.Analyzers;

namespace Saritasa.Tools.CodeAnalyzers.Benchmarks;

/// <summary>
/// Wraps a DiagnosticAnalyzer to provide a short display name for BenchmarkDotNet.
/// </summary>
public class AnalyzerParam(DiagnosticAnalyzer analyzer)
{
    /// <summary>
    /// Analyzer.
    /// </summary>
    public DiagnosticAnalyzer Analyzer { get; } = analyzer;

    /// <inheritdoc/>
    public override string ToString() => Analyzer.GetType().Name;
}

/// <summary>
/// Benchmarks for Roslyn diagnostic analyzers.
/// Analyzers are discovered dynamically via reflection from Saritasa.Tools.CodeAnalyzers assembly,
/// so no changes to this file are needed when a new analyzer is added.
/// </summary>
public class CodeAnalyzersBenchmarks
{
    private static readonly List<Compilation> compilations = new();

    /// <summary>
    /// Analyzer source.
    /// </summary>
    public static IEnumerable<AnalyzerParam> AnalyzerSource { get; } =
        typeof(LineLengthAnalyzer).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(t))
            .Select(t => new AnalyzerParam((DiagnosticAnalyzer)Activator.CreateInstance(t)!))
            .ToList();

    static CodeAnalyzersBenchmarks()
    {
        using var workspace = MSBuildWorkspace.Create();

        workspace.RegisterWorkspaceFailedHandler(args =>
        {
            Console.WriteLine($"[MSBuild] {args.Diagnostic.Message}");
        });

        var solution = workspace.OpenSolutionAsync(CodeAnalyzersBenchmarkSettings.TestProjectPath).GetAwaiter().GetResult();

        foreach (var project in solution.Projects)
        {
            var compilation = project.GetCompilationAsync().GetAwaiter().GetResult();
            if (compilation == null)
            {
                continue;
            }
            compilations.Add(compilation);
        }
    }

    /// <summary>
    /// Runs a single analyzer against all compiled projects.
    /// Generates one benchmark case per analyzer found in AnalyzerSource.
    /// </summary>
    [Benchmark]
    [ArgumentsSource(nameof(AnalyzerSource))]
    public async Task RunAnalyzer(AnalyzerParam param)
    {
        foreach (var compilation in compilations)
        {
            // A new instance must be created each iteration: CompilationWithAnalyzers caches
            // results internally, so reusing it would measure cache retrieval, not actual analysis.
            var compilationWithAnalyzers = compilation.WithAnalyzers([param.Analyzer]);
            _ = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        }
    }
}
