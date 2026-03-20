---
name: appveyor-builds
description: View recent build history for an AppVeyor project with status, branches, and durations.
user-invocable: true
arguments:
  - name: project
    description: "Project in account/slug format (e.g. myaccount/myproject)"
    required: true
  - name: count
    description: "Number of builds to show (default: 10)"
    required: false
---

# AppVeyor Build History

Show recent build history for a project.

## Instructions

1. Run:

```bash
appveyor build history {{project}} --count {{count | default: 10}} --json
```

2. Present the build history as a clear summary:
   - Build version, branch, and status (highlight failures)
   - Commit message (truncated if long)
   - Started time and duration
   - Overall success rate (e.g. "8/10 builds passed")

3. If there's a pattern of failures, mention it (e.g. "last 3 builds on branch X failed").
