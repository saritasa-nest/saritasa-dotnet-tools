using System;
using Cake.Frosting;

namespace Saritasa.Cake;

/// <summary>
/// Entry point class.
/// </summary>
public static class Program
{
    /// <summary>
    /// Entry point method.
    /// </summary>
    /// <param name="args">Program arguments.</param>
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<PackContext>()
            .InstallTool(new Uri("nuget:?package=NuGet.CommandLine&version=latest"))
            .Run(args);
    }
}

/// <summary>
/// Default task.
/// </summary>
/// <remarks>Will be executed if call "Saritasa.Cake.ps1" without "target" argument:
/// "./Saritasa.Cake.ps1" - execute "Default" task;
/// "./Saritasa.Cake.ps1 --target Clean" - execute "Clean" task.</remarks>
[IsDependentOn(typeof(PackTask))]
public sealed class Default : FrostingTask { }
