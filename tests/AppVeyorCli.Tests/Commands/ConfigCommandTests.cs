using AppVeyorCli.Api;
using AppVeyorCli.Commands.Config;
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

public class ConfigCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public ConfigCommandTests()
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
            config.AddBranch("config", c =>
            {
                c.AddCommand<ConfigGetCommand>("get");
                c.AddCommand<ConfigTestCommand>("test");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task ConfigGet_ShowsConfiguration()
    {
        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["config", "get"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("Configuration", output);
    }

    [Fact]
    public async Task ConfigTest_SuccessfulConnection()
    {
        var projects = Array.Empty<Project>();

        _server.RegisterJsonResponse("GET", "/api/projects", 200,
            projects, AppVeyorJsonContext.Default.ProjectArray);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["config", "test"]);

        Assert.Equal(0, result);
        Assert.Contains("Successfully connected", testConsole.Output);
    }

    [Fact]
    public async Task ConfigTest_Json_ReturnsValidJson()
    {
        var projects = Array.Empty<Project>();

        _server.RegisterJsonResponse("GET", "/api/projects", 200,
            projects, AppVeyorJsonContext.Default.ProjectArray);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["config", "test", "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(
            testConsole.Output.Trim(), AppVeyorJsonContext.Default.ConnectionTestResult);
        Assert.NotNull(parsed);
        Assert.True(parsed.Connected);
    }

    public void Dispose() => _server.Dispose();
}
