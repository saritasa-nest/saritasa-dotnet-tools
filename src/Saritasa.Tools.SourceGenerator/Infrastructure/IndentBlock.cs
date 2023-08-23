namespace Saritasa.Tools.SourceGenerator.Infrastructure;

/// <summary>
/// Indent block of <see cref="IndentWriter"/>.
/// </summary>
public class IndentBlock : IDisposable
{
    private readonly IndentWriter writer;
    private readonly int initialLevel;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="writer">Indent writer.</param>
    /// <param name="initialLevel">Initial indent level.</param>
    private IndentBlock(IndentWriter writer, int initialLevel)
    {
        this.writer = writer;
        this.initialLevel = initialLevel;
    }

    /// <summary>
    /// Create an indent block.
    /// </summary>
    /// <param name="writer">Indent writer.</param>
    /// <param name="level">Identation level.</param>
    /// <returns>An instance of <see cref="IndentBlock"/>.</returns>
    internal static IndentBlock Create(IndentWriter writer, int level)
    {
        var indent = new IndentBlock(writer, writer.IndentLevel);
        writer.SetIndent(level);
        return indent;
    }

    /// <inheritdoc/>
    public void Dispose() => writer.SetIndent(initialLevel);
}
