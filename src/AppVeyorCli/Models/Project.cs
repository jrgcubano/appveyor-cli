using System.Text.Json.Serialization;

namespace AppVeyorCli.Models;

public record Project(
    [property: JsonPropertyName("projectId")] int ProjectId,
    [property: JsonPropertyName("accountId")] int AccountId,
    [property: JsonPropertyName("accountName")] string AccountName,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("repositoryType")] string RepositoryType,
    [property: JsonPropertyName("repositoryName")] string RepositoryName,
    [property: JsonPropertyName("isPrivate")] bool IsPrivate,
    [property: JsonPropertyName("created")] DateTime Created,
    [property: JsonPropertyName("updated")] DateTime Updated);

public record ProjectWithBuilds(
    [property: JsonPropertyName("project")] Project Project,
    [property: JsonPropertyName("build")] Build? Build);

public record ProjectHistory(
    [property: JsonPropertyName("project")] Project Project,
    [property: JsonPropertyName("builds")] Build[] Builds);

public record ProjectSettings(
    [property: JsonPropertyName("project")] Project Project,
    [property: JsonPropertyName("settings")] ProjectSettingsDetails Settings);

public record ProjectSettingsDetails(
    [property: JsonPropertyName("configuration")] ProjectConfiguration? Configuration);

public record ProjectConfiguration(
    [property: JsonPropertyName("environmentVariables")] EnvironmentVariable[]? EnvironmentVariables);

public record EnvironmentVariable(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("isEncrypted")] bool IsEncrypted);
