using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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

        // The property must be accessed on a parameter (e.g. "user.Profile" or "user?.Profile").
        var conditionalAccess = FindConditionalAccess(propRef.Instance);
        var referenceInstance = conditionalAccess?.Operation ?? propRef.Instance;
        if (referenceInstance is not IParameterReferenceOperation paramRef)
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

        var diagnosticLocation = conditionalAccess is null
            ? propRef.Syntax.GetLocation()
            : GetLocationOfConditionalAccess(propRef, conditionalAccess);

        context.ReportDiagnostic(
            Diagnostic.Create(
                NavigationIncludeRulesProvider.GetDiagnosticDescriptor(NavigationIncludeRulesProvider.Incl1IdAddIncludeRequiredForParameter),
                diagnosticLocation,
                typeName,
                propertyName,
                paramName));
    }

    /// <summary>
    /// Returns the conditional access whose receiver the instance stands for, e.g. the "user?..." of
    /// "user?.Profile", and null when the instance is not a conditional access placeholder.
    /// </summary>
    private static IConditionalAccessOperation? FindConditionalAccess(IOperation? instance)
    {
        if (instance is not IConditionalAccessInstanceOperation)
        {
            return null;
        }

        // The placeholder belongs to the nearest conditional access that has it in its WhenNotNull part.
        var child = instance;
        for (var parent = instance.Parent; parent is not null; child = parent, parent = parent.Parent)
        {
            if (parent is IConditionalAccessOperation conditionalAccess && conditionalAccess.WhenNotNull == child)
            {
                return conditionalAccess;
            }
        }

        return null;
    }

    /// <summary>
    /// Where to report: the property access, which for "user?.Profile" spans from the receiver, since the
    /// property's own syntax is only ".Profile".
    /// </summary>
    private static Location GetLocationOfConditionalAccess(
        IPropertyReferenceOperation propRef,
        IConditionalAccessOperation conditionalAccess)
    {
        var span = TextSpan.FromBounds(conditionalAccess.Operation.Syntax.SpanStart, propRef.Syntax.Span.End);
        return Location.Create(propRef.Syntax.SyntaxTree, span);
    }
}
