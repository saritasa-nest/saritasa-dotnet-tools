using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Models;
using Saritasa.Tools.PropertyChangedGenerator.Infrastructure.Indent;
using Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;

namespace Saritasa.Tools.PropertyChangedGenerator.Models.Metadata;

/// <summary>
/// Contains attribute metadata.
/// </summary>
public class AttributeMetadata : SyntaxMetadata
{
    private readonly AttributeAnalysis analysis;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="analysis">Attribute analysis.</param>
    public AttributeMetadata(AttributeAnalysis analysis)
    {
        this.analysis = analysis;
    }

    /// <inheritdoc />
    public override string Build(IndentWriter writer)
    {
        var builder = new StringBuilder();

        builder.Append("[");
        builder.Append($"{analysis.ContainingNamespace}.{analysis.Syntax}");
        builder.Append("]");

        var attribute = builder.ToString();

        return writer.Append(attribute).ToString();
    }
}
