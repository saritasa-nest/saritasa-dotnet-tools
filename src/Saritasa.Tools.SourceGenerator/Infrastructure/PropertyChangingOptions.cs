using System.ComponentModel;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.SourceGenerator.Infrastructure;

/// <summary>
/// Options for <see cref="INotifyPropertyChanging"/>.
/// </summary>
public class PropertyChangingOptions
{
    /// <summary>
    /// Available raise method names.
    /// </summary>
    public string[] MethodNames { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public PropertyChangingOptions(AnalyzerConfigOptions options)
    {
        options.TryGetValue(Options.PropertyChangingMethodNames, out var methodNames);

        MethodNames = methodNames?.Split(',') ?? Array.Empty<string>();
    }
}
