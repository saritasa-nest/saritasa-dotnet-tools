using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.SourceGenerator.Infrastructure;

/// <summary>
/// Application options manager.
/// </summary>
public class OptionsManager
{
    /// <summary>
    /// Property changed options.
    /// </summary>
    public PropertyChangedOptions PropertyChangedOptions { get; }

    /// <summary>
    /// Property changing options.
    /// </summary>
    public PropertyChangingOptions PropertyChangingOptions { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="optionsProvider">Options provider.</param>
    /// <param name="syntaxTree">Syntax tree.</param>
    public OptionsManager(AnalyzerConfigOptions options)
    {
        PropertyChangedOptions = new(options);
        PropertyChangingOptions = new(options);
    }
}
