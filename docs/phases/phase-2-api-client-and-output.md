# Phase 2 -- API Client and Output Rendering

**Status:** Complete

> **Related ADRs:** [ADR 002](../adr/002-dual-output-rich-and-json.md), [ADR 003](../adr/003-source-gen-json-serialization.md), [ADR 006](../adr/006-console-provider-di-workaround.md)

## Summary

Implemented the AppVeyor API client, all data models, JSON source generators, and the dual output rendering system (rich Spectre tables + clean JSON).

## Tasks

- [x] Define all API model records (Project, Build, BuildJob, Environment, Deployment, etc.)
- [x] Define request records (StartBuildRequest, AddProjectRequest, etc.)
- [x] Create `AppVeyorJsonContext` with source-gen attributes for all types
- [x] Implement `IAppVeyorClient` interface covering all API endpoints
- [x] Implement `AppVeyorClient` with HttpClient, Bearer auth, and typed error handling
- [x] Implement `AppVeyorApiException` with status code mapping
- [x] Create `IOutputRenderer` abstraction
- [x] Implement `ConsoleRenderer` (Spectre tables, panels, color-coded statuses)
- [x] Implement `JsonRenderer` (clean JSON via source-gen type info)
- [x] Create `OutputRendererFactory` (selects renderer based on `--json` flag or env var)
- [x] Implement `ConfigService` (env var > config file precedence)

## Key Files

```
src/AppVeyorCli/
  Api/
    IAppVeyorClient.cs          # Interface for all API operations
    AppVeyorClient.cs           # HttpClient implementation
    AppVeyorJsonContext.cs       # Source-gen JSON context
    AppVeyorApiException.cs      # Typed API errors
  Models/
    Project.cs, Build.cs, Environment.cs, Deployment.cs, Requests.cs
  Configuration/
    AppVeyorConfig.cs            # Config model (token, account, URL)
    ConfigService.cs             # Load/save with env var precedence
  Output/
    IOutputRenderer.cs           # Abstraction
    ConsoleRenderer.cs           # Rich Spectre output
    JsonRenderer.cs              # JSON output
    OutputRendererFactory.cs     # --json / APPVEYOR_OUTPUT selection
```
