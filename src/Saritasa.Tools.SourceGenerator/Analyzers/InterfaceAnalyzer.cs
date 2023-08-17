using Microsoft.CodeAnalysis;
using Saritasa.Tools.SourceGenerator.Abstractions.Analyzers;
using Saritasa.Tools.SourceGenerator.Abstractions.Diagnostics;
using Saritasa.Tools.SourceGenerator.Models.Analyzers;
using Saritasa.Tools.SourceGenerator.Utils;

namespace Saritasa.Tools.SourceGenerator.Analyzers;

/// <summary>
/// Analyzer of node property changed implementation.
/// </summary>
public class InterfaceAnalyzer : ISyntaxAnalyzer<ITypeSymbol, InterfaceAnalysis>
{
    private readonly string[] methodNames;
    private readonly string interfaceNamespace;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="methodNames">Available method names.</param>
    /// <param name="interfaceNamespace">Interface namespace.</param>
    public InterfaceAnalyzer(string[] methodNames, string interfaceNamespace)
    {
        this.methodNames = methodNames;
        this.interfaceNamespace = interfaceNamespace;
    }

    /// <inheritdoc/>
    public InterfaceAnalysis Analyze(ITypeSymbol symbol, SemanticModel semanticModel, IDiagnosticsScope scope)
    {
        var analysis = InterfaceAnalysis.Instance;

        var @interface = semanticModel.Compilation.GetTypeByMetadataName(interfaceNamespace);
        if (@interface is null)
        {
            return analysis;
        }

        analysis.HasInterface = symbol.AllInterfaces.Contains(@interface, SymbolEqualityComparer.Default);
        if (!analysis.HasInterface)
        {
            return analysis;
        }

        var interfaceMember = @interface.GetMembers().First(member => member.Kind == SymbolKind.Event);

        analysis.EventSymbol = symbol.FindImplementationForInterfaceMember(interfaceMember) as IEventSymbol;

        if (analysis.EventSymbol != null && methodNames.Any())
        {
            analysis.MethodSymbol = SymbolUtils.FindRaiseMethod(symbol, methodNames);
        }

        return analysis;
    }
}
