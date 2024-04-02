using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;

/// <summary>
/// Attribute analysis.
/// </summary>
public record AttributeAnalysis
{
    /// <summary>
    /// Attribute syntax.
    /// </summary>
    public AttributeSyntax Syntax { get; }

    /// <summary>
    /// Attribute containing namespace.
    /// </summary>
    public string ContainingNamespace { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="syntax">Attribute syntax.</param>
    /// <param name="containingNamespace">Attribute containing namespace.</param>
    public AttributeAnalysis(AttributeSyntax syntax, string containingNamespace)
    {
        Syntax = syntax;
        ContainingNamespace = containingNamespace;
    }
}
