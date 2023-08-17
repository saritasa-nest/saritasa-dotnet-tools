namespace Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;

/// <summary>
/// Diagnostics reporter.
/// </summary>
/// <typeparam name="TDescriptor">Diagnostic descriptor.</typeparam>
public interface IDiagnosticReporter<TDescriptor> : IDiagnostics
    where TDescriptor : IDiagnosticDescriptor
{
    /// <summary>
    /// Add a diagnostic descriptor.
    /// </summary>
    /// <param name="descriptor">Diagnostic descriptor.</param>
    void AddDiagnostic(TDescriptor descriptor);
}
