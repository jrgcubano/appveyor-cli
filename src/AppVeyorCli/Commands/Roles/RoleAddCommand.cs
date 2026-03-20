using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Roles;

public sealed class RoleAddSettings : GlobalSettings
{
    [CommandOption("--name <NAME>")]
    [Description("Role name")]
    public string Name { get; init; } = string.Empty;
}

public sealed class RoleAddCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<RoleAddSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RoleAddSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var request = new AddRoleRequest(settings.Name);
        var role = await client.AddRoleAsync(request);

        if (settings.Json)
        {
            renderer.RenderJson(role, AppVeyorJsonContext.Default.RoleWithPermissions);
        }
        else
        {
            renderer.RenderSuccess($"Role '{role.Name}' created successfully.");
            renderer.RenderDetail("New Role",
                ("Role ID", role.RoleId.ToString(CultureInfo.InvariantCulture)),
                ("Name", role.Name));
        }

        return 0;
    }
}
