using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Projects;

public sealed class ProjectListCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<GlobalSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var projects = await client.GetProjectsAsync();

        if (settings.Json)
        {
            renderer.RenderJson(projects, AppVeyorJsonContext.Default.ProjectArray);
        }
        else
        {
            renderer.RenderTable("Projects", projects,
                new("Slug", p => ((Models.Project)p).Slug),
                new("Name", p => ((Models.Project)p).Name),
                new("Repository", p => ((Models.Project)p).RepositoryName),
                new("Private", p => ((Models.Project)p).IsPrivate ? "Yes" : "No"),
                new("Updated", p => ((Models.Project)p).Updated.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));
        }

        return 0;
    }
}
