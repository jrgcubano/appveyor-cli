---
name: appveyor
description: "Natural language interface for AppVeyor CI/CD. Use when the user mentions builds, deployments, CI status, AppVeyor, or asks about their CI/CD pipeline. Handles questions like 'is the build passing?', 'deploy to staging', 'what broke the build?', 'show my projects'."
user-invocable: true
arguments:
  - name: query
    description: "What the user wants to know or do (natural language)"
    required: false
---

# AppVeyor Natural Language Interface

Route natural language requests to the appropriate AppVeyor CLI commands.

## Context Detection

If the user specifies a project name or slug, use that directly. Otherwise, try to auto-detect:

```bash
git branch --show-current
git remote get-url origin 2>/dev/null
```

Extract the account/slug from the remote URL if possible. If auto-detection fails (e.g., the current repo isn't on AppVeyor), ask the user for the project slug in `account/slug` format.

The user might be working in project X but asking about a completely different AppVeyor project — that's fine. Always prefer explicit project references over auto-detection when the user names a specific project.

## Command Routing

Based on what the user asks, run the appropriate commands:

### Status / "is it passing?" / "how's the build?"
```bash
appveyor project get <account>/<slug> --json
```
If they mention a specific branch:
```bash
appveyor build history <account>/<slug> --branch <branch> --count 5 --json
```

### "what broke?" / "why did it fail?" / build failures
1. Get recent builds: `appveyor build history <account>/<slug> --count 5 --json`
2. Find the failed build's job ID from the JSON
3. Fetch the log: `appveyor build log <job-id>`
4. Analyze the log for errors and suggest fixes

### "start a build" / "run CI" / "trigger build"
Confirm first, then:
```bash
appveyor build start <account>/<slug> --branch <current-branch> --json
```

### "deploy" / "push to production" / "release to staging"
Confirm first (this is a write operation), then:
```bash
appveyor deployment start --project <account>/<slug> --environment <env> --build-version <version> --json
```
If they don't specify a version, get the latest successful build first.

### "show projects" / "list my projects"
```bash
appveyor project list --json
```

### "show environments" / "where can I deploy?"
```bash
appveyor environment list --json
```

### "who has access?" / "team" / "users"
```bash
appveyor user list --json
appveyor role list --json
```

## Response Style

- Be concise — lead with the answer (passing/failing/deployed)
- Only show details if asked or if something is wrong
- For failures, always try to fetch the log and diagnose
- For write operations (build start, deploy), always confirm first
- If the CLI isn't configured, guide them to `/appveyor-setup`
