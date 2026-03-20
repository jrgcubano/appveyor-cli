using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Collaborators;

public sealed class CollaboratorAddSettings : GlobalSettings
{
    [CommandOption("--email <EMAIL>")]
    [Description("Collaborator email address")]
    public string Email { get; init; } = string.Empty;

    [CommandOption("--role-id <ROLEID>")]
    [Description("Role ID to assign")]
    public int RoleId { get; init; }
}

public sealed class CollaboratorAddCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<CollaboratorAddSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CollaboratorAddSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var request = new AddCollaboratorRequest(settings.Email, settings.RoleId);
        await client.AddCollaboratorAsync(request);

        renderer.RenderSuccess($"Collaborator '{settings.Email}' added successfully.");
        return 0;
    }
}
