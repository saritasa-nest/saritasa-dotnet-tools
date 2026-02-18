using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Saritasa.Tools.SpecKit.Services;

/// <summary>
/// Service for Git operations.
/// </summary>
internal interface IGitService
{
    /// <summary>
    /// Gets the current git branch name.
    /// </summary>
    Task<string> GetCurrentBranchAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the repository root directory path.
    /// </summary>
    Task<string> GetRepoRootAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of Git service.
/// </summary>
internal class GitService : IGitService
{
    /// <inheritdoc />
    public async Task<string> GetCurrentBranchAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteGitCommandAsync("rev-parse --abbrev-ref HEAD", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> GetRepoRootAsync(CancellationToken cancellationToken = default)
    {
        var gitDir = await ExecuteGitCommandAsync("rev-parse --git-dir", cancellationToken);
        var gitDirPath = System.IO.Path.GetFullPath(gitDir);
        return System.IO.Path.GetDirectoryName(gitDirPath) ?? throw new InvalidOperationException("Failed to get repository root");
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
