using Microsoft.CodeAnalysis;
using Saritasa.Tools.SourceGenerator.Abstractions.Analyzers;
using Saritasa.Tools.SourceGenerator.Utils;

namespace Saritasa.Tools.SourceGenerator.Models.Analyzers;

/// <summary>
/// Class analysis based on <see cref="ITypeSymbol"/> code analysis API.
/// </summary>
public record ClassAnalysis : ISyntaxAnalysis
{
    /// <summary>
    /// Class name.
    /// </summary>
    public string? Name => Symbol?.Name;

    /// <summary>
    /// Class modifier.
    /// </summary>
    public string? Modifier => SymbolUtils.GetModifier(Symbol);

    /// <summary>
    /// Class namespace.
    /// </summary>
    public string? Namespace => Symbol?.ContainingNamespace.ToString();

    /// <summary>
    /// Related code analysis symbol data.
    /// </summary>
    public ITypeSymbol Symbol { get; internal set; }

    /// <summary>
    /// Property changed interface analysis.
    /// </summary>
    public InterfaceAnalysis PropertyChanged { get; internal set; }

    /// <summary>
    /// Property changing interface analysis.
    /// </summary>
    public InterfaceAnalysis PropertyChanging { get; internal set; }

    /// <summary>
    /// Should proceed build action.
    /// </summary>
    public bool ShouldBuild => PropertyChanged != null || PropertyChanging != null;

    /// <summary>
    /// Properties analysis.
    /// </summary>
    public IReadOnlyCollection<PropertyAnalysis> Properties { get; internal set; }

    /// <summary>
    /// Fields analysis.
    /// </summary>
    public IReadOnlyCollection<FieldAnalysis> Fields { get; internal set; }

    /// <summary>
    /// Instance of <see cref="ClassAnalysis"/>.
    /// </summary>
    public static ClassAnalysis Instance => new();
}
