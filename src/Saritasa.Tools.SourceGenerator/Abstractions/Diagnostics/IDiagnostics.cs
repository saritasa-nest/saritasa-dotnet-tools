using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;

/// <summary>
/// Diagnostics manager.
/// </summary>
public interface IDiagnostics
{
    /// <summary>
    /// List of diagnostics.
    /// </summary>
    ImmutableArray<Diagnostic> Diagnostics { get; }
}
