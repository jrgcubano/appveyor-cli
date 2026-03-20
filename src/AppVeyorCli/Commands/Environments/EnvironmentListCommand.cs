using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Environments;

public sealed class EnvironmentListCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<GlobalSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var environments = await client.GetEnvironmentsAsync();

        if (settings.Json)
        {
            renderer.RenderJson(environments, AppVeyorJsonContext.Default.DeploymentEnvironmentArray);
        }
        else
        {
            renderer.RenderTable("Environments", environments,
                new("ID", e => ((Models.DeploymentEnvironment)e).DeploymentEnvironmentId.ToString(CultureInfo.InvariantCulture)),
                new("Name", e => ((Models.DeploymentEnvironment)e).Name),
                new("Provider", e => ((Models.DeploymentEnvironment)e).Provider),
                new("Updated", e => ((Models.DeploymentEnvironment)e).Updated.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));
        }

        return 0;
    }
}
