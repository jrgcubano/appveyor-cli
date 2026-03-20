using AppVeyorCli.Api;
using AppVeyorCli.Commands.Builds;
using AppVeyorCli.Commands.Config;
using AppVeyorCli.Commands.Deployments;
using AppVeyorCli.Commands.Environments;
using AppVeyorCli.Commands.Projects;
using AppVeyorCli.Configuration;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

var services = new ServiceCollection();

// Register services
services.AddSingleton<IConfigService, ConfigService>();
services.AddSingleton<IConsoleProvider>(new ConsoleProvider(AnsiConsole.Console));
services.AddSingleton<IAppVeyorClient>(sp =>
{
    var configService = sp.GetRequiredService<IConfigService>();
    var config = configService.Load();
    if (!config.IsValid)
    {
        throw new InvalidOperationException(
            "No API token configured. Run 'appveyor config set' or set APPVEYOR_API_TOKEN environment variable.");
    }
    return new AppVeyorClient(config);
});

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("appveyor");

    config.SetExceptionHandler((ex, _) =>
    {
        if (ex is AppVeyorApiException apiEx)
        {
            AnsiConsole.MarkupLine($"[red]X[/] {Markup.Escape(apiEx.Message)}");
            return (int)apiEx.StatusCode;
        }

        AnsiConsole.MarkupLine($"[red]X[/] {Markup.Escape(ex.Message)}");
        return 1;
    });

    config.AddBranch("config", c =>
    {
        c.SetDescription("Manage CLI configuration");
        c.AddCommand<ConfigSetCommand>("set").WithDescription("Set configuration values");
        c.AddCommand<ConfigGetCommand>("get").WithDescription("Show current configuration");
        c.AddCommand<ConfigTestCommand>("test").WithDescription("Test API connection");
    });

    config.AddBranch("project", p =>
    {
        p.SetDescription("Manage AppVeyor projects");
        p.AddCommand<ProjectListCommand>("list").WithDescription("List all projects");
        p.AddCommand<ProjectGetCommand>("get").WithDescription("Get project details and last build");
        p.AddCommand<ProjectAddCommand>("add").WithDescription("Add a new project");
        p.AddCommand<ProjectDeleteCommand>("delete").WithDescription("Delete a project");
        p.AddCommand<ProjectSettingsCommand>("settings").WithDescription("View project settings");
    });

    config.AddBranch("build", b =>
    {
        b.SetDescription("Manage builds");
        b.AddCommand<BuildStartCommand>("start").WithDescription("Start a new build");
        b.AddCommand<BuildHistoryCommand>("history").WithDescription("View build history");
        b.AddCommand<BuildCancelCommand>("cancel").WithDescription("Cancel a running build");
        b.AddCommand<BuildRerunCommand>("rerun").WithDescription("Re-run a build");
        b.AddCommand<BuildLogCommand>("log").WithDescription("Download build job log");
    });

    config.AddBranch("environment", e =>
    {
        e.SetDescription("Manage deployment environments");
        e.AddCommand<EnvironmentListCommand>("list").WithDescription("List all environments");
        e.AddCommand<EnvironmentGetCommand>("get").WithDescription("Get environment details");
        e.AddCommand<EnvironmentAddCommand>("add").WithDescription("Add a new environment");
        e.AddCommand<EnvironmentDeleteCommand>("delete").WithDescription("Delete an environment");
    });

    config.AddBranch("deployment", d =>
    {
        d.SetDescription("Manage deployments");
        d.AddCommand<DeploymentGetCommand>("get").WithDescription("Get deployment details");
        d.AddCommand<DeploymentStartCommand>("start").WithDescription("Start a deployment");
        d.AddCommand<DeploymentCancelCommand>("cancel").WithDescription("Cancel a deployment");
    });

#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

return await app.RunAsync(args);
