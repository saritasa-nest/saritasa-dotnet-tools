using Saritasa.Tools.SourceGenerator.Abstractions.Models;
using Saritasa.Tools.SourceGenerator.Infrastructure;

namespace Saritasa.Tools.SourceGenerator.Models.Metadata;

/// <summary>
/// Setter metadata.
/// </summary>
public class SetterMetadata : SyntaxMetadata
{
    private readonly MemberMetadata backingField;

    /// <summary>
    /// On changed expressions.
    /// </summary>
    public IList<InvocationMethodMetadata> OnChanged { get; } = new List<InvocationMethodMetadata>();

    /// <summary>
    /// On changing expressions.
    /// </summary>
    public IList<InvocationMethodMetadata> OnChanging { get; } = new List<InvocationMethodMetadata>();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="backingField">Backing field.</param>
    public SetterMetadata(MemberMetadata backingField)
    {
        this.backingField = backingField;
    }

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
        writer.Append("set");
        writer.AppendLine();

        writer.Append("{");
        writer.SetIndent(writer.IndentLevel + 1);

        writer.AppendLine();

        foreach (var expression in OnChanging)
        {
            expression.Build(writer);
            writer.AppendLine();
        }

        writer.Append($"{backingField.Name} = value;");

        foreach (var expression in OnChanged)
        {
            writer.AppendLine();
            expression.Build(writer);
        }

        writer.AppendLine();

        writer.SetIndent(writer.IndentLevel - 1);
        writer.Append("}");

        return writer.ToString();
    }
}
