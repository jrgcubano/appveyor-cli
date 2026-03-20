using AppVeyorCli.Api;
using AppVeyorCli.Commands.Users;
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

public class UserCommandTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public UserCommandTests()
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
            config.AddBranch("user", u =>
            {
                u.AddCommand<UserListCommand>("list");
                u.AddCommand<UserGetCommand>("get");
            });
        });

        return (app, testConsole);
    }

    [Fact]
    public async Task UserList_RendersTable()
    {
        var users = new[]
        {
            new User(1, "myaccount", true, false, 100, "John Doe", "john@example.com",
                1, "Administrator", null, null, false,
                new DateTime(2024, 1, 1), new DateTime(2024, 6, 15)),
            new User(1, "myaccount", false, false, 101, "Jane Smith", "jane@example.com",
                2, "Developer", null, null, false,
                new DateTime(2024, 3, 1), new DateTime(2024, 6, 20))
        };

        _server.RegisterJsonResponse("GET", "/api/users", 200, users, AppVeyorJsonContext.Default.UserArray);
        var (app, console) = CreateApp();

        var result = await app.RunAsync(["user", "list"]);

        Assert.Equal(0, result);
        var output = console.Output;
        Assert.Contains("John Doe", output);
        Assert.Contains("Jane Smith", output);
        Assert.Contains("Administrator", output);
        Assert.Contains("Developer", output);
    }

    [Fact]
    public async Task UserList_Json_RendersJsonOutput()
    {
        var users = new[]
        {
            new User(1, "myaccount", true, false, 100, "John Doe", "john@example.com",
                1, "Administrator", null, null, false,
                new DateTime(2024, 1, 1), new DateTime(2024, 6, 15))
        };

        _server.RegisterJsonResponse("GET", "/api/users", 200, users, AppVeyorJsonContext.Default.UserArray);
        var (app, console) = CreateApp();

        var result = await app.RunAsync(["user", "list", "--json"]);

        Assert.Equal(0, result);
        var output = console.Output;
        Assert.Contains("\"fullName\"", output);
        Assert.Contains("John Doe", output);
    }

    [Fact]
    public async Task UserGet_RendersDetail()
    {
        var user = new User(1, "myaccount", true, false, 100, "John Doe", "john@example.com",
            1, "Administrator", null, null, false,
            new DateTime(2024, 1, 1), new DateTime(2024, 6, 15));
        var roles = new[] { new Role(1, "Administrator", true, new DateTime(2024, 1, 1)) };
        var details = new UserDetails(user, roles);

        _server.RegisterJsonResponse("GET", "/api/users/100", 200, details, AppVeyorJsonContext.Default.UserDetails);
        var (app, console) = CreateApp();

        var result = await app.RunAsync(["user", "get", "100"]);

        Assert.Equal(0, result);
        var output = console.Output;
        Assert.Contains("John Doe", output);
        Assert.Contains("john@example.com", output);
        Assert.Contains("Administrator", output);
    }

    public void Dispose() => _server.Dispose();
}
