using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Projects;

public sealed class ProjectSettingsSettings : GlobalSettings
{
    [CommandArgument(0, "<project>")]
    [Description("Project identifier in account/slug format")]
    public string ProjectSlug { get; init; } = string.Empty;

    [CommandOption("--yaml")]
    [Description("Show settings as YAML")]
    public bool Yaml { get; init; }

    public (string Account, string Slug) Parse() => ProjectSlugParser.Parse(ProjectSlug);
}

public sealed class ProjectSettingsCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<ProjectSettingsSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ProjectSettingsSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var (account, slug) = settings.Parse();

        if (settings.Yaml)
        {
            var yaml = await client.GetProjectSettingsYamlAsync(account, slug);
            renderer.RenderText(yaml);
        }
        else if (settings.Json)
        {
            var projectSettings = await client.GetProjectSettingsAsync(account, slug);
            renderer.RenderJson(projectSettings, AppVeyorJsonContext.Default.ProjectSettings);
        }
        else
        {
            var projectSettings = await client.GetProjectSettingsAsync(account, slug);
            renderer.RenderDetail($"Settings: {projectSettings.Project.Name}",
                ("Slug", projectSettings.Project.Slug),
                ("Account", projectSettings.Project.AccountName),
                ("Repository", projectSettings.Project.RepositoryName));

            var envVars = projectSettings.Settings?.Configuration?.EnvironmentVariables;
            if (envVars is { Length: > 0 })
            {
                consoleProvider.Console.WriteLine();
                renderer.RenderTable("Environment Variables", envVars,
                    new("Name", v => ((Models.EnvironmentVariable)v).Name),
                    new("Value", v =>
                    {
                        var ev = (Models.EnvironmentVariable)v;
                        return ev.IsEncrypted ? "(encrypted)" : ev.Value;
                    }),
                    new("Encrypted", v => ((Models.EnvironmentVariable)v).IsEncrypted ? "Yes" : "No"));
            }
        }

        return 0;
    }
}
