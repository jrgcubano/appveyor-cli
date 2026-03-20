using System.Text.Json.Serialization;

namespace AppVeyorCli.Models;

public record Deployment(
    [property: JsonPropertyName("deploymentId")] int DeploymentId,
    [property: JsonPropertyName("environment")] DeploymentEnvironment? Environment,
    [property: JsonPropertyName("project")] Project? Project,
    [property: JsonPropertyName("build")] Build? Build,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("started")] DateTime? Started,
    [property: JsonPropertyName("finished")] DateTime? Finished);

public record DeploymentDetails(
    [property: JsonPropertyName("deployment")] Deployment Deployment,
    [property: JsonPropertyName("jobs")] BuildJob[]? Jobs);
