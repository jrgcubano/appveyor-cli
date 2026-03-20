using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Projects;

public sealed class ProjectGetSettings : GlobalSettings
{
    [CommandArgument(0, "<project>")]
    [Description("Project identifier in account/slug format")]
    public string ProjectSlug { get; init; } = string.Empty;

    public (string Account, string Slug) Parse() => ProjectSlugParser.Parse(ProjectSlug);
}

public sealed class ProjectGetCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<ProjectGetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ProjectGetSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var (account, slug) = settings.Parse();
        var result = await client.GetProjectLastBuildAsync(account, slug);

        if (settings.Json)
        {
            renderer.RenderJson(result, AppVeyorJsonContext.Default.ProjectWithBuilds);
        }
        else
        {
            renderer.RenderDetail($"Project: {result.Project.Name}",
                ("Slug", result.Project.Slug),
                ("Account", result.Project.AccountName),
                ("Repository", result.Project.RepositoryName),
                ("Type", result.Project.RepositoryType),
                ("Private", result.Project.IsPrivate ? "Yes" : "No"),
                ("Created", result.Project.Created.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)),
                ("Updated", result.Project.Updated.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));

            if (result.Build is not null)
            {
                consoleProvider.Console.WriteLine();
                renderer.RenderDetail("Last Build",
                    ("Version", result.Build.Version),
                    ("Branch", result.Build.Branch),
                    ("Status", result.Build.Status),
                    ("Message", result.Build.Message ?? ""),
                    ("Started", result.Build.Started?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? ""),
                    ("Finished", result.Build.Finished?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? ""));
            }
        }

        return 0;
    }
}
