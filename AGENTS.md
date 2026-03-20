# AGENTS.md - AI Agent Guidelines for AppVeyor CLI

This file provides guidance for AI agents (Claude Code, GitHub Copilot, etc.) working on this codebase.

## Project Overview

AppVeyor CLI is a .NET 10 command-line tool for the AppVeyor CI/CD REST API. It uses Spectre.Console.Cli for command parsing and rich terminal output, with JSON output support for AI/machine consumption.

## Architecture

```
src/AppVeyorCli/
  Program.cs              - Entry point, DI setup, command tree registration
  Infrastructure/         - Spectre.Console DI bridge, global settings, read-only guard
  Configuration/          - Config file and env var management (~/.appveyor/config.json)
  Api/                    - IAppVeyorClient interface + HttpClient implementation
  Models/                 - C# record types for API entities (Project, Build, User, Role, etc.)
  Output/                 - IOutputRenderer abstraction (ConsoleRenderer vs JsonRenderer)
  Commands/               - One folder per domain (Config, Projects, Builds, Environments, Deployments, Users, Collaborators, Roles)

tests/AppVeyorCli.Tests/
  Infrastructure/         - MockAppVeyorServer (fake HTTP server for integration tests)
  Api/                    - Serialization and config unit tests
  Commands/               - Integration tests using CommandApp + MockAppVeyorServer
```

## Key Patterns

### Commands
- Each command is an `AsyncCommand<TSettings>` with settings inheriting from `GlobalSettings`
- Commands receive `IAppVeyorClient` and `IConsoleProvider` via constructor injection
- Output goes through `OutputRendererFactory.Create(settings.Json, consoleProvider.Console)` which returns either a `ConsoleRenderer` (rich Spectre tables) or `JsonRenderer` (clean JSON)
- Write commands must call `ReadOnlyGuard.ThrowIfReadOnly(settings)` as first line in ExecuteAsync

### API Client
- `AppVeyorClient` uses `HttpClient` with base URL `https://ci.appveyor.com/api/`
- All paths are relative (e.g. `projects`, not `/api/projects`)
- System.Text.Json source generators via `AppVeyorJsonContext` for AOT compatibility
- All methods accept `CancellationToken`

### DI and Console
- Spectre.Console.Cli overrides user-registered `IAnsiConsole` in DI, so we use `IConsoleProvider` as an indirection layer
- Tests inject `TestConsole` via `IConsoleProvider` to capture command output

### Testing
- `MockAppVeyorServer` uses `HttpListener` on a random port with retry logic
- Routes are registered as full paths including `/api/` prefix (e.g. `/api/projects`)
- Use `RegisterJsonResponse` with source-gen `JsonTypeInfo` for type-safe mock responses
- Use `RegisterRawResponse` for error scenarios (401, 404, etc.)

## Build

```bash
dotnet tool restore        # Restore Cake tool
dotnet run build.cs        # Clean + Build + Test (Cake SDK)
dotnet test                # Tests only
```

## Adding a New Command

1. Create settings class inheriting from `GlobalSettings` in the appropriate `Commands/` subfolder
2. Create command class as `AsyncCommand<TSettings>` with `IAppVeyorClient` and `IConsoleProvider` in constructor
3. Use `OutputRendererFactory.Create(settings.Json, consoleProvider.Console)` for output
4. For write commands, add `ReadOnlyGuard.ThrowIfReadOnly(settings)` as first line
5. Register the command in `Program.cs` under the appropriate `AddBranch`
6. Add API method to `IAppVeyorClient` and `AppVeyorClient` if needed
7. Register any new model types in `AppVeyorJsonContext`
8. Add tests in `tests/AppVeyorCli.Tests/Commands/`

## Adding a New API Model

1. Create a C# record in `Models/` with `[JsonPropertyName]` attributes
2. Register the type (and array type if needed) in `AppVeyorJsonContext`
3. Add serialization round-trip tests in `Api/SerializationTests.cs`

## Conventions

- Target framework: `net10.0`, C# 14
- Models: immutable record types
- Serialization: System.Text.Json source generators only (no reflection)
- Async: all API calls use async/await with CancellationToken
- Error handling: `AppVeyorApiException` with typed status codes
- Project slug format: `account/slug` (parsed by `settings.Parse()`)
