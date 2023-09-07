using System.ComponentModel;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Options;

/// <summary>
/// Options for <see cref="INotifyPropertyChanged"/>.
/// </summary>
public class PropertyChangedOptions
{
    /// <summary>
    /// Available raise method names.
    /// </summary>
    public string[] MethodNames { get; } = OptionValues.PropertyChangedNames;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="options">Analyzer options.</param>
    public PropertyChangedOptions(AnalyzerConfigOptions options)
    {
        if (options.TryGetValue(OptionNames.PropertyChangedMethodNames, out var methodNames))
        {
            MethodNames = methodNames.Split(',');
        }
    }
}
