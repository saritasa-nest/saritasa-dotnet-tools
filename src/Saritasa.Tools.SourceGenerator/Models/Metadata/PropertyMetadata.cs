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

        if (Getter == null && Setter == null)
        {
            builder.Append(";");
        }

        var declaration = builder.ToString();
        writer.Append(declaration);

        if (Getter != null)
        {
            writer.AppendLine();
            writer.Append("{");

            writer.SetIndent(writer.IndentLevel + 1);

            Getter?.Build(writer);
        }

        if (Setter != null)
        {
            Setter?.Build(writer);

            writer.SetIndent(writer.IndentLevel - 1);

            writer.AppendLine();
            writer.Append("}");
        }

        return writer.ToString();
    }

    /// <summary>
    /// An instance of <see cref="PropertyMetadata"/>.
    /// </summary>
    public static PropertyMetadata Instance => new();
}
