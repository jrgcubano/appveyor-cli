using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Builds;

public sealed class BuildCancelSettings : GlobalSettings
{
    [CommandArgument(0, "<project>")]
    [Description("Project identifier in account/slug format")]
    public string ProjectSlug { get; init; } = string.Empty;

    [CommandArgument(1, "<version>")]
    [Description("Build version to cancel")]
    public string Version { get; init; } = string.Empty;

    public (string Account, string Slug) Parse() => ProjectSlugParser.Parse(ProjectSlug);
}

public sealed class BuildCancelCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<BuildCancelSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BuildCancelSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var (account, slug) = settings.Parse();

        await client.CancelBuildAsync(account, slug, settings.Version);
        renderer.RenderSuccess($"Build {settings.Version} cancelled for {account}/{slug}.");
        return 0;
    }
}
