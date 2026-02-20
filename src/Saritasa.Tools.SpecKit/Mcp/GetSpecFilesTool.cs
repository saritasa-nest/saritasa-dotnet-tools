using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Saritasa.Tools.SpecKit.Services;
using Saritasa.Tools.SpecKit.Services.Artifacts;

namespace Saritasa.Tools.SpecKit.Mcp;

/// <summary>
/// MCP tool for getting spec files.
/// </summary>
[McpServerToolType]
internal static class GetSpecFilesTool
{
    /// <summary>
    /// Get list of spec files.
    /// </summary>
    [McpServerTool(Name = "get_spec_files")]
    [Description("Gets the paths to spec.md, plan.md, and tasks.md for the current git branch")]
    public static async Task<string> GetSpecFilesAsync(
        ArtifactsService artifactsService,
        GitService gitService,
        ILoggerFactory loggerFactory,
        [Description("Absolute path to parent folder which contains currently opened folder")] string projectFolder,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(GetSpecFilesTool));

        try
        {
            if (!artifactsService.TryGetSpecKitDirectory(projectFolder, out var specKitDirectory))
            {
                return "Cannot find .speckit directory";
            }

            var currentBranch = await gitService.GetCurrentBranchAsync(projectFolder, cancellationToken);
            var featureName = gitService.GetFeatureName(currentBranch);
            var featureArtifacts = artifactsService.GetFeatureArtifacts(specKitDirectory!, featureName);

            return JsonSerializer.Serialize(featureArtifacts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get spec files");
            return $"Error: {ex.Message}";
        }
    }
}
