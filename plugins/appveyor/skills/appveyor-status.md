---
name: appveyor-status
description: Check the current build status of an AppVeyor project. Shows latest build version, branch, status, and timing.
user-invocable: true
arguments:
  - name: project
    description: "Project in account/slug format (e.g. myaccount/myproject)"
    required: true
---

# AppVeyor Build Status

Check the current build status for the given project.

## Instructions

1. Run the following command to get the project's latest build status:

```bash
appveyor project get {{project}} --json
```

2. Parse the JSON output and present a clear summary including:
   - Project name and repository
   - Last build version, branch, and status
   - Build started/finished times and duration
   - If the build failed, highlight the failure clearly

3. If the build is currently running, mention that it's in progress.

4. If the command fails, check if the user has configured their token:
```bash
appveyor config get
```
