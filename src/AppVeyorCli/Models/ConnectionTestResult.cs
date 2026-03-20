using System.Text.Json.Serialization;

namespace AppVeyorCli.Models;

public record ConnectionTestResult(
    [property: JsonPropertyName("connected")] bool Connected);

public record BuildLogResult(
    [property: JsonPropertyName("jobId")] string JobId,
    [property: JsonPropertyName("log")] string Log);
