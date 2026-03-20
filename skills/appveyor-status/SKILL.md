---
name: appveyor-status
description: Check the current build status of an AppVeyor project.
user-invocable: true
arguments:
  - name: project
    description: "Project in account/slug format (e.g. myaccount/myproject)"
    required: true
---

# AppVeyor Build Status

1. Run: `appveyor project get {{project}} --json`
2. Present: project name, last build version/branch/status, timing
3. If failing, highlight the failure and suggest `/appveyor-logs` with the job ID
4. If not configured, suggest `/appveyor-setup`
