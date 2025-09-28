# Test Suite Environment Setup Requirements

**Version:** 1.0  
**Last Updated:** 2025-09-28  
**Parent:** [`Development`](./README.md)

## 1. Purpose & Scope

### Overview
This document provides comprehensive guidance for setting up test environments to achieve optimal test suite performance with minimal skip rates and maximum coverage effectiveness. It covers all environment types from local development to production deployment validation.

### Environment Classifications
- **Unconfigured Environment**: Basic setup with external services unavailable (26.7% skip threshold)
- **Configured Environment**: Full setup with all services available (1.2% skip threshold)  
- **Production Environment**: Complete deployment validation setup (0% skip threshold)

### Goals
- Minimize test skip rates through proper environment configuration
- Enable comprehensive test coverage across all test categories
- Provide clear setup instructions for different deployment contexts
- Support progressive coverage growth toward comprehensive coverage through continuous testing excellence

## 2. Core Infrastructure Requirements

### 2.1 Base Development Environment

#### Required Software
- **.NET 8 SDK** (8.0.x or later)
- **Docker Desktop** (4.0.0 or later)
- **Git** (2.30.0 or later)
- **PowerShell** (7.0 or later) or **Bash** (4.0 or later)

#### Installation Commands
```bash
# Verify .NET installation
dotnet --version

# Verify Docker installation  
docker --version
docker info

# Verify Git installation
git --version
```

#### Required .NET Tools
```bash
# Install/update global tools
dotnet tool update --global dotnet-ef --version 8.*
dotnet tool update --global dotnet-reportgenerator-globaltool

# Restore local tools (if .config/dotnet-tools.json exists)
dotnet tool restore
```

### 2.2 Database Infrastructure

#### PostgreSQL with TestContainers
The test suite uses Testcontainers for database integration testing, providing isolated database instances.

**Requirements:**
- Docker daemon running and accessible
- Sufficient system resources (4GB+ RAM recommended)
- Network access for PostgreSQL container images

**Configuration:**
```json
// appsettings.Testing.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ZarichneyTestDb;Username=testuser;Password=testpass"
  }
}
```

**Verification:**
```bash
# Test Docker accessibility  
docker run --rm hello-world

# Test PostgreSQL container pull
docker pull postgres:15

# Verify test database connectivity (run integration tests)
dotnet test --filter "Category=Database" --logger "console;verbosity=normal"
```

### 2.3 Node.js & Frontend Testing (Future Consideration)

#### Current Requirements
- Node.js 18.x or later (for potential frontend test integration)
- npm or yarn package manager

#### Installation
```bash
# Install Node.js (via nvm recommended)
nvm install 18
nvm use 18

# Verify installation
node --version
npm --version
```

## 3. External Service Configuration

### 3.1 Service Configuration Overview

To achieve **Configured Environment** status (1.2% skip threshold), configure these external services:

| Service | Purpose | Required For |
|---------|---------|--------------|
| OpenAI API | LLM integration testing | `ExternalOpenAI` test category |
| Stripe API | Payment processing testing | `ExternalStripe` test category |  
| Microsoft Graph API | Office 365 integration testing | `ExternalMSGraph` test category |
| GitHub API | Repository management testing | `ExternalGitHub` test category |

### 3.2 OpenAI API Configuration

#### Setup Steps
1. **Obtain API Key**: Register at [platform.openai.com](https://platform.openai.com)
2. **Configure Secrets**: Add API key to user secrets or environment variables
3. **Verify Configuration**: Run OpenAI-dependent tests

#### Configuration Methods

**User Secrets (Recommended for Development):**
```bash
# Navigate to server project
cd Code/Zarichney.Server

# Set OpenAI API key
dotnet user-secrets set "OpenAI:ApiKey" "your-api-key-here"
```

**Environment Variables:**
```bash
export OPENAI__APIKEY="your-api-key-here"
```

**appsettings.Development.json:**
```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here",
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 4000
  }
}
```

#### Verification
```bash
# Test OpenAI integration
dotnet test --filter "Category=ExternalOpenAI" --logger "console;verbosity=normal"
```

### 3.3 Stripe API Configuration

#### Setup Steps
1. **Create Stripe Account**: Register at [stripe.com](https://stripe.com)
2. **Obtain API Keys**: Get publishable and secret keys from dashboard
3. **Use Test Mode**: Always use test keys for testing environments

#### Configuration
```bash
# Set Stripe configuration via user secrets
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_..."
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
```

#### Verification
```bash
# Test Stripe integration
dotnet test --filter "Category=ExternalStripe" --logger "console;verbosity=normal"
```

### 3.4 Microsoft Graph API Configuration

#### Setup Steps
1. **Azure App Registration**: Register app in Azure Active Directory
2. **Configure Permissions**: Set appropriate Graph API permissions
3. **Obtain Credentials**: Get client ID, secret, and tenant ID

#### Configuration
```bash
# Set Microsoft Graph configuration
dotnet user-secrets set "MicrosoftGraph:ClientId" "your-client-id"
dotnet user-secrets set "MicrosoftGraph:ClientSecret" "your-client-secret"
dotnet user-secrets set "MicrosoftGraph:TenantId" "your-tenant-id"
```

#### Verification
```bash
# Test Microsoft Graph integration
dotnet test --filter "Category=ExternalMSGraph" --logger "console;verbosity=normal"
```

### 3.5 GitHub API Configuration

#### Setup Steps
1. **Generate Personal Access Token**: Create token in GitHub settings
2. **Configure Permissions**: Ensure appropriate repository access
3. **Set Token in Configuration**: Add to user secrets or environment variables

#### Configuration
```bash
# Set GitHub API configuration
dotnet user-secrets set "GitHub:PersonalAccessToken" "your-token-here"
dotnet user-secrets set "GitHub:RepositoryOwner" "Zarichney-Development"
dotnet user-secrets set "GitHub:RepositoryName" "zarichney-api"
```

#### Verification
```bash
# Test GitHub integration
dotnet test --filter "Category=ExternalGitHub" --logger "console;verbosity=normal"
```

## 4. Environment-Specific Setup

### 4.1 Local Development Environment

#### Target Classification: Unconfigured → Configured
**Goal**: Reduce skip rate from 26.7% to 1.2% through service configuration

#### Setup Checklist
- [ ] **.NET 8 SDK** installed and verified
- [ ] **Docker Desktop** running and accessible
- [ ] **Database tests** passing (TestContainers working)
- [ ] **OpenAI API** configured and tested
- [ ] **Stripe API** configured and tested (test mode)
- [ ] **Microsoft Graph API** configured and tested
- [ ] **GitHub API** configured and tested
- [ ] **All external service tests** passing

#### Verification Commands
```bash
# Comprehensive environment validation
./Scripts/run-test-suite.sh report

# Check specific service categories
dotnet test --filter "Category=Database" --logger "console;verbosity=normal"
dotnet test --filter "Category=ExternalOpenAI" --logger "console;verbosity=normal"
dotnet test --filter "Category=ExternalStripe" --logger "console;verbosity=normal"
dotnet test --filter "Category=ExternalMSGraph" --logger "console;verbosity=normal"
dotnet test --filter "Category=ExternalGitHub" --logger "console;verbosity=normal"

# Verify baseline validation results
jq '.environment.classification' TestResults/baseline_validation.json
jq '.metrics.skipPercentage' TestResults/baseline_validation.json
```

### 4.2 CI/CD Environment Configuration

#### GitHub Actions Setup
Configure GitHub Actions secrets for external service access:

```yaml
# .github/workflows/build.yml - Secret configuration
env:
  OPENAI__APIKEY: ${{ secrets.OPENAI_API_KEY }}
  STRIPE__PUBLISHABLEKEY: ${{ secrets.STRIPE_PUBLISHABLE_KEY }}
  STRIPE__SECRETKEY: ${{ secrets.STRIPE_SECRET_KEY }}
  MICROSOFTGRAPH__CLIENTID: ${{ secrets.MSGRAPH_CLIENT_ID }}
  MICROSOFTGRAPH__CLIENTSECRET: ${{ secrets.MSGRAPH_CLIENT_SECRET }}
  MICROSOFTGRAPH__TENANTID: ${{ secrets.MSGRAPH_TENANT_ID }}
  GITHUB__PERSONALACCESSTOKEN: ${{ secrets.GITHUB_TOKEN }}
```

#### Required GitHub Secrets
| Secret Name | Purpose | Source |
|-------------|---------|--------|
| `OPENAI_API_KEY` | OpenAI API access | platform.openai.com |
| `STRIPE_PUBLISHABLE_KEY` | Stripe test key | stripe.com dashboard |
| `STRIPE_SECRET_KEY` | Stripe secret key | stripe.com dashboard |
| `MSGRAPH_CLIENT_ID` | Azure app registration | Azure portal |
| `MSGRAPH_CLIENT_SECRET` | Azure app secret | Azure portal |
| `MSGRAPH_TENANT_ID` | Azure tenant ID | Azure portal |
| `GITHUB_TOKEN` | GitHub API access | GitHub settings |

#### Docker Configuration
Ensure Docker daemon is available in CI environment:

```yaml
# GitHub Actions Docker setup
services:
  docker:
    image: docker:latest
    options: --privileged
```

### 4.3 Production Environment Setup

#### Target Classification: Production (0% skip tolerance)
**Goal**: Complete test validation with zero skips acceptable

#### Production Considerations
- All external services must be production-ready
- Service credentials must be production-valid  
- Infrastructure must be fully available
- Network connectivity must be reliable
- Performance must meet production standards

#### Production Testing Strategy
- Use production-equivalent service configurations
- Validate all external service integrations
- Ensure zero test failures or skips
- Perform complete end-to-end validation

## 5. Service Virtualization & Mocking

### 5.1 WireMock.Net Integration (Future Enhancement)

#### Purpose
Reduce dependency on external services while maintaining comprehensive test coverage.

#### Implementation Plan
```csharp
// Example WireMock server setup
public class ExternalServiceMockFixture
{
    private WireMockServer _mockServer;
    
    public void SetupMockServices()
    {
        _mockServer = WireMockServer.Start();
        
        // Mock OpenAI API responses
        _mockServer
            .Given(Request.Create().WithPath("/v1/chat/completions"))
            .RespondWith(Response.Create().WithBodyAsJson(new { /* mock response */ }));
    }
}
```

#### Benefits
- Reduced external service dependency
- Faster test execution
- More predictable test results
- Lower skip rates in unconfigured environments

### 5.2 Service Abstraction Patterns

#### Recommended Architecture
```csharp
// Service abstraction for testability
public interface IExternalServiceClient
{
    Task<ApiResponse> CallServiceAsync(ServiceRequest request);
}

// Production implementation
public class ExternalServiceClient : IExternalServiceClient { }

// Test implementation  
public class MockExternalServiceClient : IExternalServiceClient { }
```

## 6. Troubleshooting Common Issues

### 6.1 Docker Issues

#### Symptoms
- Integration tests failing with database connection errors
- TestContainers unable to start containers
- High skip rate for `Database` category tests

#### Diagnostic Commands
```bash
# Check Docker status
docker info
docker ps

# Test container creation
docker run --rm -p 5432:5432 -e POSTGRES_PASSWORD=testpass postgres:15

# Check Docker permissions (Linux)
groups $USER
sudo usermod -aG docker $USER
```

#### Solutions
- Ensure Docker Desktop is running
- Verify Docker daemon accessibility
- Check system resources (memory, disk space)
- Restart Docker service if necessary

### 6.2 External Service Authentication Issues

#### Symptoms
- High skip rate for external service categories
- Authentication failures in test logs
- Invalid API key or permission errors

#### Diagnostic Steps
```bash
# Check configuration values
dotnet user-secrets list --project Code/Zarichney.Server

# Test API connectivity manually
curl -H "Authorization: Bearer $OPENAI_API_KEY" https://api.openai.com/v1/models

# Verify environment variable loading
echo $OPENAI__APIKEY
```

#### Solutions
- Verify API keys are correctly configured
- Check service-specific permissions and scopes
- Ensure credentials haven't expired
- Test API connectivity outside of test suite

### 6.3 Network and Connectivity Issues

#### Symptoms
- Intermittent test failures
- Timeout errors in external service calls
- Inconsistent skip rates between runs

#### Diagnostic Approaches
- Check network connectivity to service endpoints
- Review firewall and proxy settings
- Monitor service availability and response times
- Implement retry mechanisms in test code

## 7. Performance Optimization

### 7.1 Test Execution Performance

#### Optimization Strategies
- **Parallel Execution**: Configure appropriate parallelism levels
- **Resource Management**: Optimize container resource allocation
- **Test Data Management**: Use efficient test data creation patterns
- **Service Mocking**: Replace slow external calls with mocks where appropriate

#### Configuration Example
```json
// xunit.runner.json
{
  "parallelizeAssemblies": false,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4,
  "preEnumerateTheories": false
}
```

### 7.2 Container Optimization

#### Docker Performance Tips
- Use specific container versions (avoid `latest`)
- Implement container image caching
- Optimize container resource limits
- Use volume mounts for persistent test data

#### Example TestContainer Configuration
```csharp
public class DatabaseFixture
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .WithCleanUp(true)
        .Build();
}
```

## 8. Monitoring and Maintenance

### 8.1 Environment Health Monitoring

#### Regular Health Checks
```bash
# Weekly environment validation
./Scripts/run-test-suite.sh report summary

# Check service connectivity
curl -f https://api.openai.com/v1/models || echo "OpenAI unavailable"
curl -f https://api.stripe.com/v1/account || echo "Stripe unavailable"

# Monitor skip rate trends
grep "skipPercentage" TestResults/baseline_validation*.json | tail -10
```

#### Automated Monitoring
- Set up alerts for skip rate increases
- Monitor external service availability
- Track test execution performance trends
- Alert on baseline validation failures

### 8.2 Maintenance Tasks

#### Monthly Maintenance
- Review and update API credentials
- Check for service API changes or deprecations
- Update container images to latest stable versions
- Review test categorization accuracy

#### Quarterly Maintenance  
- Comprehensive environment audit
- Performance optimization review
- Service configuration optimization
- Skip rate reduction analysis

## 9. Quick Reference

### 9.1 Environment Setup Checklist

#### Unconfigured → Configured Environment
- [ ] .NET 8 SDK installed
- [ ] Docker Desktop running
- [ ] PostgreSQL TestContainers working
- [ ] OpenAI API configured
- [ ] Stripe API configured (test mode)
- [ ] Microsoft Graph API configured
- [ ] GitHub API configured
- [ ] All external service tests passing
- [ ] Skip rate ≤1.2%

#### Production Environment
- [ ] All configured environment requirements met
- [ ] Production-equivalent service configurations
- [ ] Zero test failures acceptable
- [ ] Complete end-to-end validation
- [ ] Skip rate = 0%

### 9.2 Common Commands

```bash
# Environment setup validation
./Scripts/run-test-suite.sh report summary

# Service-specific testing
dotnet test --filter "Category=Database"
dotnet test --filter "Category=ExternalOpenAI"
dotnet test --filter "Category=ExternalStripe"

# Configuration management
dotnet user-secrets list --project Code/Zarichney.Server
dotnet user-secrets set "Key" "Value" --project Code/Zarichney.Server

# Docker diagnostics
docker info
docker ps
docker logs [container-id]

# Environment classification check
jq '.environment.classification' TestResults/baseline_validation.json
```

### 9.3 Service Endpoint References

| Service | Endpoint | Documentation |
|---------|----------|---------------|
| OpenAI | `https://api.openai.com` | [docs.openai.com](https://docs.openai.com) |
| Stripe | `https://api.stripe.com` | [stripe.com/docs](https://stripe.com/docs) |
| Microsoft Graph | `https://graph.microsoft.com` | [docs.microsoft.com/graph](https://docs.microsoft.com/en-us/graph/) |
| GitHub | `https://api.github.com` | [docs.github.com](https://docs.github.com/en/rest) |

---

**Related Documents:**
- [TestSuiteStandards.md](../Standards/TestSuiteStandards.md) - Comprehensive standards framework
- [TestSuiteBaselineGuide.md](./TestSuiteBaselineGuide.md) - Baseline interpretation guide
- [TestingStandards.md](../Standards/TestingStandards.md) - Overarching testing standards