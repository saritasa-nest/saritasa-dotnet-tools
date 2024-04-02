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

        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            var syntax = reference.GetSyntax();

            var field = SyntaxUtils.FindParentOfType<FieldDeclarationSyntax>(syntax);
            if (field == null)
            {
                continue;
            }

            var attributes = field.AttributeLists
                .Where(IsPropertyAttribute)
                .SelectMany(list => list.Attributes);

            foreach (var attribute in attributes)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(attribute);

                // Roslyn ignores attributes in an attribute list with an invalid target, so we can't get the AttributeData as usual.
                // To reconstruct all necessary attribute info to generate the serialized model, we use the following steps:
                //   - We try to get the attribute symbol from the semantic model, for the current attribute syntax. In case this is not
                //     available (in theory it shouldn't, but it can be), we try to get it from the candidate symbols list for the node.
                //     If there are no candidates or more than one, we just issue a diagnostic and stop processing the current attribute.
                //     The returned symbols might be method symbols (constructor attribute) so in that case we can get the declaring type.
                //   - We then go over each attribute argument expression and get the operation for it. This will still be available even
                //     though the rest of the attribute is not validated nor bound at all. From the operation we can still retrieve all
                //     constant values to build the AttributeInfo model. After all, attributes only support constant values, typeof(T)
                //     expressions, or arrays of either these two types, or of other arrays with the same rules, recursively.
                //   - From the syntax, we can also determine the identifier names for named attribute arguments, if any.
                // There is no need to validate anything here: the attribute will be forwarded as is, and then Roslyn will validate on the
                // generated property. Users will get the same validation they'd have had directly over the field. The only drawback is the
                // lack of IntelliSense when constructing attributes over the field, but this is the best we can do from this end anyway.
                if (!symbolInfo.TryGetAttributeTypeSymbol(out var nameSymbol))
                {
                    continue;
                }

                var @namespace = SymbolUtils.GetNamespace(nameSymbol!);
                var attributeAnalysis = new AttributeAnalysis(attribute, @namespace);

                analysis.AddAttribute(attributeAnalysis);
            }
        }

        return analysis;
    }

    private static bool IsPropertyAttribute(AttributeListSyntax attribute)
        => attribute.Target != null && attribute.Target.Identifier.IsKind(SyntaxKind.PropertyKeyword);

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
