using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Users;

public sealed class UserDeleteSettings : GlobalSettings
{
    [CommandArgument(0, "<userId>")]
    [Description("User ID to delete")]
    public int UserId { get; init; }

    [CommandOption("--force")]
    [Description("Skip confirmation prompt")]
    public bool Force { get; init; }
}

public sealed class UserDeleteCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<UserDeleteSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, UserDeleteSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);

        if (!settings.Force && !settings.Json)
        {
            if (!consoleProvider.Console.Confirm($"Are you sure you want to delete user [red]{settings.UserId}[/]?", false))
            {
                renderer.RenderError("Operation cancelled.");
                return 1;
            }
        }

        await client.DeleteUserAsync(settings.UserId);
        renderer.RenderSuccess($"User {settings.UserId.ToString(CultureInfo.InvariantCulture)} deleted successfully.");
        return 0;
    }
}
