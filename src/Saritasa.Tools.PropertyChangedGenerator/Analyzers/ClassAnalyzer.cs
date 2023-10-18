using Microsoft.CodeAnalysis;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Analyzers;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Diagnostics;
using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Options;
using Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;
using Saritasa.Tools.PropertyChangedGenerator.Utils;

namespace Saritasa.Tools.PropertyChangedGenerator.Analyzers;

/// <summary>
/// Analyzer for partial class based on <see cref="ITypeSymbol"/> code analysis API.
/// </summary>
public class ClassAnalyzer : ISyntaxAnalyzer<ITypeSymbol, ClassAnalysis>
{
    private readonly OptionsManager optionsManager;

    private readonly ISyntaxAnalyzer<ITypeSymbol, InterfaceAnalysis> propertyChangedAnalyzer;
    private readonly ISyntaxAnalyzer<ITypeSymbol, InterfaceAnalysis> propertyChangingAnalyzer;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="optionsManager">Options manager.</param>
    public ClassAnalyzer(OptionsManager optionsManager)
    {
        this.optionsManager = optionsManager;

        this.propertyChangedAnalyzer = InterfaceAnalyzerFactory.Create(optionsManager, InterfaceType.PropertyChanged);
        this.propertyChangingAnalyzer = InterfaceAnalyzerFactory.Create(optionsManager, InterfaceType.PropertyChanging);
    }

    /// <inheritdoc/>
    public ClassAnalysis Analyze(ITypeSymbol symbol, SemanticModel semanticModel, IDiagnosticsScope scope)
    {
        var analysis = GetMembersAnalysis(symbol, semanticModel, scope);

        return new ClassAnalysis
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
        var fields = members.OfType<IFieldSymbol>()
            .Where(field => field.CanBeReferencedByName)
            .Where(field => FieldUtils.FollowConvention(field.Name, optionsManager.FieldOptions));
        var properties = members.OfType<IPropertySymbol>();

        var fieldAnalysis = new List<FieldAnalysis>();
        var fieldAnalyzer = new FieldAnalyzer(fields, properties);
        foreach (var field in fields)
        {
            var analysis = fieldAnalyzer.Analyze(field, semanticModel, scope);
            fieldAnalysis.Add(analysis);
        }

        var propertyAnalysis = new List<PropertyAnalysis>();
        var propertyAnalyzer = new PropertyAnalyzer(fieldAnalysis, optionsManager.FieldOptions);
        foreach (var property in properties)
        {
            var analysis = propertyAnalyzer.Analyze(property, semanticModel, scope);
            propertyAnalysis.Add(analysis);
        }

        return propertyAnalysis.Concat<MemberAnalysis>(fieldAnalysis).ToArray();
    }
}
