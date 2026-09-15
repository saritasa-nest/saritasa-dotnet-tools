using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// What a statement assigns to a variable: a new value (<c>user = value</c>)
/// or one of its properties (<c>user.Profile = value</c>).
/// </summary>
/// <remarks>
/// A variable is a local, a parameter or a compiler temporary (<see cref="CaptureId"/>), see <see cref="Of"/>.
/// </remarks>
internal sealed class VariableAssignment
{
    private VariableAssignment(IOperation? value, string? propertyName)
    {
        Value = value;
        PropertyName = propertyName;
    }

    /// <summary>
    /// For <c>user = value</c>: the value. Null if it is unknown (an out argument of an arbitrary method).
    /// </summary>
    public IOperation? Value { get; }

    /// <summary>
    /// For <c>user.Profile = value</c>: "Profile". Null if the whole variable is assigned.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// Maps a statement to what it assigns to the variable. Null if the statement does not assign it.
    /// </summary>
    /// <param name="statement">Statement.</param>
    /// <param name="variable">Variable, see <see cref="Of"/>.</param>
    /// <returns>Assignment or null.</returns>
    public static VariableAssignment? Find(IOperation statement, object variable)
    {
        var operation = statement is IExpressionStatementOperation expressionStatement
            ? expressionStatement.Operation
            : statement;

        return operation switch
        {
            // "user = ..." and "var user = ...".
            ISimpleAssignmentOperation assignment when AreSame(Of(assignment.Target), variable)
                => new VariableAssignment(assignment.Value, propertyName: null),

            // "user.Profile = ..." (the compiler also rewrites "new User { Profile = ... }" into this form).
            ISimpleAssignmentOperation { Target: IPropertyReferenceOperation property }
                when AreSame(Of(property.Instance), variable)
                => new VariableAssignment(value: null, property.Property.Name),

            // "#1 = ...": a temporary value the compiler creates for ?:, ??, object initializers and foreach.
            IFlowCaptureOperation capture when AreSame(capture.Id, variable)
                => new VariableAssignment(capture.Value, propertyName: null),

            // "var (id, user) = pair" and "foreach (var (id, user) in dictionary)": user is a part of the pair.
            IDeconstructionAssignmentOperation deconstruction
                when deconstruction.Target.DescendantsAndSelf().Any(target => AreSame(Of(target), variable))
                => new VariableAssignment(deconstruction.Value, propertyName: null),

            _ => FindOutArgument(statement, variable),
        };
    }

    /// <summary>
    /// Returns the local, parameter or compiler temporary the operation reads; null for anything else.
    /// </summary>
    /// <param name="operation">Operation.</param>
    /// <returns>Variable or null.</returns>
    public static object? Of(IOperation? operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation switch
        {
            ILocalReferenceOperation local => local.Local,
            IParameterReferenceOperation parameter => parameter.Parameter,
            IFlowCaptureReferenceOperation capture => capture.Id,
            IDeclarationExpressionOperation declaration => Of(declaration.Expression), // "out var user"
            _ => null,
        };
    }

    /// <summary>
    /// "dictionary.TryGetValue(id, out var user)": user is a value of the dictionary.
    /// For other methods the value of the out argument is unknown.
    /// </summary>
    private static VariableAssignment? FindOutArgument(IOperation statement, object variable)
    {
        var outArgument = statement
            .DescendantsAndSelf()
            .OfType<IArgumentOperation>()
            .FirstOrDefault(argument => argument.Parameter?.RefKind == RefKind.Out && AreSame(Of(argument.Value), variable));

        return outArgument switch
        {
            null => null,
            { Parent: IInvocationOperation { TargetMethod.Name: "TryGetValue" } call }
                => new VariableAssignment(call.Instance, propertyName: null),
            _ => new VariableAssignment(value: null, propertyName: null),
        };
    }

    private static bool AreSame(object? first, object second)
    {
        if (first is ISymbol firstSymbol && second is ISymbol secondSymbol)
        {
            return SymbolEqualityComparer.Default.Equals(firstSymbol, secondSymbol);
        }

        return first is CaptureId && first.Equals(second);
    }
}
