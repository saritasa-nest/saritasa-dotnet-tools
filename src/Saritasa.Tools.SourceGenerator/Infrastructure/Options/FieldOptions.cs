using Microsoft.CodeAnalysis.Diagnostics;

namespace Saritasa.Tools.SourceGenerator.Infrastructure.Options;

/// <summary>
/// Options for backing field generation process.
/// </summary>
public class FieldOptions
{
    /// <summary>
    /// Backing field naming convention.
    /// </summary>
    public NamingConvention Convention { get; } = OptionValues.DefaultBackingFieldNamingConvention;

    /// <summary>
    /// Should backing field use underscore at the name beginning. <br/>
    /// Backing field must always use underscore with <see cref="NamingConvention.PascalCase"/> convention.
    /// </summary>
    public bool UseUnderscore { get; } = OptionValues.DefaultBackingFieldShouldUseUnderscore;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="options">Analyzer options.</param>
    public FieldOptions(AnalyzerConfigOptions options)
    {
        if (options.TryGetValue(OptionNames.BackingFieldsNamingConventionName, out var convention))
        {
            if (Enum.TryParse<NamingConvention>(convention, ignoreCase: true, out var value))
            {
                Convention = value;
            }
        }

        if (options.TryGetValue(OptionNames.BackingFieldsUseUnderscoreName, out var useUnderscore))
        {
            if (bool.TryParse(useUnderscore, out var value))
            {
                UseUnderscore = value;
            }
        }
    }
}
