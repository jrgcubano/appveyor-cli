# AppVeyor CLI — Design Specification

## Overview

A .NET 10 CLI tool for the AppVeyor CI/CD REST API, built with Spectre.Console.Cli. Designed to be both human-friendly (rich terminal output) and AI-ready (structured JSON output).

## Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Runtime | .NET 10 | 10.0.201 |
| Language | C# 14 | — |
| CLI Framework | Spectre.Console.Cli | 0.53.1 |
| Rich Output | Spectre.Console | 0.54.0 |
| DI | Microsoft.Extensions.DependencyInjection | 10.0.x |
| HTTP | Microsoft.Extensions.Http | 10.0.x |
| Serialization | System.Text.Json (source gen) | built-in |
| Build | Cake SDK | 6.0.0 |
| Versioning | MinVer | latest |
| Testing | xUnit + NSubstitute + Spectre.Console.Testing | latest |

## Scope (v1)

Four API domains:
- **Projects** — list, get, add, delete, settings, YAML config, env vars, build cache
- **Builds** — start, cancel, rerun, delete, history, log download
- **Environments** — list, get settings, get deployments, add, update, delete
- **Deployments** — get, start, cancel

Plus a `config` command for credential management.

## Authentication

1. Check `APPVEYOR_API_TOKEN` environment variable
2. Fall back to `~/.appveyor/config.json`
3. If neither found, show error prompting `appveyor config set`

Config file:
```json
{
  "token": "v2.xxxx",
  "accountName": "my-account",
  "apiUrl": "https://ci.appveyor.com"
}
```

When `accountName` is set and a v2 token is used, API calls use `/api/account/{accountName}/...` prefix.

## Command Tree

```
appveyor config set --token <token> [--account <name>] [--api-url <url>]
appveyor config get
appveyor config test

appveyor project list
appveyor project get <account>/<slug>
appveyor project add --repo-provider <provider> --repo-name <name>
appveyor project delete <account>/<slug>
appveyor project settings <account>/<slug> [--yaml]

appveyor build start <account>/<slug> --branch <branch> [--commit <id>]
appveyor build history <account>/<slug> [--count <n>] [--branch <branch>]
appveyor build cancel <account>/<slug> <version>
appveyor build rerun <buildId> [--incomplete-only]
appveyor build log <jobId>

appveyor environment list
appveyor environment get <environmentId>
appveyor environment add --name <name> --provider <provider>
appveyor environment delete <environmentId>

appveyor deployment get <deploymentId>
appveyor deployment start --environment <name> --project <account>/<slug> --build-version <ver>
appveyor deployment cancel <deploymentId>
```

All commands support: `--json`, `--verbose`, `--help`

## Architecture

### Project Structure

```
appveyor-cli/
├── build.cs
├── global.json
├── Directory.Build.props
├── Directory.Packages.props
├── AppVeyorCli.slnx
├── install.ps1
├── install.sh
├── src/
│   └── AppVeyorCli/
│       ├── AppVeyorCli.csproj
│       ├── Program.cs
│       ├── Infrastructure/
│       │   ├── TypeRegistrar.cs
│       │   ├── TypeResolver.cs
│       │   └── GlobalSettings.cs
│       ├── Configuration/
│       │   ├── AppVeyorConfig.cs
│       │   └── ConfigService.cs
│       ├── Api/
│       │   ├── IAppVeyorClient.cs
│       │   ├── AppVeyorClient.cs
│       │   └── AppVeyorJsonContext.cs
│       ├── Models/
│       │   ├── Project.cs
│       │   ├── Build.cs
│       │   ├── BuildJob.cs
│       │   ├── Environment.cs
│       │   ├── Deployment.cs
│       │   └── Requests.cs
│       ├── Output/
│       │   ├── IOutputRenderer.cs
│       │   ├── JsonRenderer.cs
│       │   └── ConsoleRenderer.cs
│       └── Commands/
│           ├── Config/
│           ├── Projects/
│           ├── Builds/
│           ├── Environments/
│           └── Deployments/
└── tests/
    └── AppVeyorCli.Tests/
```

### Entry Point

Spectre.Console.Cli `CommandApp` with `AddBranch` for each domain. DI via `TypeRegistrar` bridging to `Microsoft.Extensions.DependencyInjection`.

### Output Rendering

`IOutputRenderer` abstraction with two implementations:
- `ConsoleRenderer` — Spectre tables, panels, progress spinners, color-coded statuses
- `JsonRenderer` — clean System.Text.Json to stdout, no ANSI codes

Selection: `--json` flag on any command, or `APPVEYOR_OUTPUT=json` env var.

### API Client

`IAppVeyorClient` interface with single `AppVeyorClient` implementation:
- HttpClient with Bearer token auth
- System.Text.Json source generators for AOT compatibility
- CancellationToken on all methods
- Typed `AppVeyorApiException` for error responses

### Models

C# record types for all API entities. Immutable, value-based equality, source-gen serializable.

## Build & CI/CD

### Build System

Cake SDK style (`build.cs` with `#:sdk Cake.Sdk@6.0.0`):
- Tasks: Clean, Build, Test, Publish, Archive
- Run via: `dotnet tool restore && dotnet make`

### GitHub Actions

- **ci.yaml** (on PR): build + test
- **release.yaml** (on tag push): matrix build (linux-x64, win-x64, osx-x64, osx-arm64), self-contained publish, GitHub Release with archives + checksums

### Distribution

Self-contained binaries via GitHub Releases. Install scripts:
- `install.ps1` (Windows)
- `install.sh` (Linux/macOS)

### Versioning

MinVer from git tags. No manual version management.

## Testing

- **Unit tests**: JSON serialization, config loading, output rendering
- **Integration tests**: MockAppVeyorServer + CommandAppTester end-to-end command tests
- **Error handling tests**: 401, 404, network failures, missing config

Packages: xUnit, NSubstitute, Spectre.Console.Testing
