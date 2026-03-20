using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Roles;

public sealed class RoleGetSettings : GlobalSettings
{
    [CommandArgument(0, "<roleId>")]
    [Description("Role ID")]
    public int RoleId { get; init; }
}

public sealed class RoleGetCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<RoleGetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RoleGetSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var role = await client.GetRoleAsync(settings.RoleId);

        if (settings.Json)
        {
            renderer.RenderJson(role, AppVeyorJsonContext.Default.RoleWithPermissions);
        }
        else
        {
            renderer.RenderDetail($"Role: {Markup.Escape(role.Name)}",
                ("Role ID", role.RoleId.ToString(CultureInfo.InvariantCulture)),
                ("Name", role.Name),
                ("System", role.IsSystem ? "Yes" : "No"),
                ("Created", role.Created.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));

            if (role.Groups is { Length: > 0 })
            {
                foreach (var group in role.Groups)
                {
                    consoleProvider.Console.WriteLine();
                    renderer.RenderTable($"Permissions: {Markup.Escape(group.Name)}", group.Permissions,
                        new("Permission", p => Markup.Escape(((Models.Permission)p).Name)),
                        new("Description", p => Markup.Escape(((Models.Permission)p).Description)),
                        new("Allowed", p => ((Models.Permission)p).Allowed ? "[green]Yes[/]" : "[red]No[/]"));
                }
            }
        }

        return 0;
    }
}
