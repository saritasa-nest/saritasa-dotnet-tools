using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Saritasa.Tools.SpecKit.Services;

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
        IArtifactsService artifactsService,
        IGitService gitService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(SetupTasksTool));

        try
        {
            // Get current branch and repo root
            var currentBranch = await gitService.GetCurrentBranchAsync(cancellationToken);
            var repoRoot = await gitService.GetRepoRootAsync(cancellationToken);

            // Extract feature name (remove prefix before first '/')
            var featureName = currentBranch.Contains('/')
                ? currentBranch.Substring(currentBranch.IndexOf('/') + 1)
                : currentBranch;

            logger.LogInformation("Setting up tasks for branch {Branch}, feature {Feature}", currentBranch, featureName);

            // Setup tasks
            var result = await artifactsService.SetupTasksAsync(currentBranch, featureName, repoRoot, cancellationToken);

            logger.LogInformation("Tasks setup completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to setup tasks");
            return $"Error: {ex.Message}";
        }
    }
}
