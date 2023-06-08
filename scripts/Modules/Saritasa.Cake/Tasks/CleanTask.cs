using Cake.Frosting;

namespace Saritasa.Cake;

/// <summary>
/// Clean task.
/// </summary>
[TaskName("Clean")]
[TaskDescription("Clean solution")]
public sealed class CleanTask : FrostingTask<CommonContext>
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
    public override void Run(CommonContext context)
    {
        context.SafeDeleteFiles($"{context.SolutionDir}Saritasa.*.nupkg");
        context.SafeDeleteFiles($"{context.SolutionDir}Saritasa.*.zip");
        context.SafeDeleteFiles($"{context.SolutionDir} src\\*.suo");
        context.SafeDeleteFiles($"{context.SolutionDir}*.tempex");

        foreach (var project in projectForClean)
        {
            context.SafeCleanDirectory($"{context.SolutionDir}{project}\\bin");
            context.SafeCleanDirectory($"{context.SolutionDir}{project}\\obj");
        }

        context.SafeDeleteFile($"{context.SolutionDir}src\\StyleCop.Cache");
        context.SafeDeleteFile($"{context.SolutionDir}scripts\\nuget.exe");
    }
}
