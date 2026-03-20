# ADR 002: Dual Output -- Rich Terminal and JSON

**Date:** 2026-03
**Status:** Accepted

> **Related:** [Phase 2](../phases/phase-2-api-client-and-output.md)

## Context

The CLI needs to serve two audiences: humans at a terminal who want readable output, and AI agents (Claude CLI, GitHub Copilot, scripts) that need structured data.

## Decision

Implement an `IOutputRenderer` abstraction with two implementations:
- `ConsoleRenderer` -- Spectre.Console tables, panels, and color-coded statuses (default)
- `JsonRenderer` -- clean System.Text.Json output to stdout, no ANSI codes

Selection is via `--json` flag on any command or `APPVEYOR_OUTPUT=json` environment variable.

## Alternatives Considered

| Alternative | Pros | Cons | Why rejected |
|-------------|------|------|--------------|
| JSON only | Simplest, always machine-readable | Poor human experience | Defeats purpose of Spectre.Console |
| Rich only with `--json` flag | Simple default | AI agents need structured data by default | Doesn't serve scripting use case well |
| Triple mode (+ YAML) | Maximum flexibility | Extra complexity, YAML adds no value over JSON | YAGNI |

## Consequences

**Positive:**
- AI agents can set `APPVEYOR_OUTPUT=json` once and consume all commands
- Human users get rich, readable tables by default
- Commands don't need to know about output format -- they call the renderer

**Negative:**
- Every command must handle both paths (render table vs render JSON)
- `JsonRenderer.RenderTable` fallback uses reflection-based serialization (callers should prefer `RenderJson` with type info)
