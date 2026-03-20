---
name: appveyor-setup
description: Configure the AppVeyor CLI with API token and account settings.
user-invocable: true
---

# AppVeyor CLI Setup

Help the user configure the AppVeyor CLI.

## Instructions

1. First check if the CLI is installed:

```bash
appveyor --help
```

2. If not installed, guide the user:
   - **macOS/Linux**: `curl -sSL https://raw.githubusercontent.com/jrgcubano/appveyor-cli/main/install.sh | bash`
   - **Windows**: `iwr -useb https://raw.githubusercontent.com/jrgcubano/appveyor-cli/main/install.ps1 | iex`

3. Check current configuration:

```bash
appveyor config get
```

4. If not configured, guide the user to set up:
   - They need an API token from https://ci.appveyor.com/api-keys
   - Run `appveyor config set --token <their-token>`
   - Optionally set account name: `appveyor config set --account <account-name>`

5. Test the connection:

```bash
appveyor config test
```

6. If test passes, suggest trying `/appveyor-projects` to see their projects.
