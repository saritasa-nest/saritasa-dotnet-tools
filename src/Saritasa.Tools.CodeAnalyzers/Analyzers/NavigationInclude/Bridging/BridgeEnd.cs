namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;

/// <summary>
/// One side of a <see cref="Bridge"/>: a place at a call where entities can sit.
/// </summary>
/// <remarks>
/// A call offers exactly three such places, which is why there are three ends here: what it hands back, what
/// it was used on, and what was passed to it. A bridge is a pair of them, so the same three words describe
/// every move the search can make.
/// </remarks>
internal abstract class BridgeEnd
{
    /// <summary>
    /// What the call handed back: the value of <c>users.ToList()</c>, <c>users[0]</c>, <c>task.Result</c>.
    /// </summary>
    /// <remarks>
    /// Only ever the side a bridge runs from. Nothing moves forward into a result.
    /// </remarks>
    public sealed class Result : BridgeEnd
    {
    }

    /// <summary>
    /// The value the member was used on: the <c>users</c> of <c>users[0]</c>, the <c>pair</c> of
    /// <c>pair.Value</c>.
    /// </summary>
    /// <remarks>
    /// Only ever the side a bridge lands on. For an extension method there is no instance of its own: the
    /// value in front of the dot is its first parameter, and that is what this end means there.
    /// </remarks>
    public sealed class Instance : BridgeEnd
    {
    }

    /// <summary>
    /// Something that was passed to the call, named by the parameter it was passed for.
    /// </summary>
    /// <remarks>
    /// One end for three shapes, since all the search needs is which parameter the entities travelled
    /// through: an ordinary argument, an <c>out</c> argument, and a parameter of a callback.
    /// </remarks>
    public sealed class Parameter : BridgeEnd
    {
        /// <summary>
        /// Initializes the end.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="lambdaPosition">Position inside the callback, when the parameter takes one.</param>
        public Parameter(string name, int lambdaPosition = 0)
        {
            Name = name;
            LambdaPosition = lambdaPosition;
        }

        /// <summary>
        /// Name of the parameter the entities travelled through.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Position of the callback's own parameter the entities sit in, when the named parameter takes a
        /// callback. Ignored otherwise.
        /// </summary>
        /// <remarks>
        /// Written down rather than worked out: a callback is handed values from more than one place and
        /// their types do not tell them apart. The accumulator of <c>Aggregate(seed, (acc, u) =&gt; ...)</c>
        /// has the element type and is not an element.
        /// </remarks>
        public int LambdaPosition { get; }
    }
}
