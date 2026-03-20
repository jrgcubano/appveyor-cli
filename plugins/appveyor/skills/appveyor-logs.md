---
name: appveyor-logs
description: Fetch and analyze build job logs from AppVeyor to diagnose build failures.
user-invocable: true
arguments:
  - name: job-id
    description: "Build job ID to fetch logs for"
    required: true
---

# AppVeyor Build Logs

Fetch and analyze build logs to diagnose issues.

## Instructions

1. Fetch the build log:

```bash
appveyor build log {{job-id}}
```

2. Analyze the log output:
   - Look for error messages, failed test results, or compilation errors
   - Identify the root cause of the failure
   - Suggest specific fixes based on the error patterns

3. Present findings clearly:
   - Summary of what went wrong
   - The relevant error lines from the log
   - Suggested fix or next steps

4. If the user doesn't have a job ID, suggest they first check build status:
   - `appveyor project get <project> --json` to find the last build's job IDs
   - `appveyor build history <project> --json` to find older builds
