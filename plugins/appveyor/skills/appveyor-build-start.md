---
name: appveyor-build-start
description: Start a new build for an AppVeyor project on a specific branch.
user-invocable: true
arguments:
  - name: project
    description: "Project in account/slug format (e.g. myaccount/myproject)"
    required: true
  - name: branch
    description: "Branch to build (default: main)"
    required: false
---

# AppVeyor Start Build

Start a new build for the specified project.

## Instructions

1. Before starting, confirm with the user that they want to trigger a build:
   - Project: {{project}}
   - Branch: {{branch | default: main}}

2. Once confirmed, run:

```bash
appveyor build start {{project}} --branch {{branch | default: main}} --json
```

3. Parse the response and report:
   - Build version that was queued
   - Branch and status
   - A note that they can check status with `/appveyor-status {{project}}`

4. If the command fails due to read-only mode, explain that `--read-only` or `APPVEYOR_READ_ONLY=true` is set and how to disable it.
