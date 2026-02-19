using System.Diagnostics;
using System.Text;

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

    private static async Task<string> ExecuteGitCommandAsync(string arguments, CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                output.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                error.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Git command failed: {error}");
        }

        return output.ToString().Trim();
    }
}
