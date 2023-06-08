using Cake.Common;
using Cake.Core;
using Cake.Frosting;

namespace Saritasa.Cake;

/// <summary>
/// Common context.
/// </summary>
public class CommonContext : FrostingContext
{
    /// <summary>
    /// Path to solution directory.
    /// </summary>
    /// <remarks>Relative path built from "Saritasa.Cake.csproj" folder.
    /// DefaultValue - "..\\..\\..\\".
    /// Was override if invoke "Saritasa.Cake.ps1" with "solutionDir" argument (.\Saritasa.Cake.ps1 --solutionDir ..\\SolutionFolder\\).</remarks>
    public string SolutionDir { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="context">Cake context.</param>
    public CommonContext(ICakeContext context) : base(context)
    {
        SolutionDir = context.Argument("solutionDir", "..\\..\\..\\");
    }
}
