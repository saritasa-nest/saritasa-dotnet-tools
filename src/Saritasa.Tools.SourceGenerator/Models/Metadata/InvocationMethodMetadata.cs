using System.Text;
using Saritasa.Tools.SourceGenerator.Abstractions.Models;
using Saritasa.Tools.SourceGenerator.Infrastructure.Indent;

namespace Saritasa.Tools.SourceGenerator.Models.Metadata;

/// <summary>
/// Invocation method metadata.
/// </summary>
public class InvocationMethodMetadata : SyntaxMetadata
{
    /// <summary>
    /// Method name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Is nullable.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Is delegate.
    /// </summary>
    public bool IsDelegate { get; set; }

    /// <summary>
    /// Expression arguments.
    /// </summary>
    public IList<string> Arguments { get; private set; } = new List<string>();

    /// <summary>
    /// Readable arguments.
    /// </summary>
    public string Args => string.Join(", ", Arguments);

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
        var builder = new StringBuilder();

        builder.Append($"{Name}");

        if (IsNullable)
        {
            builder.Append("?");
        }

        if (IsDelegate)
        {
            builder.Append(".Invoke");
        }

        builder.Append("(");
        builder.Append(Args);
        builder.Append(");");

        var member = builder.ToString();

        return writer.Append(member).ToString();
    }

    /// <summary>
    /// An instance of <see cref="InvocationMethodMetadata"/>.
    /// </summary>
    public static InvocationMethodMetadata Instance => new();

    /// <inheritdoc/>
    public override object Clone()
    {
        var metadata = (InvocationMethodMetadata)MemberwiseClone();
        metadata.Arguments = new List<string>();
        return metadata;
    }
}
