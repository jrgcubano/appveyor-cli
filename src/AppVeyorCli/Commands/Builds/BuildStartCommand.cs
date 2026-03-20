using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Builds;

public sealed class BuildStartSettings : GlobalSettings
{
    [CommandArgument(0, "<project>")]
    [Description("Project identifier in account/slug format")]
    public string ProjectSlug { get; init; } = string.Empty;

    [CommandOption("--branch <BRANCH>")]
    [Description("Branch to build (default: main)")]
    public string Branch { get; init; } = "main";

    [CommandOption("--commit <COMMIT>")]
    [Description("Specific commit ID to build")]
    public string? Commit { get; init; }

    public (string Account, string Slug) Parse() => ProjectSlugParser.Parse(ProjectSlug);
}

public sealed class BuildStartCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<BuildStartSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BuildStartSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var (account, slug) = settings.Parse();

        var request = new StartBuildRequest(account, slug, settings.Branch, settings.Commit);
        var build = await client.StartBuildAsync(request);

        if (settings.Json)
        {
            renderer.RenderJson(build, AppVeyorJsonContext.Default.Build);
        }
        else
        {
            renderer.RenderSuccess($"Build {build.Version} queued for {account}/{slug} on branch {settings.Branch}.");
            renderer.RenderDetail("Build Started",
                ("Version", build.Version),
                ("Branch", build.Branch),
                ("Status", build.Status));
        }

        return 0;
    }
}
