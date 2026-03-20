using AppVeyorCli.Api;
using AppVeyorCli.Commands.Projects;
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

public class ProjectCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public ProjectCommandTests()
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
            config.AddBranch("project", p =>
            {
                p.AddCommand<ProjectListCommand>("list");
                p.AddCommand<ProjectGetCommand>("get");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task ProjectList_ReturnsProjects()
    {
        var projects = new[]
        {
            new Project(1, 10, "myaccount", "MyApp", "myapp", "gitHub", "owner/myapp", false,
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 3, 19, 10, 30, 0, DateTimeKind.Utc))
        };

        _server.RegisterJsonResponse("GET", "/api/projects", 200, projects, AppVeyorJsonContext.Default.ProjectArray);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["project", "list"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("myapp", output);
        Assert.Contains("MyApp", output);
    }

    [Fact]
    public async Task ProjectList_Json_ReturnsValidJson()
    {
        var projects = new[]
        {
            new Project(1, 10, "myaccount", "MyApp", "myapp", "gitHub", "owner/myapp", false,
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 3, 19, 10, 30, 0, DateTimeKind.Utc))
        };

        _server.RegisterJsonResponse("GET", "/api/projects", 200, projects, AppVeyorJsonContext.Default.ProjectArray);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["project", "list", "--json"]);

        Assert.Equal(0, result);
        var output = testConsole.Output.Trim();
        var parsed = System.Text.Json.JsonSerializer.Deserialize(output, AppVeyorJsonContext.Default.ProjectArray);
        Assert.NotNull(parsed);
        Assert.Single(parsed);
        Assert.Equal("myapp", parsed[0].Slug);
    }

    [Fact]
    public async Task ProjectGet_ShowsProjectDetails()
    {
        var projectWithBuild = new ProjectWithBuilds(
            new Project(1, 10, "myaccount", "MyApp", "myapp", "gitHub", "owner/myapp", false,
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 3, 19, 10, 30, 0, DateTimeKind.Utc)),
            new Build(100, "1.0.42", "success", "main", "abc123", "Fix something", "Author",
                new DateTime(2026, 3, 19, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 3, 19, 10, 5, 0, DateTimeKind.Utc), null));

        _server.RegisterJsonResponse("GET", "/api/projects/myaccount/myapp", 200,
            projectWithBuild, AppVeyorJsonContext.Default.ProjectWithBuilds);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["project", "get", "myaccount/myapp"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("MyApp", output);
        Assert.Contains("myapp", output);
        Assert.Contains("1.0.42", output);
    }

    [Fact]
    public async Task ProjectGet_Json_ReturnsValidJson()
    {
        var projectWithBuild = new ProjectWithBuilds(
            new Project(1, 10, "myaccount", "MyApp", "myapp", "gitHub", "owner/myapp", false,
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 3, 19, 10, 30, 0, DateTimeKind.Utc)),
            null);

        _server.RegisterJsonResponse("GET", "/api/projects/myaccount/myapp", 200,
            projectWithBuild, AppVeyorJsonContext.Default.ProjectWithBuilds);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["project", "get", "myaccount/myapp", "--json"]);

        Assert.Equal(0, result);
        var output = testConsole.Output.Trim();
        var parsed = System.Text.Json.JsonSerializer.Deserialize(output, AppVeyorJsonContext.Default.ProjectWithBuilds);
        Assert.NotNull(parsed);
        Assert.Equal("myapp", parsed.Project.Slug);
    }

    public void Dispose() => _server.Dispose();
}
