using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Diagnostics;
using Saritasa.Tools.PropertyChangedGenerator.Diagnostics.Reporters;
using Saritasa.Tools.PropertyChangedGenerator.Models.Descriptors;

namespace Saritasa.Tools.PropertyChangedGenerator.Diagnostics;

/// <summary>
/// Scope of <see cref="Diagnostic"/> descriptors.
/// </summary>
public class DiagnosticsScope : IDiagnosticsScope
{
    private static readonly WarningReporter warningReporter = new();
    private static readonly ErrorReporter errorReporter = new();

    private static Dictionary<Type, IDiagnostics> Reporters { get; } = new()
    {
        { typeof(WarningDescriptor), warningReporter },
        { typeof(ErrorDescriptor), errorReporter },
    };

    /// <inheritdoc/>
    public void AddDiagnostic<TDescriptor>(TDescriptor descriptor)
        where TDescriptor : IDiagnosticDescriptor
        => GetReporter<TDescriptor>().AddDiagnostic(descriptor);

    /// <inheritdoc/>
    public ImmutableArray<Diagnostic> GetDiagnostics<TDescriptor>()
        where TDescriptor : IDiagnosticDescriptor
        => GetReporter<TDescriptor>().Diagnostics;

    /// <inheritdoc/>
    public ImmutableArray<Diagnostic> GetDiagnostics()
        => Reporters.Values
            .SelectMany(reporter => reporter.Diagnostics)
            .ToImmutableArray();

    private IDiagnosticReporter<TDescriptor> GetReporter<TDescriptor>()
        where TDescriptor : IDiagnosticDescriptor
        => (IDiagnosticReporter<TDescriptor>)Reporters[typeof(TDescriptor)];
}
