using AppVeyorCli.Api;
using AppVeyorCli.Commands.Roles;
using AppVeyorCli.Configuration;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Models;
using AppVeyorCli.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using AppVeyorCli.Output;
using Spectre.Console.Cli;
using System.Globalization;
using Spectre.Console.Testing;

namespace AppVeyorCli.Tests.Commands;

public class RoleCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public RoleCommandTests()
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
            config.AddBranch("role", r =>
            {
                r.AddCommand<RoleListCommand>("list");
                r.AddCommand<RoleGetCommand>("get");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task RoleList_RendersTable()
    {
        var roles = new[]
        {
            new Role(1, "Administrator", true, new DateTime(2024, 1, 1)),
            new Role(2, "Developer", false, new DateTime(2024, 3, 1)),
            new Role(3, "Read-Only", false, new DateTime(2024, 5, 1))
        };

        _server.RegisterJsonResponse("GET", "/api/roles", 200, roles, AppVeyorJsonContext.Default.RoleArray);
        var (app, console) = CreateApp();

        var result = await app.RunAsync(["role", "list"]);

        Assert.Equal(0, result);
        var output = console.Output;
        Assert.Contains("Administrator", output);
        Assert.Contains("Developer", output);
        Assert.Contains("Read-Only", output);
    }

    [Fact]
    public async Task RoleList_Json_RendersJsonOutput()
    {
        var roles = new[]
        {
            new Role(1, "Administrator", true, new DateTime(2024, 1, 1))
        };

        _server.RegisterJsonResponse("GET", "/api/roles", 200, roles, AppVeyorJsonContext.Default.RoleArray);
        var (app, console) = CreateApp();

        var result = await app.RunAsync(["role", "list", "--json"]);

        Assert.Equal(0, result);
        var output = console.Output;
        Assert.Contains("\"name\"", output);
        Assert.Contains("Administrator", output);
    }

    [Fact]
    public async Task RoleGet_RendersDetailWithPermissions()
    {
        var role = new RoleWithPermissions(1, "Developer", false, new DateTime(2024, 1, 1),
        [
            new PermissionGroup("Projects",
            [
                new Permission("ManageProjects", "Create and delete projects", true),
                new Permission("RunProjectBuild", "Run project builds", true)
            ]),
            new PermissionGroup("Environments",
            [
                new Permission("DeployToEnvironment", "Deploy to environment", false)
            ])
        ]);

        _server.RegisterJsonResponse("GET", "/api/roles/1", 200, role, AppVeyorJsonContext.Default.RoleWithPermissions);
        var (app, console) = CreateApp();

        var result = await app.RunAsync(["role", "get", "1"]);

        Assert.Equal(0, result);
        var output = console.Output;
        Assert.Contains("Developer", output);
        Assert.Contains("ManageProjects", output);
        Assert.Contains("DeployToEnvironment", output);
    }

    public void Dispose() => _server.Dispose();
}
