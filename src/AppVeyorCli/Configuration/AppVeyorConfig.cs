namespace AppVeyorCli.Configuration;

public sealed class AppVeyorConfig
{
    public string? Token { get; set; }
    public string? AccountName { get; set; }
    public string ApiUrl { get; set; } = "https://ci.appveyor.com";

    public bool IsValid => !string.IsNullOrWhiteSpace(Token);

    public string GetApiBaseUrl() => ApiUrl.TrimEnd('/') + "/api/";
}
