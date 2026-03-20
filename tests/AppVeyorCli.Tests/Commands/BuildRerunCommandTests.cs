using AppVeyorCli.Api;
using AppVeyorCli.Commands.Builds;
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

public class BuildRerunCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public BuildRerunCommandTests()
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
            config.AddBranch("build", b =>
            {
                b.AddCommand<BuildRerunCommand>("rerun");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task BuildRerun_RerunsBuild()
    {
        var build = new Build(12345, "1.0.50", "queued", "main", null, null, null, null, null, null);

        _server.RegisterJsonResponse("PUT", "/api/builds", 200,
            build, AppVeyorJsonContext.Default.Build);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["build", "rerun", "12345"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("1.0.50", output);
        Assert.Contains("re-run", output);
    }

    [Fact]
    public async Task BuildRerun_Json_ReturnsValidJson()
    {
        var build = new Build(12345, "1.0.50", "queued", "main", null, null, null, null, null, null);

        _server.RegisterJsonResponse("PUT", "/api/builds", 200,
            build, AppVeyorJsonContext.Default.Build);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["build", "rerun", "12345", "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(
            testConsole.Output.Trim(), AppVeyorJsonContext.Default.Build);
        Assert.NotNull(parsed);
        Assert.Equal(12345, parsed.BuildId);
        Assert.Equal("1.0.50", parsed.Version);
    }

    public void Dispose() => _server.Dispose();
}
