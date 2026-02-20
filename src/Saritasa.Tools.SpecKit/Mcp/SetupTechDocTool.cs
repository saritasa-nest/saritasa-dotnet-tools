using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Saritasa.Tools.SpecKit.Services.Artifacts;

namespace Saritasa.Tools.SpecKit.Mcp;

/// <summary>
/// MCP tool for setting up technical documentation files from templates.
/// </summary>
[McpServerToolType]
internal static class SetupTechDocTool
{
    /// <summary>
    /// Setup technical documentation file from template.
    /// </summary>
    [McpServerTool(Name = "setup_tech_doc")]
    [Description("Creates a technical documentation file from template at the specified destination path")]
    public static async Task<string> SetupTechDocAsync(
        ArtifactsService artifactsService,
        ILoggerFactory loggerFactory,
        [Description("Absolute destination file path for the technical documentation")] string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(SetupTechDocTool));

        try
        {
            artifactsService.SetupTechDocAsync(destinationPath);

            logger.LogInformation("Tech doc setup completed successfully");
            return $"Technical documentation created at {destinationPath}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to setup tech doc");
            return $"Error: {ex.Message}";
        }
    }
}
