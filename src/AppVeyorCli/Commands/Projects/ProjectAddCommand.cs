using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Projects;

public sealed class ProjectAddSettings : GlobalSettings
{
    [CommandOption("--repo-provider <PROVIDER>")]
    [Description("Repository provider (gitHub, bitBucket, vso, gitLab, kiln, stash, git, mercurial, subversion)")]
    public string RepoProvider { get; init; } = string.Empty;

    [CommandOption("--repo-name <NAME>")]
    [Description("Repository name (e.g. owner/repo)")]
    public string RepoName { get; init; } = string.Empty;
}

public sealed class ProjectAddCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<ProjectAddSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ProjectAddSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var request = new AddProjectRequest(settings.RepoProvider, settings.RepoName);
        var project = await client.AddProjectAsync(request);

        if (settings.Json)
        {
            renderer.RenderJson(project, AppVeyorJsonContext.Default.Project);
        }
        else
        {
            renderer.RenderSuccess($"Project '{project.Name}' added successfully.");
            renderer.RenderDetail("New Project",
                ("Slug", project.Slug),
                ("Account", project.AccountName),
                ("Repository", project.RepositoryName));
        }

        return 0;
    }
}
