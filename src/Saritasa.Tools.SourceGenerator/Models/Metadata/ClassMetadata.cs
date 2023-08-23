using System.Text;
using Saritasa.Tools.SourceGenerator.Abstractions.Models;
using Saritasa.Tools.SourceGenerator.Infrastructure.Indent;
using Saritasa.Tools.SourceGenerator.Models.Metadata;

namespace Saritasa.Tools.SourceGenerator.Models.Nodes;

/// <summary>
/// Class metadata.
/// </summary>
public class ClassMetadata : SyntaxMetadata
{
    /// <summary>
    /// Class name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Class namespace.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Class modifier.
    /// </summary>
    public string Modifier { get; set; } = MemberModifiers.Public;

    /// <summary>
    /// Class methods.
    /// </summary>
    public IReadOnlyCollection<MethodMetadata> Methods { get; set; } = new List<MethodMetadata>();

    /// <summary>
    /// Class members.
    /// </summary>
    public IReadOnlyCollection<MemberMetadata> Members { get; set; } = new List<MemberMetadata>();

    /// <summary>
    /// Class inherited interfaces.
    /// </summary>
    public IList<string> Interfaces { get; set; } = new List<string>();

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
        writer.Append("using System.ComponentModel;");
        writer.AppendLine();

        writer.AppendLine().Append($"namespace {Namespace}");
        writer.AppendLine().Append("{");

        using (writer.IncreaseIndent())
        {
            var builder = new StringBuilder();
            builder.Append($"{Modifier} partial class {Name}");

            if (Interfaces.Any())
            {
                builder.Append($" : {string.Join(", ", Interfaces)}");
            }

            var declaration = builder.ToString();

            writer.AppendLine().Append(declaration);
            writer.AppendLine().Append("{");

            using (writer.IncreaseIndent())
            {
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
            }

            writer.AppendLine().Append("}");
        }

        writer.AppendLine().Append("}");

        return writer.ToString();
    }
}
