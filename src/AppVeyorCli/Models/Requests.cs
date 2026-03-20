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

// Team management requests

public record AddUserRequest(
    [property: JsonPropertyName("fullName")] string FullName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("roleId")] int RoleId,
    [property: JsonPropertyName("generatePassword")] bool GeneratePassword = true);

public record UpdateUserRequest(
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("fullName")] string? FullName = null,
    [property: JsonPropertyName("email")] string? Email = null,
    [property: JsonPropertyName("roleId")] int? RoleId = null);

public record AddCollaboratorRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("roleId")] int RoleId);

public record UpdateCollaboratorRequest(
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("roleId")] int RoleId);

public record AddRoleRequest(
    [property: JsonPropertyName("name")] string Name);
