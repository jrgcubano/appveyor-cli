using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Environments;

public sealed class EnvironmentDeleteSettings : GlobalSettings
{
    [CommandArgument(0, "<environmentId>")]
    [Description("Deployment environment ID")]
    public int EnvironmentId { get; init; }

    [CommandOption("--force")]
    [Description("Skip confirmation prompt")]
    public bool Force { get; init; }
}

public sealed class EnvironmentDeleteCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<EnvironmentDeleteSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EnvironmentDeleteSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);

        if (!settings.Force && !settings.Json)
        {
            if (!consoleProvider.Console.Confirm($"Are you sure you want to delete environment [red]{settings.EnvironmentId}[/]?", false))
            {
                renderer.RenderError("Operation cancelled.");
                return 1;
            }
        }

        await client.DeleteEnvironmentAsync(settings.EnvironmentId);
        renderer.RenderSuccess($"Environment {settings.EnvironmentId} deleted successfully.");
        return 0;
    }
}
