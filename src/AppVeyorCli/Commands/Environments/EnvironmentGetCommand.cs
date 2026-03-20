using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Environments;

public sealed class EnvironmentGetSettings : GlobalSettings
{
    [CommandArgument(0, "<environmentId>")]
    [Description("Deployment environment ID")]
    public int EnvironmentId { get; init; }
}

public sealed class EnvironmentGetCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<EnvironmentGetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EnvironmentGetSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var result = await client.GetEnvironmentSettingsAsync(settings.EnvironmentId);

        if (settings.Json)
        {
            renderer.RenderJson(result, AppVeyorJsonContext.Default.EnvironmentWithSettings);
        }
        else
        {
            renderer.RenderDetail($"Environment: {result.Environment.Name}",
                ("ID", result.Environment.DeploymentEnvironmentId.ToString(CultureInfo.InvariantCulture)),
                ("Name", result.Environment.Name),
                ("Provider", result.Environment.Provider),
                ("Created", result.Environment.Created.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)),
                ("Updated", result.Environment.Updated.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));
        }

        return 0;
    }
}
