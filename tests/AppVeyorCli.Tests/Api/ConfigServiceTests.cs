using System.Text.Json;
using AppVeyorCli.Api;
using AppVeyorCli.Configuration;

namespace AppVeyorCli.Tests.Api;

public class ConfigServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _configFile;

    public ConfigServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "appveyor-cli-test-" + Guid.NewGuid().ToString("N")[..8]);
        _configFile = Path.Combine(_tempDir, "config.json");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Load_ReturnsDefault_WhenNoConfigExists()
    {
        var service = new ConfigService();
        var config = service.Load();

        // Without env vars or config file, token should be null
        Assert.Equal("https://ci.appveyor.com", config.ApiUrl);
    }

    [Fact]
    public void AppVeyorConfig_IsValid_WhenTokenSet()
    {
        var config = new AppVeyorConfig { Token = "test-token" };
        Assert.True(config.IsValid);
    }

    [Fact]
    public void AppVeyorConfig_IsNotValid_WhenTokenMissing()
    {
        var config = new AppVeyorConfig();
        Assert.False(config.IsValid);
    }

    [Fact]
    public void AppVeyorConfig_IsNotValid_WhenTokenEmpty()
    {
        var config = new AppVeyorConfig { Token = "" };
        Assert.False(config.IsValid);
    }

    [Fact]
    public void AppVeyorConfig_GetApiBaseUrl_ReturnsCorrectUrl()
    {
        var config = new AppVeyorConfig { ApiUrl = "https://ci.appveyor.com" };
        Assert.Equal("https://ci.appveyor.com/api/", config.GetApiBaseUrl());
    }

    [Fact]
    public void AppVeyorConfig_GetApiBaseUrl_TrimsTrailingSlash()
    {
        var config = new AppVeyorConfig { ApiUrl = "https://ci.appveyor.com/" };
        Assert.Equal("https://ci.appveyor.com/api/", config.GetApiBaseUrl());
    }

    [Fact]
    public void ConfigFile_RoundTrips()
    {
        var original = new AppVeyorConfig
        {
            Token = "v2.test-token",
            AccountName = "myaccount",
            ApiUrl = "https://custom.appveyor.com"
        };

        Directory.CreateDirectory(Path.GetDirectoryName(_configFile)!);
        var json = JsonSerializer.Serialize(original, AppVeyorJsonContext.Default.AppVeyorConfig);
        File.WriteAllText(_configFile, json);

        var loaded = JsonSerializer.Deserialize(File.ReadAllText(_configFile), AppVeyorJsonContext.Default.AppVeyorConfig);

        Assert.NotNull(loaded);
        Assert.Equal(original.Token, loaded.Token);
        Assert.Equal(original.AccountName, loaded.AccountName);
        Assert.Equal(original.ApiUrl, loaded.ApiUrl);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }
}
