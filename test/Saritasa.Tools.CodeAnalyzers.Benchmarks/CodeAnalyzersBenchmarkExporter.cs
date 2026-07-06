using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Reports;

namespace Saritasa.Tools.CodeAnalyzers.Benchmarks;

/// <summary>
/// A custom JSON exporter that replaces the benchmark method name with the value of its first parameter
/// for a more convenient display of code analyzer test results.
/// </summary>
public class CodeAnalyzersBenchmarkExporter : JsonExporterBase
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public CodeAnalyzersBenchmarkExporter() : base(indentJson: true, excludeMeasurements: true)
    {
    }

    /// <inheritdoc/>
    protected override IReadOnlyDictionary<string, object> GetDataToSerialize(BenchmarkReport report)
    {
        var dict = base.GetDataToSerialize(report);

        var firstParam = report.BenchmarkCase.Parameters.Items.FirstOrDefault();
        var methodName = firstParam?.Value?.ToString() ?? report.BenchmarkCase.Descriptor.WorkloadMethod.Name;

        var copy = dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        copy["Method"] = methodName;
        copy["MethodTitle"] = methodName;

        return copy;
    }
}
