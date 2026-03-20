---
name: appveyor-logs
description: Fetch and analyze build job logs from AppVeyor to diagnose failures.
user-invocable: true
arguments:
  - name: job-id
    description: "Build job ID"
    required: true
---

# AppVeyor Build Logs

1. Run: `appveyor build log {{job-id}}`
2. Analyze the output for errors, failed tests, or compilation issues
3. Present: summary of what went wrong, relevant error lines, suggested fix
4. If no job ID provided, suggest getting one from `/appveyor-status <project>`
