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

public class EnvironmentCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public EnvironmentCommandTests()
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
                e.AddCommand<EnvironmentListCommand>("list");
                e.AddCommand<EnvironmentGetCommand>("get");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task EnvironmentList_ShowsEnvironments()
    {
        var envs = new[]
        {
            new DeploymentEnvironment(1, "Production", "Agent",
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 3, 19, 0, 0, 0, DateTimeKind.Utc)),
            new DeploymentEnvironment(2, "Staging", "AzureWebJob",
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc))
        };

        _server.RegisterJsonResponse("GET", "/api/environments", 200, envs, AppVeyorJsonContext.Default.DeploymentEnvironmentArray);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["environment", "list"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("Production", output);
        Assert.Contains("Staging", output);
    }

    [Fact]
    public async Task EnvironmentList_Json_ReturnsValidJson()
    {
        var envs = new[]
        {
            new DeploymentEnvironment(1, "Production", "Agent",
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 3, 19, 0, 0, 0, DateTimeKind.Utc))
        };

        _server.RegisterJsonResponse("GET", "/api/environments", 200, envs, AppVeyorJsonContext.Default.DeploymentEnvironmentArray);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["environment", "list", "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(testConsole.Output.Trim(), AppVeyorJsonContext.Default.DeploymentEnvironmentArray);
        Assert.NotNull(parsed);
        Assert.Single(parsed);
        Assert.Equal("Production", parsed[0].Name);
    }

    public void Dispose() => _server.Dispose();
}
