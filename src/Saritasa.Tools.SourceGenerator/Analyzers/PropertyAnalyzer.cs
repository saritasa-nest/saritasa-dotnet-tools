using Microsoft.CodeAnalysis;
using Saritasa.Tools.SourceGenerator.Abstractions.Analyzers;
using Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;
using Saritasa.Tools.SourceGenerator.Infrastructure.Options;
using Saritasa.Tools.SourceGenerator.Models.Analyzers;
using Saritasa.Tools.SourceGenerator.Utils;

namespace Saritasa.Tools.SourceGenerator.Analyzers;

/// <summary>
/// Analyzer of <see cref="IPropertySymbol"/>.
/// </summary>
internal class PropertyAnalyzer : ISyntaxAnalyzer<IPropertySymbol, PropertyAnalysis>
{
    private readonly IEnumerable<FieldAnalysis> fields;
    private readonly FieldOptions fieldOptions;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="fields">Field analysis.</param>
    /// <param name="fieldOptions">Field options.</param>
    public PropertyAnalyzer(IEnumerable<FieldAnalysis> fields, FieldOptions fieldOptions)
    {
        this.fields = fields;
        this.fieldOptions = fieldOptions;
    }

    /// <inheritdoc/>
    public PropertyAnalysis Analyze(IPropertySymbol symbol, SemanticModel semanticModel, IDiagnosticsScope scope)
    {
        var propertyAnalysis = new PropertyAnalysis()
        {
            Name = symbol.Name,
            Type = SymbolUtils.GetType(symbol),
            Modifier = SymbolUtils.GetModifier(symbol),
        };

        var propertyFieldName = PropertyUtils.GetFieldName(symbol.Name, fieldOptions);
        var backingField = fields.FirstOrDefault(field => field.Name == propertyFieldName);
        if (backingField != null)
        {
            backingField.AssociatedProperty = propertyAnalysis;
            propertyAnalysis.BackingField = backingField;
        }

        return propertyAnalysis;
    }
}
