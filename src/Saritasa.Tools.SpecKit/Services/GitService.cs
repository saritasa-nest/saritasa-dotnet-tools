using System.Diagnostics;

namespace Saritasa.Tools.SpecKit.Services;

/// <summary>
/// Implementation of Git service.
/// </summary>
internal class GitService
{
    /// <summary>
    /// Gets the current git branch name.
    /// </summary>
    public async Task<string> GetCurrentBranchAsync(string projectFolder, CancellationToken cancellationToken = default)
    {
        return await ExecuteGitCommandAsync($"-C \"{projectFolder}\" rev-parse --abbrev-ref HEAD", cancellationToken);
    }

    /// <summary>
    /// Get feature name by branch name.
    /// </summary>
    /// <param name="branchName">Branch name.</param>
    public string GetFeatureName(string branchName)
    {
        return branchName
            .Split("/")
            .Last();
    }

    private async Task<string> ExecuteGitCommandAsync(string arguments, CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        // Close stdin immediately so git doesn't wait for input from MCP client.
        process.StandardInput.Close();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new InvalidOperationException($"Git command failed: {error}");
        }

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

        return output.TrimEnd();
    }
}
