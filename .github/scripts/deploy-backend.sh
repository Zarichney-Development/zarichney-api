#!/bin/bash

# Backend Deployment Script
# Extracted from .github/workflows/main.yml for maintainability
# Version: 1.0
# Last Updated: 2025-07-27

# Source common functions
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common-functions.sh"

# Script configuration
readonly SERVER_PROJECT="Code/Zarichney.Server"
readonly EC2_USERNAME="ec2-user"
readonly EC2_APP_PATH="/opt/cookbook-api"

# Help function
show_help() {
    cat << EOF
Backend Deployment Script

USAGE:
    $0 [OPTIONS] [TARGET]

TARGETS:
    production      Deploy to production environment [default]
    staging         Deploy to staging environment
    dev             Deploy to development environment

OPTIONS:
    --skip-migrations   Skip database migration application
    --skip-health       Skip health checks after deployment
    --rollback          Rollback to previous deployment
    --dry-run           Show what would be deployed without making changes
    --config CONFIG     Build configuration (default: Release)
    --help              Show this help message

ENVIRONMENT VARIABLES:
    AWS_REGION              AWS region for deployment
    EC2_HOST_BACKEND        Backend EC2 instance hostname
    EC2_SSH_KEY             SSH private key for EC2 access
    SECRET_ID               AWS Secrets Manager secret ID
    SECRET_DB_PASSWORD_KEY  Key for database password in secrets
    DOTNET_VERSION          .NET version to use

EXAMPLES:
    $0                      # Deploy to production with migrations
    $0 staging              # Deploy to staging environment
    $0 --skip-migrations    # Deploy without running migrations
    $0 --dry-run production # Preview deployment actions
EOF
}

# Parse command line arguments
TARGET_ENV="production"
SKIP_MIGRATIONS=false
SKIP_HEALTH=false
ROLLBACK=false
DRY_RUN=false
BUILD_CONFIG="Release"

while [[ $# -gt 0 ]]; do
    case $1 in
        production|staging|dev)
            TARGET_ENV="$1"
            shift
            ;;
        --skip-migrations)
            SKIP_MIGRATIONS=true
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
        --config)
            BUILD_CONFIG="$2"
            shift 2
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
    
    log_section "Backend Deployment to $TARGET_ENV"
    
    # Validate deployment preconditions
    validate_deployment_prereqs
    
    # Handle rollback if requested
    if [[ "$ROLLBACK" == "true" ]]; then
        perform_rollback
        return 0
    fi
    
    # Setup .NET environment for publishing
    setup_dotnet_environment
    
    # Generate EF migrations script
    if [[ "$SKIP_MIGRATIONS" != "true" ]]; then
        generate_migrations_script
    fi
    
    # Publish application
    publish_application
    
    # Deploy to EC2
    deploy_to_ec2
    
    # Run health checks unless skipped
    if [[ "$SKIP_HEALTH" != "true" ]]; then
        run_health_checks
    fi
    
    # Create deployment record
    create_deployment_record
    
    end_timer "$start_time" "backend-deployment"
    log_success "Backend deployment to $TARGET_ENV completed successfully"
}

validate_deployment_prereqs() {
    log_section "Validating Deployment Prerequisites"
    
    # Check required tools
    check_required_tools dotnet aws ssh scp
    
    # Check required environment variables
    local required_vars=("AWS_REGION" "EC2_HOST_BACKEND" "EC2_SSH_KEY" "DOTNET_VERSION")

    if [[ "$SKIP_MIGRATIONS" != "true" ]]; then
        required_vars+=("SECRET_ID" "SECRET_DB_PASSWORD_KEY" "DB_HOST" "DB_USER" "DB_NAME")
    fi
    
    check_required_env "${required_vars[@]}"
    
    # Verify AWS credentials
    log_info "Verifying AWS credentials..."
    if ! aws sts get-caller-identity >/dev/null 2>&1; then
        die "AWS credentials not configured or invalid"
    fi
    
    # Verify server project exists
    if [[ ! -d "$SERVER_PROJECT" ]]; then
        die "Server project directory not found: $SERVER_PROJECT"
    fi
    
    if [[ ! -f "$SERVER_PROJECT/Zarichney.Server.csproj" ]]; then
        die "Server project file not found: $SERVER_PROJECT/Zarichney.Server.csproj"
    fi
    
    log_success "Prerequisites validation passed"
}

setup_dotnet_environment() {
    log_section "Setting up .NET Environment for Publishing"
    
    # Install EF Core tools with retry
    log_info "Installing/updating EF Core tools..."
    for attempt in {1..3}; do
        if dotnet tool update --global dotnet-ef --version 8.*; then
            log_success "EF Core tools updated successfully"
            break
        else
            log_warning "EF Core tools update attempt $attempt failed, retrying in 5 seconds..."
            sleep 5
        fi
        
        if [[ $attempt -eq 3 ]]; then
            die "Failed to update EF Core tools after 3 attempts"
        fi
    done
    
    # Verify EF tools are working
    if ! dotnet ef --version >/dev/null 2>&1; then
        die "EF Core tools not working properly"
    fi
    
    log_success ".NET environment setup completed"
}

generate_migrations_script() {
    log_section "Generating EF Migrations Script"
    
    # Create migrations directory
    mkdir -p ./publish/migrations
    
    log_info "Generating idempotent migrations script..."
    
    if [[ "$DRY_RUN" == "true" ]]; then
        log_info "DRY RUN: Would generate migrations script"
        return 0
    fi
    
    # Generate migrations script
    if dotnet ef migrations script \
        --context UserDbContext \
        --project "$SERVER_PROJECT/Zarichney.Server.csproj" \
        -o ./publish/migrations/ApplyAllMigrations.sql \
        --idempotent; then
        log_success "Migrations script generated successfully"
    else
        die "Failed to generate migrations script"
    fi
    
    # Verify script was created
    if [[ ! -f "./publish/migrations/ApplyAllMigrations.sql" ]]; then
        die "Migrations script file not found after generation"
    fi
    
    log_info "Migrations script size: $(wc -l < ./publish/migrations/ApplyAllMigrations.sql) lines"
}

publish_application() {
    log_section "Publishing Application"
    
    log_info "Publishing $SERVER_PROJECT in $BUILD_CONFIG configuration..."
    
    if [[ "$DRY_RUN" == "true" ]]; then
        log_info "DRY RUN: Would publish application to ./publish"
        return 0
    fi
    
    # Publish application
    if dotnet publish "$SERVER_PROJECT/Zarichney.Server.csproj" -c "$BUILD_CONFIG" -o ./publish; then
        log_success "Application published successfully"
    else
        die "Application publishing failed"
    fi

    # After successful publish, copy necessary scripts
    log_info "Copying startup and maintenance scripts..."
    cp Scripts/start-server.sh ./publish/
    cp Scripts/cleanup-playwright.sh ./publish/
    log_success "Scripts copied to publish directory"
    
    # Verify publish output
    if [[ ! -d "./publish" ]]; then
        die "Publish directory not found after publishing"
    fi
    
    log_info "Published files:"
    ls -la ./publish/ | head -10 || true
    
    # Display publish directory size
    local publish_size
    publish_size=$(du -sh ./publish 2>/dev/null | cut -f1)
    log_info "Published application size: $publish_size"
}

deploy_to_ec2() {
    log_section "Deploying to EC2 Instance"
    
    # Setup SSH key
    setup_ssh_key
    
    # Deploy files to EC2
    deploy_files_to_ec2
    
    # Apply migrations and restart service
    deploy_application_on_ec2
    
    # Cleanup SSH key
    cleanup_ssh_key
}

setup_ssh_key() {
    log_info "Setting up SSH key for EC2 access..."
    
    echo "$EC2_SSH_KEY" > private_key
    chmod 600 private_key
    
    # Test SSH connection
    if ! ssh -o StrictHostKeyChecking=no -o ConnectTimeout=10 -i private_key "${EC2_USERNAME}@${EC2_HOST_BACKEND}" "echo 'Connection test successful'" >/dev/null 2>&1; then
        log_warning "SSH connection test failed, but continuing..."
    fi
}

deploy_files_to_ec2() {
    log_info "Deploying files to EC2 instance..."
    
    if [[ "$DRY_RUN" == "true" ]]; then
        log_info "DRY RUN: Would deploy files to ${EC2_HOST_BACKEND}:${EC2_APP_PATH}"
        return 0
    fi
    
    # Deploy files with retry
    for attempt in {1..3}; do
        log_info "File deployment attempt $attempt/3"
        
        if scp -r -o StrictHostKeyChecking=no -o ConnectTimeout=30 -i private_key ./publish/* "${EC2_USERNAME}@${EC2_HOST_BACKEND}:${EC2_APP_PATH}/"; then
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
        log_info "DRY RUN: Would apply migrations and restart service on EC2"
        return 0
    fi
    
    # Create the deployment script content
    local deployment_script
    deployment_script=$(cat << 'EOF'
set -e # Exit on error

echo "🔐 Retrieving database password from Secrets Manager..."
for i in {1..3}; do
    DB_PASSWORD=$(aws secretsmanager get-secret-value \
        --secret-id "$SECRET_ID" \
        --region "$AWS_REGION" \
        --query SecretString --output text | \
        jq -r ".$SECRET_DB_PASSWORD_KEY" 2>/dev/null)
    
    if [ -n "$DB_PASSWORD" ] && [ "$DB_PASSWORD" != "null" ]; then
        echo "✅ Database password retrieved successfully"
        break
    else
        echo "⚠️ DB password retrieval attempt $i failed, retrying in 5 seconds..."
        sleep 5
    fi
done

if [ -z "$DB_PASSWORD" ] || [ "$DB_PASSWORD" == "null" ]; then
    echo "❌ ERROR: Failed to retrieve database password after retries."
    exit 1
fi

export PGPASSWORD="$DB_PASSWORD"

echo "📊 Applying database migrations..."
cd /opt/cookbook-api

# Ensure scripts have execute permissions
echo "🔧 Setting script permissions..."
chmod +x /opt/cookbook-api/start-server.sh 2>/dev/null || true
chmod +x /opt/cookbook-api/cleanup-playwright.sh 2>/dev/null || true

# Check for migrations file in the correct location
if [ -f "migrations/ApplyAllMigrations.sql" ]; then
    echo "🔄 Running migrations script..."
    echo "🔍 Testing database connectivity..."

    # Test database connectivity with retry
    for i in {1..3}; do
        echo "🔌 Database connection attempt $i/3 to $DB_HOST..."
        if nslookup "$DB_HOST" >/dev/null 2>&1; then
            echo "✅ DNS resolution successful for $DB_HOST"
            break
        else
            echo "❌ DNS resolution failed for $DB_HOST, retrying in 5 seconds..."
            sleep 5
            if [ $i -eq 3 ]; then
                echo "💥 ERROR: Cannot resolve database hostname $DB_HOST after 3 attempts"
                exit 1
            fi
        fi
    done

    # Apply migrations with proper error handling
    if psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -f migrations/ApplyAllMigrations.sql; then
        echo "✅ Migrations applied successfully"
    else
        echo "❌ ERROR: Failed to apply migrations to database"
        echo "Database details: Host=$DB_HOST, User=$DB_USER, Database=$DB_NAME"
        exit 1
    fi
else
    echo "⚠️ No migrations script found at migrations/ApplyAllMigrations.sql, skipping..."
fi

echo "🔄 Restarting cookbook-api service..."
sudo systemctl restart cookbook-api

echo "⏳ Waiting for service to start..."
sleep 10

# Check service status
if systemctl is-active --quiet cookbook-api; then
    echo "✅ Service started successfully"
    sudo systemctl status cookbook-api --no-pager -l
else
    echo "❌ Service failed to start"
    sudo systemctl status cookbook-api --no-pager -l
    sudo journalctl -u cookbook-api --no-pager -l --lines=20
    exit 1
fi
EOF
    )
    
    # Execute deployment script on EC2
    ssh -o StrictHostKeyChecking=no -o ConnectTimeout=30 -i private_key "${EC2_USERNAME}@${EC2_HOST_BACKEND}" \
        "SECRET_ID='$SECRET_ID' AWS_REGION='$AWS_REGION' SECRET_DB_PASSWORD_KEY='$SECRET_DB_PASSWORD_KEY' DB_HOST='$DB_HOST' DB_USER='$DB_USER' DB_NAME='$DB_NAME' bash -s" <<< "$deployment_script"
    
    log_success "Application deployment completed"
}

cleanup_ssh_key() {
    log_info "Cleaning up SSH key..."
    rm -f private_key
}

run_health_checks() {
    log_section "Running Health Checks"

    # Determine base URL and health endpoints based on environment
    local base_url health_url status_url
    case "$TARGET_ENV" in
        "production")
            base_url="https://zarichney.com"
            ;;
        "staging")
            base_url="https://staging-zarichney.com"
            ;;
        "dev")
            base_url="http://${EC2_HOST_BACKEND}:5000"
            ;;
        *)
            log_warning "Unknown environment: $TARGET_ENV, skipping health checks"
            return 0
            ;;
    esac

    health_url="$base_url/api/health"
    status_url="$base_url/api/status"

    log_info "Running health checks against:"
    log_info "  - Basic health: $health_url"
    log_info "  - Status check: $status_url"

    # Wait for application to be ready
    sleep 15

    # Health check with retry
    for attempt in {1..6}; do
        log_info "Health check attempt $attempt/6"

        # Test basic health endpoint
        if curl -f -s -o /dev/null --max-time 30 "$health_url"; then
            log_success "Basic health check passed"

            # Test comprehensive status endpoint
            if curl -f -s -o /dev/null --max-time 30 "$status_url"; then
                log_success "Status check passed"

                # Get detailed health/status info
                local health_info status_info
                health_info=$(curl -s --max-time 10 "$health_url" 2>/dev/null || echo "Health details unavailable")
                status_info=$(curl -s --max-time 10 "$status_url" 2>/dev/null || echo "Status details unavailable")

                log_info "Health details: $health_info"
                log_info "Status details: $status_info"
                log_success "All health checks passed successfully"

                return 0
            else
                log_warning "Status check failed"
            fi
        else
            log_warning "Basic health check failed"
        fi

        if [[ $attempt -lt 6 ]]; then
            log_info "Retrying in 20 seconds..."
            sleep 20
        fi
    done

    log_error "Health checks failed after 6 attempts"
    log_error "Both /api/health and /api/status endpoints must respond with HTTP 200"
    return 1
}

perform_rollback() {
    log_section "Performing Rollback"
    
    log_warning "Rollback functionality not yet implemented"
    log_info "To rollback manually:"
    log_info "1. SSH to EC2 instance: ssh -i private_key ${EC2_USERNAME}@${EC2_HOST_BACKEND}"
    log_info "2. Stop service: sudo systemctl stop cookbook-api"
    log_info "3. Restore previous application files in ${EC2_APP_PATH}"
    log_info "4. Restore previous database state if needed"
    log_info "5. Start service: sudo systemctl start cookbook-api"
    
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
    "deployment_type": "backend",
    "build_config": "$BUILD_CONFIG",
    "ec2_host": "$EC2_HOST_BACKEND",
    "app_path": "$EC2_APP_PATH",
    "deployment_options": {
        "skip_migrations": $SKIP_MIGRATIONS,
        "skip_health": $SKIP_HEALTH
    }
}
EOF
    )
    
    # Save deployment record
    mkdir -p artifacts/backend 2>/dev/null || true
    echo "$deployment_record" > "artifacts/backend/deployment-record.json" 2>/dev/null || true
    
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