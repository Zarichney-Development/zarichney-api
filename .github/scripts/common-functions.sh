#!/bin/bash

# Common Functions for CI/CD Pipeline Scripts
# Version: 1.0
# Last Updated: 2025-07-27

set -euo pipefail

# Colors for output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}ℹ️  INFO:${NC} $*" >&2
}

log_success() {
    echo -e "${GREEN}✅ SUCCESS:${NC} $*" >&2
}

log_warning() {
    echo -e "${YELLOW}⚠️  WARNING:${NC} $*" >&2
}

log_error() {
    echo -e "${RED}❌ ERROR:${NC} $*" >&2
}

log_section() {
    echo >&2
    echo -e "${BLUE}==== $* ====${NC}" >&2
    echo >&2
}

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check required environment variables
check_required_env() {
    local vars=("$@")
    local missing=()
    
    for var in "${vars[@]}"; do
        if [[ -z "${!var:-}" ]]; then
            missing+=("$var")
        fi
    done
    
    if [[ ${#missing[@]} -gt 0 ]]; then
        log_error "Missing required environment variables: ${missing[*]}"
        return 1
    fi
}

# Check required tools
check_required_tools() {
    local tools=("$@")
    local missing=()
    
    for tool in "${tools[@]}"; do
        if ! command_exists "$tool"; then
            missing+=("$tool")
        fi
    done
    
    if [[ ${#missing[@]} -gt 0 ]]; then
        log_error "Missing required tools: ${missing[*]}"
        return 1
    fi
}

# Docker group membership check (for integration tests)
check_docker_access() {
    if command_exists docker; then
        if docker info >/dev/null 2>&1; then
            log_info "Docker access confirmed"
            return 0
        else
            log_warning "Docker daemon not accessible, trying with 'sg docker'"
            if sg docker -c "docker info" >/dev/null 2>&1; then
                log_info "Docker access confirmed via 'sg docker'"
                export DOCKER_PREFIX="sg docker -c"
                return 0
            else
                local sg_exit_code=$?
                if [[ $sg_exit_code -eq 123 ]]; then
                    log_warning "Docker group switching failed (exit 123), proceeding without Docker access"
                    return 1
                else
                    log_error "Docker access failed. Please ensure Docker is running and user has proper permissions"
                    return 1
                fi
            fi
        fi
    else
        log_error "Docker not found"
        return 1
    fi
}

# Run command with optional Docker prefix
run_with_docker() {
    if [[ -n "${DOCKER_PREFIX:-}" ]]; then
        if eval "$DOCKER_PREFIX \"$*\""; then
            return 0
        else
            local exit_code=$?
            if [[ $exit_code -eq 123 ]]; then
                log_warning "Docker group switching failed (exit 123), falling back to direct execution"
                eval "$*"
            else
                return $exit_code
            fi
        fi
    else
        eval "$*"
    fi
}

# Create artifact directory
create_artifact_dir() {
    local dir="${1:-artifacts}"
    mkdir -p "$dir"
    echo "ARTIFACT_DIR=$dir" >> "${GITHUB_OUTPUT:-/dev/null}"
    log_info "Artifact directory created: $dir"
}

# Upload artifacts (GitHub Actions specific)
upload_artifact() {
    local name="$1"
    local path="$2"
    local retention="${3:-30}"
    
    if [[ -n "${GITHUB_ACTIONS:-}" ]]; then
        log_info "Uploading artifact: $name from $path"
        # This will be handled by workflow using actions/upload-artifact
        echo "UPLOAD_ARTIFACT_NAME=$name" >> "${GITHUB_OUTPUT:-/dev/null}"
        echo "UPLOAD_ARTIFACT_PATH=$path" >> "${GITHUB_OUTPUT:-/dev/null}"
        echo "UPLOAD_ARTIFACT_RETENTION=$retention" >> "${GITHUB_OUTPUT:-/dev/null}"
    else
        log_info "Local execution: Artifact $name would be uploaded from $path"
    fi
}

# Download artifacts (GitHub Actions specific)
download_artifact() {
    local name="$1"
    local path="${2:-.}"
    
    if [[ -n "${GITHUB_ACTIONS:-}" ]]; then
        log_info "Downloading artifact: $name to $path"
        # This will be handled by workflow using actions/download-artifact
        echo "DOWNLOAD_ARTIFACT_NAME=$name" >> "${GITHUB_OUTPUT:-/dev/null}"
        echo "DOWNLOAD_ARTIFACT_PATH=$path" >> "${GITHUB_OUTPUT:-/dev/null}"
    else
        log_info "Local execution: Artifact $name would be downloaded to $path"
    fi
}

# Exit with error message
die() {
    log_error "$*"
    exit 1
}

# Performance tracking
start_timer() {
    echo $SECONDS
}

end_timer() {
    local start_time="$1"
    local operation="${2:-operation}"
    local duration=$((SECONDS - start_time))
    log_info "$operation completed in ${duration}s"
    echo "DURATION_${operation//-/_}=$duration" >> "${GITHUB_OUTPUT:-/dev/null}"
}

# Git utilities
get_changed_files() {
    local base_ref="${1:-origin/develop}"
    local head_ref="${2:-HEAD}"
    
    git diff --name-only "$base_ref" "$head_ref" 2>/dev/null || echo ""
}

has_changes_in_path() {
    local path_pattern="$1"
    local base_ref="${2:-origin/develop}"
    local head_ref="${3:-HEAD}"
    
    local changed_files
    changed_files=$(get_changed_files "$base_ref" "$head_ref")
    
    if [[ -z "$changed_files" ]]; then
        return 1
    fi
    
    echo "$changed_files" | grep -E "$path_pattern" >/dev/null
}

# Quality gates
check_exit_code() {
    local exit_code="$1"
    local operation="$2"
    
    if [[ $exit_code -eq 0 ]]; then
        log_success "$operation completed successfully"
        return 0
    else
        log_error "$operation failed with exit code $exit_code"
        return $exit_code
    fi
}

# Initialize common environment
init_pipeline() {
    log_section "Initializing Pipeline Environment"
    
    # Set default values
    export DOTNET_VERSION="${DOTNET_VERSION:-8.0.x}"
    export NODE_VERSION="${NODE_VERSION:-18.x}"
    export AWS_REGION="${AWS_REGION:-us-east-2}"
    
    # Check basic tools
    check_required_tools git
    
    log_info "Pipeline environment initialized"
    log_info "Working directory: $(pwd)"
    log_info "Git branch: $(git branch --show-current 2>/dev/null || echo 'unknown')"
    log_info "Git commit: $(git rev-parse --short HEAD 2>/dev/null || echo 'unknown')"
}

# Cleanup function
cleanup() {
    log_section "Cleanup"
    # Remove temporary files, stop background processes, etc.
    # This function should be called in trap handlers
}

# Trap handler for cleanup
trap cleanup EXIT