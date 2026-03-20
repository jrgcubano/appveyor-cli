---
name: appveyor-build-start
description: Start a new build for an AppVeyor project.
user-invocable: true
arguments:
  - name: project
    description: "Project in account/slug format"
    required: true
  - name: branch
    description: "Branch to build (default: main)"
    required: false
---

# AppVeyor Start Build

1. Confirm with the user before proceeding (write operation)
2. Run: `appveyor build start {{project}} --branch {{branch | default: main}} --json`
3. Report: build version queued, suggest `/appveyor-status {{project}}` to monitor
