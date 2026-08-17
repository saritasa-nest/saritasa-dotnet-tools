using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.RequestHandlersAnalyzer;

/// <summary>
/// Request handlers analyzer tests.
/// </summary>
public class RequestHandlersAnalyzerTests
{
    private static readonly ReferenceAssemblies References = new ReferenceAssemblies(
            "net6.0",
            new PackageIdentity("Microsoft.NETCore.App.Ref", "6.0.0"),
            Path.Combine("ref", "net6.0"))
        .AddPackages(
            new[]
            {
                new PackageIdentity("MediatR", "12.1.1")
            }.ToImmutableArray());

    private static async Task VerifyAnalyzerAsync(string source)
    {
        var test = new CSharpAnalyzerTest<Analyzers.RequestHandlersAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = References
        };
        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Request handler analyzer test.
    /// </summary>
    [Fact]
    public async Task DiagnosticCode_ClassWithRequestHandlerWithoutReturnType_DiagnosticWarning()
    {
        var sourceCode =
            /* lang=c# */
            """
                using System;
                using MediatR;
                using System.Threading;
                using System.Threading.Tasks;

                namespace TestApplication
                {
                    class TestRequest : IRequest { }
                    class [|TestRequestHandler|] : IRequestHandler<TestRequest>
                    {
                        public Task Handle(TestRequest request, CancellationToken cancellationToken)
                             => throw new NotImplementedException();
                    }
                }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Request handler analyzer test.
    /// </summary>
    [Fact]
    public async Task DiagnosticCode_ClassWithRequestHandlerAndReturnType_NoDiagnostic()
    {
        var sourceCode =
            /* lang=c# */
            """
                using System;
                using MediatR;
                using System.Threading;
                using System.Threading.Tasks;

                namespace TestApplication
                {
                    class TestRequest : IRequest<int> { }
                    class TestRequestHandler : IRequestHandler<TestRequest, int>
                    {
                        public Task<int> Handle(TestRequest request, CancellationToken cancellationToken)
                             => throw new NotImplementedException();
                    }
                }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }

    /// <summary>
    /// Request handler analyzer test.
    /// </summary>
    [Fact]
    public async Task DiagnosticCode_ClassWithoutRequestHandler_NoDiagnostic()
    {
        const string sourceCode =
            /* lang=c# */
            """
                namespace TestApplication
                {
                    class TestClass
                    {
                    }
                }
            """;

        await VerifyAnalyzerAsync(sourceCode);
    }
}
