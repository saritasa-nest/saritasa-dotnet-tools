using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saritasa.Tools.SourceGenerator.Abstractions.Syntax;

namespace Saritasa.Tools.SourceGenerator.Syntax.Filters;

/// <summary>
/// Partial class filter for <see cref="SyntaxManager"/>.
/// </summary>
internal class ClassDeclarationFilter : ISyntaxFilter<ClassDeclarationSyntax>
{
    /// <summary>
    /// Class modifier kind.
    /// </summary>
    public SyntaxKind[] ModifierKinds { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="modifierKinds">Declaration modifier kinds.</param>
    public ClassDeclarationFilter(SyntaxKind[] modifierKinds)
    {
        ModifierKinds = modifierKinds;
    }

    /// <inheritdoc/>
    public IncrementalValuesProvider<ClassDeclarationSyntax> Apply(IncrementalValuesProvider<ClassDeclarationSyntax> nodes)
        => nodes.Where(IsValid);

    /// <inheritdoc/>
    public bool IsValid(ClassDeclarationSyntax node)
    {
        var classModifiers = node.Modifiers.Select(m => m.Kind());
        return ModifierKinds.All(classModifiers.Contains);
    }
}
