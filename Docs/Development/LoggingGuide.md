# Enhanced Logging System Guide

**Version:** 1.0
**Last Updated:** 2025-01-25

## Introduction

This guide describes the enhanced logging system implemented in the Zarichney API project. The logging system has been designed with the following goals:

- **Quieter by Default:** The application defaults to Warning level logging to reduce noise in production environments
- **Highly Configurable:** Log levels can be easily adjusted through configuration files without code changes
- **Environment-Specific:** Different environments (Development, Testing, Production) can have tailored logging configurations
- **Namespace-Specific Control:** Fine-grained control over logging levels for specific namespaces or classes

## Default Logging Level

The application's default logging level is now set to **Warning**. This means that by default, only the following log levels will be displayed:

- **Warning:** Potential issues that don't prevent the application from functioning
- **Error:** Error events that might still allow the application to continue running
- **Fatal:** Very severe error events that will presumably lead to application abort

Lower-level messages (`Information`, `Debug`, `Verbose`) are filtered out unless explicitly overridden through configuration.

## Configuring Log Levels (Application)

### Basic Configuration Structure

Log levels are controlled through the `"Serilog"` section in your `appsettings.json` files. The basic structure is:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Namespace.Or.Class": "LogLevel"
      }
    }
  }
}
```

### Production Configuration (`api-server/appsettings.json`)

The production configuration maintains quiet defaults while allowing more detailed logging for application-specific namespaces:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Zarichney": "Information"
      }
    }
  }
}
```

### Development Configuration (`api-server/appsettings.Development.json`)

Development environments can use more verbose logging for easier debugging:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Zarichney": "Debug",
        "Zarichney.Services.Auth": "Verbose",
        "Zarichney.Cookbook.Recipes.RecipeService": "Debug"
      }
    }
  }
}
```

### Common Override Examples

**For Information Level (general application flow):**
```json
"Override": {
  "Zarichney": "Information"
}
```

**For Debug Level (detailed application flow):**
```json
"Override": {
  "Zarichney.Services": "Debug"
}
```

**For Verbose Level (very detailed tracing):**
```json
"Override": {
  "Zarichney.Services.Auth.AuthService": "Verbose"
}
```

**For Specific Controllers:**
```json
"Override": {
  "Zarichney.Controllers.AuthController": "Debug"
}
```

## Logging in the Test Environment

### Test Configuration (`api-server/appsettings.Testing.json`)

The test environment also defaults to Warning level to keep test output clean:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Zarichney": "Warning"
      }
    }
  }
}
```

### Debugging Tests with Verbose Logging

When debugging specific tests, you can temporarily modify `appsettings.Testing.json` to get more detailed output:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Zarichney": "Debug",
        "Zarichney.Services.Auth": "Verbose"
      }
    }
  }
}
```

### Test-Specific Logging

For debugging specific test scenarios, you can target particular namespaces:

```json
"Override": {
  "Zarichney.Tests.Integration.Services.Auth": "Debug",
  "Zarichney.Services.Auth.AuthService": "Verbose"
}
```

## Viewing Logs

Logs can be viewed through several channels depending on your environment and configuration:

### Console Output
- **Development:** All logs appear in the console where the application is running
- **Testing:** Logs appear in the xUnit test output via the injectable test output sink

### Seq (Structured Logging)
- Configure the `LoggingConfig:SeqUrl` in your `appsettings.json` to send logs to a Seq server
- Default development URL: `http://localhost:5341/`

### File Logging
- When Seq is not available, logs are written to `logs/api-server.log`
- Files are rotated daily with 7 days retention

### Test Output
- Integration tests use the `InjectableTestOutput` sink to route logs to xUnit test output
- Logs appear in your test runner's output window

## Best Practices

### Production Environments
- **Keep Default Levels Conservative:** Use `Warning` or `Information` as the default to avoid performance impact and log noise
- **Use Targeted Overrides:** Instead of setting global verbose logging, target specific namespaces that need detailed logging
- **Monitor Log Volume:** Verbose logging can significantly impact performance and storage

### Development Environments
- **Use Environment-Specific Files:** Leverage `appsettings.Development.json` for more detailed development-time logging
- **Target Problem Areas:** Use specific namespace overrides to focus on the components you're debugging
- **Reset After Debugging:** Remember to revert verbose settings after debugging sessions

### Testing Environments
- **Keep Tests Clean:** Default to `Warning` level to keep test output focused on test results
- **Temporary Debug Settings:** Only increase verbosity temporarily when debugging specific test failures
- **Use Targeted Overrides:** Focus logging on the specific components being tested

### Configuration Examples

**For API Development:**
```json
"Override": {
  "Zarichney.Controllers": "Information",
  "Zarichney.Services.Auth": "Debug"
}
```

**For Database Debugging:**
```json
"Override": {
  "Microsoft.EntityFrameworkCore": "Information",
  "Zarichney.Services.Auth.UserDbContext": "Debug"
}
```

**For Recipe Processing:**
```json
"Override": {
  "Zarichney.Cookbook": "Information",
  "Zarichney.Services.AI": "Debug"
}
```

## Environment Configuration Priority

Configuration sources are applied in the following order (later sources override earlier ones):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific)
3. User Secrets (Development only)
4. Environment Variables (highest priority)

This allows you to override logging settings at any level without modifying the base configuration files.

## Troubleshooting

### Logs Not Appearing
- Verify the log level is set appropriately for the namespace generating the logs
- Check that the logger is properly configured in the target class
- Ensure the environment-specific configuration file is being loaded

### Too Many Logs
- Increase the minimum level for noisy namespaces (e.g., set Microsoft to `Warning`)
- Use more specific namespace overrides instead of broad patterns
- Consider the performance impact of verbose logging in production

### Test Logs Missing
- Check `appsettings.Testing.json` configuration
- Verify the test is using the `CustomWebApplicationFactory`
- Ensure the test output sink is properly configured

---

For implementation details and technical specifications, refer to:
- [`../../api-server/Startup/README.md`](../../api-server/Startup/README.md) - Core logging configuration
- [`../../api-server.Tests/README.md`](../../api-server.Tests/README.md) - Test environment logging setup
