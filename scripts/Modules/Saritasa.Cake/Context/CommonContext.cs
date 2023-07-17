using Cake.Core;
using Cake.Frosting;

namespace Saritasa.Cake.Context;

/// <summary>
/// Common context.
/// </summary>
public class CommonContext : FrostingContext
{
    /// <summary>
    /// Path to solution directory.
    /// </summary>
    public string SolutionDir { get; } = Path.Combine("..", "..", "..");

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="context">Cake context.</param>
    public CommonContext(ICakeContext context) : base(context)
    {
    }
}
