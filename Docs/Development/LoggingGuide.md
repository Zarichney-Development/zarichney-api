# Enhanced Logging System Guide

**Version:** 1.1
**Last Updated:** 2025-05-25

## Introduction

This guide describes the enhanced logging system implemented in the Zarichney API project. The logging system has been designed with the following goals:

- **Quieter by Default:** The application defaults to Warning level logging to reduce noise in production environments
- **Highly Configurable:** Log levels can be easily adjusted through configuration files without code changes
- **Environment-Specific:** Different environments (Development, Testing, Production) can have tailored logging configurations
- **Namespace-Specific Control:** Fine-grained control over logging levels for specific namespaces or classes

## Code Examples

For detailed coding conventions regarding logger usage, message formatting, structured property naming, and error logging, please refer to the 'Logging Standards' section in the main [`Docs/Standards/CodingStandards.md`](../Standards/CodingStandards.md) document.

### Logger Injection

The standard way to use logging in the application is through constructor dependency injection with `ILogger<T>`:

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoSomething(string parameter1, int parameter2)
    {
        _logger.LogInformation("DoSomething called with Parameter1: {Parameter1}, Parameter2: {Parameter2}", 
            parameter1, parameter2);
        
        // ... business logic ...
        
        _logger.LogInformation("DoSomething completed successfully for Parameter1: {Parameter1}", parameter1);
    }
}
```

### Structured Logging Examples

Use structured logging with named placeholders for better searchability and filtering in log management systems:

```csharp
public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessOrderAsync(string orderId, string userId)
    {
        _logger.LogInformation("Processing order {OrderId} for user {UserId}", orderId, userId);
        
        try
        {
            // Validate order
            var order = await ValidateOrder(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} validation failed: Order not found", orderId);
                return;
            }

            // Process payment
            _logger.LogInformation("Processing payment for order {OrderId}, Amount: {Amount}", 
                orderId, order.TotalAmount);
            
            await ProcessPayment(order);
            
            _logger.LogInformation("Order {OrderId} processed successfully for user {UserId}", orderId, userId);
        }
        catch (PaymentException ex)
        {
            _logger.LogWarning(ex, "Payment failed for order {OrderId}: {ErrorMessage}", orderId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing order {OrderId}", orderId);
            throw;
        }
    }
}
```

### Key Benefits of Structured Logging

- **Searchability:** You can search for all logs related to a specific `OrderId` or `UserId`
- **Filtering:** Log management systems can filter by structured properties
- **Correlation:** Related operations can be easily correlated using common identifiers
- **Performance:** More efficient than string interpolation for log processing

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

When debugging specific tests, you can temporarily modify `api-server/appsettings.Testing.json` to get more detailed output from the system under test (SUT). The test logs will appear in your test runner's output window.

**Current Test Configuration:**
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

**Examples for Debugging Specific Tests:**

To debug a specific test class in verbose detail:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Zarichney": "Warning",
        // Example: Debug authentication integration tests
        "Zarichney.Services.Auth.AuthService": "Verbose",
        "Zarichney.Services.Auth.ApiKeyService": "Debug"
      }
    }
  }
}
```

To debug all tests in a specific namespace:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning", 
        "Zarichney": "Warning",
        // Example: Debug all recipe-related services during tests
        "Zarichney.Cookbook.Recipes": "Debug",
        "Zarichney.Services.AI": "Information"
      }
    }
  }
}
```

**Common Test Debugging Scenarios:**

For debugging controller tests:
```json
"Override": {
  "Zarichney.Controllers.AuthController": "Debug",
  "Zarichney.Services.Auth": "Verbose"
}
```

For debugging database integration tests:
```json
"Override": {
  "Zarichney.Services.Auth.UserDbContext": "Information",
  "Microsoft.EntityFrameworkCore.Database.Command": "Information"
}
```

For debugging payment processing tests:
```json
"Override": {
  "Zarichney.Services.Payment": "Debug",
  "Zarichney.Controllers.PaymentController": "Debug"
}
```

**Important Notes:**
- These overrides only affect SUT (System Under Test) logs during test execution
- Test framework logs are handled separately by the xUnit output sink
- Remember to revert changes after debugging to keep test output clean
- Use `dotnet test --verbosity detailed` for more test framework output if needed

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
