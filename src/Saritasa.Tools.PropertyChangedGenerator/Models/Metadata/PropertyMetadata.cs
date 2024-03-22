using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Indent;
using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Options;

namespace Saritasa.Tools.PropertyChangedGenerator.Models.Metadata;

/// <summary>
/// Property metadata.
/// </summary>
public class PropertyMetadata : MemberMetadata
{
    /// <summary>
    /// Property getter.
    /// </summary>
    public GetterMetadata? Getter { get; set; }

    /// <summary>
    /// Property setter.
    /// </summary>
    public SetterMetadata? Setter { get; set; }

    /// <summary>
    /// Property attributes.
    /// </summary>
    public IEnumerable<AttributeMetadata> Attributes { get; set; } = Enumerable.Empty<AttributeMetadata>();

    /// <summary>
    /// Indicates if property is delegate.
    /// </summary>
    public bool IsDelegate { get; set; }

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
        foreach (var attribute in Attributes)
        {
            attribute.Build(writer);

            writer.AppendLine();
        }

        var builder = new StringBuilder();

        builder.Append(Modifier);

        if (IsDelegate)
        {
            builder.Append(" event");
        }

        builder.Append($" {Type}");

        if (IsNullable)
        {
            builder.Append("?");
        }

        builder.Append($" {Name}");

        var shouldBuildAccessors = Getter != null && Setter != null;

        if (!shouldBuildAccessors)
        {
            var property = builder.Append(";").ToString();
            return writer.Append(property).ToString();
        }

        var declaration = builder.ToString();

        writer.Append(declaration);

        writer.AppendLine().Append("{");

        using (writer.IncreaseIndent())
        {
            Getter?.Build(writer);
            Setter?.Build(writer);
        }

        writer.AppendLine().Append("}");

        return writer.ToString();
    }

    /// <summary>
    /// An instance of <see cref="PropertyMetadata"/>.
    /// </summary>
    public static PropertyMetadata Instance => new();
}
