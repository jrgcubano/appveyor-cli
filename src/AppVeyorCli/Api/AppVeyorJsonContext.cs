using System.Text.Json;
using System.Text.Json.Serialization;
using AppVeyorCli.Configuration;
using AppVeyorCli.Models;

namespace AppVeyorCli.Api;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
[JsonSerializable(typeof(AppVeyorConfig))]
[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(Project[]))]
[JsonSerializable(typeof(ProjectWithBuilds))]
[JsonSerializable(typeof(ProjectHistory))]
[JsonSerializable(typeof(ProjectSettings))]
[JsonSerializable(typeof(Build))]
[JsonSerializable(typeof(Build[]))]
[JsonSerializable(typeof(BuildJob))]
[JsonSerializable(typeof(BuildJob[]))]
[JsonSerializable(typeof(DeploymentEnvironment))]
[JsonSerializable(typeof(DeploymentEnvironment[]))]
[JsonSerializable(typeof(EnvironmentWithSettings))]
[JsonSerializable(typeof(EnvironmentDeployments))]
[JsonSerializable(typeof(Deployment))]
[JsonSerializable(typeof(Deployment[]))]
[JsonSerializable(typeof(DeploymentDetails))]
[JsonSerializable(typeof(EnvironmentVariable))]
[JsonSerializable(typeof(EnvironmentVariable[]))]
[JsonSerializable(typeof(StartBuildRequest))]
[JsonSerializable(typeof(RerunBuildRequest))]
[JsonSerializable(typeof(AddProjectRequest))]
[JsonSerializable(typeof(CreateEnvironmentRequest))]
[JsonSerializable(typeof(UpdateEnvironmentRequest))]
[JsonSerializable(typeof(StartDeploymentRequest))]
[JsonSerializable(typeof(CancelDeploymentRequest))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(User[]))]
[JsonSerializable(typeof(UserDetails))]
[JsonSerializable(typeof(Role))]
[JsonSerializable(typeof(Role[]))]
[JsonSerializable(typeof(RoleWithPermissions))]
[JsonSerializable(typeof(PermissionGroup))]
[JsonSerializable(typeof(Permission))]
[JsonSerializable(typeof(AddUserRequest))]
[JsonSerializable(typeof(UpdateUserRequest))]
[JsonSerializable(typeof(AddCollaboratorRequest))]
[JsonSerializable(typeof(UpdateCollaboratorRequest))]
[JsonSerializable(typeof(AddRoleRequest))]
[JsonSerializable(typeof(ConnectionTestResult))]
[JsonSerializable(typeof(BuildLogResult))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class AppVeyorJsonContext : JsonSerializerContext;
