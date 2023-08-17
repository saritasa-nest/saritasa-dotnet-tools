using Microsoft.CodeAnalysis;
using Saritasa.Tools.SourceGenerator.Abstractions.Analyzers;
using Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;
using Saritasa.Tools.SourceGenerator.Infrastructure;
using Saritasa.Tools.SourceGenerator.Models.Analyzers;
using Saritasa.Tools.SourceGenerator.Utils;

namespace Saritasa.Tools.SourceGenerator.Analyzers;

/// <summary>
/// Analyzer of symbol node.
/// </summary>
public class SymbolAnalyzer : ISyntaxAnalyzer<ITypeSymbol, SymbolAnalysis>
{
    private readonly ISyntaxAnalyzer<ITypeSymbol, InterfaceAnalysis> propertyChangedAnalyzer;
    private readonly ISyntaxAnalyzer<ITypeSymbol, InterfaceAnalysis> propertyChangingAnalyzer;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="optionsManager">Options manager.</param>
    public SymbolAnalyzer(OptionsManager optionsManager)
    {
        this.propertyChangedAnalyzer = InterfaceAnalyzerFactory.Create(optionsManager, InterfaceType.PropertyChanged);
        this.propertyChangingAnalyzer = InterfaceAnalyzerFactory.Create(optionsManager, InterfaceType.PropertyChanging);
    }

    /// <inheritdoc/>
    public SymbolAnalysis Analyze(ITypeSymbol symbol, SemanticModel semanticModel, IDiagnosticsScope scope)
    {
        var analysis = GetMembersAnalysis(symbol, semanticModel, scope);

        return new SymbolAnalysis
        {
            Symbol = symbol,
            PropertyChanged = propertyChangedAnalyzer.Analyze(symbol, semanticModel, scope),
            PropertyChanging = propertyChangingAnalyzer.Analyze(symbol, semanticModel, scope),
            Properties = analysis.OfType<PropertyAnalysis>().ToList(),
            Fields = analysis.OfType<FieldAnalysis>().ToList(),
        };
    }

    private IReadOnlyCollection<MemberAnalysis> GetMembersAnalysis(
        ITypeSymbol symbol,
        SemanticModel semanticModel,
        IDiagnosticsScope scope)
    {
        var members = SymbolUtils.GetMembers(symbol);
        var fields = members.OfType<IFieldSymbol>();
        var properties = members.OfType<IPropertySymbol>();

        var fieldAnalysis = new List<FieldAnalysis>();
        var fieldAnalyzer = new FieldAnalyzer(fields);
        foreach (var field in fields)
        {
            var analysis = fieldAnalyzer.Analyze(field, semanticModel, scope);
            fieldAnalysis.Add(analysis);
        }

        var propertyAnalysis = new List<PropertyAnalysis>();
        var propertyAnalyzer = new PropertyAnalyzer(fieldAnalysis);
        foreach (var property in properties)
        {
            var analysis = propertyAnalyzer.Analyze(property, semanticModel, scope);
            propertyAnalysis.Add(analysis);
        }

        return propertyAnalysis.Concat<MemberAnalysis>(fieldAnalysis).ToArray();
    }
}
