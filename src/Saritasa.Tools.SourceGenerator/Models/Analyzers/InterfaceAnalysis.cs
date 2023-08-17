using Microsoft.CodeAnalysis;
using Saritasa.Tools.SourceGenerator.Abstractions.Analyzers;

namespace Saritasa.Tools.SourceGenerator.Models.Analyzers;

/// <summary>
/// Interface analysis.
/// </summary>
public record InterfaceAnalysis : ISyntaxAnalysis
{
    /// <summary>
    /// Event property symbol.
    /// </summary>
    public IEventSymbol? EventSymbol { get; set; }

    /// <summary>
    /// Event raise method symbol.
    /// </summary>
    public IMethodSymbol? MethodSymbol { get; set; }

    /// <summary>
    /// Indicates if an interface is inherited by parent.
    /// </summary>
    public bool HasInterface { get; set; }

    /// <summary>
    /// An instance of <see cref="InterfaceAnalysis"/>.
    /// </summary>
    public static InterfaceAnalysis Instance => new();
}
