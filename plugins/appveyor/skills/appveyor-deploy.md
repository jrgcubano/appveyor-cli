---
name: appveyor-deploy
description: Deploy a specific build version to an AppVeyor deployment environment.
user-invocable: true
arguments:
  - name: project
    description: "Project in account/slug format"
    required: true
  - name: environment
    description: "Target environment name (e.g. Production, Staging)"
    required: true
  - name: version
    description: "Build version to deploy"
    required: true
---

# AppVeyor Deploy

Deploy a build to a target environment.

## Instructions

1. This is a write operation. Always confirm with the user before proceeding:
   - Project: {{project}}
   - Environment: {{environment}}
   - Build version: {{version}}

2. Once confirmed, run:

```bash
appveyor deployment start --project {{project}} --environment {{environment}} --build-version {{version}} --json
```

3. Parse the response and report:
   - Deployment ID and status
   - Target environment
   - A note that they can check deployment status with `appveyor deployment get <id>`

4. If it fails, check:
   - Read-only mode blocking the operation
   - Invalid environment name (suggest `appveyor environment list` to see available ones)
   - Invalid build version (suggest `appveyor build history {{project}}` to see available versions)
