using System.Net;
using AppVeyorCli.Api;
using AppVeyorCli.Configuration;
using AppVeyorCli.Tests.Infrastructure;

namespace AppVeyorCli.Tests.Api;

public class ApiClientErrorTests : IDisposable
{
    private readonly MockAppVeyorServer _server;

    public ApiClientErrorTests()
    {
        _server = new MockAppVeyorServer();
    }

    private AppVeyorClient CreateClient()
    {
        var config = new AppVeyorConfig { Token = "test-token" };
        return new AppVeyorClient(config, _server.CreateHttpClient());
    }

    [Fact]
    public async Task GetProjects_401_ThrowsWithAuthMessage()
    {
        _server.RegisterRawResponse("GET", "/api/projects", 401, "{\"message\":\"Unauthorized\"}");

        using var client = CreateClient();
        var ex = await Assert.ThrowsAsync<AppVeyorApiException>(() => client.GetProjectsAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Contains("token", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetProject_404_ThrowsWithNotFoundMessage()
    {
        _server.RegisterRawResponse("GET", "/api/projects/myaccount/nonexistent", 404);

        using var client = CreateClient();
        var ex = await Assert.ThrowsAsync<AppVeyorApiException>(() =>
            client.GetProjectLastBuildAsync("myaccount", "nonexistent"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetProjects_403_ThrowsWithForbiddenMessage()
    {
        _server.RegisterRawResponse("GET", "/api/projects", 403, "{\"message\":\"Forbidden\"}");

        using var client = CreateClient();
        var ex = await Assert.ThrowsAsync<AppVeyorApiException>(() => client.GetProjectsAsync());

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.Contains("denied", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ThrowsWhenConfigInvalid()
    {
        var config = new AppVeyorConfig();
        Assert.Throws<ArgumentException>(() => new AppVeyorClient(config));
    }

    [Fact]
    public async Task TestConnection_ReturnsFalse_OnError()
    {
        _server.RegisterRawResponse("GET", "/api/projects", 401, "{\"message\":\"Unauthorized\"}");

        using var client = CreateClient();
        var connected = await client.TestConnectionAsync();

        Assert.False(connected);
    }

    [Fact]
    public async Task TestConnection_ReturnsTrue_OnSuccess()
    {
        _server.RegisterJsonResponse("GET", "/api/projects", 200,
            Array.Empty<AppVeyorCli.Models.Project>(),
            AppVeyorJsonContext.Default.ProjectArray);

        using var client = CreateClient();
        var connected = await client.TestConnectionAsync();

        Assert.True(connected);
    }

    public void Dispose() => _server.Dispose();
}
