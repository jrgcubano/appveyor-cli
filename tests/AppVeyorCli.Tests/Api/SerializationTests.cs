using System.Text.Json;
using AppVeyorCli.Api;
using AppVeyorCli.Models;

namespace AppVeyorCli.Tests.Api;

public class SerializationTests
{
    [Fact]
    public void Project_RoundTrips()
    {
        var project = new Project(1, 10, "myaccount", "MyApp", "myapp", "gitHub", "owner/repo", false,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 19, 0, 0, 0, DateTimeKind.Utc));

        var json = JsonSerializer.Serialize(project, AppVeyorJsonContext.Default.Project);
        var deserialized = JsonSerializer.Deserialize(json, AppVeyorJsonContext.Default.Project);

        Assert.NotNull(deserialized);
        Assert.Equal(project.ProjectId, deserialized.ProjectId);
        Assert.Equal(project.Slug, deserialized.Slug);
        Assert.Equal(project.AccountName, deserialized.AccountName);
        Assert.Equal(project.RepositoryName, deserialized.RepositoryName);
    }

    [Fact]
    public void Build_RoundTrips()
    {
        var build = new Build(100, "1.0.42", "success", "main", "abc123", "Fix bug",
            "Author", DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow, null);

        var json = JsonSerializer.Serialize(build, AppVeyorJsonContext.Default.Build);
        var deserialized = JsonSerializer.Deserialize(json, AppVeyorJsonContext.Default.Build);

        Assert.NotNull(deserialized);
        Assert.Equal(build.BuildId, deserialized.BuildId);
        Assert.Equal(build.Version, deserialized.Version);
        Assert.Equal(build.Status, deserialized.Status);
        Assert.Equal(build.Branch, deserialized.Branch);
    }

    [Fact]
    public void DeploymentEnvironment_RoundTrips()
    {
        var env = new DeploymentEnvironment(5, "Production", "Agent",
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 19, 0, 0, 0, DateTimeKind.Utc));

        var json = JsonSerializer.Serialize(env, AppVeyorJsonContext.Default.DeploymentEnvironment);
        var deserialized = JsonSerializer.Deserialize(json, AppVeyorJsonContext.Default.DeploymentEnvironment);

        Assert.NotNull(deserialized);
        Assert.Equal(env.DeploymentEnvironmentId, deserialized.DeploymentEnvironmentId);
        Assert.Equal(env.Name, deserialized.Name);
        Assert.Equal(env.Provider, deserialized.Provider);
    }

    [Fact]
    public void StartBuildRequest_Serializes()
    {
        var request = new StartBuildRequest("myaccount", "myapp", "main", "abc123");
        var json = JsonSerializer.Serialize(request, AppVeyorJsonContext.Default.StartBuildRequest);

        Assert.Contains("\"accountName\"", json);
        Assert.Contains("\"projectSlug\"", json);
        Assert.Contains("\"branch\"", json);
        Assert.Contains("\"commitId\"", json);
    }

    [Fact]
    public void ProjectArray_Serializes()
    {
        var projects = new[]
        {
            new Project(1, 10, "acc", "App1", "app1", "gitHub", "owner/app1", false, DateTime.UtcNow, DateTime.UtcNow),
            new Project(2, 10, "acc", "App2", "app2", "gitHub", "owner/app2", true, DateTime.UtcNow, DateTime.UtcNow)
        };

        var json = JsonSerializer.Serialize(projects, AppVeyorJsonContext.Default.ProjectArray);
        var deserialized = JsonSerializer.Deserialize(json, AppVeyorJsonContext.Default.ProjectArray);

        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Length);
    }

    [Fact]
    public void NullFields_AreOmitted()
    {
        var build = new Build(1, "1.0.0", "queued", "main", null, null, null, null, null, null);
        var json = JsonSerializer.Serialize(build, AppVeyorJsonContext.Default.Build);

        Assert.DoesNotContain("\"commitId\"", json);
        Assert.DoesNotContain("\"message\"", json);
        Assert.DoesNotContain("\"started\"", json);
    }

    [Fact]
    public void ConnectionTestResult_Serializes()
    {
        var result = new ConnectionTestResult(true);
        var json = JsonSerializer.Serialize(result, AppVeyorJsonContext.Default.ConnectionTestResult);

        Assert.Contains("\"connected\"", json);
        Assert.Contains("true", json);
    }
}
