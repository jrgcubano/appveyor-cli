using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Builds;

public sealed class BuildLogSettings : GlobalSettings
{
    [CommandArgument(0, "<jobId>")]
    [Description("Build job ID")]
    public string JobId { get; init; } = string.Empty;
}

public sealed class BuildLogCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<BuildLogSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BuildLogSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var log = await client.GetBuildLogAsync(settings.JobId);

        if (settings.Json)
        {
            renderer.RenderJson(new BuildLogResult(settings.JobId, log), AppVeyorJsonContext.Default.BuildLogResult);
        }
        else
        {
            renderer.RenderText(log);
        }

        return 0;
    }
}
