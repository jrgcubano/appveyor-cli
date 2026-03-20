# ADR 006: IConsoleProvider DI Workaround

**Date:** 2026-03
**Status:** Accepted

> **Related:** [Phase 2](../phases/phase-2-api-client-and-output.md)

## Context

Spectre.Console.Cli automatically registers its own `IAnsiConsole` instance in the DI container, overriding any user-registered instance. This prevents tests from injecting a `TestConsole` to capture command output.

## Decision

Introduce `IConsoleProvider` as an indirection layer. Commands depend on `IConsoleProvider` (not `IAnsiConsole`), and access the console via `consoleProvider.Console`. Since Spectre doesn't know about `IConsoleProvider`, it can't override our registration.

## Alternatives Considered

| Alternative | Pros | Cons | Why rejected |
|-------------|------|------|--------------|
| Use `CommandAppTester` | Built for testing | Only supports single default command, not branched command trees | Doesn't work with our multi-branch setup |
| Register `IAnsiConsole` after Spectre | Might work | Spectre registers during `Build()`, timing is unreliable | Fragile, implementation-dependent |
| Skip console DI, use static `AnsiConsole` | Simple | Untestable, can't capture output in tests | Breaks testing story |

## Consequences

**Positive:**
- Tests can inject `TestConsole` and capture all command output
- Clean separation -- commands don't depend on Spectre DI behavior

**Negative:**
- Extra indirection layer (`consoleProvider.Console` instead of `console`)
- Every command constructor takes `IConsoleProvider` instead of the more natural `IAnsiConsole`
