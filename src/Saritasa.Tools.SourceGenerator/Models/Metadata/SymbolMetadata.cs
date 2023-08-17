using System.Text;
using Saritasa.Tools.SourceGenerator.Abstractions.Models;
using Saritasa.Tools.SourceGenerator.Infrastructure;
using Saritasa.Tools.SourceGenerator.Models.Metadata;

namespace Saritasa.Tools.SourceGenerator.Models.Nodes;

/// <summary>
/// Symbol metadata.
/// </summary>
public class SymbolMetadata : SyntaxMetadata
{
    /// <summary>
    /// Symbol name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Symbol namespace.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Symbol modifier.
    /// </summary>
    public string Modifier { get; set; } = MemberModifiers.Public;

    /// <summary>
    /// Symbol methods.
    /// </summary>
    public IReadOnlyCollection<MethodMetadata> Methods { get; set; } = new List<MethodMetadata>();

    /// <summary>
    /// Symbol members.
    /// </summary>
    public IReadOnlyCollection<MemberMetadata> Members { get; set; } = new List<MemberMetadata>();

    /// <summary>
    /// Interfaces.
    /// </summary>
    public IList<string> Interfaces { get; set; } = new List<string>();

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
        writer.Append("using System.ComponentModel;");
        writer.AppendLine();

        writer.AppendLine().Append($"namespace {Namespace}");
        writer.AppendLine().Append("{");

        writer.SetIndent(indentLevel: 1);

        var builder = new StringBuilder();
        builder.Append($"{Modifier} partial class {Name}");

        if (Interfaces.Any())
        {
            builder.Append($" : {string.Join(", ", Interfaces)}");
        }

        var declaration = builder.ToString();

        writer.AppendLine().Append(declaration);
        writer.AppendLine().Append("{");

        writer.SetIndent(indentLevel: 2);

        foreach (var member in Members)
        {
            writer.AppendLine();
            member.Build(writer);
            writer.AppendLine();
        }

        foreach (var method in Methods)
        {
            writer.AppendLine();
            method.Build(writer);
        }

        writer.SetIndent(indentLevel: 1);
        writer.AppendLine().Append("}");

        writer.SetIndent(indentLevel: 0);
        writer.AppendLine().Append("}");

        return writer.ToString();
    }
}
