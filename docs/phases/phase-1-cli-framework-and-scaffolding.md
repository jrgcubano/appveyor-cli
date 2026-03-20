# Phase 1 -- CLI Framework and Scaffolding

**Status:** Complete

> **Related ADRs:** [ADR 001](../adr/001-spectre-console-over-system-commandline.md), [ADR 006](../adr/006-console-provider-di-workaround.md)

## Summary

Set up the .NET 10 project structure, Spectre.Console.Cli integration, DI bridge, and global settings.

## Tasks

- [x] Initialize git repo and solution structure
- [x] Create `global.json` pinning .NET 10 SDK
- [x] Create `Directory.Build.props` with shared build settings (C# 14, nullable, deterministic)
- [x] Create `Directory.Packages.props` with central package management
- [x] Create `AppVeyorCli.csproj` with Spectre.Console dependencies
- [x] Create `AppVeyorCli.Tests.csproj` with xUnit + Spectre.Console.Testing
- [x] Implement `TypeRegistrar` / `TypeResolver` (Spectre DI bridge to MS DI)
- [x] Implement `GlobalSettings` base class (`--json`, `--verbose`, `--read-only`)
- [x] Implement `IConsoleProvider` workaround for Spectre DI override
- [x] Wire up `Program.cs` with `CommandApp` and branch registration

## Key Files

```
src/AppVeyorCli/
  Program.cs
  Infrastructure/
    TypeRegistrar.cs
    TypeResolver.cs
    GlobalSettings.cs
    ReadOnlyGuard.cs
  Output/
    ConsoleProvider.cs
```
