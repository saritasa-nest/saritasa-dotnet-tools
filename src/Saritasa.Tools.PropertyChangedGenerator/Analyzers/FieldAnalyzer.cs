using Microsoft.CodeAnalysis;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Analyzers;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Diagnostics;
using Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;
using Saritasa.Tools.PropertyChangedGenerator.Utils;

namespace Saritasa.Tools.PropertyChangedGenerator.Analyzers;

/// <summary>
/// Analyzer of <see cref="IFieldSymbol"/>.
/// </summary>
public class FieldAnalyzer : ISyntaxAnalyzer<IFieldSymbol, FieldAnalysis>
{
    private readonly IEnumerable<IFieldSymbol> fields;
    private readonly IEnumerable<IPropertySymbol> properties;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="fields">Field symbols.</param>
    /// <param name="properties">Property symbols.</param>
    public FieldAnalyzer(IEnumerable<IFieldSymbol> fields, IEnumerable<IPropertySymbol> properties)
    {
        this.fields = fields;
        this.properties = properties;
    }

    /// <inheritdoc/>
    public FieldAnalysis Analyze(IFieldSymbol symbol, SemanticModel semanticModel, IDiagnosticsScope scope)
    {
        var analysis = new FieldAnalysis
        {
            Name = symbol.Name,
            Type = SymbolUtils.GetType(symbol),
            Modifier = SymbolUtils.GetModifier(symbol),
            DoNotNotify = SymbolUtils.ContainsAttribute(symbol, attributeName: Constants.DoNotNotifyAttributeName),
        };

        var containsAlsoNotify = SymbolUtils.ContainsAttribute(symbol, attributeName: Constants.AlsoNotifyAttributeName);
        if (containsAlsoNotify)
        {
            var alsoNotify = SymbolUtils.GetAttribute(symbol, attributeName: Constants.AlsoNotifyAttributeName)!;

            var fieldNames = fields.Select(symbol => symbol.Name);
            var propertyNames = properties.Select(symbol => symbol.Name);
            var names = fieldNames.Concat(propertyNames);

            analysis.AlsoNotifyMembers = alsoNotify.ConstructorArguments
                .SelectMany(GetArgumentValues)
                .OfType<string>()
                .Where(names.Contains);
        }

        return analysis;
    }

    private static IEnumerable<object?> GetArgumentValues(TypedConstant argument)
    {
        if (argument.Kind == TypedConstantKind.Primitive)
        {
            yield return argument.Value;
        }

        if (argument.Kind == TypedConstantKind.Array)
        {
            foreach (var value in argument.Values)
            {
                yield return value.Value;
            }
        }
    }
}
