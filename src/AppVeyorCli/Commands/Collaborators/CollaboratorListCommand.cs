using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Collaborators;

public sealed class CollaboratorListCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<GlobalSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var collaborators = await client.GetCollaboratorsAsync();

        if (settings.Json)
        {
            renderer.RenderJson(collaborators, AppVeyorJsonContext.Default.UserArray);
        }
        else
        {
            renderer.RenderTable("Collaborators", collaborators,
                new("User ID", c => ((Models.User)c).UserId.ToString(CultureInfo.InvariantCulture)),
                new("Name", c => Markup.Escape(((Models.User)c).FullName)),
                new("Email", c => Markup.Escape(((Models.User)c).Email)),
                new("Role", c => Markup.Escape(((Models.User)c).RoleName)));
        }

        return 0;
    }
}
