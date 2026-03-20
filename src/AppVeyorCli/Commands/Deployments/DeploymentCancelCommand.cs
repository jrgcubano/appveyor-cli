using System.ComponentModel;
using AppVeyorCli.Api;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Deployments;

public sealed class DeploymentCancelSettings : GlobalSettings
{
    [CommandArgument(0, "<deploymentId>")]
    [Description("Deployment ID to cancel")]
    public int DeploymentId { get; init; }
}

public sealed class DeploymentCancelCommand(IAppVeyorClient client, IConsoleProvider consoleProvider) : AsyncCommand<DeploymentCancelSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DeploymentCancelSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);

        await client.CancelDeploymentAsync(settings.DeploymentId);
        renderer.RenderSuccess($"Deployment #{settings.DeploymentId} cancelled.");
        return 0;
    }
}
