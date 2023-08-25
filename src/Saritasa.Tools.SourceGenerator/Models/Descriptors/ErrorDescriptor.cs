using Microsoft.CodeAnalysis;
using Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;

namespace Saritasa.Tools.SourceGenerator.Models.Descriptors;

/// <summary>
/// Descriptor for <see cref="DiagnosticSeverity.Error"/>.
/// </summary>
public class ErrorDescriptor : IDiagnosticDescriptor
{
    /// <inheritdoc/>
    public string Code { get; }

    /// <inheritdoc/>
    public string Title { get; }

    /// <inheritdoc/>
    public string Message { get; }

    /// <inheritdoc/>
    public string Category { get; }

    /// <inheritdoc/>
    public DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="title">Error title.</param>
    /// <param name="message">Error message.</param>
    /// <param name="code">Error code.</param>
    /// <param name="category">Error category.</param>
    public ErrorDescriptor(string title, string message, string code, string category)
    {
        Title = title;
        Message = message;
        Code = code;
        Category = category;
    }
}
