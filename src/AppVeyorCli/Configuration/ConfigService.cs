using System.Text.Json;
using AppVeyorCli.Api;

namespace AppVeyorCli.Configuration;

public interface IConfigService
{
    AppVeyorConfig Load();
    void Save(AppVeyorConfig config);
    string GetConfigFilePath();
}

public sealed class ConfigService : IConfigService
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".appveyor");

    private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

    public AppVeyorConfig Load()
    {
        var config = new AppVeyorConfig();

        // Try loading from file first
        if (File.Exists(ConfigFile))
        {
            var json = File.ReadAllText(ConfigFile);
            var fileConfig = JsonSerializer.Deserialize(json, AppVeyorJsonContext.Default.AppVeyorConfig);
            if (fileConfig is not null)
            {
                config = fileConfig;
            }
        }

        // Environment variables take precedence
        var envToken = Environment.GetEnvironmentVariable("APPVEYOR_API_TOKEN");
        if (!string.IsNullOrWhiteSpace(envToken))
        {
            config.Token = envToken;
        }

        var envAccount = Environment.GetEnvironmentVariable("APPVEYOR_ACCOUNT_NAME");
        if (!string.IsNullOrWhiteSpace(envAccount))
        {
            config.AccountName = envAccount;
        }

        var envApiUrl = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");
        if (!string.IsNullOrWhiteSpace(envApiUrl))
        {
            config.ApiUrl = envApiUrl;
        }

        return config;
    }

    public void Save(AppVeyorConfig config)
    {
        Directory.CreateDirectory(ConfigDir);
        var json = JsonSerializer.Serialize(config, AppVeyorJsonContext.Default.AppVeyorConfig);
        File.WriteAllText(ConfigFile, json);
    }

    public string GetConfigFilePath() => ConfigFile;
}
