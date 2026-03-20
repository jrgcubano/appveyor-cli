---
name: appveyor
description: "Natural language interface for AppVeyor CI/CD. Use when the user mentions builds, deployments, CI status, AppVeyor, or asks about their CI/CD pipeline."
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

The user might be working in project X but asking about a completely different AppVeyor project — that's fine. Always prefer explicit project references over auto-detection.

## Command Routing

### Status / "is it passing?" / "how's the build?"
```bash
appveyor project get <account>/<slug> --json
```

### "what broke?" / "why did it fail?"
1. Get recent builds: `appveyor build history <account>/<slug> --count 5 --json`
2. Find the failed build's job ID
3. Fetch the log: `appveyor build log <job-id>`
4. Analyze and suggest fixes

### "start a build" / "run CI"
Confirm first, then: `appveyor build start <account>/<slug> --branch <branch> --json`

### "deploy" / "push to production"
Confirm first, then: `appveyor deployment start --project <account>/<slug> --environment <env> --build-version <version> --json`

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

- Lead with the answer (passing/failing/deployed)
- For failures, fetch the log and diagnose
- For write operations, always confirm first
- If the CLI isn't configured, guide to `/appveyor-setup`
