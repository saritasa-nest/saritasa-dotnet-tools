using System.Text;
using Saritasa.Tools.SourceGenerator.Infrastructure.Options;

namespace Saritasa.Tools.SourceGenerator.Infrastructure.Indent;

/// <summary>
/// Indent writer.
/// </summary>
public class IndentWriter
{
    private readonly StringBuilder builder = new();
    private readonly IndentOptions options;

    private string indent = string.Empty;
    private int indentLevel = 0;

    /// <summary>
    /// Indent level.
    /// </summary>
    public int IndentLevel => indentLevel;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="options">Application indentation options.</param>
    public IndentWriter(IndentOptions options)
    {
        this.options = options;
    }

    /// <summary>
    /// Set indent level.
    /// </summary>
    /// <param name="indentLevel">Indent level.</param>
    public void SetIndent(int indentLevel)
    {
        this.indentLevel = indentLevel;

        var builder = new StringBuilder();
        if (options.IndentStyle == IndentStyle.Tab)
        {
            builder.Append('\t');
        }
        else
        {
            builder.Append(' ', indentLevel * options.IndentSize);
        }

        indent = builder.ToString();
    }

    /// <summary>
    /// Increase an indentation by one.
    /// </summary>
    /// <returns>Disposable identation block.</returns>
    public IDisposable IncreaseIndent() => IndentBlock.Create(this, level: indentLevel + 1);

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

    /// <inheritdoc/>
    public override string ToString() => builder.ToString();
}
