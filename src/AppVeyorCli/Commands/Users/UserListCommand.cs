using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Users;

public sealed class UserListCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<GlobalSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var users = await client.GetUsersAsync();

        if (settings.Json)
        {
            renderer.RenderJson(users, AppVeyorJsonContext.Default.UserArray);
        }
        else
        {
            renderer.RenderTable("Users", users,
                new("User ID", u => ((Models.User)u).UserId.ToString(CultureInfo.InvariantCulture)),
                new("Name", u => Markup.Escape(((Models.User)u).FullName)),
                new("Email", u => Markup.Escape(((Models.User)u).Email)),
                new("Role", u => Markup.Escape(((Models.User)u).RoleName)),
                new("Owner", u => ((Models.User)u).IsOwner ? "Yes" : "No"));
        }

        return 0;
    }
}
