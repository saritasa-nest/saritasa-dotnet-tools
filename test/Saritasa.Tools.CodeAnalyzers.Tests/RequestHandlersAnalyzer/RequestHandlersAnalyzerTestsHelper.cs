using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Saritasa.Tools.CodeAnalyzers.Tests.RequestHandlersAnalyzer
{
    /// <summary>
    /// Helper for request handlers analyzer tests.
    /// </summary>
    public class RequestHandlersAnalyzerTestsHelper
        : CSharpAnalyzerTest<Analyzers.RequestHandlersAnalyzer, MSTestVerifier>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public RequestHandlersAnalyzerTestsHelper()
        {
            ReferenceAssemblies = new ReferenceAssemblies(
                    "net6.0",
                    new PackageIdentity("Microsoft.NETCore.App.Ref", "6.0.0"),
                    Path.Combine("ref", "net6.0"))
                .AddPackages(new[]
                {
                    new PackageIdentity("MediatR", "12.1.1")
                }.ToImmutableArray());
        }
    }
}

