using AppVeyorCli.Api;
using AppVeyorCli.Commands.Environments;
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

public class EnvironmentWriteCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public EnvironmentWriteCommandTests()
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
            config.AddBranch("environment", e =>
            {
                e.AddCommand<EnvironmentAddCommand>("add");
                e.AddCommand<EnvironmentDeleteCommand>("delete");
                e.AddCommand<EnvironmentGetCommand>("get");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task EnvironmentAdd_CreatesEnvironment()
    {
        var env = new DeploymentEnvironment(1, "Production", "Agent",
            new DateTime(2024, 1, 1), new DateTime(2024, 6, 1));

        _server.RegisterJsonResponse("POST", "/api/environments", 200,
            env, AppVeyorJsonContext.Default.DeploymentEnvironment);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["environment", "add",
            "--name", "Production", "--provider", "Agent"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("Production", output);
        Assert.Contains("created", output);
    }

    [Fact]
    public async Task EnvironmentAdd_Json_ReturnsValidJson()
    {
        var env = new DeploymentEnvironment(1, "Production", "Agent",
            new DateTime(2024, 1, 1), new DateTime(2024, 6, 1));

        _server.RegisterJsonResponse("POST", "/api/environments", 200,
            env, AppVeyorJsonContext.Default.DeploymentEnvironment);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["environment", "add",
            "--name", "Production", "--provider", "Agent", "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(
            testConsole.Output.Trim(), AppVeyorJsonContext.Default.DeploymentEnvironment);
        Assert.NotNull(parsed);
        Assert.Equal("Production", parsed.Name);
        Assert.Equal("Agent", parsed.Provider);
    }

    [Fact]
    public async Task EnvironmentDelete_WithForce_DeletesEnvironment()
    {
        _server.RegisterRawResponse("DELETE", "/api/environments/1", 204);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["environment", "delete", "1", "--force"]);

        Assert.Equal(0, result);
        Assert.Contains("deleted", testConsole.Output);
    }

    [Fact]
    public async Task EnvironmentGet_ShowsEnvironmentSettings()
    {
        var env = new DeploymentEnvironment(1, "Production", "Agent",
            new DateTime(2024, 1, 1), new DateTime(2024, 6, 1));
        var envWithSettings = new EnvironmentWithSettings(env, null);

        _server.RegisterJsonResponse("GET", "/api/environments/1/settings", 200,
            envWithSettings, AppVeyorJsonContext.Default.EnvironmentWithSettings);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["environment", "get", "1"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("Production", output);
        Assert.Contains("Agent", output);
    }

    [Fact]
    public async Task EnvironmentGet_Json_ReturnsValidJson()
    {
        var env = new DeploymentEnvironment(1, "Production", "Agent",
            new DateTime(2024, 1, 1), new DateTime(2024, 6, 1));
        var envWithSettings = new EnvironmentWithSettings(env, null);

        _server.RegisterJsonResponse("GET", "/api/environments/1/settings", 200,
            envWithSettings, AppVeyorJsonContext.Default.EnvironmentWithSettings);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["environment", "get", "1", "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(
            testConsole.Output.Trim(), AppVeyorJsonContext.Default.EnvironmentWithSettings);
        Assert.NotNull(parsed);
        Assert.Equal("Production", parsed.Environment.Name);
    }

    public void Dispose() => _server.Dispose();
}
