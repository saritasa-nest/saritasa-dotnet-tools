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
    public PropertyChangedOptions(AnalyzerConfigOptions options)
    {
        options.TryGetValue("property_changed_raise_method_name", out var methodNames);

        MethodNames = methodNames?.Split(',') ?? Array.Empty<string>();
    }
}
