using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Saritasa.Tools.SpecKit.Services;

/// <summary>
/// Service for managing SpecKit artifacts.
/// </summary>
internal interface IArtifactsService
{
    /// <summary>
    /// Gets the path to the artifacts directory.
    /// </summary>
    string ArtifactsPath { get; }

    /// <summary>
    /// Installs SpecKit artifacts to the specified destination.
    /// </summary>
    Task InstallAsync(string destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets up a new spec file based on the current git branch.
    /// </summary>
    Task<string> SetupSpecAsync(string currentBranch, string featureName, string repoRoot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets up a new plan file based on the current git branch.
    /// </summary>
    Task<string> SetupPlanAsync(string currentBranch, string featureName, string repoRoot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets up a new tasks file based on the current git branch.
    /// </summary>
    Task<string> SetupTasksAsync(string currentBranch, string featureName, string repoRoot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the spec metadata for the current branch.
    /// </summary>
    Task<string> GetSpecMetadataAsync(string repoRoot, string featureName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of artifacts service.
/// </summary>
internal class ArtifactsService : IArtifactsService
{
    // Directory and file name constants
    private const string SrcDirectory = "src";
    private const string SpeckitDirectory = ".speckit";
    private const string SpecsDirectory = "specs";
    private const string TemplatesDirectory = "templates";
    private const string MemoryDirectory = "memory";
    private const string GithubDirectory = ".github";
    private const string AgentsDirectory = "agents";
    private const string PromptsDirectory = "prompts";
    private const string ArtifactsDirectory = "artifacts";

    // File name constants
    private const string SpecFileName = "spec.md";
    private const string PlanFileName = "plan.md";
    private const string TasksFileName = "tasks.md";
    private const string SpecTemplateFileName = "spec-template.md";
    private const string PlanTemplateFileName = "plan-template.md";
    private const string TasksTemplateFileName = "tasks-template.md";
    private const string ReadmeFileName = "README.md";

    private readonly ILogger<ArtifactsService> logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    public ArtifactsService(ILogger<ArtifactsService> logger)
    {
        this.logger = logger;
        ArtifactsPath = GetArtifactsPath();
    }

    /// <inheritdoc />
    public string ArtifactsPath { get; }

    /// <inheritdoc />
    public async Task InstallAsync(string destination, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Installing Spec Kit artifacts to: {Destination}", destination);

        if (!Directory.Exists(ArtifactsPath))
        {
            throw new InvalidOperationException($"Artifacts path not found: {ArtifactsPath}");
        }

        // Force update .github/agents and .github/prompts
        await CopyDirectoryAsync(
            BuildGithubAgentsPath(ArtifactsPath),
            BuildGithubAgentsPath(destination),
            overwrite: true,
            cancellationToken);
        logger.LogInformation("Updated .github/agents");

        await CopyDirectoryAsync(
            BuildGithubPromptsPath(ArtifactsPath),
            BuildGithubPromptsPath(destination),
            overwrite: true,
            cancellationToken);
        logger.LogInformation("Updated .github/prompts");

        // Add memory files only if they don't exist
        var memorySourcePath = BuildSpeckitMemoryPath(ArtifactsPath);
        var memoryDestPath = BuildSpeckitMemoryPath(destination);

        if (Directory.Exists(memorySourcePath))
        {
            Directory.CreateDirectory(memoryDestPath);
            foreach (var file in Directory.GetFiles(memorySourcePath))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(memoryDestPath, fileName);
                if (!File.Exists(destFile))
                {
                    await CopyFileAsync(file, destFile, cancellationToken);
                    logger.LogInformation("Added .speckit/memory/{FileName}", fileName);
                }
                else
                {
                    logger.LogInformation("Skipped .speckit/memory/{FileName} (already exists)", fileName);
                }
            }
        }

        logger.LogInformation("Spec Kit installation completed successfully");
    }

    /// <inheritdoc />
    public async Task<string> SetupSpecAsync(string currentBranch, string featureName, string repoRoot, CancellationToken cancellationToken = default)
    {
        var speckitFolder = BuildSpeckitFolderPath(repoRoot);
        var specsFolder = BuildSpecsFolderPath(speckitFolder);
        var featureSpecFolder = BuildFeatureSpecFolderPath(specsFolder, featureName);
        var specPath = BuildSpecFilePath(featureSpecFolder);

        // Create the spec folder only if it doesn't exist
        if (!Directory.Exists(featureSpecFolder))
        {
            Directory.CreateDirectory(featureSpecFolder);
            logger.LogInformation("Created spec folder: {SpecFolder}", featureSpecFolder);
        }

        // Create the spec.md file only if it doesn't exist
        if (!File.Exists(specPath))
        {
            var template = BuildTemplateFilePath(TasksFileName);
            if (File.Exists(template))
            {
                await CopyFileAsync(template, specPath, cancellationToken, overwrite: false);
                logger.LogInformation("Created spec file: {SpecPath}", specPath);
            }
            else
            {
                throw new FileNotFoundException($"Spec template not found: {template}");
            }
        }

        var result = new
        {
            CurrentBranch = currentBranch,
            FeatureName = featureName,
            SpecPath = specPath
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <inheritdoc />
    public async Task<string> SetupPlanAsync(string currentBranch, string featureName, string repoRoot, CancellationToken cancellationToken = default)
    {
        var speckitFolder = BuildSpeckitFolderPath(repoRoot);
        var specsFolder = BuildSpecsFolderPath(speckitFolder);
        var featureSpecFolder = BuildFeatureSpecFolderPath(specsFolder, featureName);
        var specPath = BuildSpecFilePath(featureSpecFolder);
        var planPath = BuildPlanFilePath(featureSpecFolder);

        // Ensure spec exists
        if (!File.Exists(specPath))
        {
            throw new InvalidOperationException($"Spec file not found: {specPath}. Create the spec.md first.");
        }

        // Create the plan.md file only if it doesn't exist
        if (!File.Exists(planPath))
        {
            var template = BuildTemplateFilePath(PlanFileName);
            if (File.Exists(template))
            {
                await CopyFileAsync(template, planPath, cancellationToken, overwrite: false);
                logger.LogInformation("Created plan file: {PlanPath}", planPath);
            }
            else
            {
                throw new FileNotFoundException($"Plan template not found: {template}");
            }
        }

        var result = new
        {
            CurrentBranch = currentBranch,
            FeatureName = featureName,
            SpecPath = specPath,
            PlanPath = planPath
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <inheritdoc />
    public async Task<string> SetupTasksAsync(string currentBranch, string featureName, string repoRoot, CancellationToken cancellationToken = default)
    {
        var speckitFolder = BuildSpeckitFolderPath(repoRoot);
        var specsFolder = BuildSpecsFolderPath(speckitFolder);
        var featureSpecFolder = BuildFeatureSpecFolderPath(specsFolder, featureName);
        var specPath = BuildSpecFilePath(featureSpecFolder);
        var planPath = BuildPlanFilePath(featureSpecFolder);
        var tasksPath = BuildTasksFilePath(featureSpecFolder);

        // Ensure spec exists
        if (!File.Exists(specPath))
        {
            throw new InvalidOperationException($"Spec file not found: {specPath}. Create the spec.md first.");
        }

        // Ensure plan exists
        if (!File.Exists(planPath))
        {
            throw new InvalidOperationException($"Plan file not found: {planPath}. Create the plan.md first.");
        }

        // Create the tasks.md file only if it doesn't exist
        if (!File.Exists(tasksPath))
        {
            var template = BuildTemplateFilePath(TasksFileName);
            if (File.Exists(template))
            {
                await CopyFileAsync(template, tasksPath, cancellationToken, overwrite: false);
                logger.LogInformation("Created tasks file: {TasksPath}", tasksPath);
            }
            else
            {
                throw new FileNotFoundException($"Tasks template not found: {template}");
            }
        }

        var result = new
        {
            CurrentBranch = currentBranch,
            FeatureName = featureName,
            SpecPath = specPath,
            PlanPath = planPath,
            TasksPath = tasksPath
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <inheritdoc />
    public Task<string> GetSpecMetadataAsync(string repoRoot, string featureName, CancellationToken cancellationToken = default)
    {
        var speckitFolder = BuildSpeckitFolderPath(repoRoot);
        var specsFolder = BuildSpecsFolderPath(speckitFolder);
        var featureSpecFolder = BuildFeatureSpecFolderPath(specsFolder, featureName);
        var specPath = BuildSpecFilePath(featureSpecFolder);
        var planPath = BuildPlanFilePath(featureSpecFolder);
        var tasksPath = BuildTasksFilePath(featureSpecFolder);

        var result = new
        {
            SpecPath = File.Exists(specPath) ? specPath : string.Empty,
            PlanPath = File.Exists(planPath) ? planPath : string.Empty,
            TasksPath = File.Exists(tasksPath) ? tasksPath : string.Empty
        };

        return Task.FromResult(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
    }

    #region Path Building Methods

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
    /// Builds the path to src/.speckit directory.
    /// </summary>
    private static string BuildSpeckitFolderPath(string repoRoot) =>
        Path.Combine(repoRoot, SrcDirectory, SpeckitDirectory);

    /// <summary>
    /// Builds the path to src/.speckit/specs directory.
    /// </summary>
    private static string BuildSpecsFolderPath(string speckitFolder) =>
        Path.Combine(speckitFolder, SpecsDirectory);

    /// <summary>
    /// Builds the path to src/.speckit/specs/[featureName] directory.
    /// </summary>
    private static string BuildFeatureSpecFolderPath(string specsFolder, string featureName) =>
        Path.Combine(specsFolder, featureName);

    /// <summary>
    /// Builds the path to spec.md file in feature folder.
    /// </summary>
    private static string BuildSpecFilePath(string featureFolder) =>
        Path.Combine(featureFolder, SpecFileName);

    /// <summary>
    /// Builds the path to plan.md file in feature folder.
    /// </summary>
    private static string BuildPlanFilePath(string featureFolder) =>
        Path.Combine(featureFolder, PlanFileName);

    /// <summary>
    /// Builds the path to tasks.md file in feature folder.
    /// </summary>
    private static string BuildTasksFilePath(string featureFolder) =>
        Path.Combine(featureFolder, TasksFileName);

    /// <summary>
    /// Builds the path to *-template.md file.
    /// </summary>
    private static string BuildTemplateFilePath(string templateName) =>
        Path.Combine(GetArtifactsPath(), TemplatesDirectory, templateName);

    #endregion

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
            await CopyFileAsync(file, destFile, cancellationToken, overwrite);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(directory);
            await CopyDirectoryAsync(directory, Path.Combine(destDir, dirName), overwrite, cancellationToken);
        }
    }

    private static async Task CopyFileAsync(
        string sourceFile,
        string destFile,
        CancellationToken cancellationToken,
        bool overwrite = true)
    {
        if (!overwrite && File.Exists(destFile))
        {
            return;
        }

        using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        using var destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await sourceStream.CopyToAsync(destStream, cancellationToken);
    }
}
