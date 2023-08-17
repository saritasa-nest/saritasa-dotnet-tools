using Microsoft.CodeAnalysis;
using Saritasa.Tools.SourceGenerator.Abstractions.Analyzers;
using Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;
using Saritasa.Tools.SourceGenerator.Models.Analyzers;
using Saritasa.Tools.SourceGenerator.Utils;

namespace Saritasa.Tools.SourceGenerator.Analyzers;

/// <summary>
/// Analyzer of <see cref="IFieldSymbol"/>.
/// </summary>
public class FieldAnalyzer : ISyntaxAnalyzer<IFieldSymbol, FieldAnalysis>
{
    private readonly IEnumerable<IFieldSymbol> symbols;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="symbols">Field symbols.</param>
    public FieldAnalyzer(IEnumerable<IFieldSymbol> symbols)
    {
        this.symbols = symbols;
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
            var fieldNames = symbols.Select(symbol => symbol.Name);
            analysis.AlsoNotifyMembers = alsoNotify.ConstructorArguments
                .SelectMany(GetArgumentValues)
                .OfType<string>()
                .Where(name => fieldNames.Contains(name, StringComparer.OrdinalIgnoreCase));
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
