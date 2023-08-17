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
    public string Code { get; }

    /// <inheritdoc/>
    public string Format { get; }

    /// <inheritdoc/>
    public DiagnosticSeverity Severity => DiagnosticSeverity.Warning;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="title">Error title.</param>
    /// <param name="code">Error code.</param>
    /// <param name="format">Error format.</param>
    public WarningDescriptor(string title, string code, string format)
    {
        Title = title;
        Code = code;
        Format = format;
    }
}
