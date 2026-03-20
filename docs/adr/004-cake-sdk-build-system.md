# ADR 004: Cake SDK Build System

**Date:** 2026-03
**Status:** Accepted

> **Related:** [Phase 3](../phases/phase-3-build-ci-cd.md)

## Context

The project needs a build system for clean/build/test/publish workflows. The reference project (freshdesk-cli) uses PowerShell scripts; Spectre.Console uses Cake SDK.

## Decision

Use Cake SDK style (`build.cs` with `#:sdk Cake.Sdk@6.0.0`), following the Spectre.Console project pattern. This is a .NET 10 file-based program that runs via `dotnet run build.cs`.

## Alternatives Considered

| Alternative | Pros | Cons | Why rejected |
|-------------|------|------|--------------|
| Cake Frosting | Strongly typed, debuggable | Separate project, more setup | Heavier than needed for our build |
| PowerShell scripts (freshdesk-cli pattern) | Simple, no dependencies | Platform-specific, no task dependencies | Doesn't compose well, Windows-centric |
| NUKE | Full C# build system | Large dependency, steep learning curve | Overkill for this project |
| Raw `dotnet` CLI commands | Zero dependencies | No task graph, no reuse | Doesn't scale |

## Consequences

**Positive:**
- Single `build.cs` file with full C# support
- Task dependency graph (Clean -> Build -> Test)
- Same pattern as Spectre.Console (familiar to contributors)
- Run via `dotnet run build.cs` -- no extra tools needed beyond `dotnet tool restore`

**Negative:**
- Requires .NET 10 for `#:sdk` file-based program support
- Cake.Sdk 6.0.0 is relatively new
