using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.SpecKit.Services;

namespace Saritasa.Tools.SpecKit.Commands;

/// <summary>
/// Install command to deploy spec kit artifacts.
/// </summary>
[Command(Name = "install", Description = "Install spec kit artifacts to a destination path")]
internal class InstallCommand
{
    private readonly IArtifactsService artifactsService;
    private readonly ILogger<InstallCommand> logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    public InstallCommand(IArtifactsService artifactsService, ILogger<InstallCommand> logger)
    {
        this.artifactsService = artifactsService;
        this.logger = logger;
    }

    /// <summary>
    /// The destination path where spec kit artifacts will be installed.
    /// </summary>
    [Argument(0, Description = "The destination path where spec kit artifacts will be installed")]
    public string Destination { get; set; } = System.Environment.CurrentDirectory;

    /// <summary>
    /// Execute the command.
    /// </summary>
    public async Task<int> OnExecuteAsync()
    {
        try
        {
            await artifactsService.InstallAsync(Destination);
            return 0;
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "Error during installation");
            return 1;
        }
    }
}
