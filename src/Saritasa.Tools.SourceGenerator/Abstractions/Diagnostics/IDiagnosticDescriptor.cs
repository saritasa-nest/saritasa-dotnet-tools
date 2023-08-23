using Microsoft.CodeAnalysis;

namespace Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;

/// <summary>
/// Diagnostic descriptor.
/// </summary>
public interface IDiagnosticDescriptor
{
    /// <summary>
    /// Diagnostic title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Diagnostic message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Diagnostic code.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Message format.
    /// </summary>
    string Format { get; }

    /// <summary>
    /// Diagnostic severity.
    /// </summary>
    DiagnosticSeverity Severity { get; }
}
