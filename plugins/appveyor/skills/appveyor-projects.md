---
name: appveyor-projects
description: List all AppVeyor projects in the configured account with their latest build status.
user-invocable: true
---

# AppVeyor Projects

List all projects in the user's AppVeyor account.

## Instructions

1. Run:

```bash
appveyor project list --json
```

2. Present the results as a readable summary:
   - Project name and slug
   - Repository name and type
   - Whether the project is private
   - Last updated date

3. If there are many projects, organize them in a clear list format.

4. If the command fails with an auth error, suggest running `appveyor config set` to configure the API token.
