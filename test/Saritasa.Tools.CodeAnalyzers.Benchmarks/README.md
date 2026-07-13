# Saritasa.Tools.CodeAnalyzers.Benchmarks

A BenchmarkDotNet-based performance benchmarking tool for Roslyn diagnostic analyzers defined in `Saritasa.Tools.CodeAnalyzers`.

## Overview

The tool opens a target .NET solution via MSBuild, compiles each project in it, and measures how long every `DiagnosticAnalyzer` takes to analyze those compilations. Analyzers are discovered automatically through reflection — no changes to benchmark code are required when a new analyzer is added.

Results are exported as indented JSON files into an `--artifacts` directory using a custom exporter that uses the analyzer class name instead of the benchmark method name for readability.

## Arguments

| Argument            | Required | Description                                         |
|---------------------|----------|-----------------------------------------------------|
| `--testProjectPath` | Yes      | Path to the `.sln` file to analyze     |
| `--artifacts`       | No       | Output directory for benchmark result JSON files    |

Additional BenchmarkDotNet arguments (e.g. `--filter`) can be appended after the required ones.

## Running

### Option 1 — Launch profile (VS Code / Visual Studio)

Open [Properties/launchSettings.json](Properties/launchSettings.json) and update `--testProjectPath` to point to the solution you want to benchmark:

```json
"commandLineArgs": "--testProjectPath \"C:\\path\\to\\Your.sln\" --artifacts artifacts"
```

Then start the project using the **Saritasa.Tools.CodeAnalyzers.Benchmarks** launch profile.

### Option 2 — Command line

Run from the project directory (must use `Release` configuration):

```bash
dotnet run -c Release -- --testProjectPath "C:\\path\\to\\Your.sln" --artifacts artifacts
```

> **Note:** The `--` separator is required to distinguish application arguments from `dotnet run` arguments.

## Output

Benchmark results are written as JSON files to the directory specified by `--artifacts`.