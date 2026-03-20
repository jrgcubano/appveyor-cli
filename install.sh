#!/usr/bin/env bash
#
# AppVeyor CLI Installer
#
# Usage:
#   curl -sSL https://raw.githubusercontent.com/jrgcubano/appveyor-cli/main/install.sh | bash
#   wget -qO- https://raw.githubusercontent.com/jrgcubano/appveyor-cli/main/install.sh | bash
#
# Or download and run:
#   ./install.sh
#   ./install.sh --dry-run
#   INSTALL_DIR=/custom/path ./install.sh
#

set -e

# Configuration
REPO_OWNER="jrgcubano"
REPO_NAME="appveyor-cli"
BINARY_NAME="appveyor"
INSTALL_DIR="${INSTALL_DIR:-${HOME}/.local/bin}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

info() { echo -e "${GREEN}[INFO]${NC} $1"; }
warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
error() { echo -e "${RED}[ERROR]${NC} $1"; exit 1; }

detect_platform() {
    local os arch

    case "$(uname -s)" in
        Linux*)  os="linux" ;;
        Darwin*) os="osx" ;;
        MINGW*|MSYS*|CYGWIN*)
            error "Please use install.ps1 for Windows" ;;
        *)
            error "Unsupported OS: $(uname -s)" ;;
    esac

    case "$(uname -m)" in
        x86_64|amd64) arch="x64" ;;
        aarch64|arm64) arch="arm64" ;;
        armv7l) arch="arm" ;;
        *) error "Unsupported architecture: $(uname -m)" ;;
    esac

    echo "${os}-${arch}"
}

check_requirements() {
    local missing_tools=()

    command -v curl >/dev/null 2>&1 || missing_tools+=("curl")
    command -v tar >/dev/null 2>&1 || missing_tools+=("tar")

    if [ ${#missing_tools[@]} -ne 0 ]; then
        error "Missing required tools: ${missing_tools[*]}\nPlease install them and try again."
    fi
}

get_latest_version() {
    local include_prerelease="${1:-false}"
    local api_url

    if [ "$include_prerelease" = true ]; then
        api_url="https://api.github.com/repos/${REPO_OWNER}/${REPO_NAME}/releases"
    else
        api_url="https://api.github.com/repos/${REPO_OWNER}/${REPO_NAME}/releases/latest"
    fi

    local response
    response=$(curl -s "$api_url") || error "Failed to fetch release information"

    local version
    if [ "$include_prerelease" = true ]; then
        version=$(echo "$response" | grep '"tag_name"' | head -1 | sed -E 's/.*"tag_name":\s*"([^"]+)".*/\1/')
    else
        version=$(echo "$response" | grep '"tag_name"' | head -1 | sed -E 's/.*"tag_name":\s*"([^"]+)".*/\1/')
    fi

    if [ -z "$version" ]; then
        error "Could not determine latest version"
    fi

    echo "$version"
}

install_binary() {
    local version="$1"
    local platform="$2"
    local dry_run="${3:-false}"

    local download_url="https://github.com/${REPO_OWNER}/${REPO_NAME}/releases/download/${version}/${BINARY_NAME}-${version}-${platform}.tar.gz"
    local temp_dir
    temp_dir=$(mktemp -d)
    local temp_file="${temp_dir}/${BINARY_NAME}.tar.gz"

    info "Downloading ${BINARY_NAME} ${version} for ${platform}..."

    if ! curl -L --progress-bar -o "$temp_file" "$download_url"; then
        rm -rf "$temp_dir"
        error "Failed to download from: $download_url"
    fi

    info "Extracting binary..."
    if ! tar -xzf "$temp_file" -C "$temp_dir"; then
        rm -rf "$temp_dir"
        error "Failed to extract archive"
    fi

    local binary_path
    binary_path=$(find "$temp_dir" -name "$BINARY_NAME" -type f | head -1)

    if [ -z "$binary_path" ]; then
        rm -rf "$temp_dir"
        error "Binary not found in archive"
    fi

    if [ "$dry_run" = true ]; then
        info "DRY-RUN: Would install binary to ${INSTALL_DIR}/${BINARY_NAME}"
        info "DRY-RUN: Binary found at: $binary_path"

        if "$binary_path" --help >/dev/null 2>&1; then
            info "DRY-RUN: Binary test successful"
        else
            warn "DRY-RUN: Binary test failed - may not be compatible with this system"
        fi

        rm -rf "$temp_dir"
        return 0
    fi

    mkdir -p "$INSTALL_DIR"

    info "Installing to ${INSTALL_DIR}/${BINARY_NAME}..."
    mv "$binary_path" "${INSTALL_DIR}/${BINARY_NAME}"
    chmod +x "${INSTALL_DIR}/${BINARY_NAME}"

    rm -rf "$temp_dir"
}

check_path() {
    if [[ ":$PATH:" != *":${INSTALL_DIR}:"* ]]; then
        warn "${INSTALL_DIR} is not in your PATH"
        echo ""
        echo "Add it to your PATH by adding this line to your shell profile:"
        echo ""

        if [ -n "$ZSH_VERSION" ]; then
            echo "  echo 'export PATH=\"\$PATH:${INSTALL_DIR}\"' >> ~/.zshrc"
            echo "  source ~/.zshrc"
        elif [ -n "$BASH_VERSION" ]; then
            echo "  echo 'export PATH=\"\$PATH:${INSTALL_DIR}\"' >> ~/.bashrc"
            echo "  source ~/.bashrc"
        else
            echo "  export PATH=\"\$PATH:${INSTALL_DIR}\""
        fi
        echo ""
    fi
}

uninstall_appveyor() {
    echo "==================================="
    echo "  AppVeyor CLI Uninstaller"
    echo "==================================="
    echo ""

    local binary_path="${INSTALL_DIR}/${BINARY_NAME}"
    local config_dir="${HOME}/.appveyor"
    local removed_something=false

    if [ -f "$binary_path" ]; then
        info "Removing binary from $binary_path"
        rm "$binary_path" || error "Failed to remove binary"
        info "Binary removed successfully"
        removed_something=true
    else
        warn "Binary not found at $binary_path"
    fi

    if [ -d "$config_dir" ]; then
        echo ""
        echo -n "Remove configuration directory $config_dir? [y/N]: "
        read -r response
        if [[ "$response" =~ ^[Yy]$ ]]; then
            rm -rf "$config_dir" || error "Failed to remove configuration directory"
            info "Configuration directory removed"
            removed_something=true
        else
            info "Configuration directory preserved"
        fi
    fi

    echo ""
    if [ "$removed_something" = true ]; then
        info "Uninstall completed successfully"
    else
        warn "Nothing was removed - AppVeyor CLI may not have been installed"
    fi
}

main() {
    local dry_run=false
    local include_beta=false
    local uninstall=false

    for arg in "$@"; do
        case $arg in
            --dry-run|--dry|-n)
                dry_run=true
                ;;
            --beta|--pre|--prerelease)
                include_beta=true
                ;;
            --uninstall|--remove)
                uninstall=true
                ;;
            --help|-h)
                echo "Usage: $0 [OPTIONS]"
                echo ""
                echo "Options:"
                echo "  --dry-run, -n       Download and verify but don't install"
                echo "  --beta, --pre       Include beta/pre-release versions"
                echo "  --uninstall         Remove AppVeyor CLI and optionally config"
                echo "  --help, -h          Show this help message"
                echo ""
                echo "Environment variables:"
                echo "  INSTALL_DIR         Set custom installation directory (default: ~/.local/bin)"
                exit 0
                ;;
        esac
    done

    if [ "$uninstall" = true ]; then
        uninstall_appveyor
        exit 0
    fi

    echo "==================================="
    echo "  AppVeyor CLI Installer"
    echo "==================================="
    echo ""

    if [ "$dry_run" = true ]; then
        warn "Running in DRY-RUN mode - will download but not install"
        echo ""
    fi

    check_requirements

    local platform
    platform=$(detect_platform)
    info "Detected platform: ${platform}"

    if [ "$include_beta" = true ]; then
        info "Fetching latest release information (including pre-releases)..."
    else
        info "Fetching latest stable release information..."
    fi
    local version
    version=$(get_latest_version "$include_beta")
    info "Latest version: ${version}"

    if [ "$include_beta" = true ] && [[ "$version" =~ (beta|rc|alpha|preview) ]]; then
        warn "Installing pre-release version: ${version}"
    fi

    if [ -f "${INSTALL_DIR}/${BINARY_NAME}" ]; then
        warn "Existing installation found"

        read -p "Do you want to overwrite it? [y/N] " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            echo "Installation cancelled"
            exit 0
        fi
    fi

    install_binary "$version" "$platform" "$dry_run"

    if [ "$dry_run" = true ]; then
        echo ""
        info "DRY-RUN complete! No changes were made to your system."
        info "To actually install, run without --dry-run flag"
    else
        if "${INSTALL_DIR}/${BINARY_NAME}" --help >/dev/null 2>&1; then
            info "Successfully installed ${BINARY_NAME} ${version}"
        else
            error "Installation verification failed"
        fi

        check_path

        echo ""
        echo "Installation complete!"
        echo ""
        echo "Run '${BINARY_NAME} --help' to get started"
    fi
}

main "$@"
