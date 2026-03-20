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

public class BuildCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public BuildCommandTests()
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
                b.AddCommand<BuildStartCommand>("start");
                b.AddCommand<BuildHistoryCommand>("history");
                b.AddCommand<BuildCancelCommand>("cancel");
                b.AddCommand<BuildLogCommand>("log");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task BuildStart_QueuesBuild()
    {
        var build = new Build(200, "1.0.50", "queued", "main", null, null, null, null, null, null);
        _server.RegisterJsonResponse("POST", "/api/builds", 200, build, AppVeyorJsonContext.Default.Build);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["build", "start", "myaccount/myapp", "--branch", "main"]);

        Assert.Equal(0, result);
        Assert.Contains("1.0.50", testConsole.Output);
        Assert.Contains("queued", testConsole.Output);
    }

    [Fact]
    public async Task BuildStart_Json_ReturnsValidJson()
    {
        var build = new Build(200, "1.0.50", "queued", "main", null, null, null, null, null, null);
        _server.RegisterJsonResponse("POST", "/api/builds", 200, build, AppVeyorJsonContext.Default.Build);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["build", "start", "myaccount/myapp", "--branch", "main", "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(testConsole.Output.Trim(), AppVeyorJsonContext.Default.Build);
        Assert.NotNull(parsed);
        Assert.Equal("1.0.50", parsed.Version);
        Assert.Equal("queued", parsed.Status);
    }

    [Fact]
    public async Task BuildHistory_ShowsBuilds()
    {
        var history = new ProjectHistory(
            new Project(1, 10, "myaccount", "MyApp", "myapp", "gitHub", "owner/myapp", false, DateTime.UtcNow, DateTime.UtcNow),
            [
                new Build(100, "1.0.42", "success", "main", "abc", "Fix bug", "Author",
                    new DateTime(2026, 3, 19, 10, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 3, 19, 10, 2, 31, DateTimeKind.Utc), null),
                new Build(99, "1.0.41", "failed", "main", "def", "Add feature", "Author",
                    new DateTime(2026, 3, 19, 9, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 3, 19, 9, 1, 12, DateTimeKind.Utc), null)
            ]);

        _server.RegisterJsonResponse("GET", "/api/projects/myaccount/myapp/history", 200,
            history, AppVeyorJsonContext.Default.ProjectHistory);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["build", "history", "myaccount/myapp"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("1.0.42", output);
        Assert.Contains("1.0.41", output);
    }

    [Fact]
    public async Task BuildCancel_CancelsBuild()
    {
        _server.RegisterRawResponse("DELETE", "/api/builds/myaccount/myapp/1.0.42", 204);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["build", "cancel", "myaccount/myapp", "1.0.42"]);

        Assert.Equal(0, result);
        Assert.Contains("cancelled", testConsole.Output);
    }

    [Fact]
    public async Task BuildLog_ReturnsLog()
    {
        _server.RegisterRawResponse("GET", "/api/buildjobs/job-123/log", 200, "Build log output line 1\nBuild log output line 2");

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["build", "log", "job-123"]);

        Assert.Equal(0, result);
        Assert.Contains("Build log output", testConsole.Output);
    }

    public void Dispose() => _server.Dispose();
}
