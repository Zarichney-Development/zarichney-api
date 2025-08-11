# Testing Environment Setup Guide

**Version:** 1.1
**Last Updated:** 2025-08-11

> **Parent:** [`README.md`](./README.md)

**Scope**: Local testing environment configuration and external service setup  
**Related**: [`../Maintenance/TestingSetup.md`](../Maintenance/TestingSetup.md) for CI/CD integration and maintenance procedures

## Purpose

This document provides comprehensive setup instructions for establishing testing environments that support the full zarichney-api test suite. It focuses on configuration requirements for external services and infrastructure dependencies needed to achieve optimal test pass rates.

## Environment Types

### Unconfigured Environment (Default)
- **Skip Rate**: 26.7% (23/86 tests)  
- **Description**: Local development or CI environment without external service configurations
- **Use Case**: Basic development, unit testing, minimal integration testing

### Configured Environment (Target)
- **Skip Rate**: 1.2% (1/86 tests)
- **Description**: Full external service integration with all dependencies available
- **Use Case**: Comprehensive integration testing, pre-production validation

### Production Environment
- **Skip Rate**: 0% tolerance for failures
- **Description**: Production deployment validation
- **Use Case**: Production readiness verification, deployment quality gates

## External Service Requirements

### OpenAI API Integration
**Tests Affected**: 6 tests in AI controller functionality

**Setup Requirements**:
1. Create OpenAI Platform account at https://platform.openai.com/
2. Generate test API key with usage limits
3. Obtain organization ID from account settings
4. Configure test models: `gpt-4o-mini` for completions, `whisper-1` for transcription

**Environment Variables**:
```bash
export OPENAI_API_KEY="sk-test-your-openai-api-key-here"
export OPENAI_ORG_ID="org-your-organization-id-here"
```

**Test Coverage**:
- Completion endpoints with text prompts
- Completion endpoints with audio prompts  
- Transcription services
- Service unavailable behavior validation

### Stripe Payment Processing
**Tests Affected**: 6 tests in payment controller functionality

**Setup Requirements**:
1. Create Stripe account at https://dashboard.stripe.com/
2. Enable test mode for all API operations
3. Generate test publishable and secret keys
4. Configure webhook endpoints for test environment
5. Use Stripe's test card numbers for payment testing

**Environment Variables**:
```bash
export STRIPE_SECRET_KEY="sk_test_your-stripe-secret-key"
export STRIPE_PUBLISHABLE_KEY="pk_test_your-stripe-publishable-key"
export STRIPE_WEBHOOK_SECRET="whsec_your-webhook-secret"
```

**Test Coverage**:
- Payment intent creation and management
- Payment status retrieval
- Checkout session functionality
- Service unavailable behavior validation

### Microsoft Graph Services
**Tests Affected**: 4 tests in email and identity services

**Setup Requirements**:
1. Register application in Azure Portal
2. Configure required Graph API permissions
3. Generate client secret for authentication
4. Use dedicated test tenant for safety

**Environment Variables**:
```bash
export MSGRAPH_TENANT_ID="your-azure-tenant-id"
export MSGRAPH_CLIENT_ID="your-azure-app-client-id"
export MSGRAPH_CLIENT_SECRET="your-azure-app-secret"
```

**Test Coverage**:
- Email service integration
- Identity provider connectivity
- Mail validation services

## Infrastructure Dependencies

### Database Configuration
**Tests Affected**: 6 tests in authentication and role management

**Setup Requirements**:
1. SQL Server instance (local or accessible connection)
2. Create test database named `ZarichneyTest`  
3. Configure connection string with integrated security or SQL authentication
4. Entity Framework handles automatic schema creation via code-first migrations

**Environment Variables**:
```bash
export CONNECTION_STRING_USER_DB="Data Source=localhost;Initial Catalog=ZarichneyTest;Integrated Security=true;TrustServerCertificate=true;"
```

**Test Coverage**:
- User authentication flows
- Role initialization and management
- Identity database operations

### Docker Configuration
**Tests Affected**: Integration tests using Testcontainers

**Setup Requirements**:
1. Install Docker Desktop
2. Ensure Docker daemon is running
3. Verify Docker group membership for current user
4. Configure Testcontainers for PostgreSQL instances

**Validation Commands**:
```bash
# Verify Docker availability
docker info

# Check group membership
groups $USER

# Use sg command if not in docker group
sg docker -c "dotnet test --filter 'Category=Integration'"
```

## Configuration Files

### Primary Test Configuration
**File**: `Code/Zarichney.Server/appsettings.Test.json`

Features:
- Environment variable substitution (`${VARIABLE_NAME:-default}`)
- Test-safe defaults where possible  
- Comprehensive external service configurations
- Appropriate test timeouts and retry limits

### Consolidated Test Configuration  
**File**: `Code/Zarichney.Server/appsettings.Testing.json`

Features:
- Comprehensive external service configurations
- Environment variable substitution (`${VARIABLE_NAME:-default}`)
- Full integration with OpenAI, Stripe, MS Graph, and database services
- Test-safe defaults and appropriate timeouts

## Validation and Troubleshooting

### Environment Validation
```bash
# Verify all required environment variables
echo "OpenAI: $OPENAI_API_KEY"
echo "Stripe: $STRIPE_SECRET_KEY"  
echo "Graph: $MSGRAPH_TENANT_ID"
echo "Database: $CONNECTION_STRING_USER_DB"
```

### Test Suite Validation
```bash
# Quick validation run
./Scripts/run-test-suite.sh report summary

# Comprehensive analysis
./Scripts/run-test-suite.sh report markdown
```

### Common Issues

**External Service Connection Failures**:
- Verify API keys are valid and have sufficient quotas
- Ensure test mode is enabled for payment services
- Check tenant IDs and app permissions for Graph services
- Confirm SQL Server is running and accessible

**Docker/Testcontainers Issues**:
- Verify Docker Desktop is running
- Check Docker group membership
- Use `sg docker -c` command if permissions are needed
- Monitor Docker resource usage during test execution

**Environment Variable Issues**:
- Confirm variables are exported in current shell
- Check for typos in variable names
- Verify variable values don't contain invalid characters
- Consider using shell profile files for persistence

## Security Considerations

### Credential Management
- Use test-only API keys and credentials
- Never commit real credentials to version control  
- Rotate test credentials regularly
- Maintain separate test accounts from production

### Data Isolation
- Use dedicated test databases
- Implement proper test data cleanup
- Ensure test data doesn't persist between runs
- Monitor API usage to prevent quota exhaustion

## Testing Standards Integration

This setup guide directly supports the testing infrastructure defined in:
- `../../Code/Zarichney.Server.Tests/README.md` - Test project overview and execution
- `../Standards/TestingStandards.md` - Overall testing philosophy and quality standards
- `../Standards/IntegrationTestCaseDevelopment.md` - Integration test development practices

## Success Metrics

**Properly Configured Environment Results**:
- Total Tests: 86
- Passed: 85 (98.8% success rate)
- Skipped: 1 (intentional production safety skip)
- Failed: 0

**Validation Command**:
```bash
./Scripts/run-test-suite.sh report summary
```

Expected output should show 85/86 tests passing with only 1 intentional production safety skip remaining.