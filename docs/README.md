# Documentation

Start with [Architecture.md](Architecture.md) for an overview of the project structure, key patterns, and design decisions.

## Features

| Feature | Description |
|---------|-------------|
| [Dual Output](features/dual-output.md) | Rich Spectre tables + JSON for AI agents |

## Phases

| Phase | Name | Status |
|-------|------|--------|
| [Phase 0](phases/phase-0-design.md) | Design and Specification | Complete |
| [Phase 1](phases/phase-1-cli-framework-and-scaffolding.md) | CLI Framework and Scaffolding | Complete |
| [Phase 2](phases/phase-2-api-client-and-output.md) | API Client and Output Rendering | Complete |
| [Phase 3](phases/phase-3-build-ci-cd.md) | Build System and CI/CD | Complete |
| [Phase 4](phases/phase-4-commands-and-features.md) | Commands and Features | Complete |
| [Phase 5](phases/phase-5-testing.md) | Testing | Complete |
| [Phase 6](phases/phase-6-team-management.md) | Team Management | Complete |

## Architecture Decision Records

| ADR | Decision | Date |
|-----|----------|------|
| [001](adr/001-spectre-console-over-system-commandline.md) | Spectre.Console.Cli over System.CommandLine | 2026-03 |
| [002](adr/002-dual-output-rich-and-json.md) | Dual output: rich terminal + JSON | 2026-03 |
| [003](adr/003-source-gen-json-serialization.md) | System.Text.Json source generators | 2026-03 |
| [004](adr/004-cake-sdk-build-system.md) | Cake SDK build system | 2026-03 |
| [005](adr/005-read-only-mode.md) | Read-only mode for safe exploration | 2026-03 |
| [006](adr/006-console-provider-di-workaround.md) | IConsoleProvider DI workaround | 2026-03 |

## Templates

- [ADR Template](templates/ADR-Template.md)
- [Feature Template](templates/Feature-Template.md)
