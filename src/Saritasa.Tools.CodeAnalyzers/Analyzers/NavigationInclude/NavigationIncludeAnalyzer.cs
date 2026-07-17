using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Attributes;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude;

/// <summary>
/// Detects navigation property accesses marked with <c>[TrackIncludeRequired]</c>
/// on methods that do not declare a matching <c>[IncludeRequired]</c> attribute,
/// and detects local variables passed to such methods without the required navigation
/// property being loaded (INCL002).
/// </summary>
/// <remarks>
/// <para>
/// Mark navigation properties with <c>[TrackIncludeRequired]</c> to signal they must be
/// loaded before use. Any method that receives such an entity as a parameter and touches
/// the property must carry <c>[IncludeRequired("paramName", "PropertyName")]</c>.
/// </para>
/// <para>
/// The requirement propagates up the call chain: if method A passes its own parameter to
/// method B that declares <c>[IncludeRequired]</c>, then A must declare the same
/// requirement for that parameter (INCL001).
/// </para>
/// <para>
/// For local variables the analyzer checks the initializer instead of propagating the
/// attribute, because the variable is created inside the current method (INCL002).
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NavigationIncludeAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Usage";
    private const string DiagnosticIncl1Id = "INCL001";
    private const string DiagnosticIncl2Id = "INCL002";

    private static readonly LocalizableString titleIncl1 =
        "Method parameter should require a navigation property";

    private static readonly LocalizableString messageFormatIncl1 =
        "Navigation property '{0}.{1}' is required, but not checked. Use [IncludeRequired(\"{2}\", \"{1}\")] to this method.";

    private static readonly LocalizableString descriptionIncl1 =
        "Navigation properties marked with [TrackIncludeRequired] must be declared via [IncludeRequired] on every " +
        "method that accesses them directly or propagates them to callee methods.";

    private static readonly DiagnosticDescriptor ruleIncl1 = new(
        DiagnosticIncl1Id,
        titleIncl1,
        messageFormatIncl1,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: descriptionIncl1);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ruleIncl1);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzePropertyReference, OperationKind.PropertyReference);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    /// <summary>
    /// Handles INCL001 for the <em>direct-access</em> scenario: fires when a method reads
    /// or writes a <c>[TrackIncludeRequired]</c> navigation property directly through one
    /// of its own parameters, but does not declare
    /// <c>[IncludeRequired(paramName, propertyName)]</c>.
    /// </summary>
    /// <example>
    /// INCL001:
    /// <code>
    ///    class User
    ///    {
    ///        public Organization Organization { get; set; }
    ///        [TrackIncludeRequired]
    ///        public UserProfile Profile { get; set; }
    ///    }
    ///
    ///    void SetTimezone_Nocheck(User user, string timezone)
    ///    {
    ///        // INCL001: navigation property is required, but not checked. Use IncludeRequiredAttribute.
    ///        user.Profile.Timezone = timezone;
    ///    }
    ///
    ///    void SetTimezone(User user, string timezone)
    ///    {
    ///        // Does not produce INCL001 because the method has IncludeRequired attribute.
    ///        user.Profile.Timezone = timezone;
    ///    }
    /// </code>
    /// </example>
    private static void AnalyzePropertyReference(OperationAnalysisContext context)
    {
        if (context.Operation is not IPropertyReferenceOperation propRef)
        {
            return;
        }

        // We only care about navigation properties explicitly marked for tracking.
        if (!HasTrackIncludeRequired(propRef.Property))
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
        if (!containingMethod.Parameters.Any(p =>
                SymbolEqualityComparer.Default.Equals(p, paramRef.Parameter)))
        {
            return;
        }

        var paramName = paramRef.Parameter.Name;
        var propertyName = propRef.Property.Name;
        var typeName = propRef.Property.ContainingType.Name;

        if (MethodHasIncludeRequired(containingMethod, paramName, propertyName))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(ruleIncl1, propRef.Syntax.GetLocation(), typeName, propertyName, paramName));
    }

    /// <summary>
    /// Handles the <em>propagation</em> scenarios for INCL001 and INCL002.
    /// Runs on every method-call operation and inspects the <c>[IncludeRequired]</c>
    /// attributes on the called method:
    /// <list type="bullet">
    ///   <item>
    ///     <term>INCL001 — parameter propagation</term>
    ///     <description>
    ///       When the argument for an <c>[IncludeRequired]</c> parameter is one of the
    ///       <em>caller's own parameters</em>, the caller must also declare
    ///       <c>[IncludeRequired]</c> for that parameter, otherwise INCL001 is reported.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>INCL002 — local variable</term>
    ///     <description>
    ///       When the argument is a <em>local variable</em>, the analyzer checks the
    ///       variable's initializer. If the required navigation property is not loaded
    ///       via <c>.Include()</c>, an object initializer, or a method with
    ///       <c>[Includes("Property")]</c>, INCL002 is reported.
    ///     </description>
    ///   </item>
    /// </list>
    /// </summary>
    /// <example>
    /// INCL001:
    /// <code>
    ///     class User
    ///     {
    ///         public Organization Organization { get; set; }
    ///         [TrackIncludeRequired]
    ///         public UserProfile Profile { get; set; }
    ///     }
    ///
    ///     // Uncommenting this line will fix the INCL001 warning.
    ///     // [IncludeRequired(nameof(entity), nameof(@User.Profile))]
    ///     void UpdateUserProfile(User entity, SaveUserDto dto)
    ///     {
    ///         // Does not produce INCL001 because Organization has no TrackIncludeRequired attribute.
    ///         entity.Organization = dto.Organization;
    ///         // INCL001: the called method captures the `user` argument and has IncludeRequired attribute.
    ///         // Use IncludeRequiredAttribute on the current method as well.
    ///         SetTimezone(entity, dto.Timezone);
    ///     }
    /// </code>
    /// </example>
    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocation)
        {
            return;
        }

        if (context.ContainingSymbol is not IMethodSymbol containingMethod)
        {
            return;
        }

        foreach (var attr in invocation.TargetMethod.GetAttributes())
        {
            if (!TryGetIncludeRequiredArgs(attr, out var targetParamName, out var propertyName))
            {
                continue;
            }

            // Resolve the exact parameter symbol in the called method that the attribute
            // references by name (e.g. "user" in [IncludeRequired("user", "Profile")]).
            var targetParam = invocation.TargetMethod.Parameters
                .FirstOrDefault(p => p.Name == targetParamName);
            if (targetParam is null)
            {
                continue;
            }

            // Match the resolved parameter symbol to the actual argument at this call site.
            var argument = invocation.Arguments
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.Parameter, targetParam));
            if (argument is null)
            {
                continue;
            }

            // --- INCL001: argument is one of the calling method's own parameters ----------
            if (argument.Value is IParameterReferenceOperation callerParamRef)
            {
                var callerParamName = callerParamRef.Parameter.Name;

                if (MethodHasIncludeRequired(containingMethod, callerParamName, propertyName!))
                {
                    continue;
                }

                var typeName = callerParamRef.Parameter.Type.Name;

                context.ReportDiagnostic(
                    Diagnostic.Create(ruleIncl1, invocation.Syntax.GetLocation(), typeName, propertyName, callerParamName));
                continue;
            }
        }
    }

    private static bool HasTrackIncludeRequired(ISymbol symbol)
    {
        return symbol
            .GetAttributes()
            .Any(a => string.Equals(
                a.AttributeClass?.Name,
                nameof(TrackIncludeRequiredAttribute),
                StringComparison.Ordinal));
    }

    private static bool TryGetIncludeRequiredArgs(
        AttributeData attr,
        out string? paramName,
        out string? propertyName)
    {
        paramName = null;
        propertyName = null;

        var name = attr.AttributeClass?.Name;
        if (!string.Equals(name, nameof(IncludeRequiredAttribute), StringComparison.Ordinal))
        {
            return false;
        }

        if (attr.ConstructorArguments.Length < 2)
        {
            return false;
        }

        paramName = attr.ConstructorArguments[0].Value as string;
        propertyName = attr.ConstructorArguments[1].Value as string;

        return paramName is not null && propertyName is not null;
    }

    private static bool MethodHasIncludeRequired(
        IMethodSymbol method,
        string paramName,
        string propertyName)
    {
        foreach (var attr in method.GetAttributes())
        {
            if (!TryGetIncludeRequiredArgs(attr, out var attrParamName, out var attrPropertyName))
            {
                continue;
            }

            if (string.Equals(attrParamName, paramName, StringComparison.Ordinal) &&
                string.Equals(attrPropertyName, propertyName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
