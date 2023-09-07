using Saritasa.Tools.PropertyChangedGenerator.Abstractions.Syntax;
using Saritasa.Tools.PropertyChangedGenerator.Models.Analyzers;
using Saritasa.Tools.PropertyChangedGenerator.Models.Metadata;
using Saritasa.Tools.PropertyChangedGenerator.Models.Nodes;

namespace Saritasa.Tools.PropertyChangedGenerator.Builders;

/// <summary>
/// Interface raise method builder for <see cref="InterfaceAnalysis.EventSymbol"/> implementation member.
/// </summary>
public class InterfaceMethodBuilder : ISyntaxBuilder<MethodMetadata, InterfaceAnalysis>
{
    private readonly string methodName;
    private readonly InvocationMethodMetadata memberInvocation;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="methodName">Method name.</param>
    /// <param name="memberInvocation">Interface member invocation.</param>
    public InterfaceMethodBuilder(string methodName, InvocationMethodMetadata memberInvocation)
    {
        this.methodName = methodName;
        this.memberInvocation = memberInvocation;
    }

    /// <inheritdoc/>
    public MethodMetadata Build(InterfaceAnalysis analysis)
    {
        var method = new MethodMetadata()
        {
            Name = methodName,
            Modifier = MemberModifiers.Protected,
        };
        method.Invocations.Add(memberInvocation);

        var defaultParameter = new MethodParameterMetadata
        {
            Name = "propertyName",
            AttributeName = "System.Runtime.CompilerServices.CallerMemberNameAttribute",
            Type = typeof(string),
            IsNullable = true,
            Value = "null",
        };
        method.Parameters.Add(defaultParameter);

        return method;
    }
}
