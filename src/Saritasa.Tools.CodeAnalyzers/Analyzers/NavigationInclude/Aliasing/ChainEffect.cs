namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// What an assignment <c>t.P = v</c> does to the slots a reference such as <c>user.Profile.Settings</c> goes through.
/// </summary>
internal enum ChainEffect
{
    /// <summary>
    /// The assignment changes none of the slots.
    /// </summary>
    None,

    /// <summary>
    /// The assignment may change one of the slots, depending on the branch or on an object nothing is known about.
    /// </summary>
    Maybe,

    /// <summary>
    /// The assignment changes one of the slots on every path.
    /// </summary>
    Sure,
}
