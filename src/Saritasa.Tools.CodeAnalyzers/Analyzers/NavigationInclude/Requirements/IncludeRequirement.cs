namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;

/// <summary>
/// Describes a parameter which requires including a navigation path from argument prior to passing.
/// </summary>
internal class IncludeRequirement
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="path">Required navigation path.</param>
    public IncludeRequirement(string parameterName, NavigationPath path)
    {
        ParameterName = parameterName;
        Path = path;
    }

    /// <summary>
    /// Name of parameter which should contain a <see cref="Path"/>.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Required navigation path, e.g. "Profile" or "Settings.Profile".
    /// </summary>
    public NavigationPath Path { get; }
}
