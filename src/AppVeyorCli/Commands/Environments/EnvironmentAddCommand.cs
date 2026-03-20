using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Environments;

public sealed class EnvironmentAddSettings : GlobalSettings
{
    [CommandOption("--name <NAME>")]
    [Description("Environment name")]
    public string Name { get; init; } = string.Empty;

    [CommandOption("--provider <PROVIDER>")]
    [Description("Environment provider (e.g. Agent, AzureWebJob, AzureCS, FTP, WebDeploy)")]
    public string Provider { get; init; } = string.Empty;
}

public sealed class EnvironmentAddCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<EnvironmentAddSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EnvironmentAddSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var request = new CreateEnvironmentRequest(settings.Name, settings.Provider);
        var env = await client.AddEnvironmentAsync(request);

        if (settings.Json)
        {
            renderer.RenderJson(env, AppVeyorJsonContext.Default.DeploymentEnvironment);
        }
        else
        {
            renderer.RenderSuccess($"Environment '{env.Name}' created successfully.");
            renderer.RenderDetail("New Environment",
                ("ID", env.DeploymentEnvironmentId.ToString(CultureInfo.InvariantCulture)),
                ("Name", env.Name),
                ("Provider", env.Provider));
        }

        return 0;
    }
}
