using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Saritasa.Tools.CodeAnalyzers.Tests.Verifiers.CSharpCodeFixVerifier<
    Saritasa.Tools.CodeAnalyzers.Analyzers.RequestHandlersAnalyzer,
    Saritasa.Tools.CodeAnalyzers.CodeFixProviders.RequestHandlersCodeFixProvider>;

namespace Saritasa.Tools.CodeAnalyzers.Tests.RequestHandlersAnalyzer;

/// <summary>
/// Request handlers analyzer tests.
/// </summary>
[TestClass]
public class RequestHandlersAnalyzerTests
{
    /// <summary>
    /// Request handler analyzer test.
    /// </summary>
    [TestMethod]
    public async Task DiagnosticCode_ClassWithRequestHandlerWithoutReturnType_DiagnosticWarning()
    {
        var diagnosticArgument = "TestRequestHandler";
        var sourceCode = $@"
                using System;
                using MediatR;
                using System.Threading;
                using System.Threading.Tasks;

                namespace TestApplication
                {{
                    class TestRequest : IRequest {{ }}
                    class {diagnosticArgument} : IRequestHandler<TestRequest>
                    {{
                        public Task Handle(TestRequest request, CancellationToken cancellationToken)
                             => throw new NotImplementedException();
                    }}
                }}";

        await new RequestHandlersAnalyzerTestsHelper
        {
            TestCode = sourceCode,
            ExpectedDiagnostics =
            {
                VerifyCS.Diagnostic(Analyzers.RequestHandlersAnalyzer.DiagnosticId)
                    .WithSeverity(Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
                    .WithSpan(10, 31, 10, 49)
                    .WithArguments(diagnosticArgument)
            }
        }.RunAsync();
    }

    /// <summary>
    /// Request handler analyzer test.
    /// </summary>
    [TestMethod]
    public async Task DiagnosticCode_ClassWithRequestHandlerAndReturnType_NoDiagnostic()
    {
        const string sourceCode = @"
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
                }";

        await new RequestHandlersAnalyzerTestsHelper
        {
            TestCode = sourceCode
        }.RunAsync();
    }

    /// <summary>
    /// Request handler analyzer test.
    /// </summary>
    [TestMethod]
    public async Task DiagnosticCode_ClassWithoutRequestHandler_NoDiagnostic()
    {
        const string sourceCode = @"
                namespace TestApplication
                {
                    class TestClass
                    {
                    }
                }";

        await new RequestHandlersAnalyzerTestsHelper
        {
            TestCode = sourceCode
        }.RunAsync();
    }
}
