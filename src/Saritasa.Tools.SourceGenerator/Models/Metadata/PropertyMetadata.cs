using System.Text;
using Saritasa.Tools.SourceGenerator.Infrastructure;

namespace Saritasa.Tools.SourceGenerator.Models.Metadata;

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
    /// Indicates if property is delegate.
    /// </summary>
    public bool IsDelegate { get; set; }

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
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
