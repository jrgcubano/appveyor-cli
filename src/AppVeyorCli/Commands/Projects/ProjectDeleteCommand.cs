using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Projects;

public sealed class ProjectDeleteSettings : GlobalSettings
{
    [CommandArgument(0, "<project>")]
    [Description("Project identifier in account/slug format")]
    public string ProjectSlug { get; init; } = string.Empty;

    [CommandOption("--force")]
    [Description("Skip confirmation prompt")]
    public bool Force { get; init; }

    public (string Account, string Slug) Parse() => ProjectSlugParser.Parse(ProjectSlug);
}

public sealed class ProjectDeleteCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<ProjectDeleteSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ProjectDeleteSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var (account, slug) = settings.Parse();

        if (!settings.Force && !settings.Json)
        {
            if (!consoleProvider.Console.Confirm($"Are you sure you want to delete project [red]{account}/{slug}[/]?", false))
            {
                renderer.RenderError("Operation cancelled.");
                return 1;
            }
        }

        await client.DeleteProjectAsync(account, slug);
        renderer.RenderSuccess($"Project '{account}/{slug}' deleted successfully.");
        return 0;
    }
}
