# Phase 6 -- Team Management

**Status:** Complete

## Summary

Implemented the AppVeyor Teams API surface with 11 new commands across 3 domains: Users, Collaborators, and Roles.

## Tasks

- [x] Research AppVeyor Teams API endpoints (users, collaborators, roles)
- [x] Create `User`, `UserDetails` model records
- [x] Create `Role`, `RoleWithPermissions`, `PermissionGroup`, `Permission` model records
- [x] Create request records (`AddUserRequest`, `UpdateUserRequest`, `AddCollaboratorRequest`, etc.)
- [x] Add void `PostAsync` HTTP helper for 204 No Content responses
- [x] Implement 16 API client methods across users, collaborators, and roles
- [x] Register all new types in `AppVeyorJsonContext`
- [x] Implement user commands: `list`, `get`, `add`, `delete`
- [x] Implement collaborator commands: `list`, `add`, `delete`
- [x] Implement role commands: `list`, `get`, `add`, `delete`
- [x] Add `ReadOnlyGuard` to all write commands
- [x] Add `Markup.Escape()` on all API-provided strings (Copilot review fix)
- [x] Add 6 integration tests (user list/get/json, role list/get-with-permissions/json)

## Command tree

```
appveyor
  user list|get|add|delete
  collaborator list|add|delete
  role list|get|add|delete
```

## Key Files

```
src/AppVeyorCli/
  Models/
    User.cs              User, UserDetails records
    Role.cs              Role, RoleWithPermissions, PermissionGroup, Permission records
    Requests.cs          AddUserRequest, UpdateUserRequest, AddCollaboratorRequest, etc.
  Commands/
    Users/               UserListCommand, UserGetCommand, UserAddCommand, UserDeleteCommand
    Collaborators/       CollaboratorListCommand, CollaboratorAddCommand, CollaboratorDeleteCommand
    Roles/               RoleListCommand, RoleGetCommand, RoleAddCommand, RoleDeleteCommand

tests/AppVeyorCli.Tests/Commands/
  UserCommandTests.cs    User list/get table + JSON tests
  RoleCommandTests.cs    Role list/get-with-permissions table + JSON tests
```
