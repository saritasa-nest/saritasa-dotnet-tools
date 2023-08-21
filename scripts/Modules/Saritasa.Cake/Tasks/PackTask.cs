using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.NuGet;
using Cake.Common.Tools.NuGet.Pack;
using Cake.Core;
using Cake.Core.Tooling;
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
    public const string AssemblyVersionAttribute = "AssemblyVersion";
    public const string AssemblyFileVersionAttribute = "AssemblyFileVersion";
    public const string AssemblyInformationalVersionAttribute = "AssemblyInformationalVersion";

    private readonly string[] projectNamesForPack = new[]
    {
        "Saritasa.Tools.Common",
        "Saritasa.Tools.Domain",
        "Saritasa.Tools.EntityFrameworkCore",
        "Saritasa.Tools.Emails",
        "Saritasa.Tools.Misc",
    };

    /// <inheritdoc />
    public override void Run(PackContext context)
    {
        UpdateAssemblyVersions(context);

        PackProjects(context);

        RevertAssemblyVersions(context);
    }

    private void UpdateAssemblyVersions(PackContext context)
    {
        var revcount = context.ExecuteGitCommand("rev-list --all --count").Replace(Environment.NewLine, string.Empty);
        var hash = context.ExecuteGitCommand("log --pretty=format:%h -n 1").Replace(Environment.NewLine, string.Empty);

        foreach (var projectName in projectNamesForPack)
        {
            var assemblyVersion = GetProjectAssemblyVersion(context, projectName).ToString();
            var fileVersion = $"{GetVersion(context, projectName)}.{revcount}";
            var productVersion = $"{fileVersion}-{hash}";
            context.Information($"{projectName} has versions {assemblyVersion} {fileVersion} {productVersion}");

            ReplaceAttributeValueInAssemblyInfo(context, projectName, AssemblyVersionAttribute, assemblyVersion);
            ReplaceAttributeValueInAssemblyInfo(context, projectName, AssemblyFileVersionAttribute, fileVersion);
            ReplaceAttributeValueInAssemblyInfo(context, projectName, AssemblyInformationalVersionAttribute, productVersion);
        }
    }

    private void PackProjects(PackContext context)
    {
        foreach (var projectName in projectNamesForPack)
        {
            var projectDir = Path.Combine(context.SolutionDir, "src", projectName);

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
        var nuspecFile = Path.Combine(projectDir, $"{projectName}.nuspec");

        context.CopyFile($"{nuspecFile}.template", nuspecFile);

        UpdateVariableInFile(nuspecFile, "CommitHash", longHash);
        var assemblyVersion = GetVersion(context, projectName);

        context.NuGetPack(nuspecFile, new NuGetPackSettings
        {
            Version = assemblyVersion,
            OutputDirectory = context.SolutionDir,
            ArgumentCustomization = args => args.Append("-NonInteractive")
                                                .Append("-Exclude .snk")
        }.WithExpectedExitCode(1));
    }

    private void RevertAssemblyVersions(PackContext context)
    {
        foreach (var projectName in projectNamesForPack)
        {
            ReplaceAttributeValueInAssemblyInfo(context, projectName, AssemblyVersionAttribute, "1.0.0.0");
            ReplaceAttributeValueInAssemblyInfo(context, projectName, AssemblyFileVersionAttribute, "1.0.0.0");
            ReplaceAttributeValueInAssemblyInfo(context, projectName, AssemblyInformationalVersionAttribute, "1.0.0.0");
        }
    }

    private string GetVersion(PackContext context, string projectName)
    {
        var versionFile = Path.Combine(context.SolutionDir, "src", projectName, "VERSION.txt");
        return File.ReadAllText(versionFile).Trim();
    }

    private Version GetProjectAssemblyVersion(PackContext context, string projectName)
    {
        var versionString = GetVersion(context, projectName);
        var version = new Version(versionString);

        return new Version(
            version.Major,
            version.Minor,
            // We use only major and major version for AssemblyVersion, since "build" is used more for fixes and should not break an API.
            0,
            0);
    }

    private void ReplaceAttributeValueInAssemblyInfo(PackContext context, string projectName, string attribute, string value)
    {
        var assemblyInfoFile = Path.Combine(context.SolutionDir, "src", projectName, "Properties", "AssemblyInfo.cs");
        var assemblyInfo = File.ReadAllText(assemblyInfoFile);

        var newAssemblyInfo = ReplaceAttributeValueInAssemblyInfo(assemblyInfo, attribute, value);

        File.WriteAllText(assemblyInfoFile, newAssemblyInfo, new UTF8Encoding(true));
    }

    /// <summary>
    /// Replace attribute value in assembly info.
    /// </summary>
    /// <param name="assemblyInfo">Assembly info text.</param>
    /// <param name="attribute">Attribute name.</param>
    /// <param name="value">New attribute value.</param>
    /// <returns>New assembly info text.</returns>
    public string ReplaceAttributeValueInAssemblyInfo(string assemblyInfo, string attribute, string value)
    {
        //[assembly: AssemblyVersion("1.0.0.0")] -> match "AssemblyVersion("1.0.0.0")"
        var pattern = attribute + @"\(""[0-9]+(\.([0-9a-zA-Z\-]+|\*)){1,3}""\)";

        return Regex.Replace(assemblyInfo, pattern, $"{attribute}(\"{value}\")");
    }

    private void UpdateVariableInFile(string file, string variable, string value)
    {
        var content = File.ReadAllText(file);
        var newContent = content.Replace($"$({variable})", value)
                                .Replace($"%{variable}%", value);

        File.WriteAllText(file, newContent, new UTF8Encoding());
    }
}
