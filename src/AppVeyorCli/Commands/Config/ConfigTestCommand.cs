using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Config;

public sealed class ConfigTestCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<GlobalSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);

        var success = await client.TestConnectionAsync();

        if (settings.Json)
        {
            renderer.RenderJson(new ConnectionTestResult(success), AppVeyorJsonContext.Default.ConnectionTestResult);
            return success ? 0 : 1;
        }

        if (success)
        {
            renderer.RenderSuccess("Successfully connected to AppVeyor API.");
        }
        else
        {
            renderer.RenderError("Failed to connect to AppVeyor API. Check your token and network.");
        }

        return success ? 0 : 1;
    }
}
