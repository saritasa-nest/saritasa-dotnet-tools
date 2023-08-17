using Saritasa.Tools.SourceGenerator.Abstractions.Analyzers;
using Saritasa.Tools.SourceGenerator.Models.Metadata;

namespace Saritasa.Tools.SourceGenerator.Models.Analyzers;

/// <summary>
/// Member analysis.
/// </summary>
public abstract record MemberAnalysis : ISyntaxAnalysis
{
    /// <summary>
    /// Member name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Member modifier.
    /// </summary>
    public string Modifier { get; set; } = MemberModifiers.Public;

    /// <summary>
    /// Member type.
    /// </summary>
    public string Type { get; set; }
}
