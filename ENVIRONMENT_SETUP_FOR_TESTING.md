# Environment Setup for 100% Test Pass Rate

This document provides comprehensive instructions for setting up a fully configured environment where all 85/86 tests pass (98.8% pass rate), achieving the goal defined in Issue #85.

## Overview

The zarichney-api test suite contains 86 tests total:
- **Target**: 85 tests passing, 1 production safety skip (98.8% pass rate)
- **Current**: ~63 tests passing, 23 tests skipping (73.3% pass rate)

## Prerequisites

### Required Software
- **.NET 8 SDK** (version 8.0.412 or later)
- **Docker Desktop** (for integration tests with containers)
- **SQL Server** (local instance or accessible connection)

### Required External Service Accounts
1. **OpenAI API** - Test API key with sufficient credits
2. **Stripe** - Test mode account with webhook endpoints configured
3. **Microsoft Azure AD** - Test tenant with app registration
4. **SQL Server** - Database instance for identity/authentication tests

## Configuration Setup

### 1. Environment Variables

Create the following environment variables on your system:

```bash
# OpenAI Configuration (6 tests depend on this)
export OPENAI_API_KEY="sk-test-your-openai-api-key-here"
export OPENAI_ORG_ID="org-your-organization-id-here"

# Stripe Configuration (6 tests depend on this)  
export STRIPE_SECRET_KEY="sk_test_your-stripe-secret-key"
export STRIPE_PUBLISHABLE_KEY="pk_test_your-stripe-publishable-key"
export STRIPE_WEBHOOK_SECRET="whsec_your-webhook-secret"

# Microsoft Graph Configuration (4 tests depend on this)
export MSGRAPH_TENANT_ID="your-azure-tenant-id"
export MSGRAPH_CLIENT_ID="your-azure-app-client-id"
export MSGRAPH_CLIENT_SECRET="your-azure-app-secret"

# Database Configuration (6 tests depend on this)
export CONNECTION_STRING_USER_DB="Data Source=localhost;Initial Catalog=ZarichneyTest;Integrated Security=true;TrustServerCertificate=true;"
```

### 2. Configuration Files

The repository includes `appsettings.Test.json` which references these environment variables. This configuration file:

- Uses environment variable substitution (`${VARIABLE_NAME:-default}`)
- Provides test-safe defaults where possible
- Includes comprehensive external service configurations
- Sets appropriate test timeouts and retry limits

### 3. External Service Setup Details

#### OpenAI API Setup
1. **Create OpenAI Account**: Visit [OpenAI Platform](https://platform.openai.com/)
2. **Generate API Key**: Create a test API key with usage limits
3. **Set Organization ID**: Get your organization ID from account settings
4. **Configure Models**: Tests use `gpt-4o-mini` for completions and `whisper-1` for transcription

**Tests Affected**: 6 tests in `AiControllerTests.cs`
- Completion endpoints with text prompts
- Completion endpoints with audio prompts  
- Transcription endpoints
- Service unavailable behavior validation

#### Stripe Payment Integration
1. **Create Stripe Account**: Visit [Stripe Dashboard](https://dashboard.stripe.com/)
2. **Enable Test Mode**: Use test mode for all API calls
3. **Get API Keys**: Retrieve test publishable and secret keys
4. **Configure Webhooks**: Set up webhook endpoints for test environment
5. **Test Payment Methods**: Use Stripe's test card numbers

**Tests Affected**: 6 tests in `PaymentControllerTests.cs`
- Payment intent creation
- Payment status retrieval
- Checkout session creation
- Service unavailable behavior validation

#### Microsoft Graph Setup  
1. **Create Azure AD App**: Register application in Azure Portal
2. **Configure Permissions**: Set up required Graph API permissions
3. **Generate Secret**: Create client secret for authentication
4. **Test Tenant**: Use dedicated test tenant for safety

**Tests Affected**: 4 tests related to email/identity services
- Email service integration
- Identity provider connectivity
- Mail validation services

#### Database Configuration
1. **SQL Server Instance**: Local SQL Server or connection to test database
2. **Create Test Database**: Database named `ZarichneyTest`
3. **Connection String**: Integrated security or SQL authentication
4. **Entity Framework**: Automatic schema creation via code-first migrations

**Tests Affected**: 6 tests in authentication and role management
- User authentication flows
- Role initialization and management
- Identity database operations

## Running Tests with Full Configuration

### 1. Verify Environment Setup
```bash
# Check all environment variables are set
echo $OPENAI_API_KEY
echo $STRIPE_SECRET_KEY  
echo $MSGRAPH_TENANT_ID
echo $CONNECTION_STRING_USER_DB
```

### 2. Validate Configuration
```bash
# Run the comprehensive test suite with analysis
./Scripts/run-test-suite.sh report

# Or run with Docker group permissions if needed
sg docker -c "./Scripts/run-test-suite.sh report"
```

### 3. Expected Results
When properly configured, you should see:
- **Total Tests**: 86
- **Passed**: 85 (98.8%)
- **Skipped**: 1 (production safety skip only)
- **Failed**: 0

## Test Categories and Dependencies

### External Service Tests (16 tests)
- **OpenAI API**: 6 tests using `[DependencyFact(ExternalServices.OpenAiApi)]`
- **Stripe Payment**: 6 tests using `[DependencyFact(ExternalServices.Stripe)]`  
- **Microsoft Graph**: 4 tests using `[DependencyFact(ExternalServices.MsGraph)]`

### Infrastructure Tests (6 tests)
- **Database**: 6 tests using `[DependencyFact(InfrastructureDependency.Database)]`

### Performance Tests (2 tests)
- **API Performance**: 2 tests using `[DependencyFact(InfrastructureDependency.Database)]`
- Previously hard-coded Skip attributes removed

### Smoke Tests (3 tests)  
- **Authentication Flow**: Database-dependent smoke test
- **Cookbook Functionality**: Database-dependent smoke test
- **Payment Endpoints**: Stripe-dependent smoke test

## Troubleshooting

### Common Issues

#### Tests Still Skipping
```bash
# Check if environment variables are properly set
dotnet test --logger "console;verbosity=detailed"

# Verify configuration loading
./Scripts/run-test-suite.sh report summary
```

#### External Service Connection Failures
- **OpenAI**: Verify API key validity and quota limits
- **Stripe**: Ensure test mode is enabled and webhooks are configured
- **Microsoft Graph**: Check tenant ID and app permissions
- **Database**: Verify SQL Server is running and connection string is correct

#### Docker Issues
```bash
# Verify Docker is running
docker info

# Check Docker group membership
groups $USER

# If not in docker group, use sg command
sg docker -c "dotnet test"
```

### Test-Specific Debugging

#### Database Tests
```bash
# Check database connectivity
sqlcmd -S localhost -d ZarichneyTest -Q "SELECT 1"

# Verify Entity Framework migrations
dotnet ef database update --project Code/Zarichney.Server
```

#### External API Tests
```bash
# Test OpenAI API connectivity
curl -H "Authorization: Bearer $OPENAI_API_KEY" https://api.openai.com/v1/models

# Test Stripe API connectivity  
curl -H "Authorization: Bearer $STRIPE_SECRET_KEY" https://api.stripe.com/v1/payment_methods
```

## Validation Commands

### Quick Validation
```bash
# Run test suite in report mode with summary output
./Scripts/run-test-suite.sh report summary
```

### Comprehensive Analysis
```bash
# Full test analysis with AI-powered insights
./Scripts/run-test-suite.sh report markdown

# Or using the custom Claude command
/test-report
```

### CI/CD Integration
```bash
# Machine-readable output for automated systems
./Scripts/run-test-suite.sh report json
```

## Security Considerations

### Environment Variables
- Use test-only API keys and credentials
- Never commit real credentials to version control
- Rotate test credentials regularly
- Use separate test tenants/accounts from production

### Database Security
- Use dedicated test database
- Implement proper connection string security
- Ensure test data isolation from production

### API Rate Limits
- Monitor API usage during test runs
- Implement appropriate retry policies
- Use test mode wherever available

## Maintenance

### Regular Updates
- Keep external service credentials current
- Monitor API deprecations and updates
- Update test configurations as needed
- Verify test pass rates after dependency updates

### Documentation Updates
- Update this document when new external dependencies are added
- Document any new environment setup requirements
- Maintain troubleshooting section based on common issues

---

**Success Criteria**: When properly configured, running `./Scripts/run-test-suite.sh report summary` should show 85/86 tests passing (98.8% pass rate) with only 1 intentional production safety skip remaining.