using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Syntax;
using Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;
using Saritasa.Tools.PropertyChangedGenerator.Models.Metadata;

namespace Saritasa.Tools.PropertyChangedGenerator.Builders;

/// <summary>
/// Builder for <see cref="InterfaceAnalysis"/>.
/// </summary>
public class InterfacePropertyBuilder : ISyntaxBuilder<PropertyMetadata, InterfaceAnalysis>
{
    private readonly string propertyName;
    private readonly Type propertyType;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    /// <param name="propertyType">Property type.</param>
    public InterfacePropertyBuilder(string propertyName, Type propertyType)
    {
        this.propertyName = propertyName;
        this.propertyType = propertyType;
    }

    /// <inheritdoc/>
    public PropertyMetadata Build(InterfaceAnalysis analysis)
    {
        var method = new PropertyMetadata()
        {
            Name = propertyName,
            Type = propertyType.Name,
            Modifier = MemberModifiers.Public,
            IsDelegate = true,
            IsNullable = true,
        };
        return method;
    }
}
