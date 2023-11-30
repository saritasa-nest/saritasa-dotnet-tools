using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Saritasa.Tools.CodeAnalyzers.Tests.Verifiers;

/// <summary>
/// CSharp analyzer verifier.
/// </summary>
/// <typeparam name="TAnalyzer">Analyzer type.</typeparam>
public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <inheritdoc />
    public class Test : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Test()
        {
            SolutionTransforms.Add((solution, projectId) =>
            {
                var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                    compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                return solution;
            });
        }
    }
}
