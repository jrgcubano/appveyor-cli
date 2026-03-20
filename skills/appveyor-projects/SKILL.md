---
name: appveyor-projects
description: List all AppVeyor projects in the configured account.
user-invocable: true
---

# AppVeyor Projects

1. Run: `appveyor project list --json`
2. Present: project name, slug, repository, private flag, last updated
3. If auth fails, suggest `/appveyor-setup`
