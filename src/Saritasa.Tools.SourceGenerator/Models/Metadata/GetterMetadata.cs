using Saritasa.Tools.SourceGenerator.Abstractions.Models;
using Saritasa.Tools.SourceGenerator.Infrastructure.Indent;

namespace Saritasa.Tools.SourceGenerator.Models.Metadata;

/// <summary>
/// Getter metadata.
/// </summary>
public class GetterMetadata : SyntaxMetadata
{
    private readonly MemberMetadata backingField;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="backingField">Backing field.</param>
    public GetterMetadata(MemberMetadata backingField)
    {
        this.backingField = backingField;
    }

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
        writer.AppendLine();

        writer.Append($"get => {backingField.Name};");

        writer.AppendLine();

        return writer.ToString();
    }
}
