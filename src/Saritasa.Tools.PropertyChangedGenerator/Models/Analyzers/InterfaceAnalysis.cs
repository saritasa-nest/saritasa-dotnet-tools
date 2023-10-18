using Microsoft.CodeAnalysis;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Analyzers;

namespace Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;

/// <summary>
/// Interface analysis.
/// </summary>
public record InterfaceAnalysis : ISyntaxAnalysis
{
    /// <summary>
    /// Interface event syntax symbol.
    /// It's an implementation of interface <see cref="ISymbol"/> member.
    /// </summary>
    public IEventSymbol? EventSymbol { get; set; }

    /// <summary>
    /// Raise method of <see cref="EventSymbol"/> implementation member.
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
