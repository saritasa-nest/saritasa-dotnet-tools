using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Analyzers;

namespace Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;

/// <summary>
/// Contains attribute analysis.
/// </summary>
public class AttributeAnalysis : ISyntaxAnalysis
{
    /// <summary>
    /// Attribute namespace.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Attribute name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Attribute constructor arguments syntax.
    /// </summary>
    public AttributeArgumentListSyntax? Arguments { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="namespace">Attribute namespace.</param>
    /// <param name="name">Attribute name.</param>
    /// <param name="arguments">Attribute constructor arguments syntax.</param>
    public AttributeAnalysis(string @namespace, string name, AttributeArgumentListSyntax? arguments = null)
    {
        Namespace = @namespace;
        Name = name;
        Arguments = arguments;
    }
}
