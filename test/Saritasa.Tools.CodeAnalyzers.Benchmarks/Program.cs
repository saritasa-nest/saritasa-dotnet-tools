using System.CommandLine;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Microsoft.Build.Locator;

namespace Saritasa.Tools.CodeAnalyzers.Benchmarks;

/// <summary>
/// Program class.
/// </summary>
internal class Program
{
    /// <summary>
    /// Entry point.
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        var testProjectPathOption = new Option<string>("--testProjectPath")
        {
            Description = "Path to the test project for code analyzer benchmarks.",
            Required = true
        };

        var rootCommand = new RootCommand();
        rootCommand.Options.Add(testProjectPathOption);
        rootCommand.TreatUnmatchedTokensAsErrors = false;

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count == 0 && parseResult.GetValue(testProjectPathOption) is string testProjectPath)
        {
            CodeAnalyzersBenchmarkSettings.TestProjectPath = testProjectPath;

            MSBuildLocator.RegisterDefaults();

            var config = ManualConfig.CreateEmpty()
                // InProcess is required because MSBuildLocator.RegisterDefaults() registers an
                // assembly resolver in the current AppDomain. The default out-of-process toolchain
                // spawns a child process with a BenchmarkDotNet-generated Main that never calls
                // MSBuildLocator, so MSBuild assemblies cannot be resolved.
                .AddJob(Job.Default
                    .WithToolchain(InProcessEmitToolchain.Instance))
                    .WithOption(ConfigOptions.StopOnFirstError, true)
                .AddLogger(ConsoleLogger.Default)
                    .WithOption(ConfigOptions.DisableLogFile, true)
                .AddDiagnoser(MemoryDiagnoser.Default)
                .AddExporter(new CodeAnalyzersBenchmarkExporter())
                .AddColumnProvider(DefaultColumnProviders.Instance);

            BenchmarkRunner.Run<CodeAnalyzersBenchmarks>(config, parseResult.UnmatchedTokens.ToArray());
            return 0;
        }

        foreach (var parseError in parseResult.Errors)
        {
            await Console.Error.WriteLineAsync(parseError.Message);
        }

        return 1;
    }
}
