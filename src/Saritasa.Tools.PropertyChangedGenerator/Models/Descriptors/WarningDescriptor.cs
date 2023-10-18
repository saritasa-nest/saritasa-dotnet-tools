using Microsoft.CodeAnalysis;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Diagnostics;

namespace Saritasa.Tools.PropertyChangedGenerator.Models.Descriptors;

/// <summary>
/// Descriptor for <see cref="DiagnosticSeverity.Warning"/>.
/// </summary>
public class WarningDescriptor : IDiagnosticDescriptor
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
    public DiagnosticSeverity Severity => DiagnosticSeverity.Warning;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="code">Warning code.</param>
    /// <param name="title">Warning title.</param>
    /// <param name="message">Warning message.</param>
    /// <param name="category">Warning category.</param>
    public WarningDescriptor(string code, string title, string message, string category)
    {
        Code = code;
        Title = title;
        Message = message;
        Category = category;
    }
}
