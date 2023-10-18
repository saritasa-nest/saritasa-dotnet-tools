using System.ComponentModel;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Options;

/// <summary>
/// Options for <see cref="INotifyPropertyChanging"/>.
/// </summary>
public class PropertyChangingOptions
{
    /// <summary>
    /// Available raise method names.
    /// </summary>
    public string[] MethodNames { get; } = OptionValues.PropertyChangingNames;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="options">Analyzer options.</param>
    public PropertyChangingOptions(AnalyzerConfigOptions options)
    {
        if (options.TryGetValue(OptionNames.PropertyChangingMethodNames, out var methodNames))
        {
            MethodNames = methodNames.Split(',');
        }
    }
}
