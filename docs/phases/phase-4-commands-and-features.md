# Phase 4 -- Commands and Features

**Status:** Complete

> **Related ADRs:** [ADR 005](../adr/005-read-only-mode.md)

## Summary

Implemented all 20 CLI commands across 5 domains (config, project, build, environment, deployment) with read-only mode protection on write operations.

## Tasks

- [x] Config commands: `set`, `get`, `test`
- [x] Project commands: `list`, `get`, `add`, `delete`, `settings` (with `--yaml`)
- [x] Build commands: `start`, `history`, `cancel`, `rerun`, `log`
- [x] Environment commands: `list`, `get`, `add`, `delete`
- [x] Deployment commands: `get`, `start`, `cancel`
- [x] Add `ReadOnlyGuard` to all 10 write commands
- [x] Wire all commands in `Program.cs` command tree

## Command tree

```
appveyor
  config set|get|test
  project list|get|add|delete|settings
  build start|history|cancel|rerun|log
  environment list|get|add|delete
  deployment get|start|cancel
```

## Key Files

```
src/AppVeyorCli/Commands/
  Config/        ConfigSetCommand, ConfigGetCommand, ConfigTestCommand
  Projects/      ProjectListCommand, ProjectGetCommand, ProjectAddCommand,
                 ProjectDeleteCommand, ProjectSettingsCommand
  Builds/        BuildStartCommand, BuildHistoryCommand, BuildCancelCommand,
                 BuildRerunCommand, BuildLogCommand
  Environments/  EnvironmentListCommand, EnvironmentGetCommand,
                 EnvironmentAddCommand, EnvironmentDeleteCommand
  Deployments/   DeploymentGetCommand, DeploymentStartCommand,
                 DeploymentCancelCommand
```

## Related

- [ADR 005](../adr/005-read-only-mode.md)
