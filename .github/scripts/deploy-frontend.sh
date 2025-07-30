#!/bin/bash

# Frontend Deployment Script
# Extracted from .github/workflows/main.yml for maintainability
# Version: 1.0
# Last Updated: 2025-07-27

# Source common functions
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common-functions.sh"

# Script configuration
readonly FRONTEND_DIR="Code/Zarichney.Website"
readonly S3_BUCKET="static.zarichney.com"
readonly EC2_USERNAME="ec2-user"

# Help function
show_help() {
    cat << EOF
Frontend Deployment Script

USAGE:
    $0 [OPTIONS] [TARGET]

TARGETS:
    production      Deploy to production environment [default]
    staging         Deploy to staging environment
    dev             Deploy to development environment

OPTIONS:
    --s3-only       Deploy only to S3 (skip EC2)
    --ec2-only      Deploy only to EC2 (skip S3)
    --skip-health   Skip health checks after deployment
    --rollback      Rollback to previous deployment
    --dry-run       Show what would be deployed without making changes
    --help          Show this help message

ENVIRONMENT VARIABLES:
    AWS_REGION              AWS region for deployment
    EC2_HOST_FRONTEND       Frontend EC2 instance hostname
    EC2_SSH_KEY             SSH private key for EC2 access
    S3_BUCKET               S3 bucket for static assets (default: static.zarichney.com)

EXAMPLES:
    $0                      # Deploy to production
    $0 staging              # Deploy to staging environment
    $0 --s3-only            # Deploy only static assets to S3
    $0 --dry-run production # Preview deployment actions
EOF
}

# Parse command line arguments
TARGET_ENV="production"
S3_ONLY=false
EC2_ONLY=false
SKIP_HEALTH=false
ROLLBACK=false
DRY_RUN=false

while [[ $# -gt 0 ]]; do
    case $1 in
        production|staging|dev)
            TARGET_ENV="$1"
            shift
            ;;
        --s3-only)
            S3_ONLY=true
            shift
            ;;
        --ec2-only)
            EC2_ONLY=true
            shift
            ;;
        --skip-health)
            SKIP_HEALTH=true
            shift
            ;;
        --rollback)
            ROLLBACK=true
            shift
            ;;
        --dry-run)
            DRY_RUN=true
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
    
    log_section "Frontend Deployment to $TARGET_ENV"
    
    # Validate deployment preconditions
    validate_deployment_prereqs
    
    # Download build artifacts
    download_build_artifacts
    
    # Handle rollback if requested
    if [[ "$ROLLBACK" == "true" ]]; then
        perform_rollback
        return 0
    fi
    
    # Deploy to S3 unless EC2-only
    if [[ "$EC2_ONLY" != "true" ]]; then
        deploy_to_s3
    fi
    
    # Deploy to EC2 unless S3-only
    if [[ "$S3_ONLY" != "true" ]]; then
        deploy_to_ec2
    fi
    
    # Run health checks unless skipped
    if [[ "$SKIP_HEALTH" != "true" ]]; then
        run_health_checks
    fi
    
    # Create deployment record
    create_deployment_record
    
    end_timer "$start_time" "frontend-deployment"
    log_success "Frontend deployment to $TARGET_ENV completed successfully"
}

validate_deployment_prereqs() {
    log_section "Validating Deployment Prerequisites"
    
    # Check required tools
    check_required_tools aws ssh scp
    
    # Check required environment variables
    local required_vars=("AWS_REGION")
    
    if [[ "$S3_ONLY" != "true" ]]; then
        required_vars+=("EC2_HOST_FRONTEND" "EC2_SSH_KEY")
    fi
    
    check_required_env "${required_vars[@]}"
    
    # Verify AWS credentials
    log_info "Verifying AWS credentials..."
    if ! aws sts get-caller-identity >/dev/null 2>&1; then
        die "AWS credentials not configured or invalid"
    fi
    
    # Verify frontend directory exists
    if [[ ! -d "$FRONTEND_DIR" ]]; then
        die "Frontend directory not found: $FRONTEND_DIR"
    fi
    
    log_success "Prerequisites validation passed"
}

download_build_artifacts() {
    log_section "Downloading Build Artifacts"
    
    # In GitHub Actions, this would download from actions/download-artifact
    # For local execution, check for local build artifacts
    if [[ -n "${GITHUB_ACTIONS:-}" ]]; then
        log_info "Build artifacts should be downloaded by workflow"
        download_artifact "frontend-build-artifacts" "$FRONTEND_DIR/"
    else
        log_info "Local execution: Checking for build artifacts..."
        if [[ ! -d "$FRONTEND_DIR/dist" ]]; then
            log_warning "No build artifacts found. Running frontend build..."
            "$SCRIPT_DIR/build-frontend.sh" --prod
        fi
    fi
    
    # Verify artifacts exist
    if [[ ! -d "$FRONTEND_DIR/dist" ]]; then
        die "Build artifacts not found in $FRONTEND_DIR/dist"
    fi
    
    log_info "Build artifacts verified:"
    ls -la "$FRONTEND_DIR/dist/" || true
}

deploy_to_s3() {
    log_section "Deploying Static Assets to S3"
    
    cd "$FRONTEND_DIR" || die "Failed to change to frontend directory"
    
    local s3_bucket="${S3_BUCKET}"
    if [[ "$TARGET_ENV" != "production" ]]; then
        s3_bucket="${TARGET_ENV}-${S3_BUCKET}"
    fi
    
    log_info "Deploying to S3 bucket: s3://$s3_bucket"
    
    if [[ "$DRY_RUN" == "true" ]]; then
        log_info "DRY RUN: Would sync dist/browser to s3://$s3_bucket"
        aws s3 sync dist/browser "s3://$s3_bucket" --delete --dryrun
        return 0
    fi
    
    # Deploy with retry logic
    for attempt in {1..3}; do
        log_info "S3 deployment attempt $attempt/3"
        
        if aws s3 sync dist/browser "s3://$s3_bucket" --delete; then
            log_success "S3 deployment successful"
            return 0
        else
            log_warning "S3 deployment attempt $attempt failed"
            if [[ $attempt -lt 3 ]]; then
                log_info "Retrying in 15 seconds..."
                sleep 15
            fi
        fi
    done
    
    die "S3 deployment failed after 3 attempts"
}

deploy_to_ec2() {
    log_section "Deploying to EC2 Instance"
    
    cd "$FRONTEND_DIR" || die "Failed to change to frontend directory"
    
    # Prepare server files
    prepare_server_files
    
    # Setup SSH key
    setup_ssh_key
    
    # Deploy files to EC2
    deploy_files_to_ec2
    
    # Deploy application on EC2
    deploy_application_on_ec2
    
    # Cleanup SSH key
    cleanup_ssh_key
}

prepare_server_files() {
    log_info "Preparing server files for EC2 deployment..."
    
    mkdir -p server-deploy/scripts
    
    # Copy server files
    if [[ -d "dist/server" ]]; then
        cp -r dist/server/* server-deploy/
    else
        log_warning "No server files found in dist/server"
    fi
    
    # Copy package files
    for file in package.json package-lock.json; do
        if [[ -f "$file" ]]; then
            cp "$file" server-deploy/
        else
            log_warning "$file not found"
        fi
    done
    
    # Copy scripts if they exist
    if [[ -d "scripts" ]]; then
        cp -r scripts/* server-deploy/scripts/ || true
    fi
    
    log_success "Server files prepared"
}

setup_ssh_key() {
    log_info "Setting up SSH key for EC2 access..."
    
    echo "$EC2_SSH_KEY" > private_key
    chmod 600 private_key
    
    # Test SSH connection
    if ! ssh -o StrictHostKeyChecking=no -o ConnectTimeout=10 -i private_key "${EC2_USERNAME}@${EC2_HOST_FRONTEND}" "echo 'Connection test successful'" >/dev/null 2>&1; then
        log_warning "SSH connection test failed, but continuing..."
    fi
}

deploy_files_to_ec2() {
    log_info "Deploying files to EC2 instance..."
    
    if [[ "$DRY_RUN" == "true" ]]; then
        log_info "DRY RUN: Would deploy files to ${EC2_HOST_FRONTEND}"
        return 0
    fi
    
    # Deploy files with retry
    for attempt in {1..3}; do
        log_info "File deployment attempt $attempt/3"
        
        if scp -r -o StrictHostKeyChecking=no -o ConnectTimeout=30 -i private_key ./server-deploy/* "${EC2_USERNAME}@${EC2_HOST_FRONTEND}:~/app/"; then
            log_success "File deployment successful"
            return 0
        else
            log_warning "File deployment attempt $attempt failed"
            if [[ $attempt -lt 3 ]]; then
                log_info "Retrying in 10 seconds..."
                sleep 10
            fi
        fi
    done
    
    die "File deployment failed after 3 attempts"
}

deploy_application_on_ec2() {
    log_info "Deploying application on EC2..."
    
    if [[ "$DRY_RUN" == "true" ]]; then
        log_info "DRY RUN: Would restart application on EC2"
        return 0
    fi
    
    ssh -o StrictHostKeyChecking=no -o ConnectTimeout=30 -i private_key "${EC2_USERNAME}@${EC2_HOST_FRONTEND}" 'bash -s' <<'EOF'
set -e

echo "Changing to application directory..."
cd ~/app

echo "Installing dependencies..."
npm ci --omit=dev --legacy-peer-deps

echo "Restarting application..."
if pm2 list | grep -q "server"; then
    echo "Restarting existing PM2 process..."
    pm2 restart server
else
    echo "Starting new PM2 process..."
    pm2 start "npm run serve:ssr" --name server
fi

echo "Waiting for application to start..."
sleep 5

# Verify PM2 process is running
if pm2 list | grep -q "server.*online"; then
    echo "✅ Application started successfully"
else
    echo "❌ Application failed to start properly"
    pm2 logs server --lines 10
    exit 1
fi
EOF
    
    log_success "Application deployment completed"
}

cleanup_ssh_key() {
    log_info "Cleaning up SSH key..."
    rm -f private_key
}

run_health_checks() {
    log_section "Running Health Checks"
    
    # Determine health check URL based on environment
    local health_url
    case "$TARGET_ENV" in
        "production")
            health_url="https://zarichney.com/health"
            ;;
        "staging")
            health_url="https://staging.zarichney.com/health"
            ;;
        "dev")
            health_url="http://${EC2_HOST_FRONTEND}:3000/health"
            ;;
        *)
            log_warning "Unknown environment: $TARGET_ENV, skipping health checks"
            return 0
            ;;
    esac
    
    log_info "Running health check against: $health_url"
    
    # Wait for application to be ready
    sleep 10
    
    # Health check with retry
    for attempt in {1..5}; do
        log_info "Health check attempt $attempt/5"
        
        if curl -f -s -o /dev/null --max-time 30 "$health_url"; then
            log_success "Health check passed"
            return 0
        else
            log_warning "Health check attempt $attempt failed"
            if [[ $attempt -lt 5 ]]; then
                log_info "Retrying in 15 seconds..."
                sleep 15
            fi
        fi
    done
    
    log_error "Health checks failed after 5 attempts"
    return 1
}

perform_rollback() {
    log_section "Performing Rollback"
    
    log_warning "Rollback functionality not yet implemented"
    log_info "To rollback manually:"
    log_info "1. Restore previous S3 bucket contents"
    log_info "2. SSH to EC2 and restore previous application version"
    log_info "3. Restart PM2 processes"
    
    return 1
}

create_deployment_record() {
    log_section "Creating Deployment Record"
    
    local deployment_record
    deployment_record=$(cat << EOF
{
    "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%S.%3NZ")",
    "environment": "$TARGET_ENV",
    "commit_sha": "$(git rev-parse HEAD 2>/dev/null || echo 'unknown')",
    "branch": "$(git branch --show-current 2>/dev/null || echo 'unknown')",
    "deployed_by": "${GITHUB_ACTOR:-$(whoami)}",
    "deployment_type": "frontend",
    "s3_bucket": "${S3_BUCKET}",
    "ec2_host": "${EC2_HOST_FRONTEND:-'N/A'}",
    "deployment_options": {
        "s3_only": $S3_ONLY,
        "ec2_only": $EC2_ONLY,
        "skip_health": $SKIP_HEALTH
    }
}
EOF
    )
    
    # Save deployment record
    echo "$deployment_record" > "artifacts/frontend/deployment-record.json" 2>/dev/null || true
    
    # Set GitHub Actions output
    if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
        echo "deployment_timestamp=$(date -u +"%Y-%m-%dT%H:%M:%S.%3NZ")" >> "$GITHUB_OUTPUT"
        echo "deployment_environment=$TARGET_ENV" >> "$GITHUB_OUTPUT"
    fi
    
    log_info "Deployment record created"
}

# Error handling
set -euo pipefail
trap cleanup EXIT

# Check if script is being sourced or executed
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi