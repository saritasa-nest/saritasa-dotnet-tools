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

        RevertAssemlyVersions(context);
    }

    private void UpdateAssemlyVersions(PackContext context)
    {
        var revcount = context.ExecuteGitCommand("rev-list --all --count").Replace(Environment.NewLine, string.Empty);
        var hash = context.ExecuteGitCommand("log --pretty=format:%h -n 1").Replace(Environment.NewLine, string.Empty);

        foreach (var projectName in projectNamesForPack)
        {
            var assemblyVersion = GetProjectAssemblyVersion(context, projectName);
            var fileVersion = $"{GetVersion(context, projectName)}.{revcount}";
            var productVersion = $"{fileVersion}-{hash}";
            context.Information($"{projectName} has versions {assemblyVersion} {fileVersion} {productVersion}");

            ReplaceAttributeValueInAssemblyInfo(context, projectName, "AssemblyVersion", assemblyVersion);
            ReplaceAttributeValueInAssemblyInfo(context, projectName, "AssemblyFileVersion", fileVersion);
            ReplaceAttributeValueInAssemblyInfo(context, projectName, "AssemblyInformationalVersion", productVersion);
        }
    }

    private void PackProjects(PackContext context)
    {
        foreach (var projectName in projectNamesForPack)
        {
            var projectDir = $"{context.SolutionDir}src\\{projectName}";

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

        var assemblyVersion = GetVersion(context, projectName);
        var resultPackNuGet = context.ExecuteNuGetCommand($"pack {nuspecFile} " +
            $"-NonInteractive " +
            $"-Version {assemblyVersion} " +
            $"-Exclude .snk " +
            $"-OutputDirectory {context.SolutionDir}");

        context.Information(resultPackNuGet);
    }

    private void RevertAssemlyVersions(PackContext context)
    {
        foreach (var projectName in projectNamesForPack)
        {
            ReplaceAttributeValueInAssemblyInfo(context, projectName, "AssemblyVersion", "1.0.0.0");
            ReplaceAttributeValueInAssemblyInfo(context, projectName, "AssemblyFileVersion", "1.0.0.0");
            ReplaceAttributeValueInAssemblyInfo(context, projectName, "AssemblyInformationalVersion", "1.0.0.0");
        }
    }

    private string GetVersion(PackContext context, string projectName)
    {
        return File.ReadAllText($"{context.SolutionDir}src\\{projectName}\\VERSION.txt").Trim();
    }

    private string GetProjectAssemblyVersion(PackContext context, string projectName)
    {
        var version = GetVersion(context, projectName);
        return version.Substring(0, version.LastIndexOf(".")) + ".0.0";
    }

    private void ReplaceAttributeValueInAssemblyInfo(PackContext context, string projectName, string attribute, string value)
    {
        var assemblyInfoFile = $"{context.SolutionDir}src\\{projectName}\\Properties\\AssemblyInfo.cs";
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
