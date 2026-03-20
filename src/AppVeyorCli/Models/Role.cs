using System.Text.Json.Serialization;

namespace AppVeyorCli.Models;

public record Role(
    [property: JsonPropertyName("roleId")] int RoleId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("isSystem")] bool IsSystem,
    [property: JsonPropertyName("created")] DateTime Created);

public record RoleWithPermissions(
    [property: JsonPropertyName("roleId")] int RoleId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("isSystem")] bool IsSystem,
    [property: JsonPropertyName("created")] DateTime Created,
    [property: JsonPropertyName("groups")] PermissionGroup[]? Groups);

public record PermissionGroup(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("permissions")] Permission[] Permissions);

public record Permission(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("allowed")] bool Allowed);
