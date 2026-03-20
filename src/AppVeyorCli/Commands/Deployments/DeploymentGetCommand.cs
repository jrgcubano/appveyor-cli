using System.ComponentModel;
using System.Globalization;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Deployments;

public sealed class DeploymentGetSettings : GlobalSettings
{
    [CommandArgument(0, "<deploymentId>")]
    [Description("Deployment ID")]
    public int DeploymentId { get; init; }
}

public sealed class DeploymentGetCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<DeploymentGetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DeploymentGetSettings settings)
    {
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);
        var result = await client.GetDeploymentAsync(settings.DeploymentId);

        if (settings.Json)
        {
            renderer.RenderJson(result, AppVeyorJsonContext.Default.DeploymentDetails);
        }
        else
        {
            renderer.RenderDetail($"Deployment #{result.Deployment.DeploymentId}",
                ("Status", result.Deployment.Status),
                ("Environment", result.Deployment.Environment?.Name ?? ""),
                ("Project", result.Deployment.Project?.Name ?? ""),
                ("Build Version", result.Deployment.Build?.Version ?? ""),
                ("Started", result.Deployment.Started?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? ""),
                ("Finished", result.Deployment.Finished?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? ""));

            if (result.Jobs is { Length: > 0 })
            {
                consoleProvider.Console.WriteLine();
                renderer.RenderTable("Jobs", result.Jobs,
                    new("Job ID", j => ((Models.BuildJob)j).JobId),
                    new("Name", j => ((Models.BuildJob)j).Name ?? ""),
                    new("Status", j => ((Models.BuildJob)j).Status, Colorize: true));
            }
        }

        return 0;
    }
}
