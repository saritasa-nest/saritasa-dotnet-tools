using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;

/// <summary>
/// Diagnostic descriptor.
/// </summary>
public interface IDiagnosticDescriptor
{
    /// <summary>
    /// Diagnostic code.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Diagnostic title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Diagnostic message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Diagnostic category.
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Diagnostic severity.
    /// </summary>
    DiagnosticSeverity Severity { get; }
}
