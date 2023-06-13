using System.IO;
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
    /// Project folders to be cleared.
    /// </summary>
    private readonly string[] projectForClean = new[]
    {
        Path.Combine("src", "Saritasa.Tools.Common"),
        Path.Combine("src", "Saritasa.Tools.Domain"),
        Path.Combine("src", "Saritasa.Tools.EntityFrameworkCore"),
        Path.Combine("src", "Saritasa.Tools.Emails"),
        Path.Combine("src", "Saritasa.Tools.Misc"),
        Path.Combine("test", "Saritasa.Tools.Common.Tests"),
        Path.Combine("test", "Saritasa.Tools.Tests"),
    };

    /// <inheritdoc />
    public override void Run(CommonContext context)
    {
        context.SafeDeleteFiles(Path.Combine(context.SolutionDir, "Saritasa.*.nupkg"));
        context.SafeDeleteFiles(Path.Combine(context.SolutionDir, "Saritasa.*.zip"));
        context.SafeDeleteFiles(Path.Combine(context.SolutionDir, "src", "*.suo"));
        context.SafeDeleteFiles(Path.Combine(context.SolutionDir, "*.tempex"));

        foreach (var project in projectForClean)
        {
            context.SafeCleanDirectory(Path.Combine(context.SolutionDir, project, "bin"));
            context.SafeCleanDirectory(Path.Combine(context.SolutionDir, project, "obj"));
        }

        context.SafeDeleteFile(Path.Combine(context.SolutionDir, "src", "StyleCop.Cache"));
        context.SafeDeleteFile(Path.Combine(context.SolutionDir, "scripts", "nuget.exe"));
    }
}
