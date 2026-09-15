using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// Describes how one statement changes the <see cref="LoadedProperties"/> table.
/// This is the only place where the table changes.
/// </summary>
internal static class StatementEffects
{
    /// <summary>
    /// Returns the table after the statement has run.
    /// </summary>
    /// <param name="statement">Statement from a control flow graph block.</param>
    /// <param name="table">Table before the statement.</param>
    /// <returns>Table after the statement.</returns>
    public static LoadedProperties UpdateTableForStatement(IOperation statement, LoadedProperties table)
    {
        if (statement is IExpressionStatementOperation expressionStatement)
        {
            statement = expressionStatement.Operation;
        }

        if (statement is ISimpleAssignmentOperation assignment)
        {
            // "variable = expression" and "var variable = expression": the old value is forgotten.
            if (ExpressionLoadedProperties.TryGetVariable(assignment.Target, out var variable))
            {
                return table.SetProperties(variable, ExpressionLoadedProperties.Get(assignment.Value, table));
            }

            // "variable.Property = expression": the property is now loaded.
            // The compiler also rewrites "new User { Profile = p }" into this form.
            if (assignment.Target is IPropertyReferenceOperation property &&
                ExpressionLoadedProperties.TryGetVariable(property.Instance, out var owner))
            {
                return table.AddProperty(owner, property.Property.Name);
            }
        }

        // Temporary value created by the compiler: "#1 = expression".
        // Used for object initializers, ?:, ?? and the foreach enumerator.
        if (statement is IFlowCaptureOperation capture)
        {
            return table.SetProperties(capture.Id, ExpressionLoadedProperties.Get(capture.Value, table));
        }

        return table;
    }
}
