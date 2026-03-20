using AppVeyorCli.Api;
using AppVeyorCli.Commands.Collaborators;
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

public class CollaboratorCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public CollaboratorCommandTests()
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
            config.AddBranch("collaborator", co =>
            {
                co.AddCommand<CollaboratorListCommand>("list");
                co.AddCommand<CollaboratorAddCommand>("add");
                co.AddCommand<CollaboratorDeleteCommand>("delete");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task CollaboratorList_ShowsCollaborators()
    {
        var collaborators = new[]
        {
            new User(10, "myaccount", false, true, 101, "Jane Doe", "jane@example.com",
                2, "Developer", null, null, false,
                new DateTime(2024, 1, 1), new DateTime(2024, 6, 1))
        };

        _server.RegisterJsonResponse("GET", "/api/collaborators", 200,
            collaborators, AppVeyorJsonContext.Default.UserArray);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["collaborator", "list"]);

        Assert.Equal(0, result);
        var output = testConsole.Output;
        Assert.Contains("Jane Doe", output);
        Assert.Contains("jane@example.com", output);
    }

    [Fact]
    public async Task CollaboratorList_Json_ReturnsValidJson()
    {
        var collaborators = new[]
        {
            new User(10, "myaccount", false, true, 101, "Jane Doe", "jane@example.com",
                2, "Developer", null, null, false,
                new DateTime(2024, 1, 1), new DateTime(2024, 6, 1))
        };

        _server.RegisterJsonResponse("GET", "/api/collaborators", 200,
            collaborators, AppVeyorJsonContext.Default.UserArray);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["collaborator", "list", "--json"]);

        Assert.Equal(0, result);
        var parsed = System.Text.Json.JsonSerializer.Deserialize(
            testConsole.Output.Trim(), AppVeyorJsonContext.Default.UserArray);
        Assert.NotNull(parsed);
        Assert.Single(parsed);
        Assert.Equal("Jane Doe", parsed[0].FullName);
    }

    [Fact]
    public async Task CollaboratorAdd_AddsCollaborator()
    {
        _server.RegisterRawResponse("POST", "/api/collaborators", 204);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["collaborator", "add",
            "--email", "jane@example.com", "--role-id", "2"]);

        Assert.Equal(0, result);
        Assert.Contains("added", testConsole.Output);
    }

    [Fact]
    public async Task CollaboratorDelete_WithForce_DeletesCollaborator()
    {
        _server.RegisterRawResponse("DELETE", "/api/collaborators/101", 204);

        var (app, testConsole) = CreateApp();
        var result = await app.RunAsync(["collaborator", "delete", "101", "--force"]);

        Assert.Equal(0, result);
        Assert.Contains("removed", testConsole.Output);
    }

    public void Dispose() => _server.Dispose();
}
