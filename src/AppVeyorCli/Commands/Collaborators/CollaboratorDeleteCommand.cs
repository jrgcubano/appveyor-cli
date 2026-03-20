using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Collaborators;

public sealed class CollaboratorDeleteSettings : GlobalSettings
{
    [CommandArgument(0, "<userId>")]
    [Description("Collaborator user ID to remove")]
    public int UserId { get; init; }

    [CommandOption("--force")]
    [Description("Skip confirmation prompt")]
    public bool Force { get; init; }
}

public sealed class CollaboratorDeleteCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<CollaboratorDeleteSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CollaboratorDeleteSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);

        if (!settings.Force && !settings.Json)
        {
            if (!consoleProvider.Console.Confirm($"Are you sure you want to remove collaborator [red]{settings.UserId}[/]?", false))
            {
                renderer.RenderError("Operation cancelled.");
                return 1;
            }
        }

        await client.DeleteCollaboratorAsync(settings.UserId);
        renderer.RenderSuccess($"Collaborator {settings.UserId.ToString(CultureInfo.InvariantCulture)} removed successfully.");
        return 0;
    }
}
