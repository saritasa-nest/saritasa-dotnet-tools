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

        var containsAlsoNotify =
            SymbolUtils.ContainsAttribute(symbol, attributeName: Constants.AlsoNotifyAttributeName);
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

        var containsAccessibility =
            SymbolUtils.ContainsAttribute(symbol, attributeName: Constants.AccessibilityAttributeName);
        if (containsAccessibility)
        {
            var accessibility = SymbolUtils.GetAttribute(symbol, attributeName: Constants.AccessibilityAttributeName);

            const string getterAttributeName = "Getter";
            const string setterAttributeName = "Setter";

            foreach (var constructorArgument in accessibility.ConstructorArguments)
            {
                if (constructorArgument.Value == null)
                {
                    continue;
                }

                switch (constructorArgument.Type?.Name)
                {
                    case getterAttributeName:
                        analysis.GetterAccessibility = (Accessibility)constructorArgument.Value;
                        break;
                    case setterAttributeName:
                        analysis.SetterAccessibility = (Accessibility)constructorArgument.Value;
                        break;
                }
            }
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
