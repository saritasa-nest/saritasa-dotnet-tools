using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Flow;

/// <summary>
/// One thing the <see cref="Walker"/> found above the place a variable is read: what gave the variable its
/// value, or why nothing did.
/// </summary>
/// <remarks>
/// A write is a fact about the code and never a decision. What it means for the navigation property is decided
/// by <see cref="IncludeSearch"/>, which is why nothing here says "loaded".
/// </remarks>
internal abstract class Write
{
    /// <summary>
    /// "user = x", "var user = x", "#1 = x" and "var (id, user) = pair": the variable got the value of
    /// an expression.
    /// </summary>
    public sealed class Written : Write
    {
        /// <summary>
        /// Initializes the write.
        /// </summary>
        /// <param name="value">The value the variable got.</param>
        public Written(Value value)
        {
            Value = value;
        }

        /// <summary>
        /// The value the variable got.
        /// </summary>
        public Value Value { get; }
    }

    /// <summary>
    /// "user.Profile = x": the navigation property the search looks for was written on the variable itself.
    /// </summary>
    public sealed class MemberWritten : Write
    {
    }

    /// <summary>
    /// "dictionary.TryGetValue(id, out var user)": a call wrote the variable through an out argument.
    /// </summary>
    public sealed class OutArgument : Write
    {
        /// <summary>
        /// Initializes the write.
        /// </summary>
        /// <param name="call">The call that wrote the out argument.</param>
        public OutArgument(Value call)
        {
            Call = call;
        }

        /// <summary>
        /// The call that wrote the out argument.
        /// </summary>
        public Value Call { get; }
    }

    /// <summary>
    /// The walk reached the start of a method, constructor or local function, so the variable is a parameter
    /// and the caller is the one that fills it.
    /// </summary>
    public sealed class MethodParameter : Write
    {
        /// <summary>
        /// Initializes the write.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public MethodParameter(IParameterSymbol parameter)
        {
            Parameter = parameter;
        }

        /// <summary>
        /// The parameter.
        /// </summary>
        public IParameterSymbol Parameter { get; }
    }

    /// <summary>
    /// The walk reached the start of a lambda body, so the variable is a parameter of the lambda and the call
    /// that created it is the one that fills it: "users.Select(u =&gt; ...)".
    /// </summary>
    public sealed class LambdaParameter : Write
    {
        /// <summary>
        /// Initializes the write.
        /// </summary>
        /// <param name="parameter">The parameter of the lambda.</param>
        /// <param name="lambda">The lambda itself, which knows the call that created it.</param>
        /// <param name="creation">The statement that creates the lambda.</param>
        public LambdaParameter(
            IParameterSymbol parameter,
            IFlowAnonymousFunctionOperation lambda,
            CodePosition creation)
        {
            Parameter = parameter;
            Lambda = lambda;
            Creation = creation;
        }

        /// <summary>
        /// The parameter of the lambda.
        /// </summary>
        public IParameterSymbol Parameter { get; }

        /// <summary>
        /// The lambda itself, which knows the call that created it.
        /// </summary>
        public IFlowAnonymousFunctionOperation Lambda { get; }

        /// <summary>
        /// The statement that creates the lambda.
        /// </summary>
        public CodePosition Creation { get; }
    }

    /// <summary>
    /// A loop came back to a block the walk already read, so this path adds nothing new.
    /// </summary>
    public sealed class NothingNew : Write
    {
    }

    /// <summary>
    /// Nothing wrote the variable and nobody fills it from outside, for example a local declared without
    /// a value on a path that never assigns it.
    /// </summary>
    public sealed class NeverWritten : Write
    {
    }

    /// <summary>
    /// The walk cannot read this path: unreachable code, or an out argument of something that is not a call.
    /// </summary>
    public sealed class Unreadable : Write
    {
    }
}
