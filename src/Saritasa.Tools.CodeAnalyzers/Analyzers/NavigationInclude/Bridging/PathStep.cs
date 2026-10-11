namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// What crossing a <see cref="Bridge"/> does to the navigation path the search asks for.
/// </summary>
/// <remarks>
/// Most bridges move the same entities, so the path stays as it is. A projection such as
/// <c>users.Select(u =&gt; u.Settings)</c> hands back other entities, the Settings of users, so "Profile" of its
/// result is "Settings.Profile" of users. What it adds is only known at the call, so the step names where to read it.
/// </remarks>
internal abstract class PathStep
{
    /// <summary>
    /// The same entities on both sides: <c>users.Where(...)</c>, <c>users[0]</c>. The path stays as it is.
    /// </summary>
    public static readonly PathStep Keep = new KeepStep();

    /// <summary>
    /// The result holds what a selector reads from each entity: <c>users.Select(u =&gt; u.Settings)</c>. The
    /// selector's member chain goes in front of the path.
    /// </summary>
    /// <param name="parameterName">Name of the parameter that takes the selector.</param>
    /// <returns>The step.</returns>
    public static PathStep SelectorChain(string parameterName) => new SelectorChainStep(parameterName);

    /// <summary>
    /// <see cref="Keep"/>.
    /// </summary>
    public sealed class KeepStep : PathStep
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal KeepStep()
        {
        }
    }

    /// <summary>
    /// <see cref="SelectorChain"/>.
    /// </summary>
    public sealed class SelectorChainStep : PathStep
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal SelectorChainStep(string parameterName)
        {
            ParameterName = parameterName;
        }

        /// <summary>
        /// Name of the parameter that takes the selector, e.g. "selector".
        /// </summary>
        public string ParameterName { get; }
    }
}
