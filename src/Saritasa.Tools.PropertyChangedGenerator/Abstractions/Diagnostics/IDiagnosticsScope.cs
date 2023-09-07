using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Saritasa.Tools.PropertyChangedGenerator.Abstractions.Diagnostics;

/// <summary>
/// Scope of <see cref="IDiagnostics"/> descriptors.
/// </summary>
public interface IDiagnosticsScope
{
    /// <summary>
    /// Add diagnostic descriptor into scope.
    /// </summary>
    /// <typeparam name="TDescriptor">Diagnostic descriptor type.</typeparam>
    /// <param name="descriptor">Diagnostic descriptor instance.</param>
    void AddDiagnostic<TDescriptor>(TDescriptor descriptor) where TDescriptor : IDiagnosticDescriptor;

    /// <summary>
    /// Get scope diagnostics by type.
    /// </summary>
    /// <typeparam name="TDescriptor">Diagnostic descriptor type.</typeparam>
    /// <returns>Diagnostics.</returns>
    ImmutableArray<Diagnostic> GetDiagnostics<TDescriptor>() where TDescriptor : IDiagnosticDescriptor;

    /// <summary>
    /// Get scope diagnostics.
    /// </summary>
    /// <returns>Diagnostics.</returns>
    ImmutableArray<Diagnostic> GetDiagnostics();
}
