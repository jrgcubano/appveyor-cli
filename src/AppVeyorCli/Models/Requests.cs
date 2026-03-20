using System.Text.Json.Serialization;

namespace AppVeyorCli.Models;

public record StartBuildRequest(
    [property: JsonPropertyName("accountName")] string AccountName,
    [property: JsonPropertyName("projectSlug")] string ProjectSlug,
    [property: JsonPropertyName("branch")] string Branch,
    [property: JsonPropertyName("commitId")] string? CommitId = null,
    [property: JsonPropertyName("pullRequestId")] int? PullRequestId = null,
    [property: JsonPropertyName("environmentVariables")] Dictionary<string, string>? EnvironmentVariables = null);

public record RerunBuildRequest(
    [property: JsonPropertyName("buildId")] int BuildId,
    [property: JsonPropertyName("reRunIncomplete")] bool ReRunIncomplete = false);

public record AddProjectRequest(
    [property: JsonPropertyName("repositoryProvider")] string RepositoryProvider,
    [property: JsonPropertyName("repositoryName")] string RepositoryName);

public record CreateEnvironmentRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("settings")] EnvironmentSettings? Settings = null);

public record UpdateEnvironmentRequest(
    [property: JsonPropertyName("deploymentEnvironmentId")] int DeploymentEnvironmentId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("settings")] EnvironmentSettings? Settings = null);

public record StartDeploymentRequest(
    [property: JsonPropertyName("environmentName")] string EnvironmentName,
    [property: JsonPropertyName("accountName")] string AccountName,
    [property: JsonPropertyName("projectSlug")] string ProjectSlug,
    [property: JsonPropertyName("buildVersion")] string BuildVersion,
    [property: JsonPropertyName("buildJobId")] string? BuildJobId = null,
    [property: JsonPropertyName("environmentVariables")] Dictionary<string, string>? EnvironmentVariables = null);

public record CancelDeploymentRequest(
    [property: JsonPropertyName("deploymentId")] int DeploymentId);
