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

public class ProjectWriteCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public ProjectWriteCommandTests()
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
                p.AddCommand<ProjectAddCommand>("add");
                p.AddCommand<ProjectDeleteCommand>("delete");
                p.AddCommand<ProjectSettingsCommand>("settings");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task ProjectAdd_AddsProject()
    {
        var project = new Project(1, 10, "myaccount", "MyRepo", "myrepo", "gitHub", "owner/repo",
            false, new DateTime(2024, 1, 1), new DateTime(2024, 1, 1));

        _server.RegisterJsonResponse("POST", "/api/projects", 200,
            project, AppVeyorJsonContext.Default.Project);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["project", "add",
            "--repo-provider", "gitHub", "--repo-name", "owner/repo"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("added", output);
        Assert.Contains("MyRepo", output);
    }

    [Fact]
    public async Task ProjectAdd_Json_ReturnsValidJson()
    {
        var project = new Project(1, 10, "myaccount", "MyRepo", "myrepo", "gitHub", "owner/repo",
            false, new DateTime(2024, 1, 1), new DateTime(2024, 1, 1));

        _server.RegisterJsonResponse("POST", "/api/projects", 200,
            project, AppVeyorJsonContext.Default.Project);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["project", "add",
            "--repo-provider", "gitHub", "--repo-name", "owner/repo", "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(
            testConsole.Output.Trim(), AppVeyorJsonContext.Default.Project);
        Assert.NotNull(parsed);
        Assert.Equal("myrepo", parsed.Slug);
        Assert.Equal("owner/repo", parsed.RepositoryName);
    }

    [Fact]
    public async Task ProjectDelete_WithForce_DeletesProject()
    {
        _server.RegisterRawResponse("DELETE", "/api/projects/myaccount/myapp", 204);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["project", "delete", "myaccount/myapp", "--force"]);

        Assert.Equal(0, result);
        Assert.Contains("deleted", testConsole.Output);
    }

    [Fact]
    public async Task ProjectSettings_ShowsSettings()
    {
        var project = new Project(1, 10, "myaccount", "MyApp", "myapp", "gitHub", "owner/myapp",
            false, new DateTime(2024, 1, 1), new DateTime(2024, 6, 1));
        var settings = new ProjectSettings(project, null!);

        _server.RegisterJsonResponse("GET", "/api/projects/myaccount/myapp/settings", 200,
            settings, AppVeyorJsonContext.Default.ProjectSettings);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["project", "settings", "myaccount/myapp"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("MyApp", output);
        Assert.Contains("myapp", output);
        Assert.Contains("owner/myapp", output);
    }

    [Fact]
    public async Task ProjectSettings_Json_ReturnsValidJson()
    {
        var project = new Project(1, 10, "myaccount", "MyApp", "myapp", "gitHub", "owner/myapp",
            false, new DateTime(2024, 1, 1), new DateTime(2024, 6, 1));
        var settings = new ProjectSettings(project, null!);

        _server.RegisterJsonResponse("GET", "/api/projects/myaccount/myapp/settings", 200,
            settings, AppVeyorJsonContext.Default.ProjectSettings);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["project", "settings", "myaccount/myapp", "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(
            testConsole.Output.Trim(), AppVeyorJsonContext.Default.ProjectSettings);
        Assert.NotNull(parsed);
        Assert.Equal("myapp", parsed.Project.Slug);
    }

    public void Dispose() => _server.Dispose();
}
