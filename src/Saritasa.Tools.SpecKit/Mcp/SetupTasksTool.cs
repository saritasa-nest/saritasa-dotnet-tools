using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Saritasa.Tools.SpecKit.Services;
using Saritasa.Tools.SpecKit.Services.Artifacts;

namespace Saritasa.Tools.SpecKit.Mcp;

/// <summary>
/// MCP tool for setting up tasks files from templates.
/// </summary>
[McpServerToolType]
internal static class SetupTasksTool
{
    /// <summary>
    /// Setup tasks file from template.
    /// </summary>
    [McpServerTool(Name = "setup_tasks")]
    [Description("Creates a new tasks.md file for the current feature. Requires spec.md and plan.md to exist.")]
    public static async Task<string> SetupTasksAsync(
        ArtifactsService artifactsService,
        GitService gitService,
        ILoggerFactory loggerFactory,
        [Description("Absolute path to parent folder which contains currently opened folder")] string projectFolder,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(SetupTasksTool));

        try
        {
            if (!artifactsService.TryGetSpecKitDirectory(projectFolder, out var specKitDirectory))
            {
                return "Cannot find .speckit directory";
            }

            var currentBranch = await gitService.GetCurrentBranchAsync(projectFolder, cancellationToken);
            var featureName = gitService.GetFeatureName(currentBranch);

            var featureArtifacts = artifactsService.SetupTasksAsync(specKitDirectory!, featureName);

            logger.LogInformation("Plan setup completed successfully");
            return JsonSerializer.Serialize(featureArtifacts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to setup tasks");
            return $"Error: {ex.Message}";
        }
    }
}
