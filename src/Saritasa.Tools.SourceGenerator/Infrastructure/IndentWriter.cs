using System.Text;

namespace Saritasa.Tools.SourceGenerator.Infrastructure;

/// <summary>
/// Indent writer.
/// </summary>
public class IndentWriter
{
    private readonly StringBuilder builder = new();
    private string indent = string.Empty;
    private int indentLevel = 0;

    /// <summary>
    /// Indent level.
    /// </summary>
    public int IndentLevel => indentLevel;

    /// <summary>
    /// Set indent level.
    /// </summary>
    /// <param name="indentLevel">Indent level.</param>
    public void SetIndent(int indentLevel)
    {
        this.indentLevel = indentLevel;

        if (indentLevel > 0)
        {
            indent = new string('\t', indentLevel);
        }
    }

    /// <summary>
    /// Append line.
    /// </summary>
    public IndentWriter AppendLine() => Execute(() => builder.AppendLine());

    /// <summary>
    /// Append text.
    /// </summary>
    public IndentWriter Append(string text) => Execute(() => builder.Append(indent).Append(text));

    private IndentWriter Execute(Action action)
    {
        action();
        return this;
    }

    /// <summary>
    /// An instance of <see cref="IndentWriter"/>.
    /// </summary>
    public static IndentWriter Instance => new();

    /// <inheritdoc/>
    public override string ToString() => builder.ToString();
}
