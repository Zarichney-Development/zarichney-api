#!/bin/bash

# Frontend Build Script
# Extracted from .github/workflows/main.yml for maintainability
# Version: 1.0
# Last Updated: 2025-07-27

# Source common functions
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common-functions.sh"

# Script configuration
readonly FRONTEND_DIR="Code/Zarichney.Website"
readonly BUILD_COMMAND="npm run build-prod"

# Help function
show_help() {
    cat << EOF
Frontend Build Script

USAGE:
    $0 [OPTIONS]

OPTIONS:
    --dev               Build in development mode (npm run build)
    --prod              Build in production mode (npm run build-prod) [default]
    --skip-tests        Skip running frontend tests
    --skip-lint         Skip linting checks
    --clean             Clean node_modules and package-lock.json before build
    --help              Show this help message

ENVIRONMENT VARIABLES:
    NODE_VERSION        Node.js version to use (default: 18.x)
    NPM_CACHE_DIR       NPM cache directory
    CI_ENVIRONMENT      Set to true in CI/CD environment

EXAMPLES:
    $0                  # Standard production build
    $0 --dev            # Development build
    $0 --clean --prod   # Clean build in production mode
EOF
}

# Parse command line arguments
BUILD_MODE="prod"
SKIP_TESTS=false
SKIP_LINT=false
CLEAN_BUILD=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --dev)
            BUILD_MODE="dev"
            shift
            ;;
        --prod)
            BUILD_MODE="prod"
            shift
            ;;
        --skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        --skip-lint)
            SKIP_LINT=true
            shift
            ;;
        --clean)
            CLEAN_BUILD=true
            shift
            ;;
        --help)
            show_help
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Main execution
main() {
    local start_time
    start_time=$(start_timer)
    
    # Initialize environment
    init_pipeline
    
    log_section "Frontend Build Execution"
    
    # Check prerequisites
    check_required_tools node npm git
    check_required_env NODE_VERSION
    
    # Verify frontend directory exists
    if [[ ! -d "$FRONTEND_DIR" ]]; then
        die "Frontend directory not found: $FRONTEND_DIR"
    fi
    
    # Create artifact directory
    create_artifact_dir "artifacts/frontend"
    
    # Setup Node.js environment
    setup_node_environment
    
    # Install dependencies
    install_dependencies
    
    # Run linting if not skipped
    if [[ "$SKIP_LINT" != "true" ]]; then
        run_linting
    fi
    
    # Run tests if not skipped
    if [[ "$SKIP_TESTS" != "true" ]]; then
        run_frontend_tests
    fi
    
    # Build the application
    build_frontend
    
    # Analyze build results
    analyze_build_output
    
    # Create build artifacts
    create_frontend_artifacts
    
    end_timer "$start_time" "frontend-build"
    log_success "Frontend build completed successfully"
}

setup_node_environment() {
    log_section "Setting up Node.js Environment"
    
    # Display Node.js and npm versions
    log_info "Node.js version: $(node --version)"
    log_info "npm version: $(npm --version)"
    
    # Set npm configuration for CI environment
    if [[ "${CI_ENVIRONMENT:-false}" == "true" ]]; then
        npm config set fund false
        npm config set audit-level moderate
    fi
    
    # Change to frontend directory
    cd "$FRONTEND_DIR" || die "Failed to change to frontend directory"
    log_info "Working directory: $(pwd)"
}

install_dependencies() {
    log_section "Installing Frontend Dependencies"
    
    # Clean build if requested
    if [[ "$CLEAN_BUILD" == "true" ]]; then
        log_info "Cleaning previous build artifacts..."
        npm cache clean --force || true
        rm -rf node_modules package-lock.json || true
    fi
    
    # Install dependencies with retry logic
    log_info "Installing npm dependencies with retry logic..."
    
    for attempt in {1..3}; do
        log_info "Dependency installation attempt $attempt/3"
        
        if npm ci --legacy-peer-deps; then
            log_success "Dependencies installed successfully"
            return 0
        else
            log_warning "npm install attempt $attempt failed"
            
            if [[ $attempt -lt 3 ]]; then
                log_info "Retrying in 10 seconds after cleanup..."
                sleep 10
                npm cache clean --force || true
                rm -rf node_modules package-lock.json || true
            fi
        fi
    done
    
    die "Failed to install dependencies after 3 attempts"
}

run_linting() {
    log_section "Running Frontend Linting"
    
    # Check if lint script exists
    if npm run --silent | grep -q "lint"; then
        log_info "Running linting checks..."
        if npm run lint; then
            log_success "Linting passed"
        else
            log_error "Linting failed"
            return 1
        fi
    else
        log_warning "No lint script found in package.json, skipping..."
    fi
}

run_frontend_tests() {
    log_section "Running Frontend Tests"
    
    # Check if test script exists
    if npm run --silent | grep -q "test"; then
        log_info "Running frontend tests..."
        
        # Set test environment
        export CI=true
        
        if npm run test -- --watch=false --browsers=ChromeHeadless; then
            log_success "Frontend tests passed"
        else
            log_error "Frontend tests failed"
            return 1
        fi
    else
        log_warning "No test script found in package.json, skipping..."
    fi
}

build_frontend() {
    log_section "Building Frontend Application"
    
    # Determine build command based on mode
    local build_cmd
    case "$BUILD_MODE" in
        "dev")
            build_cmd="npm run build"
            ;;
        "prod")
            build_cmd="npm run build-prod"
            ;;
        *)
            die "Unknown build mode: $BUILD_MODE"
            ;;
    esac
    
    log_info "Building frontend in $BUILD_MODE mode..."
    log_info "Command: $build_cmd"
    
    if eval "$build_cmd"; then
        log_success "Frontend build completed successfully"
    else
        die "Frontend build failed"
    fi
}

analyze_build_output() {
    log_section "Analyzing Build Output"
    
    if [[ ! -d "dist" ]]; then
        log_warning "Build output directory 'dist' not found"
        return 1
    fi
    
    log_info "Build artifacts created:"
    ls -la dist/ || true
    
    # Analyze bundle sizes
    if [[ -d "dist/browser" ]]; then
        log_info "Bundle sizes:"
        du -sh dist/browser/* 2>/dev/null | head -10 || true
        
        # Check for large bundles (warning threshold: 5MB)
        local large_files
        large_files=$(find dist/browser -name "*.js" -size +5M 2>/dev/null || true)
        
        if [[ -n "$large_files" ]]; then
            log_warning "Large JavaScript bundles detected (>5MB):"
            echo "$large_files" | while read -r file; do
                log_warning "  $(du -h "$file" 2>/dev/null || echo "Unknown size") - $file"
            done
        fi
    fi
    
    # Calculate total build size
    if command_exists du; then
        local total_size
        total_size=$(du -sh dist 2>/dev/null | cut -f1)
        log_info "Total build size: $total_size"
        
        # Set GitHub Actions output
        if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
            echo "build_size=$total_size" >> "$GITHUB_OUTPUT"
        fi
    fi
}

create_frontend_artifacts() {
    log_section "Creating Frontend Artifacts"
    
    local artifact_dir="../../artifacts/frontend"
    
    # Copy build output
    if [[ -d "dist" ]]; then
        cp -r dist "$artifact_dir/" || die "Failed to copy build output"
        log_success "Build output copied to artifacts"
    else
        die "No build output found to copy"
    fi
    
    # Copy essential project files
    for file in package.json package-lock.json; do
        if [[ -f "$file" ]]; then
            cp "$file" "$artifact_dir/" || log_warning "Failed to copy $file"
        fi
    done
    
    # Copy scripts directory if it exists
    if [[ -d "scripts" ]]; then
        cp -r scripts "$artifact_dir/" || log_warning "Failed to copy scripts directory"
    fi
    
    # Generate build info
    cat > "$artifact_dir/build-info.json" << EOF
{
    "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%S.%3NZ")",
    "commit_sha": "$(git rev-parse HEAD 2>/dev/null || echo 'unknown')",
    "branch": "$(git branch --show-current 2>/dev/null || echo 'unknown')",
    "build_mode": "$BUILD_MODE",
    "node_version": "$(node --version 2>/dev/null || echo 'unknown')",
    "npm_version": "$(npm --version 2>/dev/null || echo 'unknown')"
}
EOF
    
    # Generate dependency info
    if [[ -f "package-lock.json" ]]; then
        npm list --json > "$artifact_dir/dependencies.json" 2>/dev/null || true
    fi
    
    log_info "Frontend artifacts created in $artifact_dir"
    upload_artifact "frontend-build" "$artifact_dir"
}

# Error handling
set -euo pipefail
trap cleanup EXIT

# Check if script is being sourced or executed
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi