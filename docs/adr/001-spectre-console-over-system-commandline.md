# ADR 001: Spectre.Console.Cli over System.CommandLine

**Date:** 2026-03
**Status:** Accepted

> **Related:** [Phase 1](../phases/phase-1-cli-framework-and-scaffolding.md)

## Context

The CLI needs a command-line parsing framework. The reference project (freshdesk-cli) uses System.CommandLine but resorts to manual command routing via a large switch statement, bypassing many of the framework's features.

## Decision

Use Spectre.Console.Cli for command parsing and Spectre.Console for rich terminal output. This gives us a single dependency that handles both concerns: structured command trees with `AddBranch` and rich output (tables, panels, colors, progress bars).

## Alternatives Considered

| Alternative | Pros | Cons | Why rejected |
|-------------|------|------|--------------|
| System.CommandLine | Microsoft-backed, AOT-friendly | Still in beta, complex API, no rich output | Requires separate output library, API still evolving |
| Cocona | Minimal boilerplate | Smaller community, less output control | Less mature than Spectre for CLI apps |
| Manual routing (freshdesk-cli pattern) | Full control | No auto-generated help, no validation, lots of boilerplate | Doesn't scale with 20+ commands |

## Consequences

**Positive:**
- Auto-generated help for all commands and branches
- Built-in argument validation and type conversion
- Rich output (tables, panels, colors) from the same library
- Settings inheritance for shared options (`--json`, `--verbose`, `--read-only`)

**Negative:**
- Spectre.Console.Cli overrides user-registered `IAnsiConsole` in DI (required `IConsoleProvider` workaround)
- Not yet at 1.0 (currently 0.50.0)
