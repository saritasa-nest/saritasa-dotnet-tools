using System.ComponentModel;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.SourceGenerator.Infrastructure;

/// <summary>
/// Options for <see cref="INotifyPropertyChanged"/>.
/// </summary>
public class PropertyChangedOptions
{
    /// <summary>
    /// Available raise method names.
    /// </summary>
    public string[] MethodNames { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="options"></param>
    public PropertyChangedOptions(AnalyzerConfigOptions options)
    {
        options.TryGetValue(Options.PropertyChangedMethodNames, out var methodNames);

        MethodNames = methodNames?.Split(',') ?? Array.Empty<string>();
    }
}
