using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Deployments;

public sealed class DeploymentStartSettings : GlobalSettings
{
    [CommandOption("--environment <NAME>")]
    [Description("Target environment name")]
    public string EnvironmentName { get; init; } = string.Empty;

    [CommandOption("--project <PROJECT>")]
    [Description("Project in account/slug format")]
    public string ProjectSlug { get; init; } = string.Empty;

    [CommandOption("--build-version <VERSION>")]
    [Description("Build version to deploy")]
    public string BuildVersion { get; init; } = string.Empty;

    [CommandOption("--job-id <JOBID>")]
    [Description("Specific build job ID (optional)")]
    public string? JobId { get; init; }

    public (string Account, string Slug) ParseProject() => ProjectSlugParser.Parse(ProjectSlug);
}

public sealed class DeploymentStartCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<DeploymentStartSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DeploymentStartSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var (account, slug) = settings.ParseProject();

        var request = new StartDeploymentRequest(
            settings.EnvironmentName, account, slug, settings.BuildVersion, settings.JobId);
        var deployment = await client.StartDeploymentAsync(request);

        if (settings.Json)
        {
            renderer.RenderJson(deployment, AppVeyorJsonContext.Default.Deployment);
        }
        else
        {
            renderer.RenderSuccess($"Deployment #{deployment.DeploymentId} started to '{settings.EnvironmentName}'.");
            renderer.RenderDetail("Deployment",
                ("ID", deployment.DeploymentId.ToString(CultureInfo.InvariantCulture)),
                ("Status", deployment.Status),
                ("Build", settings.BuildVersion));
        }

        return 0;
    }
}
