using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AppVeyorCli.Configuration;
using AppVeyorCli.Models;

namespace AppVeyorCli.Api;

public sealed class AppVeyorClient : IAppVeyorClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    public AppVeyorClient(AppVeyorConfig config) : this(config, null) { }

    public AppVeyorClient(AppVeyorConfig config, HttpClient? httpClient)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (!config.IsValid)
        {
            throw new ArgumentException("Invalid AppVeyor configuration. Run 'appveyor config set' to configure.", nameof(config));
        }

        if (httpClient is not null)
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(config.GetApiBaseUrl()) };
            _ownsHttpClient = true;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Token);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // Projects

    public async Task<Project[]> GetProjectsAsync(CancellationToken ct = default)
        => await GetAsync("projects", AppVeyorJsonContext.Default.ProjectArray, ct);

    public async Task<ProjectWithBuilds> GetProjectLastBuildAsync(string accountName, string slug, CancellationToken ct = default)
        => await GetAsync($"projects/{accountName}/{slug}", AppVeyorJsonContext.Default.ProjectWithBuilds, ct);

    public async Task<ProjectWithBuilds> GetProjectBranchBuildAsync(string accountName, string slug, string branch, CancellationToken ct = default)
        => await GetAsync($"projects/{accountName}/{slug}/branch/{branch}", AppVeyorJsonContext.Default.ProjectWithBuilds, ct);

    public async Task<ProjectHistory> GetProjectHistoryAsync(string accountName, string slug, int count = 10, int? startBuildId = null, string? branch = null, CancellationToken ct = default)
    {
        var url = $"projects/{accountName}/{slug}/history?recordsNumber={count}";
        if (startBuildId.HasValue)
        {
            url += $"&startBuildId={startBuildId}";
        }

        if (!string.IsNullOrEmpty(branch))
        {
            url += $"&branch={branch}";
        }
        return await GetAsync(url, AppVeyorJsonContext.Default.ProjectHistory, ct);
    }

    public async Task<Project> AddProjectAsync(AddProjectRequest request, CancellationToken ct = default)
        => await PostAsync("projects", request, AppVeyorJsonContext.Default.AddProjectRequest, AppVeyorJsonContext.Default.Project, ct);

    public async Task DeleteProjectAsync(string accountName, string slug, CancellationToken ct = default)
        => await DeleteAsync($"projects/{accountName}/{slug}", ct);

    public async Task<ProjectSettings> GetProjectSettingsAsync(string accountName, string slug, CancellationToken ct = default)
        => await GetAsync($"projects/{accountName}/{slug}/settings", AppVeyorJsonContext.Default.ProjectSettings, ct);

    public async Task<string> GetProjectSettingsYamlAsync(string accountName, string slug, CancellationToken ct = default)
    {
        var response = await SendAsync(HttpMethod.Get, $"projects/{accountName}/{slug}/settings/yaml", null, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }

    public async Task ClearBuildCacheAsync(string accountName, string slug, CancellationToken ct = default)
        => await DeleteAsync($"projects/{accountName}/{slug}/buildcache", ct);

    // Builds

    public async Task<Build> StartBuildAsync(StartBuildRequest request, CancellationToken ct = default)
        => await PostAsync("builds", request, AppVeyorJsonContext.Default.StartBuildRequest, AppVeyorJsonContext.Default.Build, ct);

    public async Task<Build> RerunBuildAsync(RerunBuildRequest request, CancellationToken ct = default)
        => await PutAsync("builds", request, AppVeyorJsonContext.Default.RerunBuildRequest, AppVeyorJsonContext.Default.Build, ct);

    public async Task CancelBuildAsync(string accountName, string slug, string buildVersion, CancellationToken ct = default)
        => await DeleteAsync($"builds/{accountName}/{slug}/{buildVersion}", ct);

    public async Task DeleteBuildAsync(int buildId, CancellationToken ct = default)
        => await DeleteAsync($"builds/{buildId}", ct);

    public async Task<string> GetBuildLogAsync(string jobId, CancellationToken ct = default)
    {
        var response = await SendAsync(HttpMethod.Get, $"buildjobs/{jobId}/log", null, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }

    // Environments

    public async Task<DeploymentEnvironment[]> GetEnvironmentsAsync(CancellationToken ct = default)
        => await GetAsync("environments", AppVeyorJsonContext.Default.DeploymentEnvironmentArray, ct);

    public async Task<EnvironmentWithSettings> GetEnvironmentSettingsAsync(int environmentId, CancellationToken ct = default)
        => await GetAsync($"environments/{environmentId}/settings", AppVeyorJsonContext.Default.EnvironmentWithSettings, ct);

    public async Task<EnvironmentDeployments> GetEnvironmentDeploymentsAsync(int environmentId, CancellationToken ct = default)
        => await GetAsync($"environments/{environmentId}/deployments", AppVeyorJsonContext.Default.EnvironmentDeployments, ct);

    public async Task<DeploymentEnvironment> AddEnvironmentAsync(CreateEnvironmentRequest request, CancellationToken ct = default)
        => await PostAsync("environments", request, AppVeyorJsonContext.Default.CreateEnvironmentRequest, AppVeyorJsonContext.Default.DeploymentEnvironment, ct);

    public async Task<DeploymentEnvironment> UpdateEnvironmentAsync(UpdateEnvironmentRequest request, CancellationToken ct = default)
        => await PutAsync("environments", request, AppVeyorJsonContext.Default.UpdateEnvironmentRequest, AppVeyorJsonContext.Default.DeploymentEnvironment, ct);

    public async Task DeleteEnvironmentAsync(int environmentId, CancellationToken ct = default)
        => await DeleteAsync($"environments/{environmentId}", ct);

    // Deployments

    public async Task<DeploymentDetails> GetDeploymentAsync(int deploymentId, CancellationToken ct = default)
        => await GetAsync($"deployments/{deploymentId}", AppVeyorJsonContext.Default.DeploymentDetails, ct);

    public async Task<Deployment> StartDeploymentAsync(StartDeploymentRequest request, CancellationToken ct = default)
        => await PostAsync("deployments", request, AppVeyorJsonContext.Default.StartDeploymentRequest, AppVeyorJsonContext.Default.Deployment, ct);

    public async Task CancelDeploymentAsync(int deploymentId, CancellationToken ct = default)
    {
        var request = new CancelDeploymentRequest(deploymentId);
        await PutAsync("deployments/stop", request, AppVeyorJsonContext.Default.CancelDeploymentRequest, ct);
    }

    // Connection test

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            await GetProjectsAsync(ct);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // HTTP helpers

    private async Task<T> GetAsync<T>(string path, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo, CancellationToken ct)
    {
        var response = await SendAsync(HttpMethod.Get, path, null, ct);
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync(stream, typeInfo, ct)
               ?? throw new AppVeyorApiException(response.StatusCode, "Empty response from API");
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path, TRequest body,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TRequest> requestType,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TResponse> responseType,
        CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body, requestType);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await SendAsync(HttpMethod.Post, path, content, ct);
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync(stream, responseType, ct)
               ?? throw new AppVeyorApiException(response.StatusCode, "Empty response from API");
    }

    private async Task<TResponse> PutAsync<TRequest, TResponse>(
        string path, TRequest body,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TRequest> requestType,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TResponse> responseType,
        CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body, requestType);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await SendAsync(HttpMethod.Put, path, content, ct);
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync(stream, responseType, ct)
               ?? throw new AppVeyorApiException(response.StatusCode, "Empty response from API");
    }

    private async Task PutAsync<TRequest>(
        string path, TRequest body,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TRequest> requestType,
        CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body, requestType);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await SendAsync(HttpMethod.Put, path, content, ct);
    }

    private async Task DeleteAsync(string path, CancellationToken ct)
        => await SendAsync(HttpMethod.Delete, path, null, ct);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, HttpContent? content, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path) { Content = content };
        var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            var message = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Invalid or missing API token. Run 'appveyor config set' to configure.",
                HttpStatusCode.Forbidden => "Access denied. Check your API token permissions.",
                HttpStatusCode.NotFound => $"Resource not found: {path}",
                _ => $"API error {(int)response.StatusCode}: {body}"
            };
            throw new AppVeyorApiException(response.StatusCode, message);
        }

        return response;
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
