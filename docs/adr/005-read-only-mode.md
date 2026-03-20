# ADR 005: Read-Only Mode for Safe Exploration

**Date:** 2026-03
**Status:** Accepted

> **Related:** [Phase 4](../phases/phase-4-commands-and-features.md)

## Context

Users and AI agents exploring an AppVeyor organization need a way to prevent accidental writes (starting builds, deleting projects, etc.). The freshdesk-cli reference has a similar `--read-only` flag.

## Decision

Implement read-only mode via:
- `--read-only` global flag on any command
- `APPVEYOR_READ_ONLY=true` environment variable (for AI agents to set once)

Write commands check `ReadOnlyGuard.ThrowIfReadOnly(settings)` as the first line in `ExecuteAsync`. The guard throws `InvalidOperationException` which the exception handler renders cleanly.

## Alternatives Considered

| Alternative | Pros | Cons | Why rejected |
|-------------|------|------|--------------|
| Interceptor-based blocking | Centralized, no per-command code | Spectre.Console interceptors lack reliable command type info | Unreliable detection of which command is running |
| Separate read-only API client | Clean separation | Duplicates client code | Unnecessary complexity |
| No read-only mode | Simpler code | Risk of accidental writes during exploration | Unacceptable for AI agent use case |

## Consequences

**Positive:**
- AI agents can set `APPVEYOR_READ_ONLY=true` globally for safe exploration
- Clean error message when a write is blocked
- Simple implementation -- one guard call per write command

**Negative:**
- Must remember to add guard to every new write command (enforced by convention, not compiler)
