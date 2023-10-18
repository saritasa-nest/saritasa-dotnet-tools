using Microsoft.CodeAnalysis;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Analyzers;
using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Diagnostics;
using Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;
using Saritasa.Tools.PropertyChangedGenerator.Utils;

namespace Saritasa.Tools.PropertyChangedGenerator.Analyzers;

/// <summary>
/// Analyzer of node property changed implementation.
/// </summary>
public class InterfaceAnalyzer : ISyntaxAnalyzer<ITypeSymbol, InterfaceAnalysis>
{
    private readonly string[] methodNames;
    private readonly Type interfaceType;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="methodNames">Available method names.</param>
    /// <param name="interfaceType">Interface type.</param>
    public InterfaceAnalyzer(string[] methodNames, Type interfaceType)
    {
        this.methodNames = methodNames;
        this.interfaceType = interfaceType;
    }

    /// <inheritdoc/>
    public InterfaceAnalysis Analyze(ITypeSymbol symbol, SemanticModel semanticModel, IDiagnosticsScope scope)
    {
        var analysis = InterfaceAnalysis.Instance;

        var @interface = semanticModel.Compilation.GetTypeByMetadataName(interfaceType.FullName);
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
