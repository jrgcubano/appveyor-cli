# Phase 3 -- Build System and CI/CD

**Status:** Complete

> **Related ADRs:** [ADR 004](../adr/004-cake-sdk-build-system.md)

## Summary

Set up Cake SDK build script, GitHub Actions workflows for CI and release, and install scripts for cross-platform distribution.

## Tasks

- [x] Create `build.cs` with Cake SDK (Clean, Build, Test, Publish tasks)
- [x] Create `.config/dotnet-tools.json` with Cake.Tool 6.0.0
- [x] Create `.github/workflows/ci.yaml` (PR validation: build + test)
- [x] Create `.github/workflows/release.yaml` (tag-triggered: matrix build, archive, GitHub Release)
- [x] Create `install.ps1` (Windows installer with auto-detect, PATH setup, dry-run)
- [x] Create `install.sh` (Linux/macOS installer with platform detection, uninstall)
- [x] Add MinVer for git-tag-based versioning
- [x] Add NuGet.Config to pin package source

## Running the build

```bash
dotnet tool restore
dotnet run build.cs                    # Default: Clean + Build + Test
dotnet run build.cs --target Publish   # Also publish self-contained binaries
```

## Key Files

```
build.cs                               # Cake SDK build script
.config/dotnet-tools.json              # Cake.Tool local tool manifest
.github/workflows/ci.yaml             # PR validation
.github/workflows/release.yaml        # Release pipeline
install.ps1                            # Windows installer
install.sh                             # Linux/macOS installer
global.json                            # .NET SDK version pin
Directory.Build.props                  # Shared build properties
Directory.Packages.props               # Central package management
```
