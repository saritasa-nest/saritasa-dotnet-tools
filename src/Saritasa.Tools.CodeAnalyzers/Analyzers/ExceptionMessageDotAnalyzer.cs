using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers;

/// <summary>
/// Ensures exception messages end with a dot.
/// </summary>
/// <remarks>
/// According to
/// <see href="https://wiki.saritasa.rocks/dotnet/development/c-sharp-style-guide/#english-spelling">6.4 code style</see>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionMessageDotAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "STAN1002";
    private const string Category = "Spelling";

    private static readonly LocalizableString title = "Exception message should end with a dot";
    private static readonly LocalizableString messageFormat = "Exception message should end with a dot";
    private static readonly LocalizableString description
        = "Ensure exception messages end with a dot to keep consistent phrasing.";

    private static readonly DiagnosticDescriptor rule = new(
        DiagnosticId,
        title,
        messageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: description);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var exceptionType = compilationContext.Compilation.GetTypeByMetadataName("System.Exception");
            if (exceptionType is null)
            {
                return;
            }

            compilationContext.RegisterOperationAction(
                operationContext => AnalyzeObjectCreation(operationContext, exceptionType),
                OperationKind.ObjectCreation);

            compilationContext.RegisterOperationAction(
                operationContext => AnalyzeBaseConstructor(operationContext, exceptionType),
                OperationKind.Invocation);
        });
    }

    private static void AnalyzeObjectCreation(OperationAnalysisContext context, INamedTypeSymbol exceptionType)
    {
        if (context.Operation is not IObjectCreationOperation creation || creation.Type is null)
        {
            return;
        }

        if (!DerivesFromException(creation.Type, exceptionType))
        {
            return;
        }

        CheckExceptionMessage(creation.Arguments, context.ReportDiagnostic);
    }

    private static void AnalyzeBaseConstructor(OperationAnalysisContext context, INamedTypeSymbol exceptionType)
    {
        if (context.Operation is not IInvocationOperation invocation ||
            invocation.TargetMethod.MethodKind != MethodKind.Constructor)
        {
            return;
        }

        if (!DerivesFromException(invocation.TargetMethod.ContainingType, exceptionType))
        {
            return;
        }

        CheckExceptionMessage(invocation.Arguments, context.ReportDiagnostic);
    }

    private static void CheckExceptionMessage(IEnumerable<IArgumentOperation> arguments, Action<Diagnostic> reportDiagnostic)
    {
        var messageArgument = arguments
            .FirstOrDefault(a => a.Parameter?.Name == "message" && IsStringType(a.Parameter.Type));

        if (messageArgument?.Value is null)
        {
            return;
        }

        if (MessageEndsWithDot(messageArgument.Value))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(rule, messageArgument.Syntax.GetLocation());
        reportDiagnostic(diagnostic);
    }

    private static bool MessageEndsWithDot(IOperation? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value.ConstantValue is { HasValue: true, Value: string constant })
        {
            return EndsWithDot(constant);
        }

        if (value is IInterpolatedStringOperation interpolated)
        {
            var lastPart = interpolated.Parts.LastOrDefault();
            if (lastPart is IInterpolatedStringTextOperation textPart &&
                textPart.Text.ConstantValue is { HasValue: true, Value: string text })
            {
                return EndsWithDot(text);
            }
        }

        // Handles such case:
        // var error = "Error"; Important: it is variable, not constant. With constant, it would be a different case.
        // throw new Exception(error + ".");
        if (value is IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } binary && IsStringType(binary.Type))
        {
            var rightMost = GetRightMostOperand(binary);
            return MessageEndsWithDot(rightMost);
        }

        if (value is IConditionalOperation conditional)
        {
            return MessageEndsWithDot(conditional.WhenTrue) && MessageEndsWithDot(conditional.WhenFalse);
        }

        if (value is ICoalesceOperation coalesce)
        {
            return MessageEndsWithDot(coalesce.Value) && MessageEndsWithDot(coalesce.WhenNull);
        }

        if (value is ISwitchExpressionOperation switchExpression)
        {
            foreach (var arm in switchExpression.Arms)
            {
                if (!MessageEndsWithDot(arm.Value))
                {
                    return false;
                }
            }

            return true;
        }

        // We cannot analyze method results.
        if (value is IInvocationOperation invocation)
        {
            if (IsStringFormat(invocation.TargetMethod))
            {
                var formatArg = invocation.Arguments.FirstOrDefault(a => a.Parameter?.Name == "format");
                if (formatArg?.Value.ConstantValue is { HasValue: true, Value: string format })
                {
                    return EndsWithDot(format);
                }
            }

            return true;
        }

        // We cannot analyze identifier values (local variables, parameters, fields, properties).
        if (value
            is ILocalReferenceOperation
            or IParameterReferenceOperation
            or IFieldReferenceOperation
            or IPropertyReferenceOperation)
        {
            return true;
        }

        return false;
    }

    private static IOperation GetRightMostOperand(IOperation operation)
    {
        var current = operation;
        while (current is IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } binary && IsStringType(binary.Type))
        {
            current = binary.RightOperand;
        }

        return current;
    }

    private static bool IsStringType(ITypeSymbol? type) => type?.SpecialType == SpecialType.System_String;

    private static bool EndsWithDot(string value) => value.TrimEnd().EndsWith(".", StringComparison.Ordinal);

    private static bool DerivesFromException(ITypeSymbol type, INamedTypeSymbol exceptionType)
    {
        var current = type;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, exceptionType))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static bool IsStringFormat(IMethodSymbol method)
    {
        return method.ContainingType.SpecialType == SpecialType.System_String && method.Name == "Format";
    }
}
