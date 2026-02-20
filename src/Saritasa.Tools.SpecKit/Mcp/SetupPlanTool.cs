using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Saritasa.Tools.SpecKit.Services;
using Saritasa.Tools.SpecKit.Services.Artifacts;

namespace Saritasa.Tools.SpecKit.Mcp;

/// <summary>
/// MCP tool for setting up plan files from templates.
/// </summary>
[McpServerToolType]
internal static class SetupPlanTool
{
    /// <summary>
    /// Setup plan file from template.
    /// </summary>
    [McpServerTool(Name = "setup_plan")]
    [Description("Creates a new plan.md file for the current feature. Requires spec.md to exist.")]
    public static async Task<string> SetupPlanAsync(
        ArtifactsService artifactsService,
        GitService gitService,
        ILoggerFactory loggerFactory,
        [Description("Absolute path to parent folder which contains currently opened folder")] string projectFolder,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(SetupPlanTool));

        try
        {
            if (!artifactsService.TryGetSpecKitDirectory(projectFolder, out var specKitDirectory))
            {
                return "Cannot find .speckit directory";
            }

            var currentBranch = await gitService.GetCurrentBranchAsync(projectFolder, cancellationToken);
            var featureName = gitService.GetFeatureName(currentBranch);

            var featureArtifacts = artifactsService.SetupPlanAsync(specKitDirectory!, featureName);

            return JsonSerializer.Serialize(featureArtifacts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to setup plan");
            return $"Error: {ex.Message}";
        }
    }
}
