using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Saritasa.Tools.SpecKit.Services.Artifacts;

/// <summary>
/// Implementation of artifacts service.
/// </summary>
internal class ArtifactsService(ILogger<ArtifactsService> logger)
{
    private const string SpeckitDirectory = ".speckit";
    private const string SpecsDirectory = "specs";
    private const string TemplatesDirectory = "templates";
    private const string MemoryDirectory = "memory";
    private const string GithubDirectory = ".github";
    private const string AgentsDirectory = "agents";
    private const string PromptsDirectory = "prompts";
    private const string ArtifactsDirectory = "artifacts";

    private const string SpecFileName = "spec.md";
    private const string PlanFileName = "plan.md";
    private const string TasksFileName = "tasks.md";
    private const string SpecTemplateFileName = "spec-template.md";
    private const string PlanTemplateFileName = "plan-template.md";
    private const string TasksTemplateFileName = "tasks-template.md";

    private readonly string bundledArtifactsRoot = GetArtifactsPath();

    /// <summary>
    /// Installs Spec Kit artifacts to the specified destination.
    /// </summary>
    /// <param name="destination">The destination directory where artifacts will be installed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InstallAsync(string destination, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Installing Spec Kit artifacts to: {Destination}", destination);

        if (!Directory.Exists(bundledArtifactsRoot))
        {
            throw new InvalidOperationException($"Artifacts path not found: {bundledArtifactsRoot}");
        }

        // Force update .github/agents and .github/prompts
        await CopyDirectoryAsync(
            BuildGithubAgentsPath(bundledArtifactsRoot),
            BuildGithubAgentsPath(destination),
            overwrite: true,
            cancellationToken);
        logger.LogInformation("Updated .github/agents");

        await CopyDirectoryAsync(
            BuildGithubPromptsPath(bundledArtifactsRoot),
            BuildGithubPromptsPath(destination),
            overwrite: true,
            cancellationToken);
        logger.LogInformation("Updated .github/prompts");

        await CopyDirectoryAsync(
            BuildSpeckitMemoryPath(bundledArtifactsRoot),
            BuildSpeckitMemoryPath(destination),
            overwrite: false,
            cancellationToken);
        logger.LogInformation("Updated memory folder");

        logger.LogInformation("Spec Kit installation completed successfully");
    }

    /// <summary>
    /// Sets up a specification file for a feature if it does not exist.
    /// </summary>
    /// <param name="projectRoot">The root directory of the project.</param>
    /// <param name="featureName">The name of the feature.</param>
    /// <returns>The path to the created or existing specification file.</returns>
    public SpecKitFeatureArtifacts SetupSpecAsync(string projectRoot, string featureName)
    {
        var featureArtifacts = GetFeatureArtifacts(projectRoot, featureName);
        if (featureArtifacts.Specification.IsExist)
        {
            return featureArtifacts;
        }

        var specFolder = Path.GetDirectoryName(featureArtifacts.Specification.Path);
        if (specFolder is null)
        {
            throw new InvalidOperationException("Cannot find a feature folder");
        }

        Directory.CreateDirectory(specFolder);

        var specTemplate = BuildTemplateFilePath(SpecTemplateFileName);
        if (!File.Exists(specTemplate))
        {
            throw new InvalidOperationException("Cannot find a specification template");
        }

        File.Copy(specTemplate, featureArtifacts.Specification.Path);

        return GetFeatureArtifacts(projectRoot, featureName);
    }

    /// <summary>
    /// Sets up a plan file for a feature if it does not exist.
    /// </summary>
    /// <param name="projectRoot">The root directory of the project.</param>
    /// <param name="featureName">The name of the feature.</param>
    /// <returns>The path to the created or existing plan file.</returns>
    public SpecKitFeatureArtifacts SetupPlanAsync(string projectRoot, string featureName)
    {
        var featureArtifacts = GetFeatureArtifacts(projectRoot, featureName);
        if (featureArtifacts.Plan.IsExist)
        {
            return featureArtifacts;
        }

        if (!featureArtifacts.Specification.IsExist)
        {
            throw new InvalidOperationException("Cannot create a plan before a spec");
        }

        var planTemplate = BuildTemplateFilePath(PlanTemplateFileName);
        if (!File.Exists(planTemplate))
        {
            throw new InvalidOperationException("Cannot find a plan template");
        }

        File.Copy(planTemplate, featureArtifacts.Plan.Path);

        return GetFeatureArtifacts(projectRoot, featureName);
    }

    /// <summary>
    /// Sets up a tasks file for a feature if it does not exist.
    /// </summary>
    /// <param name="projectRoot">The root directory of the project.</param>
    /// <param name="featureName">The name of the feature.</param>
    /// <returns>The path to the created or existing tasks file.</returns>
    public SpecKitFeatureArtifacts SetupTasksAsync(string projectRoot, string featureName)
    {
        var featureArtifacts = GetFeatureArtifacts(projectRoot, featureName);
        if (featureArtifacts.Tasks.IsExist)
        {
            return featureArtifacts;
        }

        if (!featureArtifacts.Specification.IsExist)
        {
            throw new InvalidOperationException("Cannot create a tasks before a spec");
        }

        if (!featureArtifacts.Plan.IsExist)
        {
            throw new InvalidOperationException("Cannot create a tasks before a plan");
        }

        var tasksTemplate = BuildTemplateFilePath(TasksTemplateFileName);
        if (!File.Exists(tasksTemplate))
        {
            throw new InvalidOperationException("Cannot find a tasks template");
        }

        File.Copy(tasksTemplate, featureArtifacts.Tasks.Path);

        return GetFeatureArtifacts(projectRoot, featureName);
    }

    /// <summary>
    /// Gets the artifacts (specification, plan, tasks) for a feature.
    /// </summary>
    /// <param name="specKitDirectory">The SpecKit directory.</param>
    /// <param name="featureName">The name of the feature.</param>
    /// <returns>The feature artifacts.</returns>
    public SpecKitFeatureArtifacts GetFeatureArtifacts(
        string specKitDirectory,
        string featureName)
    {
        var specPath = BuildSpecFilePath(specKitDirectory, featureName);
        var planPath = BuildPlanFilePath(specKitDirectory, featureName);
        var tasksPath = BuildTasksFilePath(specKitDirectory, featureName);

        return new SpecKitFeatureArtifacts(
            Specification: new SpecKitArtifact(specPath, File.Exists(specPath)),
            Plan: new SpecKitArtifact(planPath, File.Exists(planPath)),
            Tasks: new SpecKitArtifact(tasksPath, File.Exists(tasksPath)));
    }

    private string BuildSpecFilePath(string specKitDirectory, string featureName) =>
        Path.Combine(specKitDirectory, SpecsDirectory, featureName, SpecFileName);

    private string BuildPlanFilePath(string specKitDirectory, string featureName) =>
        Path.Combine(specKitDirectory, SpecsDirectory, featureName, PlanFileName);

    private string BuildTasksFilePath(string specKitDirectory, string featureName) =>
        Path.Combine(specKitDirectory, SpecsDirectory, featureName, TasksFileName);

    /// <summary>
    /// Tries to find the .speckit directory in the project path.
    /// </summary>
    /// <param name="projectPath">The root path of the project.</param>
    /// <param name="specKitDirectory">The found .speckit directory, or null if not found.</param>
    /// <returns>True if the directory is found, otherwise false.</returns>
    public bool TryGetSpecKitDirectory(string projectPath, out string? specKitDirectory)
    {
        var subDirectories = Directory.EnumerateDirectories(projectPath, SpeckitDirectory, SearchOption.AllDirectories);
        specKitDirectory = subDirectories.FirstOrDefault();
        return specKitDirectory is not null;
    }

    /// <summary>
    /// Builds the path to .github/agents directory.
    /// </summary>
    private static string BuildGithubAgentsPath(string root) =>
        Path.Combine(root, GithubDirectory, AgentsDirectory);

    /// <summary>
    /// Builds the path to .github/prompts directory.
    /// </summary>
    private static string BuildGithubPromptsPath(string root) =>
        Path.Combine(root, GithubDirectory, PromptsDirectory);

    /// <summary>
    /// Builds the path to .speckit/memory directory.
    /// </summary>
    private static string BuildSpeckitMemoryPath(string root) =>
        Path.Combine(root, SpeckitDirectory, MemoryDirectory);

    /// <summary>
    /// Builds the path to *-template.md file.
    /// </summary>
    private string BuildTemplateFilePath(string templateName) =>
        Path.Combine(bundledArtifactsRoot, SpeckitDirectory, TemplatesDirectory, templateName);

    private static string GetArtifactsPath()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;
        return Path.Combine(assemblyDir, ArtifactsDirectory);
    }

    private async Task CopyDirectoryAsync(
        string sourceDir,
        string destDir,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(sourceDir))
        {
            return;
        }

        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destDir, fileName);

            var skipCopy = File.Exists(destFile) && !overwrite;
            if (skipCopy)
            {
                continue;
            }

            File.Copy(file, destFile, overwrite: overwrite);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(directory);
            await CopyDirectoryAsync(directory, Path.Combine(destDir, dirName), overwrite, cancellationToken);
        }
    }
}
