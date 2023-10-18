using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Models;
using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Indent;

namespace Saritasa.Tools.PropertyChangedGenerator.Models.Metadata;

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
