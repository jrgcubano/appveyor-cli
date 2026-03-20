# AppVeyor CLI Installer for Windows
#
# Usage:
#   iwr -useb https://raw.githubusercontent.com/jrgcubano/appveyor-cli/main/install.ps1 | iex
#
# Or download and run:
#   .\install.ps1
#   .\install.ps1 -InstallDir "C:\custom\path"
#

param(
    [string]$InstallDir = "",
    [switch]$Force,
    [switch]$DryRun,
    [switch]$Beta,
    [switch]$Help
)

$ErrorActionPreference = "Stop"

if ($Help) {
    Write-Host "AppVeyor CLI Installer for Windows"
    Write-Host ""
    Write-Host "Usage: .\install.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -InstallDir <path>  Custom installation directory"
    Write-Host "  -Force              Skip confirmation prompts"
    Write-Host "  -DryRun             Download and verify but don't install"
    Write-Host "  -Beta               Include beta/pre-release versions"
    Write-Host "  -Help               Show this help message"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\install.ps1"
    Write-Host "  .\install.ps1 -DryRun"
    Write-Host "  .\install.ps1 -Beta -Force"
    Write-Host "  .\install.ps1 -InstallDir 'C:\tools\appveyor'"
    exit 0
}

# Configuration
$RepoOwner = "jrgcubano"
$RepoName = "appveyor-cli"
$BinaryName = "appveyor.exe"

if ([string]::IsNullOrEmpty($InstallDir)) {
    $InstallDir = "$env:LOCALAPPDATA\Programs\appveyor"
}

function Write-Info { Write-Host "[INFO] $args" -ForegroundColor Green }
function Write-Warn { Write-Host "[WARN] $args" -ForegroundColor Yellow }
function Write-Err { Write-Host "[ERROR] $args" -ForegroundColor Red; exit 1 }

function Get-Platform {
    $arch = if ([Environment]::Is64BitOperatingSystem) { "x64" } else { "x86" }
    return "win-$arch"
}

function Get-LatestVersion {
    param([bool]$IncludeBeta = $false)

    if ($IncludeBeta) {
        Write-Info "Fetching latest release information (including pre-releases)..."
        $apiUrl = "https://api.github.com/repos/$RepoOwner/$RepoName/releases"
    } else {
        Write-Info "Fetching latest stable release information..."
        $apiUrl = "https://api.github.com/repos/$RepoOwner/$RepoName/releases/latest"
    }

    try {
        $headers = @{ "User-Agent" = "appveyor-cli-installer" }
        $releases = Invoke-RestMethod -Uri $apiUrl -Headers $headers

        if ($IncludeBeta -and $releases -is [array]) {
            return $releases[0].tag_name
        } else {
            if ($releases -is [array]) {
                $stable = $releases | Where-Object { -not $_.prerelease } | Select-Object -First 1
                return $stable.tag_name
            } else {
                return $releases.tag_name
            }
        }
    }
    catch {
        Write-Err "Failed to fetch release information: $_"
    }
}

function Install-Binary {
    param(
        [string]$Version,
        [string]$Platform,
        [bool]$DryRun = $false
    )

    $downloadUrl = "https://github.com/$RepoOwner/$RepoName/releases/download/$Version/appveyor-$Version-$Platform.zip"
    $tempFile = "$env:TEMP\appveyor-$Version.zip"
    $tempDir = "$env:TEMP\appveyor-$Version"

    Write-Info "Downloading appveyor $Version for $Platform..."

    try {
        $ProgressPreference = 'Continue'
        Invoke-WebRequest -Uri $downloadUrl -OutFile $tempFile -UseBasicParsing
    }
    catch {
        Write-Err "Failed to download from: $downloadUrl`n$_"
    }

    Write-Info "Extracting binary..."

    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory($tempFile, $tempDir)
    }
    catch {
        Write-Err "Failed to extract archive: $_"
    }

    $binaryPath = Get-ChildItem -Path $tempDir -Filter $BinaryName -Recurse | Select-Object -First 1

    if (-not $binaryPath) {
        Write-Err "Binary not found in archive"
    }

    if ($DryRun) {
        Write-Info "DRY-RUN: Would install binary to $InstallDir\$BinaryName"
        Write-Info "DRY-RUN: Binary found at: $($binaryPath.FullName)"

        try {
            $testOutput = & $binaryPath.FullName --help 2>$null
            if ($testOutput) {
                Write-Info "DRY-RUN: Binary test successful"
            } else {
                Write-Warn "DRY-RUN: Binary test - no output received"
            }
        }
        catch {
            Write-Warn "DRY-RUN: Binary test failed - may not be compatible with this system"
        }

        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        return
    }

    if (-not (Test-Path $InstallDir)) {
        Write-Info "Creating installation directory..."
        New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
    }

    Write-Info "Installing to $InstallDir\$BinaryName..."
    Move-Item -Path $binaryPath.FullName -Destination "$InstallDir\$BinaryName" -Force

    Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}

function Add-ToPath {
    $userPath = [Environment]::GetEnvironmentVariable("Path", "User")

    if ($userPath -notlike "*$InstallDir*") {
        Write-Info "Adding installation directory to PATH..."

        $newPath = if ($userPath -eq $null -or $userPath -eq "") {
            $InstallDir
        } else {
            "$userPath;$InstallDir"
        }

        [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
        $env:Path = "$env:Path;$InstallDir"

        Write-Warn "PATH updated. You may need to restart your terminal for changes to take effect."
    }
}

function Main {
    Write-Host "===================================" -ForegroundColor Cyan
    Write-Host "  AppVeyor CLI Installer" -ForegroundColor Cyan
    Write-Host "===================================" -ForegroundColor Cyan
    Write-Host ""

    if ($DryRun) {
        Write-Warn "Running in DRY-RUN mode - will download but not install"
        Write-Host ""
    }

    $platform = Get-Platform
    Write-Info "Detected platform: $platform"

    $version = Get-LatestVersion -IncludeBeta:$Beta
    Write-Info "Latest version: $version"

    $existingBinary = "$InstallDir\$BinaryName"
    if (Test-Path $existingBinary) {
        try {
            $currentVersion = & $existingBinary --help 2>$null | Select-String -Pattern '\d+\.\d+\.\d+' | ForEach-Object { $_.Matches[0].Value }
        } catch {
            $currentVersion = "unknown"
        }

        Write-Warn "Existing installation found (version: $currentVersion)"

        if (-not $Force) {
            $response = Read-Host "Do you want to overwrite it? [y/N]"
            if ($response -ne 'y' -and $response -ne 'Y') {
                Write-Host "Installation cancelled"
                exit 0
            }
        }
    }

    Install-Binary -Version $version -Platform $platform -DryRun:$DryRun

    if ($DryRun) {
        Write-Host ""
        Write-Info "DRY-RUN complete! No changes were made to your system."
        Write-Info "To actually install, run without -DryRun flag"
    } else {
        try {
            $testOutput = & "$InstallDir\$BinaryName" --help 2>$null
            if ($testOutput) {
                Write-Info "Successfully installed appveyor $version"
            } else {
                throw "Verification failed"
            }
        }
        catch {
            Write-Err "Installation verification failed: $_"
        }

        Add-ToPath

        Write-Host ""
        Write-Host "Installation complete!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Run 'appveyor --help' to get started" -ForegroundColor Cyan
    }
}

Main
