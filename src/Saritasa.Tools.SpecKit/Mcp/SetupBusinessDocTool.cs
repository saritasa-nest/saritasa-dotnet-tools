using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Saritasa.Tools.SpecKit.Services.Artifacts;

namespace Saritasa.Tools.SpecKit.Mcp;

/// <summary>
/// MCP tool for setting up business documentation files from templates.
/// </summary>
[McpServerToolType]
internal static class SetupBusinessDocTool
{
    /// <summary>
    /// Setup business documentation file from template.
    /// </summary>
    [McpServerTool(Name = "setup_business_doc")]
    [Description("Creates a business documentation file from template at the specified destination path")]
    public static string SetupBusinessDocument(
        ArtifactsService artifactsService,
        ILoggerFactory loggerFactory,
        [Description("Absolute destination file path for the business documentation")] string destinationPath)
    {
        var logger = loggerFactory.CreateLogger(nameof(SetupBusinessDocTool));

        try
        {
            artifactsService.SetupBusinessDocAsync(destinationPath);

            return $"Business documentation created at {destinationPath}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to setup business doc");
            return $"Error: {ex.Message}";
        }
    }
}
