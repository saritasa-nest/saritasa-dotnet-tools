using Cake.Core;
using Cake.Frosting;

namespace Saritasa.Cake;

/// <summary>
/// Clean task.
/// </summary>
[TaskName("Clean")]
[TaskDescription("Clean solution")]
public sealed class CleanTask : FrostingTask
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public CleanTask()
    {
        projectForClean = new[]
        {
            "src\\Saritasa.Tools.Common",
            "src\\Saritasa.Tools.Domain",
            "src\\Saritasa.Tools.EntityFrameworkCore",
            "src\\Saritasa.Tools.Emails",
            "src\\Saritasa.Tools.Misc",
            "test\\Saritasa.Tools.Common.Tests",
            "test\\Saritasa.Tools.Tests"
        };
    }

    /// <summary>
    /// Project folders to be cleared.
    /// </summary>
    private string[] projectForClean;

    /// <inheritdoc />
    public override void Run(ICakeContext context)
    {
        var solutionDir = "..\\..\\..\\";

        context.SafeDeleteFiles($"{solutionDir}Saritasa.*.nupkg");
        context.SafeDeleteFiles($"{solutionDir}Saritasa.*.zip");
        context.SafeDeleteFiles($"{solutionDir}src\\*.suo");
        context.SafeDeleteFiles($"{solutionDir}*.tempex");

        foreach (var project in projectForClean)
        {
            context.SafeCleanDirectory($"{solutionDir}{project}\\bin");
            context.SafeCleanDirectory($"{solutionDir}{project}\\obj");
        }

        context.SafeDeleteFile($"{solutionDir}src\\StyleCop.Cache");
        context.SafeDeleteFile($"{solutionDir}scripts\\nuget.exe");
    }
}
