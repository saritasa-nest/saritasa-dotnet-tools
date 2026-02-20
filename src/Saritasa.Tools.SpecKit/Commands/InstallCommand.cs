using McMaster.Extensions.CommandLineUtils;
using Saritasa.Tools.SpecKit.Services.Artifacts;

namespace Saritasa.Tools.SpecKit.Commands;

/// <summary>
/// Install command to deploy spec kit artifacts.
/// </summary>
[Command(Name = "install", Description = "Install spec kit artifacts to a destination path")]
internal class InstallCommand(ArtifactsService artifactsService)
{
    /// <summary>
    /// The destination path where spec kit artifacts will be installed.
    /// </summary>
    [Option(template: "--destination|-d", Description = "The destination path where spec kit artifacts will be installed")]
    public string Destination { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// Execute the command.
    /// </summary>
    public async Task<int> OnExecuteAsync()
    {
        await artifactsService.InstallAsync(Destination);
        return 0;
    }
}
