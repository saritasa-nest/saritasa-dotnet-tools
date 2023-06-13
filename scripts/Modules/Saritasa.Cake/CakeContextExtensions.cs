using System;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.Command;
using Cake.Core;
using Cake.Core.IO;

namespace Saritasa.Cake;

/// <summary>
/// Cake context extensions.
/// </summary>
public static class CakeContextExtensions
{
    /// <summary>
    /// Safe delete file.
    /// </summary>
    /// <param name="context">Cake context.</param>
    /// <param name="filePath">File path.</param>
    /// <remarks>Not throw error if something went wrong.</remarks>
    public static void SafeDeleteFile(this ICakeContext context, FilePath filePath)
    {
        try
        {
            context.DeleteFile(filePath);
        }
        catch (Exception ex)
        {
            context.Debug(ex);
        }
    }

    /// <summary>
    /// Safe delete files.
    /// </summary>
    /// <param name="context">Cake context.</param>
    /// <param name="pattern">Delete pattern.</param>
    /// <remarks>Not throw error if something went wrong.</remarks>
    public static void SafeDeleteFiles(this ICakeContext context, GlobPattern pattern)
    {
        try
        {
            context.DeleteFiles(pattern);
        }
        catch (Exception ex)
        {
            context.Debug(ex);
        }
    }

    /// <summary>
    /// Safe clean directory.
    /// </summary>
    /// <param name="context">Cake context.</param>
    /// <param name="path">Directory path.</param>
    /// <remarks>Not throw error if something went wrong.</remarks>
    public static void SafeCleanDirectory(this ICakeContext context, DirectoryPath path)
    {
        try
        {
            context.CleanDirectory(path);
        }
        catch (Exception ex)
        {
            context.Debug(ex);
        }
    }

    /// <summary>
    /// Execute git command.
    /// </summary>
    /// <param name="context">Cake context.</param>
    /// <param name="commandText">Command text without "git" in start.</param>
    /// <returns>Command output text.</returns>
    public static string ExecuteGitCommand(this ICakeContext context, string commandText)
    {
        var gitCommandSettings = new CommandSettings
        {
            ToolName = "GIT",
            ToolExecutableNames = new[] { "git", "git.exe" },
        };

        context.Command(gitCommandSettings, out var resultOutput, commandText);

        return resultOutput;
    }
}
