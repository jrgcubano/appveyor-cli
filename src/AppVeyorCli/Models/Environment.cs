using System.Text.Json.Serialization;

namespace AppVeyorCli.Models;

public record DeploymentEnvironment(
    [property: JsonPropertyName("deploymentEnvironmentId")] int DeploymentEnvironmentId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("created")] DateTime Created,
    [property: JsonPropertyName("updated")] DateTime Updated);

public record EnvironmentWithSettings(
    [property: JsonPropertyName("environment")] DeploymentEnvironment Environment,
    [property: JsonPropertyName("settings")] EnvironmentSettings? Settings);

public record EnvironmentSettings(
    [property: JsonPropertyName("providerSettings")] Dictionary<string, object>? ProviderSettings,
    [property: JsonPropertyName("environmentVariables")] EnvironmentVariable[]? EnvironmentVariables);

public record EnvironmentDeployments(
    [property: JsonPropertyName("environment")] DeploymentEnvironment Environment,
    [property: JsonPropertyName("deployments")] Deployment[]? Deployments);
