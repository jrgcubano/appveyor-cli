using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Roles;

public sealed class RoleListCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<GlobalSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var roles = await client.GetRolesAsync();

        if (settings.Json)
        {
            renderer.RenderJson(roles, AppVeyorJsonContext.Default.RoleArray);
        }
        else
        {
            renderer.RenderTable("Roles", roles,
                new("Role ID", r => ((Models.Role)r).RoleId.ToString(CultureInfo.InvariantCulture)),
                new("Name", r => ((Models.Role)r).Name),
                new("System", r => ((Models.Role)r).IsSystem ? "Yes" : "No"),
                new("Created", r => ((Models.Role)r).Created.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));
        }

        return 0;
    }
}
