using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Rules;

/// <summary>
/// Reports INCL001 when a method accesses a <see cref="TrackIncludeRequiredAttribute"/>
/// property without annotating <see cref="IncludeRequiredAttribute"/>.
/// </summary>
internal static class PropertyReferenceHandler
{
    /// <summary>
    /// Analyzes a property reference operation and reports INCL001 if applicable.
    /// </summary>
    /// <param name="context">Operation analysis context.</param>
    public static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IPropertyReferenceOperation propRef)
        {
            return;
        }

        // We only care about navigation properties explicitly marked for tracking.
        if (!AttributeReader.HasTrackIncludeRequiredAttribute(propRef.Property))
        {
            return;
        }

        // The property must be accessed on a parameter (e.g. "user.Profile").
        if (propRef.Instance is not IParameterReferenceOperation paramRef)
        {
            return;
        }

        // Resolve the method that contains this property access.
        if (context.ContainingSymbol is not IMethodSymbol containingMethod)
        {
            return;
        }

        // Lambdas and anonymous methods introduce their own parameter symbols, which are
        // different objects from the enclosing method's parameters even when they share a
        // name. Only report for parameters that truly belong to the containing method.
        if (!containingMethod.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p, paramRef.Parameter)))
        {
            return;
        }

        var paramName = paramRef.Parameter.Name;
        var propertyName = propRef.Property.Name;
        var typeName = propRef.Property.ContainingType.Name;

        if (AttributeReader.MethodHasIncludeRequiredAttribute(containingMethod, paramName, propertyName))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                NavigationIncludeRulesProvider.GetDiagnosticDescriptor(NavigationIncludeRulesProvider.Incl1IdAddIncludeRequiredForParameter),
                propRef.Syntax.GetLocation(),
                typeName,
                propertyName,
                paramName));
    }
}
