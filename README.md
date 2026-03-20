# AppVeyor CLI

[![CI](https://github.com/jrgcubano/appveyor-cli/actions/workflows/ci.yaml/badge.svg?branch=main)](https://github.com/jrgcubano/appveyor-cli/actions/workflows/ci.yaml)
[![CodeQL](https://github.com/jrgcubano/appveyor-cli/actions/workflows/codeql.yaml/badge.svg?branch=main)](https://github.com/jrgcubano/appveyor-cli/actions/workflows/codeql.yaml)
[![Release](https://github.com/jrgcubano/appveyor-cli/actions/workflows/release.yaml/badge.svg?branch=main)](https://github.com/jrgcubano/appveyor-cli/actions/workflows/release.yaml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A command-line interface for the [AppVeyor](https://www.appveyor.com/) CI/CD API.

Designed for both human use (rich terminal tables, color-coded build statuses) and AI/machine consumption (structured JSON output via `--json`).

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [AI Plugin](#ai-plugin)
- [Building from Source](#building-from-source)
- [Technology](#technology)
- [API Coverage](#api-coverage)
- [License](#license)
- [Contributing](#contributing)
- [Documentation Map](#documentation-map)
- [References](#references)

## Features

- 📦 **Project Management** - List, inspect, add, and delete AppVeyor projects
- 🔨 **Build Management** - Start, cancel, re-run builds and view build history with logs
- 🌍 **Environment Management** - List, inspect, add, and delete deployment environments
- 🚀 **Deployment Management** - Start, inspect, and cancel deployments
- 👥 **Team Management** - Manage users, collaborators, and roles with permissions
- 🎨 **Rich Terminal Output** - Spectre.Console tables, panels, and color-coded statuses
- 🤖 **JSON Output** - `--json` flag on every command for scripting and AI agent consumption
- 🔒 **Read-Only Mode** - `--read-only` flag or `APPVEYOR_READ_ONLY=true` to prevent accidental writes
- 🔐 **Flexible Authentication** - Environment variables, config file, or interactive setup
- 💻 **Cross-Platform** - Pre-built binaries for Linux, macOS (Intel + ARM), and Windows

## Installation

### Quick Install

Linux/macOS:

```bash
curl -sSL https://raw.githubusercontent.com/jrgcubano/appveyor-cli/main/install.sh | bash
```

Windows (PowerShell):

```powershell
iwr -useb https://raw.githubusercontent.com/jrgcubano/appveyor-cli/main/install.ps1 | iex
```

### Pre-built Binaries

Download the latest release for your platform from the [Releases](https://github.com/jrgcubano/appveyor-cli/releases) page.

### From Source

```bash
git clone https://github.com/jrgcubano/appveyor-cli.git
cd appveyor-cli
dotnet run --project src/AppVeyorCli -- --help
```

### Uninstall

```bash
curl -sSL https://raw.githubusercontent.com/jrgcubano/appveyor-cli/main/install.sh | bash -s -- --uninstall
```

## Configuration

### Initial Setup

```bash
# Interactive configuration
appveyor config set

# Or provide values directly
appveyor config set --token v2.your-api-token --account your-account-name

# Test the connection
appveyor config test
```

### Environment Variables

Environment variables take precedence over the config file:

| Variable | Description |
|----------|-------------|
| `APPVEYOR_API_TOKEN` | API token (required) |
| `APPVEYOR_ACCOUNT_NAME` | Account name (for v2 tokens with multi-account access) |
| `APPVEYOR_API_URL` | API base URL (default: `https://ci.appveyor.com`) |
| `APPVEYOR_OUTPUT` | Set to `json` to default all output to JSON |
| `APPVEYOR_READ_ONLY` | Set to `true` to block all write operations |

### Config File

Stored at `~/.appveyor/config.json`:

```json
{
  "token": "v2.your-api-token",
  "accountName": "your-account",
  "apiUrl": "https://ci.appveyor.com"
}
```

## Usage

### Projects

```bash
# List all projects
appveyor project list

# Get project details with last build info
appveyor project get myaccount/myproject

# Add a new project from GitHub
appveyor project add --repo-provider gitHub --repo-name owner/repo

# View project settings
appveyor project settings myaccount/myproject
appveyor project settings myaccount/myproject --yaml

# Delete a project
appveyor project delete myaccount/myproject
```

### Builds

```bash
# Start a build
appveyor build start myaccount/myproject --branch main

# Start a build for a specific commit
appveyor build start myaccount/myproject --branch main --commit abc123

# View build history
appveyor build history myaccount/myproject --count 10
appveyor build history myaccount/myproject --branch main

# Cancel a running build
appveyor build cancel myaccount/myproject 1.0.42

# Re-run a build
appveyor build rerun 12345
appveyor build rerun 12345 --incomplete-only

# Download build job log
appveyor build log job-id-here
```

### Environments

```bash
# List all deployment environments
appveyor environment list

# Get environment details
appveyor environment get 123

# Add an environment
appveyor environment add --name Production --provider Agent

# Delete an environment
appveyor environment delete 123
```

### Deployments

```bash
# Start a deployment
appveyor deployment start \
  --environment Production \
  --project myaccount/myproject \
  --build-version 1.0.42

# Get deployment details
appveyor deployment get 456

# Cancel a running deployment
appveyor deployment cancel 456
```

### Users

```bash
# List all team users
appveyor user list

# Get user details
appveyor user get 100

# Add a new user
appveyor user add --name "Jane Smith" --email jane@example.com --role-id 2

# Delete a user
appveyor user delete 100
```

### Collaborators

```bash
# List all collaborators
appveyor collaborator list

# Add a collaborator
appveyor collaborator add --email external@example.com --role-id 3

# Remove a collaborator
appveyor collaborator delete 101
```

### Roles

```bash
# List all roles
appveyor role list

# Get role details with permissions
appveyor role get 1

# Add a new role
appveyor role add --name "QA Team"

# Delete a role
appveyor role delete 5
```

### JSON Output

Every command supports `--json` for machine-readable output:

```bash
# JSON output for a single command
appveyor project list --json

# Set JSON as default output for all commands
export APPVEYOR_OUTPUT=json
appveyor project list
```

### Read-Only Mode

Prevent accidental writes during exploration:

```bash
# Per-command flag
appveyor project list --read-only

# Or set globally via environment variable
export APPVEYOR_READ_ONLY=true
appveyor build start myaccount/myproject --branch main
# Output: Write operation blocked -- running in read-only mode.
```

### Global Options

All commands support these options:

| Option | Description |
|--------|-------------|
| `--json` | Output as JSON for machine/AI consumption |
| `--verbose` | Show detailed output |
| `--read-only` | Block all write operations |
| `--help` | Show help for the command |

## AI Plugin

This project includes an AI plugin that works with multiple providers. Install it to get AppVeyor commands directly in your AI coding assistant.

### Claude Code

```bash
claude plugin marketplace add jrgcubano/appveyor-cli
claude plugin install appveyor-cli@appveyor
```

### GitHub Copilot CLI

```bash
copilot plugin marketplace add jrgcubano/appveyor-cli
copilot plugin install appveyor-cli@appveyor
```

### OpenAI Codex

```bash
$skill-installer install https://github.com/jrgcubano/appveyor-cli
```

### Available Skills

Once installed, you can use these commands from your AI assistant:

| Skill | Description |
|-------|-------------|
| `/appveyor` | Natural language — "is the build passing?", "deploy to staging" |
| `/appveyor-status <project>` | Check current build status |
| `/appveyor-projects` | List all projects |
| `/appveyor-builds <project>` | View build history |
| `/appveyor-build-start <project>` | Start a new build |
| `/appveyor-deploy <project> <env> <version>` | Deploy a build |
| `/appveyor-logs <job-id>` | Fetch and diagnose build logs |
| `/appveyor-setup` | Install and configure the CLI |

### Provider Support

| Provider | Plugin Config | Skills | Agent |
|----------|--------------|--------|-------|
| Claude Code | `.claude-plugin/plugin.json` | `skills/*/SKILL.md` | `agents/ci-monitor.md` |
| GitHub Copilot CLI | `.claude-plugin/plugin.json` | `skills/*/SKILL.md` | `agents/ci-monitor.md` |
| OpenAI Codex | `.codex-plugin/plugin.json` | `skills/*/SKILL.md` | `.agents/openai.yaml` |

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build and Test

```bash
# Restore tools and run the Cake build (clean + build + test)
dotnet tool restore
dotnet run build.cs

# Or build and test directly
dotnet build
dotnet test
```

### Publish Self-Contained Binary

```bash
dotnet publish src/AppVeyorCli -c Release -r linux-x64 --self-contained -o ./publish
```

## Technology

- .NET 10, C# 14
- [Spectre.Console](https://spectreconsole.net/) for rich terminal output
- [Spectre.Console.Cli](https://spectreconsole.net/cli/) for command-line parsing
- System.Text.Json with source generators for AOT-compatible serialization
- [Cake](https://cakebuild.net/) SDK for build automation
- [MinVer](https://github.com/adamralph/minver) for git-based versioning
- xUnit for testing

## API Coverage

This CLI covers the following AppVeyor API areas:

- Projects (list, get, add, update, delete, settings, YAML config, build cache)
- Builds (start, cancel, re-run, delete, history, job log)
- Environments (list, get, add, update, delete)
- Deployments (get, start, cancel)
- Users (list, get, add, delete)
- Collaborators (list, add, delete)
- Roles (list, get, add, delete)

## License

This project is licensed under the [MIT License](LICENSE).

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) before submitting a pull request.

## Documentation Map

- Architecture: [docs/Architecture.md](docs/Architecture.md)
- Dual output (rich + JSON): [docs/features/dual-output.md](docs/features/dual-output.md)
- Phases: [docs/phases](docs/phases)
- ADRs: [docs/adr](docs/adr)
- Templates: [docs/templates](docs/templates)
- AI agent guidelines: [AGENTS.md](AGENTS.md)

## References

- [AppVeyor REST API Documentation](https://www.appveyor.com/docs/api/) - The API this CLI wraps
- [Spectre.Console](https://github.com/spectreconsole/spectre.console) - Rich terminal output and CLI framework
- [freshdesk-cli](https://github.com/Aaronontheweb/freshdesk-cli/tree/dev) - Reference .NET CLI project used as architectural inspiration
- [Cake Build](https://cakebuild.net/) - Build automation system

## Acknowledgements

- [dotnet-artisan](https://github.com/novotnyllc/dotnet-artisan) - Reference for AI plugin structure, skill layout, and multi-provider support (Claude Code, Copilot CLI, Codex)
- The [Claude Code plugin system](https://docs.anthropic.com/en/docs/claude-code/plugins) and [Agent Skills](https://github.com/anthropics/agent-skills) open standard for enabling structured, discoverable development skills
- The [.NET community and ecosystem](https://dotnet.microsoft.com/) for the frameworks, libraries, and patterns used in this project
