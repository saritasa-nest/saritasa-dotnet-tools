using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Saritasa.Tools.SpecKit.Services;

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
        IArtifactsService artifactsService,
        IGitService gitService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(SetupPlanTool));

        try
        {
            // Get current branch and repo root
            var currentBranch = await gitService.GetCurrentBranchAsync(cancellationToken);
            var repoRoot = await gitService.GetRepoRootAsync(cancellationToken);

            // Extract feature name (remove prefix before first '/')
            var featureName = currentBranch.Contains('/')
                ? currentBranch.Substring(currentBranch.IndexOf('/') + 1)
                : currentBranch;

            logger.LogInformation("Setting up plan for branch {Branch}, feature {Feature}", currentBranch, featureName);

            // Setup plan
            var result = await artifactsService.SetupPlanAsync(currentBranch, featureName, repoRoot, cancellationToken);

            logger.LogInformation("Plan setup completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to setup plan");
            return $"Error: {ex.Message}";
        }
    }
}
