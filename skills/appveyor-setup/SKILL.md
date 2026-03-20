---
name: appveyor-setup
description: Install and configure the AppVeyor CLI.
user-invocable: true
---

# AppVeyor CLI Setup

1. Check if installed: `appveyor --help`
2. If not, guide installation:
   - macOS/Linux: `curl -sSL https://raw.githubusercontent.com/jrgcubano/appveyor-cli/main/install.sh | bash`
   - Windows: `iwr -useb https://raw.githubusercontent.com/jrgcubano/appveyor-cli/main/install.ps1 | iex`
3. Check config: `appveyor config get`
4. If not configured: `appveyor config set --token <token>` (get token from https://ci.appveyor.com/api-keys)
5. Test: `appveyor config test`
6. If test passes, suggest `/appveyor-projects`
