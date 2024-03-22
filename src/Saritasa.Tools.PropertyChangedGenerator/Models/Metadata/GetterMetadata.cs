using System.Text;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Models;
using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Indent;

namespace Saritasa.Tools.PropertyChangedGenerator.Models.Metadata;

/// <summary>
/// Getter metadata.
/// </summary>
public class GetterMetadata : SyntaxMetadata
{
    private readonly MemberMetadata backingField;
    private readonly Accessibility? accessibility;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="backingField">Backing field.</param>
    /// <param name="accessibility">Getter accessibility.</param>
    public GetterMetadata(MemberMetadata backingField, Accessibility? accessibility)
    {
        this.backingField = backingField;
        this.accessibility = accessibility;
    }

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
        writer.AppendLine();

        var sb = new StringBuilder();

        if (accessibility != null)
        {
            sb.Append($"{accessibility.Value} ".ToLower());
        }

        sb.Append($"get => {backingField.Name};");

        writer.Append(sb.ToString());

        writer.AppendLine();

        return writer.ToString();
    }
}
