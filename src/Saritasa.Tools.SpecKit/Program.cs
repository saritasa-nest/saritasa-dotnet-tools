using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.SpecKit.Commands;
using Saritasa.Tools.SpecKit.Services;
using Saritasa.Tools.SpecKit.Services.Artifacts;

namespace Saritasa.Tools.SpecKit;

/// <summary>
/// Entry point class for SpecKit tool.
/// </summary>
[Command(Name = "speckit", Description = "Saritasa Spec Kit - Tool for managing specification artifacts")]
[Subcommand(typeof(InstallCommand))]

internal class Program
{
    private static IHost? app;

    /// <summary>
    /// Entry point method.
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.AddConsole(consoleLogOptions =>
        {
            // Configure all logs to go to stderr to not show provide them in chat.
            consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        ConfigureServices(builder.Services);

        app = builder.Build();

        // Command line processing.
        var commandLineApplication = new CommandLineApplication<Program>();
        using var scope = app.Services.CreateScope();

        commandLineApplication
            .Conventions
            .UseConstructorInjection(scope.ServiceProvider)
            .UseDefaultConventions();

        return await commandLineApplication.ExecuteAsync(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddTransient<GitService>();
        services.AddSingleton<ArtifactsService>();

        services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();
    }

    /// <summary>
    /// Execute when no subcommand is specified.
    /// </summary>
    public async Task OnExecuteAsync()
    {
        if (app == null)
        {
            throw new InvalidOperationException("app is not initialized.");
        }

        await app.RunAsync();
    }
}
