using AppVeyorCli.Models;

namespace AppVeyorCli.Api;

public interface IAppVeyorClient
{
    // Projects
    Task<Project[]> GetProjectsAsync(CancellationToken ct = default);
    Task<ProjectWithBuilds> GetProjectLastBuildAsync(string accountName, string slug, CancellationToken ct = default);
    Task<ProjectWithBuilds> GetProjectBranchBuildAsync(string accountName, string slug, string branch, CancellationToken ct = default);
    Task<ProjectHistory> GetProjectHistoryAsync(string accountName, string slug, int count = 10, int? startBuildId = null, string? branch = null, CancellationToken ct = default);
    Task<Project> AddProjectAsync(AddProjectRequest request, CancellationToken ct = default);
    Task DeleteProjectAsync(string accountName, string slug, CancellationToken ct = default);
    Task<ProjectSettings> GetProjectSettingsAsync(string accountName, string slug, CancellationToken ct = default);
    Task<string> GetProjectSettingsYamlAsync(string accountName, string slug, CancellationToken ct = default);
    Task ClearBuildCacheAsync(string accountName, string slug, CancellationToken ct = default);

    // Builds
    Task<Build> StartBuildAsync(StartBuildRequest request, CancellationToken ct = default);
    Task<Build> RerunBuildAsync(RerunBuildRequest request, CancellationToken ct = default);
    Task CancelBuildAsync(string accountName, string slug, string buildVersion, CancellationToken ct = default);
    Task DeleteBuildAsync(int buildId, CancellationToken ct = default);
    Task<string> GetBuildLogAsync(string jobId, CancellationToken ct = default);

    // Environments
    Task<DeploymentEnvironment[]> GetEnvironmentsAsync(CancellationToken ct = default);
    Task<EnvironmentWithSettings> GetEnvironmentSettingsAsync(int environmentId, CancellationToken ct = default);
    Task<EnvironmentDeployments> GetEnvironmentDeploymentsAsync(int environmentId, CancellationToken ct = default);
    Task<DeploymentEnvironment> AddEnvironmentAsync(CreateEnvironmentRequest request, CancellationToken ct = default);
    Task<DeploymentEnvironment> UpdateEnvironmentAsync(UpdateEnvironmentRequest request, CancellationToken ct = default);
    Task DeleteEnvironmentAsync(int environmentId, CancellationToken ct = default);

    // Deployments
    Task<DeploymentDetails> GetDeploymentAsync(int deploymentId, CancellationToken ct = default);
    Task<Deployment> StartDeploymentAsync(StartDeploymentRequest request, CancellationToken ct = default);
    Task CancelDeploymentAsync(int deploymentId, CancellationToken ct = default);

    // Connection
    Task<bool> TestConnectionAsync(CancellationToken ct = default);
}
