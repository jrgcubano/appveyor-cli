using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Roles;

public sealed class RoleDeleteSettings : GlobalSettings
{
    [CommandArgument(0, "<roleId>")]
    [Description("Role ID to delete")]
    public int RoleId { get; init; }

    [CommandOption("--force")]
    [Description("Skip confirmation prompt")]
    public bool Force { get; init; }
}

public sealed class RoleDeleteCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<RoleDeleteSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RoleDeleteSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);

        if (!settings.Force && !settings.Json)
        {
            if (!consoleProvider.Console.Confirm($"Are you sure you want to delete role [red]{settings.RoleId}[/]?", false))
            {
                renderer.RenderError("Operation cancelled.");
                return 1;
            }
        }

        await client.DeleteRoleAsync(settings.RoleId);
        renderer.RenderSuccess($"Role {settings.RoleId.ToString(CultureInfo.InvariantCulture)} deleted successfully.");
        return 0;
    }
}
