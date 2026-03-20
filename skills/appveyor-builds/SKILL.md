---
name: appveyor-builds
description: View recent build history for an AppVeyor project.
user-invocable: true
arguments:
  - name: project
    description: "Project in account/slug format"
    required: true
  - name: count
    description: "Number of builds to show (default: 10)"
    required: false
---

# AppVeyor Build History

1. Run: `appveyor build history {{project}} --count {{count | default: 10}} --json`
2. Present: version, branch, status, commit message, duration, success rate
3. If there's a pattern of failures, mention it
