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
    public static string SetupTechDocument(
        ArtifactsService artifactsService,
        ILoggerFactory loggerFactory,
        [Description("Absolute destination file path for the technical documentation")] string destinationPath)
    {
        var logger = loggerFactory.CreateLogger(nameof(SetupTechDocTool));

        try
        {
            artifactsService.SetupTechDocAsync(destinationPath);
            return $"Technical documentation created at {destinationPath}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to setup tech doc");
            return $"Error: {ex.Message}";
        }
    }
}
