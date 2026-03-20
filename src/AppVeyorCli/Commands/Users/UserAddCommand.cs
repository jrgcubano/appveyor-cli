using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Users;

public sealed class UserAddSettings : GlobalSettings
{
    [CommandOption("--name <NAME>")]
    [Description("User full name")]
    public string FullName { get; init; } = string.Empty;

    [CommandOption("--email <EMAIL>")]
    [Description("User email address")]
    public string Email { get; init; } = string.Empty;

    [CommandOption("--role-id <ROLEID>")]
    [Description("Role ID to assign")]
    public int RoleId { get; init; }
}

public sealed class UserAddCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<UserAddSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, UserAddSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var request = new AddUserRequest(settings.FullName, settings.Email, settings.RoleId);
        await client.AddUserAsync(request);

        renderer.RenderSuccess($"User '{settings.FullName}' ({settings.Email}) added successfully.");
        return 0;
    }
}
