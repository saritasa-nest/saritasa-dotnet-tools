using Microsoft.CodeAnalysis;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Diagnostics;

namespace Saritasa.Tools.PropertyChangedGenerator.Abstractions.Analyzers;

/// <summary>
/// Syntax analyzer.
/// </summary>
public interface ISyntaxAnalyzer<TSymbol, TAnalysis>
    where TSymbol : ISymbol
    where TAnalysis : ISyntaxAnalysis
{
    /// <summary>
    /// Analyze the node.
    /// </summary>
    /// <param name="symbol">Symbol to analyze.</param>
    /// <param name="semanticModel">Node semantic model.</param>
    /// <param name="scope">Diagnostics scope.</param>
    /// <returns>Syntax analysis.</returns>
    TAnalysis Analyze(TSymbol symbol, SemanticModel semanticModel, IDiagnosticsScope scope);
}
