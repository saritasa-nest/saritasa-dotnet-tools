using Saritasa.Tools.SourceGenerator.Abstractions.Syntax;
using Saritasa.Tools.SourceGenerator.Models.Analyzers;
using Saritasa.Tools.SourceGenerator.Models.Metadata;
using Saritasa.Tools.SourceGenerator.Utils;

namespace Saritasa.Tools.SourceGenerator.Builders;

/// <summary>
/// Builder of <see cref="FieldAnalysis"/>.
/// </summary>
public class PropertyBuilder : ISyntaxBuilder<PropertyMetadata, FieldAnalysis>
{
    private readonly InvocationMethodMetadata? invokePropertyChanged;
    private readonly InvocationMethodMetadata? invokePropertyChanging;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="invokePropertyChanged">Invocation method of property changed.</param>
    /// <param name="invokePropertyChanging">Invocation method of property changing.</param>
    public PropertyBuilder(
        InvocationMethodMetadata? invokePropertyChanged = null,
        InvocationMethodMetadata? invokePropertyChanging = null)
    {
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

        var setter = new SetterMetadata(fieldMetadata);

        AddSetterDelegates(setter);

        if (analysis.AlsoNotifyMembers.Any())
        {
            foreach (var fieldName in analysis.AlsoNotifyMembers)
            {
                var fieldPropertyName = PropertyUtils.Capitalize(fieldName);
                AddSetterDelegates(setter, fieldPropertyName);
            }
        }

        return new PropertyMetadata()
        {
            Name = PropertyUtils.Capitalize(analysis.Name),
            Type = analysis.Type,
            Getter = new GetterMetadata(fieldMetadata),
            Setter = setter,
        };
    }

    private void AddSetterDelegates(SetterMetadata setter, string? propertyName = null)
    {
        var propertyChanging = invokePropertyChanging?.Clone() as InvocationMethodMetadata;
        var propertyChanged = invokePropertyChanged?.Clone() as InvocationMethodMetadata;

        if (propertyName != null)
        {
            propertyChanging?.Arguments.Add(propertyName);
            propertyChanged?.Arguments.Add(propertyName);
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
