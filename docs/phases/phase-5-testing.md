# Phase 5 -- Testing

**Status:** Complete

## Summary

Implemented 31 tests across three layers: serialization unit tests, API client error handling tests, and command integration tests using MockAppVeyorServer.

## Tasks

- [x] Build `MockAppVeyorServer` (HttpListener-based fake with port retry)
- [x] Serialization tests: round-trips for Project, Build, Environment, requests, null handling
- [x] Config tests: validity checks, base URL formatting, config file round-trips
- [x] API error tests: 401, 403, 404 handling, connection test success/failure
- [x] Project command tests: `list` (table + JSON), `get` (detail + JSON)
- [x] Build command tests: `start` (table + JSON), `history`, `cancel`, `log`
- [x] Environment command tests: `list` (table + JSON)

## Test structure

```
tests/AppVeyorCli.Tests/
  GlobalUsings.cs
  Infrastructure/
    MockAppVeyorServer.cs        # Fake HTTP server with route registration
  Api/
    SerializationTests.cs        # JSON source-gen round-trips
    ConfigServiceTests.cs        # Config model + file operations
    ApiClientErrorTests.cs       # HTTP error handling + connection test
  Commands/
    ProjectCommandTests.cs       # Project list/get integration tests
    BuildCommandTests.cs         # Build start/history/cancel/log tests
    EnvironmentCommandTests.cs   # Environment list tests
```

## Running tests

```bash
dotnet test                        # All tests
dotnet test --filter "Serialization"   # Just serialization tests
dotnet test --filter "Commands"        # Just command integration tests
```

## Related

- [ADR 006](../adr/006-console-provider-di-workaround.md) -- TestConsole injection via IConsoleProvider
