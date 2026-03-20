using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Users;

public sealed class UserGetSettings : GlobalSettings
{
    [CommandArgument(0, "<userId>")]
    [Description("User ID")]
    public int UserId { get; init; }
}

public sealed class UserGetCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<UserGetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, UserGetSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var result = await client.GetUserAsync(settings.UserId);

        if (settings.Json)
        {
            renderer.RenderJson(result, AppVeyorJsonContext.Default.UserDetails);
        }
        else
        {
            renderer.RenderDetail($"User: {result.User.FullName}",
                ("User ID", result.User.UserId.ToString(CultureInfo.InvariantCulture)),
                ("Email", result.User.Email),
                ("Role", result.User.RoleName),
                ("Owner", result.User.IsOwner ? "Yes" : "No"),
                ("Created", result.User.Created.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)),
                ("Updated", result.User.Updated.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));
        }

        return 0;
    }
}
