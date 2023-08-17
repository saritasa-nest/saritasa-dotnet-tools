using System.Text;
using Saritasa.Tools.SourceGenerator.Abstractions.Models;
using Saritasa.Tools.SourceGenerator.Infrastructure;
using Saritasa.Tools.SourceGenerator.Models.Metadata;

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

        var declaration = builder.ToString();
        return writer.Append(declaration).ToString();
    }
}

/// <summary>
/// Method metadata.
/// </summary>
public class MethodMetadata : SyntaxMetadata
{
    /// <summary>
    /// Method name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Method modifier.
    /// </summary>
    public string Modifier { get; set; } = MemberModifiers.Public;

    /// <summary>
    /// Method parameters.
    /// </summary>
    public IList<MethodParameterMetadata> Parameters { get; } = new List<MethodParameterMetadata>();

    /// <summary>
    /// Method invocations.
    /// </summary>
    public IList<InvocationMethodMetadata> Invocations { get; } = new List<InvocationMethodMetadata>();

    /// <inheritdoc/>
    public override string Build(IndentWriter writer)
    {
        var builder = new StringBuilder();

        builder.Append(Modifier);
        builder.Append($" void {Name}");
        builder.Append("(");

        var parametersWriter = IndentWriter.Instance;
        var parameters = string.Join(", ", Parameters.Select(parameter => parameter.Build(parametersWriter)));
        builder.Append(parameters);
        builder.Append(")");

        var declaration = builder.ToString();
        writer.Append(declaration).AppendLine();

        writer.Append("{").AppendLine();

        writer.SetIndent(writer.IndentLevel + 1);

        var invocationsWriter = IndentWriter.Instance;
        var invocations = string.Join("\n", Invocations.Select(inv => inv.Build(invocationsWriter)));
        writer.Append(invocations).AppendLine();

        writer.SetIndent(writer.IndentLevel - 1);
        writer.Append("}");

        return writer.ToString();
    }
}
