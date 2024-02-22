using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Syntax;
using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Options;
using Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;
using Saritasa.Tools.PropertyChangedGenerator.Models.Metadata;
using Saritasa.Tools.PropertyChangedGenerator.Utils;

namespace Saritasa.Tools.PropertyChangedGenerator.Builders;

/// <summary>
/// Builder of <see cref="FieldAnalysis"/>.
/// </summary>
public class PropertyBuilder : ISyntaxBuilder<PropertyMetadata, FieldAnalysis>
{
    private readonly FieldOptions fieldOptions;

    private readonly InvocationMethodMetadata? invokePropertyChanged;
    private readonly InvocationMethodMetadata? invokePropertyChanging;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="fieldOptions">Field options.</param>
    /// <param name="invokePropertyChanged">Invocation method of property changed.</param>
    /// <param name="invokePropertyChanging">Invocation method of property changing.</param>
    public PropertyBuilder(
        FieldOptions fieldOptions,
        InvocationMethodMetadata? invokePropertyChanged = null,
        InvocationMethodMetadata? invokePropertyChanging = null)
    {
        this.fieldOptions = fieldOptions;

        this.invokePropertyChanged = invokePropertyChanged;
        this.invokePropertyChanging = invokePropertyChanging;
    }

    /// <inheritdoc/>
    public PropertyMetadata Build(FieldAnalysis analysis)
    {
        var fieldMetadata = new MemberMetadata
        {
            Name = analysis.Name,
            Type = analysis.Type,
            Modifier = analysis.Modifier,
        };

        var setter = new SetterMetadata(fieldMetadata, analysis.SetterAccessibility);

        AddSetterDelegates(setter);

        foreach (var fieldName in analysis.AlsoNotifyMembers)
        {
            var fieldPropertyName = FieldUtils.GetPropertyName(fieldName, fieldOptions);
            AddSetterDelegates(setter, fieldPropertyName);
        }

        return new PropertyMetadata()
        {
            Name = FieldUtils.GetPropertyName(analysis.Name, fieldOptions),
            Type = analysis.Type,
            Getter = new GetterMetadata(fieldMetadata, analysis.GetterAccessibility),
            Setter = setter,
        };
    }

    private void AddSetterDelegates(SetterMetadata setter, string? propertyName = null)
    {
        var propertyChanging = invokePropertyChanging?.Clone() as InvocationMethodMetadata;
        var propertyChanged = invokePropertyChanged?.Clone() as InvocationMethodMetadata;

        if (propertyName != null)
        {
            propertyChanging?.Arguments.Add($"nameof({propertyName})");
            propertyChanged?.Arguments.Add($"nameof({propertyName})");
        }

        if (propertyChanging != null)
        {
            setter.OnChanging.Add(propertyChanging);
        }

        if (propertyChanged != null)
        {
            setter.OnChanged.Add(propertyChanged);
        }
    }
}
