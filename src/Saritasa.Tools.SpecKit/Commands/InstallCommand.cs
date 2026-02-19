using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.SpecKit.Services;
using Saritasa.Tools.SpecKit.Services.Artifacts;

namespace Saritasa.Tools.SpecKit.Commands;

/// <summary>
/// Install command to deploy spec kit artifacts.
/// </summary>
[Command(Name = "install", Description = "Install spec kit artifacts to a destination path")]
internal class InstallCommand
{
    private readonly ArtifactsService artifactsService;
    private readonly ILogger<InstallCommand> logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    public InstallCommand(ArtifactsService artifactsService, ILogger<InstallCommand> logger)
    {
        this.artifactsService = artifactsService;
        this.logger = logger;
    }

    /// <summary>
    /// The destination path where spec kit artifacts will be installed.
    /// </summary>
    [Option(template: "--destination|-d", Description = "The destination path where spec kit artifacts will be installed")]
    public string Destination { get; set; } = System.Environment.CurrentDirectory;

    /// <summary>
    /// Execute the command.
    /// </summary>
    public async Task<int> OnExecuteAsync()
    {
        await artifactsService.InstallAsync(Destination);
        return 0;
    }
}
