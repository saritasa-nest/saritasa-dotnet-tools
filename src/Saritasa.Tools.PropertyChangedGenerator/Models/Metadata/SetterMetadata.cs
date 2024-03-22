using System.Text;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Models;
using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Indent;

namespace Saritasa.Tools.PropertyChangedGenerator.Models.Metadata;

/// <summary>
/// Setter metadata.
/// </summary>
public class SetterMetadata : SyntaxMetadata
{
    private readonly MemberMetadata backingField;
    private readonly Accessibility? accessibility;

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
    /// <param name="accessibility">Setter accessibility. Optional.</param>
    public SetterMetadata(MemberMetadata backingField, Accessibility? accessibility = null)
    {
        this.backingField = backingField;
        this.accessibility = accessibility;
    }

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
        var sb = new StringBuilder();

        if (accessibility != null)
        {
            sb.Append($"{accessibility.Value} ".ToLower());
        }

        sb.Append("set");

        writer.Append(sb.ToString());
        writer.AppendLine();

        writer.Append("{").AppendLine();

        using (writer.IncreaseIndent())
        {
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
        }

        writer.AppendLine().Append("}");

        return writer.ToString();
    }
}
