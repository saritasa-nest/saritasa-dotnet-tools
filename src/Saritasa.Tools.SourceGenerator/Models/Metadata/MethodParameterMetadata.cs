using System.Text;
using Saritasa.Tools.SourceGenerator.Abstractions.Models;
using Saritasa.Tools.SourceGenerator.Infrastructure.Indent;

namespace Saritasa.Tools.SourceGenerator.Models.Nodes;

/// <summary>
/// Method parameter metadata.
/// </summary>
public class MethodParameterMetadata : SyntaxMetadata
{
    /// <summary>
    /// Parameter name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Parameter type.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Parameter attribute name.
    /// </summary>
    public string? AttributeName { get; set; }

    /// <summary>
    /// Is parameter nullable.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Parameter default value.
    /// </summary>
    public string? Value { get; set; }

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
        var builder = new StringBuilder();
        var declaration = Build(builder);

        return writer.Append(declaration).ToString();
    }

    /// <summary>
    /// Build metadata.
    /// </summary>
    /// <param name="builder">String builder.</param>
    /// <returns>String metadata representation.</returns>
    public string Build(StringBuilder builder)
    {
        if (AttributeName != null)
        {
            builder.Append($"[{AttributeName}] ");
        }

        builder.Append(Type);
        if (IsNullable)
        {
            builder.Append("? ");
        }

        builder.Append(Name);
        if (Value != null)
        {
            builder.Append($" = {Value}");
        }

        return builder.ToString();
    }
}
