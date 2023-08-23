using Microsoft.CodeAnalysis;
using Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;

namespace Saritasa.Tools.SourceGenerator.Models.Descriptors;

/// <summary>
/// Descriptor for <see cref="DiagnosticSeverity.Warning"/>.
/// </summary>
public class WarningDescriptor : IDiagnosticDescriptor
{
    /// <inheritdoc/>
    public string Title { get; }

    /// <inheritdoc/>
    public string Message { get; }

    /// <inheritdoc/>
    public string Code { get; }

    /// <inheritdoc/>
    public string Format { get; }

    /// <inheritdoc/>
    public DiagnosticSeverity Severity => DiagnosticSeverity.Warning;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="title">Warning title.</param>
    /// <param name="message">Warning message.</param>
    /// <param name="code">Warning code.</param>
    /// <param name="format">Warning format.</param>
    public WarningDescriptor(string title, string message, string code, string format)
    {
        Title = title;
        Message = message;
        Code = code;
        Format = format;
    }
}
