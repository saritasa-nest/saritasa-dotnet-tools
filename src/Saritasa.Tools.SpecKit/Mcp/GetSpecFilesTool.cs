using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Saritasa.Tools.SpecKit.Services;

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
        IArtifactsService artifactsService,
        IGitService gitService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(GetSpecFilesTool));

        try
        {
            // Get current branch and repo root
            var currentBranch = await gitService.GetCurrentBranchAsync(cancellationToken);
            var repoRoot = await gitService.GetRepoRootAsync(cancellationToken);

            // Extract feature name (remove prefix before first '/')
            var featureName = currentBranch.Contains('/')
                ? currentBranch.Substring(currentBranch.IndexOf('/') + 1)
                : currentBranch;

            logger.LogInformation("Getting spec files for branch {Branch}, feature {Feature}", currentBranch, featureName);

            // Get spec metadata
            var result = await artifactsService.GetSpecMetadataAsync(repoRoot, featureName, cancellationToken);

            logger.LogInformation("Spec files retrieved successfully");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get spec files");
            return $"Error: {ex.Message}";
        }
    }
}
