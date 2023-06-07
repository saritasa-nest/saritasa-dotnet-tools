using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Frosting;

namespace Saritasa.Cake;

/// <summary>
/// Pack task.
/// </summary>
[TaskName("Pack")]
[TaskDescription("Build the library, test it and prepare nuget packages")]
[IsDependentOn(typeof(CleanTask))]
public sealed class PackTask : FrostingTask<PackContext>
{
    /// <summary>
    /// Relative path to solution dir.
    /// </summary>
    private const string SolutionDir = "..\\..\\..\\";

    /// <summary>
    /// Constructor.
    /// </summary>
    public PackTask()
    {
        projectNamesForPack = new[]
        {
            "Saritasa.Tools.Common",
            "Saritasa.Tools.Domain",
            "Saritasa.Tools.EntityFrameworkCore",
            "Saritasa.Tools.Emails",
            "Saritasa.Tools.Misc",
        };
    }

    private string[] projectNamesForPack;

    /// <inheritdoc />
    public override void Run(PackContext context)
    {
        UpdateAssemlyVersions(context);

        PackProjects(context);

        RevertAssemlyVersions();
    }

    private void UpdateAssemlyVersions(PackContext context)
    {
        var revcount = context.ExecuteGitCommand("rev-list --all --count").Replace(Environment.NewLine, string.Empty);
        var hash = context.ExecuteGitCommand("log --pretty=format:%h -n 1").Replace(Environment.NewLine, string.Empty);

        foreach (var projectName in projectNamesForPack)
        {
            var assemblyVersion = GetProjectAssemblyVersion(projectName);
            var fileVersion = $"{GetVersion(projectName)}.{revcount}";
            var productVersion = $"{fileVersion}-{hash}";
            context.Information($"{projectName} has versions {assemblyVersion} {fileVersion} {productVersion}");

            ReplaceAttributeValueInAssemblyInfo(projectName, "AssemblyVersion", assemblyVersion);
            ReplaceAttributeValueInAssemblyInfo(projectName, "AssemblyFileVersion", fileVersion);
            ReplaceAttributeValueInAssemblyInfo(projectName, "AssemblyInformationalVersion", productVersion);
        }
    }

    private void PackProjects(PackContext context)
    {
        foreach (var projectName in projectNamesForPack)
        {
            var projectDir = $"{SolutionDir}src\\{projectName}";

            context.DotNetRestore(projectDir);
            context.DotNetBuild(projectDir, new DotNetBuildSettings
            {
                Configuration = context.MsBuildConfiguration
            });

            PackNuGetFile(context, projectName, projectDir);
        }
    }

    private void PackNuGetFile(PackContext context, string projectName, string projectDir)
    {
        var longHash = context.ExecuteGitCommand("rev-parse HEAD").Replace(Environment.NewLine, string.Empty);
        var nuspecFile = $"{projectDir}\\{projectName}.nuspec";

        context.CopyFile($"{nuspecFile}.template", nuspecFile);

        UpdateVariableInFile(nuspecFile, "CommitHash", longHash);

        var assemblyVersion = GetVersion(projectName);
        var resultPackNuGet = context.ExecuteNuGetCommand($"pack {nuspecFile} " +
            $"-NonInteractive " +
            $"-Version {assemblyVersion} " +
            $"-Exclude .snk " +
            $"-OutputDirectory {SolutionDir}");

        context.Information(resultPackNuGet);
    }

    private void RevertAssemlyVersions()
    {
        foreach (var projectName in projectNamesForPack)
        {
            ReplaceAttributeValueInAssemblyInfo(projectName, "AssemblyVersion", "1.0.0.0");
            ReplaceAttributeValueInAssemblyInfo(projectName, "AssemblyFileVersion", "1.0.0.0");
            ReplaceAttributeValueInAssemblyInfo(projectName, "AssemblyInformationalVersion", "1.0.0.0");
        }
    }

    private string GetVersion(string projectName)
    {
        return File.ReadAllText($"{SolutionDir}src\\{projectName}\\VERSION.txt").Trim();
    }

    private string GetProjectAssemblyVersion(string projectName)
    {
        var version = GetVersion(projectName);
        return version.Substring(0, version.LastIndexOf(".")) + ".0.0";
    }

    private void ReplaceAttributeValueInAssemblyInfo(string projectName, string attribute, string value)
    {
        var assemblyInfoFile = $"{SolutionDir}src\\{projectName}\\Properties\\AssemblyInfo.cs";
        var assemblyInfo = File.ReadAllText(assemblyInfoFile);

        //[assembly: AssemblyVersion("1.0.0.0")] -> match "AssemblyVersion("1.0.0.0")"
        var pattern = attribute + @"\(""[0-9]+(\.([0-9a-zA-Z\-]+|\*)){1,3}""\)";

        var newAssemblyInfo = Regex.Replace(assemblyInfo, pattern, $"{attribute}(\"{value}\")");

        File.WriteAllText(assemblyInfoFile, newAssemblyInfo, new UTF8Encoding(true));
    }

    private void UpdateVariableInFile(string file, string variable, string value)
    {
        var content = File.ReadAllText(file);
        var newContent = content.Replace($"$({variable})", value)
                                .Replace($"%{variable}%", value);

        File.WriteAllText(file, newContent, new UTF8Encoding());
    }
}
