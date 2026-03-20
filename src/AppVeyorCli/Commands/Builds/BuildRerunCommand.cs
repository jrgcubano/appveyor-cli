using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Builds;

public sealed class BuildRerunSettings : GlobalSettings
{
    [CommandArgument(0, "<buildId>")]
    [Description("Build ID to re-run")]
    public int BuildId { get; init; }

    [CommandOption("--incomplete-only")]
    [Description("Only re-run failed/incomplete jobs")]
    public bool IncompleteOnly { get; init; }
}

public sealed class BuildRerunCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<BuildRerunSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BuildRerunSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var request = new RerunBuildRequest(settings.BuildId, settings.IncompleteOnly);
        var build = await client.RerunBuildAsync(request);

        if (settings.Json)
        {
            renderer.RenderJson(build, AppVeyorJsonContext.Default.Build);
        }
        else
        {
            renderer.RenderSuccess($"Build {build.Version} re-run queued.");
        }

        return 0;
    }
}
