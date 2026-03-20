# ADR 003: System.Text.Json Source Generators for Serialization

**Date:** 2026-03
**Status:** Accepted

> **Related:** [Phase 2](../phases/phase-2-api-client-and-output.md)

## Context

The CLI needs to serialize/deserialize JSON for API communication and `--json` output. .NET offers two approaches: reflection-based and source-generator-based serialization.

## Decision

Use System.Text.Json source generators exclusively via `AppVeyorJsonContext`. All model types are registered with `[JsonSerializable]` attributes. Reflection-based serialization is disabled via `JsonSerializerIsReflectionEnabledByDefault=false` in the csproj.

## Alternatives Considered

| Alternative | Pros | Cons | Why rejected |
|-------------|------|------|--------------|
| Reflection-based System.Text.Json | Less boilerplate, no context registration | Not AOT-compatible, slower startup, trim warnings | Blocks future AOT compilation |
| Newtonsoft.Json | Mature, flexible | Extra dependency, not AOT-compatible, slower | Unnecessary dependency for our use case |

## Consequences

**Positive:**
- AOT-compatible and trim-safe
- Faster serialization (no runtime reflection)
- Compile-time validation of serializable types

**Negative:**
- Every new model type must be registered in `AppVeyorJsonContext`
- `Dictionary<string, object>` serialization has limitations with source gen
