using Microsoft.CodeAnalysis.Diagnostics;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Diagnostics;
using Saritasa.Tools.PropertyChangedGenerator.Diagnostics;

namespace Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Options;

/// <summary>
/// Application options manager.
/// </summary>
public class OptionsManager
{
    /// <summary>
    /// User indent options.
    /// </summary>
    public IndentOptions IndentOptions { get; }

    /// <summary>
    /// Backing fields options.
    /// </summary>
    public FieldOptions FieldOptions { get; }

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
    /// <param name="options">Syntax analyzer editor config options.</param>
    /// <param name="scope">Diagnostics scope.</param>
    public OptionsManager(AnalyzerConfigOptions options, IDiagnosticsScope scope)
    {
        IndentOptions = new(options);
        FieldOptions = new(options);
        PropertyChangedOptions = new(options);
        PropertyChangingOptions = new(options);

        Validate(FieldOptions, scope);
    }

    private static void Validate(FieldOptions options, IDiagnosticsScope scope)
    {
        if (options.Convention == NamingConvention.PascalCase && !options.UseUnderscore)
        {
            scope.AddDiagnostic(Diagnostics.Diagnostics.BackingFieldConventionMismatch);
        }
    }
}
