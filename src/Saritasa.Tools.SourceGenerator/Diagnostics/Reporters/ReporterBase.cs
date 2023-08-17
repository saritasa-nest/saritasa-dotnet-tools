using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;

namespace Saritasa.Tools.SourceGenerator.Diagnostics.Reporters;

/// <summary>
/// Base diagnostics repoter.
/// </summary>
/// <typeparam name="TDescriptor">Diagnostic descriptor.</typeparam>
public abstract class ReporterBase<TDescriptor> : IDiagnosticReporter<TDescriptor>
    where TDescriptor : IDiagnosticDescriptor
{
    private readonly ImmutableArray<Diagnostic>.Builder diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

    /// <inheritdoc/>
    public ImmutableArray<Diagnostic> Diagnostics => diagnostics.ToImmutable();

    /// <inheritdoc/>
    public virtual void AddDiagnostic(TDescriptor descriptor)
    {
        var diagnosticDescriptor = new DiagnosticDescriptor(descriptor.Code, descriptor.Title, descriptor.Format, "", descriptor.Severity, isEnabledByDefault: true);
        var diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None);
        AddDiagnostic(diagnostic);
    }

    /// <summary>
    /// Add a diagnostic.
    /// </summary>
    /// <param name="diagnostic">Diagnostic.</param>
    protected void AddDiagnostic(Diagnostic diagnostic) => diagnostics.Add(diagnostic);
}
