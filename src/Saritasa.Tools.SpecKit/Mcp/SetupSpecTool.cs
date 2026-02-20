using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Saritasa.Tools.SpecKit.Services;
using Saritasa.Tools.SpecKit.Services.Artifacts;

namespace Saritasa.Tools.SpecKit.Mcp;

/// <summary>
/// MCP tool for setting up spec files from templates.
/// </summary>
[McpServerToolType]
internal static class SetupSpecTool
{
    /// <summary>
    /// Setup spec file from template.
    /// </summary>
    [McpServerTool(Name = "setup_spec")]
    [Description("Creates a new spec folder and spec.md file based on the current git branch")]
    public static async Task<string> SetupSpecAsync(
        ArtifactsService artifactsService,
        GitService gitService,
        ILoggerFactory loggerFactory,
        [Description("Absolute path to parent folder which contains currently opened folder")] string projectFolder,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(SetupSpecTool));

        try
        {
            if (!artifactsService.TryGetSpecKitDirectory(projectFolder, out var specKitDirectory))
            {
                return "Cannot find .speckit directory";
            }

            var currentBranch = await gitService.GetCurrentBranchAsync(projectFolder, cancellationToken);
            var featureName = gitService.GetFeatureName(currentBranch);

            var featureArtifacts = artifactsService.SetupSpecAsync(specKitDirectory!, featureName);

            return JsonSerializer.Serialize(featureArtifacts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to setup spec");
            return $"Error: {ex.Message}";
        }
    }
}
