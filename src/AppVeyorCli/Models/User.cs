using System.Text.Json.Serialization;

namespace AppVeyorCli.Models;

public record User(
    [property: JsonPropertyName("accountId")] int AccountId,
    [property: JsonPropertyName("accountName")] string AccountName,
    [property: JsonPropertyName("isOwner")] bool IsOwner,
    [property: JsonPropertyName("isCollaborator")] bool IsCollaborator,
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("fullName")] string FullName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("roleId")] int RoleId,
    [property: JsonPropertyName("roleName")] string RoleName,
    [property: JsonPropertyName("successfulBuildNotification")] string? SuccessfulBuildNotification,
    [property: JsonPropertyName("failedBuildNotification")] string? FailedBuildNotification,
    [property: JsonPropertyName("notifyWhenBuildStatusChangedOnly")] bool NotifyWhenBuildStatusChangedOnly,
    [property: JsonPropertyName("created")] DateTime Created,
    [property: JsonPropertyName("updated")] DateTime Updated);

public record UserDetails(
    [property: JsonPropertyName("user")] User User,
    [property: JsonPropertyName("roles")] Role[] Roles);
