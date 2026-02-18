using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Saritasa.Tools.SpecKit.Services;

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
        IArtifactsService artifactsService,
        IGitService gitService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(SetupSpecTool));

        try
        {
            // Get current branch and repo root
            var currentBranch = await gitService.GetCurrentBranchAsync(cancellationToken);
            var repoRoot = await gitService.GetRepoRootAsync(cancellationToken);

            // Extract feature name (remove prefix before first '/')
            var featureName = currentBranch.Contains('/')
                ? currentBranch.Substring(currentBranch.IndexOf('/') + 1)
                : currentBranch;

            logger.LogInformation("Setting up spec for branch {Branch}, feature {Feature}", currentBranch, featureName);

            // Setup spec
            var result = await artifactsService.SetupSpecAsync(currentBranch, featureName, repoRoot, cancellationToken);

            logger.LogInformation("Spec setup completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to setup spec");
            return $"Error: {ex.Message}";
        }
    }
}
