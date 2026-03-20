using AppVeyorCli.Api;
using AppVeyorCli.Commands.Deployments;
using AppVeyorCli.Configuration;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace AppVeyorCli.Tests.Commands;

public class DeploymentCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public DeploymentCommandTests()
    {
        _server = new MockAppVeyorServer();
    }

    private (CommandApp App, TestConsole Console) CreateApp()
    {
        var testConsole = new TestConsole();
        var services = new ServiceCollection();
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IConsoleProvider>(new ConsoleProvider(testConsole));
        services.AddSingleton<IAppVeyorClient>(_ =>
        {
            var config = new AppVeyorConfig { Token = "test-token" };
            var httpClient = _server.CreateHttpClient();
            return new AppVeyorClient(config, httpClient);
        });

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddBranch("deployment", d =>
            {
                d.AddCommand<DeploymentGetCommand>("get");
                d.AddCommand<DeploymentStartCommand>("start");
                d.AddCommand<DeploymentCancelCommand>("cancel");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task DeploymentGet_ShowsDeploymentDetails()
    {
        var deployment = new Deployment(456, null, null, null, "success",
            new DateTime(2024, 1, 1), new DateTime(2024, 1, 1));
        var details = new DeploymentDetails(deployment, null);

        _server.RegisterJsonResponse("GET", "/api/deployments/456", 200,
            details, AppVeyorJsonContext.Default.DeploymentDetails);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["deployment", "get", "456"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("456", output);
        Assert.Contains("success", output);
    }

    [Fact]
    public async Task DeploymentGet_Json_ReturnsValidJson()
    {
        var deployment = new Deployment(456, null, null, null, "success",
            new DateTime(2024, 1, 1), new DateTime(2024, 1, 1));
        var details = new DeploymentDetails(deployment, null);

        _server.RegisterJsonResponse("GET", "/api/deployments/456", 200,
            details, AppVeyorJsonContext.Default.DeploymentDetails);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["deployment", "get", "456", "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(
            testConsole.Output.Trim(), AppVeyorJsonContext.Default.DeploymentDetails);
        Assert.NotNull(parsed);
        Assert.Equal(456, parsed.Deployment.DeploymentId);
        Assert.Equal("success", parsed.Deployment.Status);
    }

    [Fact]
    public async Task DeploymentStart_StartsDeployment()
    {
        var started = new Deployment(789, null, null, null, "running", null, null);

        _server.RegisterJsonResponse("POST", "/api/deployments", 200,
            started, AppVeyorJsonContext.Default.Deployment);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["deployment", "start",
            "--project", "myaccount/myapp",
            "--environment", "Production",
            "--build-version", "1.0.42"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("789", output);
        Assert.Contains("started", output);
    }

    [Fact]
    public async Task DeploymentStart_Json_ReturnsValidJson()
    {
        var started = new Deployment(789, null, null, null, "running", null, null);

        _server.RegisterJsonResponse("POST", "/api/deployments", 200,
            started, AppVeyorJsonContext.Default.Deployment);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["deployment", "start",
            "--project", "myaccount/myapp",
            "--environment", "Production",
            "--build-version", "1.0.42",
            "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(
            testConsole.Output.Trim(), AppVeyorJsonContext.Default.Deployment);
        Assert.NotNull(parsed);
        Assert.Equal(789, parsed.DeploymentId);
        Assert.Equal("running", parsed.Status);
    }

    [Fact]
    public async Task DeploymentCancel_CancelsDeployment()
    {
        _server.RegisterRawResponse("PUT", "/api/deployments/stop", 204);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["deployment", "cancel", "456"]);

        Assert.Equal(0, result);
        Assert.Contains("cancelled", testConsole.Output);
    }

    public void Dispose() => _server.Dispose();
}
