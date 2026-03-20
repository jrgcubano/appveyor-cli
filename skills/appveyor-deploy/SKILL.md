---
name: appveyor-deploy
description: Deploy a build to an AppVeyor deployment environment.
user-invocable: true
arguments:
  - name: project
    description: "Project in account/slug format"
    required: true
  - name: environment
    description: "Target environment name"
    required: true
  - name: version
    description: "Build version to deploy"
    required: true
---

# AppVeyor Deploy

1. Confirm with the user before proceeding (write operation)
2. Run: `appveyor deployment start --project {{project}} --environment {{environment}} --build-version {{version}} --json`
3. Report: deployment ID and status
4. If it fails, suggest `appveyor environment list` or `appveyor build history {{project}}`
