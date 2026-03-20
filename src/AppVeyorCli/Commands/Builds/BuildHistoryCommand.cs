using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Builds;

public sealed class BuildHistorySettings : GlobalSettings
{
    [CommandArgument(0, "<project>")]
    [Description("Project identifier in account/slug format")]
    public string ProjectSlug { get; init; } = string.Empty;

    [CommandOption("--count <COUNT>")]
    [Description("Number of builds to retrieve (default: 10)")]
    public int Count { get; init; } = 10;

    [CommandOption("--branch <BRANCH>")]
    [Description("Filter by branch")]
    public string? Branch { get; init; }

    public (string Account, string Slug) Parse() => ProjectSlugParser.Parse(ProjectSlug);
}

public sealed class BuildHistoryCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<BuildHistorySettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BuildHistorySettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var (account, slug) = settings.Parse();
        var history = await client.GetProjectHistoryAsync(account, slug, settings.Count, branch: settings.Branch);

        if (settings.Json)
        {
            renderer.RenderJson(history, AppVeyorJsonContext.Default.ProjectHistory);
        }
        else
        {
            renderer.RenderTable($"Build History - {account}/{slug}", history.Builds,
                new("Version", b => ((Models.Build)b).Version),
                new("Branch", b => ((Models.Build)b).Branch),
                new("Status", b => ((Models.Build)b).Status, Colorize: true),
                new("Message", b => Truncate(((Models.Build)b).Message ?? "", 40)),
                new("Started", b => ((Models.Build)b).Started?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? ""),
                new("Duration", b => FormatDuration((Models.Build)b)));
        }

        return 0;
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";

    private static string FormatDuration(Models.Build build)
    {
        if (build.Started is null || build.Finished is null)
        {
            return "";
        }

        var duration = build.Finished.Value - build.Started.Value;
        return duration.TotalMinutes >= 1
            ? $"{(int)duration.TotalMinutes}m {duration.Seconds}s"
            : $"{(int)duration.TotalSeconds}s";
    }
}
