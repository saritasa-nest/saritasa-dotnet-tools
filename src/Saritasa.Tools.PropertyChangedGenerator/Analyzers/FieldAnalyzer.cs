using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Analyzers;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Diagnostics;
using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Options;
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
    private readonly FieldOptions options;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="fields">Field symbols.</param>
    /// <param name="properties">Property symbols.</param>
    /// <param name="options">Field options.</param>
    public FieldAnalyzer(IEnumerable<IFieldSymbol> fields, IEnumerable<IPropertySymbol> properties, FieldOptions options)
    {
        this.fields = fields;
        this.properties = properties;
        this.options = options;
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

            var fieldNames = fields.Select(field => field.Name);
            var propertyNames = properties.Select(field => field.Name);
            var fieldPropertyNames = fields.Select(field => FieldUtils.GetPropertyName(field.Name, options));
            var names = fieldNames.Concat(propertyNames).Concat(fieldPropertyNames);

            analysis.AlsoNotifyMembers = alsoNotify.ConstructorArguments
                .SelectMany(GetArgumentValues)
                .OfType<string>()
                .Where(names.Contains)
                .ToArray();
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

        var containsAttributes = SymbolUtils.ContainsAttribute(symbol, attributeName: Constants.PropertyAttributeName);
        if (containsAttributes)
        {
            var attributes = SymbolUtils.GetAttributes(symbol, attributeName: Constants.PropertyAttributeName);

            foreach (var attribute in attributes)
            {
                // This shouldn't happen, but let's keep it in mind.
                if (attribute.AttributeClass is not { IsGenericType: true })
                {
                    continue;
                }

                if (attribute.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax syntax)
                {
                    continue;
                }

                var genericAttribute = attribute.AttributeClass.TypeArguments[0];
                var genericAttributeAnalysis = new AttributeAnalysis(
                    @namespace: SymbolUtils.GetNamespace(genericAttribute),
                    genericAttribute.Name,
                    syntax.ArgumentList);

                analysis.AddAttribute(genericAttributeAnalysis);
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

        if (argument.Kind != TypedConstantKind.Array)
        {
            yield break;
        }

        foreach (var innerValue in argument.Values.SelectMany(GetArgumentValues))
        {
            yield return innerValue;
        }
    }
}
