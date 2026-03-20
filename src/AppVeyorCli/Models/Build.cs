using System.Text.Json.Serialization;

namespace AppVeyorCli.Models;

public record Build(
    [property: JsonPropertyName("buildId")] int BuildId,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("branch")] string Branch,
    [property: JsonPropertyName("commitId")] string? CommitId,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("authorName")] string? AuthorName,
    [property: JsonPropertyName("started")] DateTime? Started,
    [property: JsonPropertyName("finished")] DateTime? Finished,
    [property: JsonPropertyName("jobs")] BuildJob[]? Jobs);

public record BuildJob(
    [property: JsonPropertyName("jobId")] string JobId,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("started")] DateTime? Started,
    [property: JsonPropertyName("finished")] DateTime? Finished);
