namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Entities;

/// <summary>
/// Describes a parameter which requires including a navigation property from argument prior to passing.
/// </summary>
public class IncludeRequirement
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public IncludeRequirement(string parameterName, string navigationProperty)
    {
        ParameterName = parameterName;
        NavigationProperty = navigationProperty;
    }

    /// <summary>
    /// Name of parameter which should contain a <see cref="NavigationProperty"/>.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Required navigation property.
    /// </summary>
    public string NavigationProperty { get; }
}
