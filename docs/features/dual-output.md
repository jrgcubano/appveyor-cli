# Feature: Dual Output (Rich Terminal + JSON)

## Overview

Every command in the CLI supports two output modes: rich Spectre.Console rendering for human use, and clean JSON for AI agents, scripts, and piping.

## Summary

By default, commands render data using Spectre.Console tables, panels, and color-coded statuses. When `--json` is passed (or `APPVEYOR_OUTPUT=json` is set), commands emit structured JSON to stdout with no ANSI escape codes.

## Decisions

| Context | Choice | Alternatives considered |
|---------|--------|------------------------|
| Output format for AI agents | JSON via `--json` flag or env var | YAML (adds complexity), plain text (hard to parse) |
| How to toggle | Per-command flag + global env var | Config file setting (less convenient for scripts) |
| Implementation | `IOutputRenderer` abstraction | Per-command if/else (doesn't scale) |

## Behavior

- `appveyor project list` renders a Spectre table with rounded borders and column headers
- `appveyor project list --json` emits a JSON array to stdout
- `APPVEYOR_OUTPUT=json` makes JSON the default for all commands (AI agents set this once)
- Color-coded statuses: green for success, red for failed, blue for running, yellow for queued
- Detail views use key-value panels (e.g. `project get` shows project metadata + last build)

## Verification

- [x] Table output shows all columns with proper alignment
- [x] JSON output is valid JSON parseable by `jq` and `System.Text.Json`
- [x] `--json` flag works on every command
- [x] `APPVEYOR_OUTPUT=json` env var forces JSON output
- [x] No ANSI codes in JSON output

## Key Files

```
src/AppVeyorCli/Output/
  IOutputRenderer.cs           # Abstraction (RenderTable, RenderDetail, RenderJson, etc.)
  ConsoleRenderer.cs           # Spectre.Console implementation
  JsonRenderer.cs              # System.Text.Json implementation
  OutputRendererFactory.cs     # Selects renderer based on --json flag or env var
```

## Related

- [ADR 002](../adr/002-dual-output-rich-and-json.md)
- [Phase 2](../phases/phase-2-api-client-and-output.md)
