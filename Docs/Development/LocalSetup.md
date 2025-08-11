# Local Development Setup & Configuration

**Last Updated:** 2025-08-11

> **Parent:** [`README.md`](./README.md)

## 1. Introduction

This document provides comprehensive setup instructions for local development of the complete Zarichney platform, including both backend (.NET 8 API) and frontend (Angular 19) applications. It covers environment setup, database configuration, external service integration, and graceful degradation patterns that enable development with varying levels of functionality.

### Setup Approaches

**Quick Start (Minimal Setup)**:
* Basic functionality without external services
* Suitable for core development and testing
* ~15 minutes setup time
* See sections 2-6 for minimal requirements

**Full Setup (Production-Like)**:
* All external services configured
* Complete feature availability
* Suitable for comprehensive testing and production preparation  
* ~45-60 minutes setup time
* Includes sections 2-10 for complete configuration

## 2. Core Environment Setup

### Prerequisites

* **.NET 8.0 SDK or later**: Required to build and run the application.
* **Docker Desktop** (Recommended): For integration testing with local databases.
* **PostgreSQL** (Optional): For local Identity Database setup.
* **Git**: For source control management.
* **IDE**: Visual Studio 2025, VS Code with C# extension, or JetBrains Rider.

### Default Admin User

In non-Production environments with a configured database, the system automatically creates a default administrator account on first startup:

- **Email**: `admin@localhost.dev`
- **Username**: `admin`
- **Password**: `DevAdmin123!`

⚠️ **Security Notice**: These are development credentials only. You MUST change them immediately after initial setup. See the [Authentication System Maintenance Guide](../Maintenance/AuthenticationSystem.md#7-default-admin-user-management) for detailed instructions.

### Basic Commands

* **Build Application:**
  ```bash
  dotnet build Zarichney.sln
  ```

* **Run Application:**
  ```bash
  dotnet run --project Code/Zarichney.Server
  ```

* **Run Tests:**
  ```bash
  # Run all tests
  dotnet test Zarichney.sln
  
  # Run specific test categories
  dotnet test --filter "Category=Unit"
  dotnet test --filter "Category=Integration"
  
  # For environments where Docker group membership isn't active in current shell
  # (Required in some Linux/WSL2 setups for integration tests using Testcontainers)
  sg docker -c "dotnet test Zarichney.sln"
  sg docker -c "dotnet test --filter 'Category=Integration'"
  sg docker -c "dotnet test --filter 'Category=Integration&Dependency=Database'"
  ```


## 3. Configuration & Environment Variables

The application is configured using the following sources (in order of precedence):

1. **Environment Variables**: Highest precedence for overriding settings.
2. **User Secrets** (Development): Secure storage for local development secrets.
3. **`appsettings.{Environment}.json`**: Environment-specific settings.
4. **`appsettings.json`**: Default settings (lowest precedence).

### Critical Configuration Settings

* **Database Connection Strings:**
  ```json
  "ConnectionStrings": {
    "UserDatabase": "Host=localhost;Database=zarichney_identity;Username=postgres;Password=yourpassword"
  }
  ```

* **JWT Authentication Settings:**
  ```json
  "JwtSettings": {
    "SecretKey": "your-secure-key-at-least-32-characters-long",
    "Issuer": "https://zarichney.com/api",
    "Audience": "https://zarichney.com",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
  ```

* **External Service API Keys:**
  ```json
  "LlmConfig:ApiKey": "your-openai-api-key",
  "EmailConfig:AzureTenantId": "your-azure-tenant-id",
  // ... other external service keys
  ```

## 4. Graceful Degradation Patterns

The Zarichney API implements graceful degradation patterns that allow local development without all external dependencies configured. This section describes the behavior when key dependencies are unavailable.

### Identity Database (PostgreSQL)

The Identity Database is required for authentication and user management. The application handles missing Identity Database configuration differently based on the environment:

* **Production Environment**: 
  * The application will **fail to start** if the Identity Database connection string is missing or invalid.
  * This strict validation ensures that authentication and user management are always available in production.

* **Non-Production Environments** (Development, Testing, etc.):
  * The application will **start successfully** even if the Identity Database connection string is missing or invalid.
  * The `/status` endpoint will report `PostgresIdentityDb` as unavailable.
  * Authentication-related endpoints in the `AuthController` (e.g., `/api/auth/login`, `/api/auth/register`) will return HTTP 503 (Service Unavailable).
  * Other endpoints that don't depend on authentication will function normally.

This allows developers to run and test parts of the application even without a properly configured Identity Database.

**Mock Authentication Mode (New in Task GH-4):**
When the Identity Database is unavailable in non-Production environments, the application automatically enables a mock authentication system that:
* Allows access to `[Authorize]` protected endpoints as an authenticated user
* Creates a configurable mock user with default roles
* Supports dynamic role assignment via HTTP headers in Development environment
* Configuration in `appsettings.Development.json`:
  ```json
  "MockAuth": {
    "DefaultRoles": ["User", "Admin"],
    "DefaultUsername": "MockUser",
    "DefaultEmail": "mock@example.com",
    "DefaultUserId": "mock-user-id"
  }
  ```
* Optional: In Development environment, use the `X-Mock-Roles` header to dynamically specify roles for testing:
  ```bash
  curl -H "X-Mock-Roles: Admin,Editor" http://localhost:5000/api/admin/endpoint
  ```

Example `/status` response when Identity Database is unavailable:
```json
{
  "services": [
    {
      "name": "PostgresIdentityDb",
      "isAvailable": false,
      "missingConfigurations": ["ConnectionStrings:UserDatabase"]
    },
    // Other services...
  ]
}
```

### Other External Services

Similar graceful degradation patterns apply to other external services:

* **OpenAI API**: Required for AI features.
* **Microsoft Graph API**: Required for email sending functionality.
* **Stripe API**: Required for payment processing.
* **GitHub API**: Required for GitHub integration.

For each service, the application:
1. Uses the `ExternalServices` enum to track service status.
2. Updates the `/status` endpoint to report unavailable services.
3. Returns 503 (Service Unavailable) for endpoints that depend on unavailable services via the `[DependsOnService]` attribute.

## 5. Running Without PostgreSQL Identity Database

To run the application without setting up a PostgreSQL Identity Database:

1. Start the application in Development environment:
   ```bash
   # Set the environment to Development
   $env:ASPNETCORE_ENVIRONMENT="Development"  # Windows PowerShell
   export ASPNETCORE_ENVIRONMENT="Development" # Linux/macOS
   
   # Run without configuring UserDatabase connection string
   dotnet run --project Code/Zarichney.Server
   ```

2. Verify the application starts successfully and check the status:
   ```bash
   curl http://localhost:5000/api/status
   ```

3. Note that authentication endpoints will return 503 Service Unavailable:
   ```bash
   curl -X POST http://localhost:5000/api/auth/login -H "Content-Type: application/json" -d '{"email":"test@example.com","password":"test"}'
   ```

4. Other endpoints should function normally, allowing you to test non-authentication features.

## 6. Setting Up Local PostgreSQL Identity Database

For full functionality, we recommend setting up a local PostgreSQL database:

1. **Install PostgreSQL** (if not already installed).

2. **Create a Database**:
   ```sql
   CREATE DATABASE zarichney_identity;
   ```

3. **Configure Connection String**:
   ```bash
   # Using .NET User Secrets
   dotnet user-secrets set "ConnectionStrings:UserDatabase" "Host=localhost;Database=zarichney_identity;Username=postgres;Password=yourpassword" --project Code/Zarichney.Server
   ```

4. **Apply Migrations**:
   
   ```bash
   # Navigate to server project directory
   cd Code/Zarichney.Server
   
   # Apply migrations manually using EF CLI
   dotnet ef database update --context UserDbContext
   ```
   
   **Option B - Production Script (Alternative for advanced users)**:
   ```bash
   # Navigate to Migrations directory
   cd Code/Zarichney.Server/Services/Auth/Migrations
   
   # Run migration script
   ./ApplyMigrations.sh  # Linux/macOS
   ./ApplyMigrations.ps1 # Windows
   ```

5. **Default Administrator User Seeding (New in Task GH-4)**:
   When the application starts in non-Production environments with a configured Identity Database, it automatically seeds a default administrator user for convenience. This feature:
   
   * **Automatically creates** an admin user with credentials from configuration
   * **Assigns the "Admin" role** to the user for full system access
   * **Is idempotent** - safe to run multiple times without creating duplicates
   * **Only runs in non-Production environments** (Development, Testing, etc.)
   
   **Configuration** in `appsettings.Development.json`:
   ```json
   "DefaultAdminUser": {
     "Email": "admin@example.com",
     "UserName": "admin",
     "Password": "Password123!"
   }
   ```
   
   **Usage**:
   * The admin user is created automatically when the application starts
   * Use the configured credentials to log in through the `/api/auth/login` endpoint
   * The user will have full administrative access to all protected endpoints
   
   **Important**: Ensure the password meets ASP.NET Core Identity requirements (uppercase, lowercase, digit, special character, minimum 6 characters).

## 7. Frontend Application Setup (Angular)

The Zarichney platform includes a modern Angular 19 frontend application with Server-Side Rendering (SSR) capabilities. This section covers setup requirements for the complete full-stack development experience.

### Frontend Prerequisites

* **Node.js 18+**: Required for Angular development and build processes
* **npm**: Comes with Node.js, used for package management
* **Angular CLI** (Optional): Global installation recommended for development convenience

### Frontend Setup Steps

1. **Install Node.js**:
   * Download from [nodejs.org](https://nodejs.org/) (LTS version recommended)
   * Verify installation: `node --version` and `npm --version`

2. **Navigate to Frontend Directory**:
   ```bash
   cd Code/Zarichney.Website
   ```

3. **Install Dependencies**:
   ```bash
   npm install
   ```

4. **Configure Environment Settings**:
   * Review `src/startup/environments.dev.ts` for development configuration
   * Key settings include API base URL and Stripe publishable key
   * Example development configuration:
   ```typescript
   export const environment = {
     production: false,
     apiUrl: 'https://localhost:7061/api',
     stripePublishableKey: 'pk_test_your_stripe_key_here',
     appName: 'Zarichney Platform',
     version: '1.0.0'
   };
   ```

5. **Start Development Server**:
   ```bash
   # Standard development server with hot reload
   npm start
   
   # Or with Angular CLI (if installed globally)
   ng serve
   ```

6. **Verify Frontend Setup**:
   * Navigate to `http://localhost:4200`
   * Application should load with basic navigation
   * Check browser console for any configuration errors

### Frontend Build Options

* **Development Build**: `npm run build-dev`
* **Production Build**: `npm run build-prod` 
* **SSR Development**: `npm run serve:ssr` (requires build first)
* **Clean Build**: `npm run clean-dist` (removes previous build artifacts)

### Frontend Configuration Dependencies

The frontend application integrates with several services that require configuration:

* **Backend API**: Assumes `Code/Zarichney.Server` is running on configured port
* **Stripe Payments**: Requires valid Stripe publishable key for payment processing
* **Authentication**: Relies on JWT tokens from backend authentication system

### Common Frontend Issues

* **CORS Errors**: Ensure backend CORS is configured for development port 4200
* **API Connection**: Verify backend is running and accessible at configured API URL
* **Build Failures**: Clear `node_modules` and reinstall if experiencing dependency issues
* **SSR Issues**: Ensure code is browser/server compatible when using SSR features

## 8. External Services Configuration

This section provides comprehensive setup instructions for all external services that enhance the platform's capabilities. Each service can be configured independently, and the application gracefully degrades when services are unavailable.

### OpenAI API (AI Services)

**Purpose**: Powers AI-driven recipe analysis, cleaning, ranking, and synthesis features.

**Setup Steps**:
1. Create account at [OpenAI Platform](https://platform.openai.com/)
2. Generate API key from API Keys section
3. Configure via User Secrets or environment variables:
   ```bash
   # User Secrets (recommended for development)
   dotnet user-secrets set "LlmConfig:ApiKey" "your-openai-api-key" --project Code/Zarichney.Server
   dotnet user-secrets set "LlmConfig:OrganizationId" "your-org-id" --project Code/Zarichney.Server
   
   # Environment Variables
   export LlmConfig__ApiKey="your-openai-api-key"
   export LlmConfig__OrganizationId="your-org-id"
   ```

**Configuration Reference**:
```json
{
  "LlmConfig": {
    "ApiKey": "sk-your-openai-api-key",
    "ModelName": "gpt-4o-mini",
    "RetryAttempts": 5
  },
  "TranscribeConfig": {
    "ModelName": "whisper-1", 
    "RetryAttempts": 5
  }
}
```

### Stripe Payment Processing

**Purpose**: Handles payment processing for cookbook orders and premium features.

**Setup Steps**:
1. Create account at [Stripe Dashboard](https://dashboard.stripe.com/)
2. Enable test mode for development
3. Obtain test API keys from Developers > API Keys
4. Configure backend and frontend:
   ```bash
   # Backend configuration (User Secrets)
   dotnet user-secrets set "PaymentConfig:StripeSecretKey" "sk_test_your_stripe_secret_key" --project Code/Zarichney.Server
   dotnet user-secrets set "PaymentConfig:StripePublishableKey" "pk_test_your_stripe_publishable_key" --project Code/Zarichney.Server
   dotnet user-secrets set "PaymentConfig:StripeWebhookSecret" "whsec_your_webhook_secret" --project Code/Zarichney.Server
   
   # Frontend configuration (environments.dev.ts)
   stripePublishableKey: 'pk_test_your_stripe_publishable_key'
   ```

**Configuration Reference**:
```json
{
  "PaymentConfig": {
    "StripeSecretKey": "sk_test_your_stripe_secret_key",
    "StripePublishableKey": "pk_test_your_stripe_publishable_key", 
    "StripeWebhookSecret": "whsec_your_webhook_secret",
    "Currency": "usd",
    "RecipePrice": 0.20,
    "SuccessUrl": "/cookbook/order/{0}",
    "CancelUrl": "/cookbook/order/{0}?cancelled=true"
  }
}
```

### Microsoft Graph API (Email Services)

**Purpose**: Provides email functionality including verification emails, notifications, and admin communications.

**Setup Steps**:
1. Register application in [Azure Portal](https://portal.azure.com/)
2. Configure required Graph API permissions (Mail.Send, User.Read)
3. Generate client secret for authentication
4. Configure email settings:
   ```bash
   dotnet user-secrets set "EmailConfig:AzureTenantId" "your-tenant-id" --project Code/Zarichney.Server
   dotnet user-secrets set "EmailConfig:AzureAppId" "your-client-id" --project Code/Zarichney.Server
   dotnet user-secrets set "EmailConfig:AzureAppSecret" "your-client-secret" --project Code/Zarichney.Server
   dotnet user-secrets set "EmailConfig:FromEmail" "your-email@domain.com" --project Code/Zarichney.Server
   ```

**Configuration Reference**:
```json
{
  "EmailConfig": {
    "AzureTenantId": "your-azure-tenant-id",
    "AzureAppId": "your-azure-app-client-id", 
    "AzureAppSecret": "your-azure-app-secret",
    "FromEmail": "noreply@yourdomain.com",
    "MailCheckApiKey": "your-mailcheck-api-key",
    "TemplateDirectory": "Services/Email/Templates"
  }
}
```

### GitHub API (Development Integration)

**Purpose**: Enables GitHub integration features for repository management and issue tracking.

**Setup Steps**:
1. Generate Personal Access Token at [GitHub Settings](https://github.com/settings/tokens)
2. Configure required scopes (repo, read:org, admin:public_key)
3. Configure GitHub settings:
   ```bash
   dotnet user-secrets set "GithubConfig:AccessToken" "your-github-token" --project Code/Zarichney.Server
   dotnet user-secrets set "GithubConfig:RepositoryOwner" "your-organization" --project Code/Zarichney.Server
   dotnet user-secrets set "GithubConfig:RepositoryName" "your-repository" --project Code/Zarichney.Server
   ```

**Configuration Reference**:
```json
{
  "GithubConfig": {
    "RepositoryOwner": "your-organization-name",
    "RepositoryName": "cloud-storage",
    "BranchName": "main",
    "AccessToken": "your-github-personal-access-token",
    "RetryAttempts": 5
  }
}
```

### MailCheck Service (Email Validation)

**Purpose**: Provides email address validation and verification for user registration.

**Setup Steps**:
1. Register at [MailCheck.co](https://www.mailcheck.co/)
2. Obtain API key from dashboard
3. Configure validation settings:
   ```bash
   dotnet user-secrets set "MailCheckConfig:ApiKey" "your-mailcheck-api-key" --project Code/Zarichney.Server
   ```

### 🔄 Automatic Runtime Behaviors

The Zarichney platform includes several automatic features that enhance the developer experience by handling common setup tasks at runtime:

#### Data Directory Creation
The FileService automatically creates data directories when they don't exist during runtime. This eliminates the need for manual directory setup and ensures the application can store files for:

* **Recipe Data**: `/app/Data/Recipes` (configurable via `RecipeConfig:OutputDirectory`)
* **Order Data**: `/app/Data/Orders` (configurable via `OrderConfig:OutputDirectory`) 
* **PDF Generation**: `/app/temp` (configurable via `PdfCompilerConfig:ImageDirectory`)
* **Email Templates**: Template directory paths specified in `EmailConfig:TemplateDirectory`

**Implementation**: The `GetFiles()` method in `FileService.cs` line 66-68 automatically calls `Directory.CreateDirectory(directoryPath)` if the directory doesn't exist.

#### Database Schema Management
Entity Framework migrations must be applied manually to ensure proper database schema setup. The application does not automatically apply migrations on startup. See section 6 (Setting Up Local PostgreSQL Identity Database) for detailed migration instructions using the EF CLI tools.

#### Genesis Admin User Creation
In Development and Testing environments, the `RoleInitializer` service automatically:

* Creates the default administrator role if it doesn't exist
* Seeds a genesis admin user using configured credentials
* Assigns the admin role to the genesis user
* Uses the default credentials: `admin@localhost.dev` / `DevAdmin123!`
* Falls back to `EmailConfig:FromEmail` if `DefaultAdminUser` is not configured

**Security Note**: This feature is disabled in Production environments and should only be used for initial local development setup.

**Why These Features Matter**: These automatic behaviors reduce friction in local development setup, eliminate common "it works on my machine" issues, and ensure consistent development environments across team members.

## 9. Production Deployment Considerations

This section outlines key considerations for production deployment, including security, performance, and monitoring requirements.

### Security Configuration

**JWT Secret Key**:
* Generate cryptographically secure key (minimum 256 bits)
* Use different keys for each environment
* Store securely using cloud key management services

**HTTPS Configuration**:
* Obtain SSL/TLS certificates (Let's Encrypt recommended)
* Configure HTTPS redirection in production
* Update CORS settings for production domains

**Environment Variables**:
* Never commit secrets to source control
* Use secure environment variable management
* Rotate API keys and secrets regularly

### Performance Optimization

**Frontend Build**:
* Use production build with optimization: `npm run build-prod`
* Enable compression and caching at reverse proxy level
* Configure CDN for static assets

**Backend Configuration**:
* Enable output caching for appropriate endpoints
* Configure connection pooling for database connections
* Implement rate limiting for API endpoints

**Database Optimization**:
* Configure connection pooling appropriately
* Implement database monitoring and alerting
* Plan for database backup and recovery

### Monitoring and Logging

**Structured Logging**:
* Configure Serilog with appropriate log levels
* Use structured logging for better searchability
* Implement log aggregation (ELK stack, Seq, etc.)

**Health Checks**:
* Monitor `/api/status` endpoint for service health
* Configure alerts for service unavailability
* Implement uptime monitoring for critical services

**Performance Monitoring**:
* Monitor API response times and throughput
* Track database query performance
* Monitor external service dependency health

### Deployment Automation

**CI/CD Pipeline**:
* Implement automated testing before deployment
* Use feature flags for gradual rollouts
* Configure automatic rollback on deployment failures

**Infrastructure as Code**:
* Define infrastructure using Terraform, ARM templates, or CloudFormation
* Version control infrastructure configurations
* Implement infrastructure monitoring and alerting

## 10. Troubleshooting Guide

This section provides solutions to common setup and configuration issues encountered during development.

### Backend API Issues

**Build Failures**:
* Verify .NET 8 SDK is installed: `dotnet --version`
* Clear build artifacts: `dotnet clean` then `dotnet restore`
* Check for missing NuGet packages: `dotnet restore`

**Database Connection Issues**:
* Verify PostgreSQL is running and accessible
* Check connection string format and credentials
* Test connection using database client tools
* Ensure database exists and migrations are applied

**External Service Connectivity**:
* Verify API keys are correctly configured
* Check network connectivity to external services
* Review service status pages for outages
* Test with minimal configuration first

**Configuration Loading Issues**:
* Verify User Secrets are properly configured: `dotnet user-secrets list`
* Check environment variable names match configuration structure
* Ensure configuration precedence order is understood
* Use `/api/status/config` endpoint to verify loaded configuration

### Frontend Application Issues

**Dependencies and Build Issues**:
* Clear Node.js cache: `npm cache clean --force`
* Delete `node_modules` and reinstall: `rm -rf node_modules && npm install`
* Verify Node.js version compatibility (18+ required)
* Check for global vs local Angular CLI version conflicts

**Development Server Issues**:
* Verify port 4200 is available
* Check for CORS configuration in backend
* Ensure backend API is running and accessible
* Review browser console for specific errors

**API Communication Issues**:
* Verify API base URL in environment configuration
* Check network tab in browser developer tools
* Ensure authentication tokens are being sent correctly
* Verify backend CORS settings include frontend origin

### Integration Testing Issues

**Docker and Testcontainers**:
* Ensure Docker Desktop is running: `docker info`
* Verify Docker group membership on Linux: `groups $USER`
* Use `sg docker -c "command"` if group membership isn't active
* Check Docker resource limits for container startup

**Test Database Issues**:
* Verify PostgreSQL image can be pulled: `docker pull postgres:15`
* Check for port conflicts with existing PostgreSQL instances
* Ensure sufficient disk space for test containers
* Review Testcontainers logs for specific errors

**External Service Mocking**:
* Verify mock configurations are loaded correctly
* Check test environment configuration files
* Ensure external service dependencies are properly isolated
* Review test logs for service availability issues

### Common Configuration Mistakes

**Case Sensitivity**:
* JSON configuration keys are case-sensitive
* Environment variable names must match exactly
* Verify nested configuration structure matches expected format

**Secret Management**:
* User Secrets only work in Development environment
* Environment variables override other configuration sources
* Verify secrets are not accidentally committed to source control

**Service Dependencies**:
* Check service startup order for dependent services
* Verify health check endpoints for service availability
* Review service logs for initialization errors
* Ensure all required configuration is present before service startup

### Getting Additional Help

**Diagnostic Endpoints**:
* `/api/status` - Overall service health
* `/api/status/config` - Configuration validation
* `/swagger` - API documentation and testing

**Log Analysis**:
* Review application logs for specific error messages
* Check structured logging output for correlation IDs
* Use log aggregation tools for pattern analysis
* Enable debug logging for detailed troubleshooting

**Community Resources**:
* Check project documentation for updated troubleshooting guides
* Review GitHub issues for similar problems
* Consult framework-specific documentation and communities

## 11. Next Steps

* Review [`../../Code/Zarichney.Server/Services/Status/README.md`](../../Code/Zarichney.Server/Services/Status/README.md) for details on the Status Service implementation.
* See [`../../Code/Zarichney.Server/Controllers/AuthController.cs`](../../Code/Zarichney.Server/Controllers/AuthController.cs) for authentication endpoints.
* Explore [`../Maintenance/PostgreSqlDatabase.md`](../Maintenance/PostgreSqlDatabase.md) for database maintenance guidance.
* Review [`../Standards/TestingStandards.md`](../Standards/TestingStandards.md) for comprehensive testing approach.
* Consult [`../../Scripts/README.md`](../../Scripts/README.md) for development automation scripts.

---