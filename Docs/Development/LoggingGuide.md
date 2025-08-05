# Enhanced Logging System Guide

**Version:** 1.4
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

## Request Correlation ID

The application automatically adds a correlation ID to every HTTP request for enhanced log traceability and distributed system debugging.

### How It Works

- **Incoming Header Detection:** If a request includes an `X-Correlation-ID` header, the application uses its value as the correlation ID
- **Automatic Generation:** If no correlation ID header is provided, the application generates a new GUID for the request
- **Log Context Integration:** The correlation ID is automatically included in all log events generated during request processing
- **Response Header:** The correlation ID (either received or generated) is included in the response headers as `X-Correlation-ID`

### Benefits

- **Request Tracing:** Track a single request's journey through all log entries, even across different services
- **Debugging:** Easily filter logs to see only entries related to a specific problematic request
- **Distributed Tracing:** When multiple services forward the same correlation ID, you can trace requests across service boundaries
- **Client Correlation:** Clients can generate their own correlation IDs to correlate server-side logs with client-side events

### Usage Examples

**Client sending correlation ID:**
```http
GET /api/recipes HTTP/1.1
Host: zarichney.com/api
X-Correlation-ID: user-generated-12345
```

**Server response includes correlation ID:**
```http
HTTP/1.1 200 OK
X-Correlation-ID: user-generated-12345
Content-Type: application/json
```

**Log output with correlation ID:**
```
[14:30:15 INF] a1b2c3d4-e5f6-7890-abcd-ef1234567890 session-123 scope-456 Processing order OrderId: "ORD-001" for user UserId: "user-789"
[14:30:15 WRN] a1b2c3d4-e5f6-7890-abcd-ef1234567890 session-123 scope-456 Order OrderId: "ORD-001" validation failed: Order not found
```

### Template Format

The console log template includes the correlation ID as the first contextual identifier:
```
[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId:-} {SessionId:-} {ScopeId:-} {Message:lj}{NewLine}{Exception}
```

When no correlation ID is available (such as during application startup), it displays as "-".

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

### Production Configuration (`Zarichney.Server/appsettings.json`)

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

### Development Configuration (`Zarichney.Server/appsettings.Development.json`)

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

### Test Configuration (`Zarichney.Server/appsettings.Testing.json`)

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

When debugging specific tests, you can temporarily modify `Zarichney.Server/appsettings.Testing.json` to get more detailed output from the system under test (SUT). The test logs will appear in your test runner's output window.

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

### Adding Test Context to Logs (Test Class/Method Name)

The logging system supports enriching log events with test-specific contextual information, making it easier to identify which test generated specific log entries when debugging test failures or analyzing test behavior.

#### Benefits

- **Test Identification:** Easily identify which test class and method generated specific log entries
- **Debugging:** When tests fail, you can quickly filter logs to see only entries from the failing test
- **Analysis:** Understand the flow of execution within specific tests
- **Correlation:** Connect log entries to the specific test scenario that generated them

#### Implementation Pattern

The test context enrichment uses `Serilog.Context.LogContext.PushProperty()` to add `TestClassName` and `TestMethodName` properties to log events during test execution.

**Base Class Setup:**
```csharp
// In IntegrationTestBase.cs constructor
public IntegrationTestBase(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
{
    _apiClientFixture = apiClientFixture;
    apiClientFixture.AttachToSerilog(testOutputHelper);
    
    // Push test class name to logging context for all tests in this class
    _testClassContext = Serilog.Context.LogContext.PushProperty("TestClassName", GetType().Name);
}

// Cleanup in DisposeAsync
public Task DisposeAsync()
{
    _testClassContext?.Dispose();
    return Task.CompletedTask;
}
```

**Individual Test Method:**
```csharp
[Fact]
public async Task GetHealth_WhenCalled_ReturnsOkStatus()
{
    using (CreateTestMethodContext(nameof(GetHealth_WhenCalled_ReturnsOkStatus)))
    {
        // Arrange
        // Any log events generated within this using block will include:
        // - TestClassName: "PublicControllerIntegrationTests" 
        // - TestMethodName: "GetHealth_WhenCalled_ReturnsOkStatus"
        
        // Act & Assert
        var act = () => ApiClient.Health();
        await act.Should().NotThrowAsync<ApiException>();
    }
}
```

**Helper Method Usage:**
```csharp
protected IDisposable CreateTestMethodContext(string testMethodName)
{
    return Serilog.Context.LogContext.PushProperty("TestMethodName", testMethodName);
}
```

#### Log Output Format

When test context properties are added, they appear in the console log template:

```
[14:30:15 INF] a1b2c3d4-e5f6 session-123 scope-456 PublicControllerIntegrationTests GetHealth_WhenCalled_ReturnsOkStatus Processing health check request
[14:30:15 INF] a1b2c3d4-e5f6 session-123 scope-456 PublicControllerIntegrationTests GetHealth_WhenCalled_ReturnsOkStatus Health check completed successfully
```

**Template Format:**
```
[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId:-} {SessionId:-} {ScopeId:-} {TestClassName:-} {TestMethodName:-} {Message:lj}{NewLine}{Exception}
```

#### Usage Guidelines

1. **Test Class Context:** Automatically applied in `IntegrationTestBase` constructor for all integration tests
2. **Test Method Context:** Use `CreateTestMethodContext(nameof(YourTestMethod))` within a `using` statement in individual test methods
3. **Scope Management:** The `using` statement ensures proper cleanup of the test method context
4. **Naming Convention:** Use `nameof()` to get the method name to avoid typos and enable refactoring support

#### Non-Test Logs

For logs generated outside of test contexts (such as during normal application operation), the test context properties will display as "-" in the log output, ensuring consistent formatting without errors.

## Viewing Logs

Logs can be viewed through several channels depending on your environment and configuration:

### Console Output
- **Development:** All logs appear in the console where the application is running
- **Testing:** Logs appear in the xUnit test output via the injectable test output sink

### Seq (Structured Logging)
- Configure the `LoggingConfig:SeqUrl` in your `appsettings.json` to send logs to a Seq server
- Default development URL: `http://localhost:5341/`

### File Logging
- When Seq is not available, logs are written to `logs/Zarichney.Server.log`
- Files are rotated daily with 7 days retention

### Test Output
- Integration tests use the `InjectableTestOutput` sink to route logs to xUnit test output
- Logs appear in your test runner's output window

#### Test Output Format

**Template Limitations:** The `Serilog.Sinks.XUnit.Injectable` sink (version 3.0.64) does not support custom output templates or formatters. The `WriteTo.InjectableTestOutput()` method only accepts the sink instance and does not provide parameters for `outputTemplate`, `formatter`, or other formatting options.

**Current Behavior:** Test logs use the same format as defined in the main application's console template from `ConfigurationStartup.cs`:
```
[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId:-} {SessionId:-} {ScopeId:-} {TestClassName:-} {TestMethodName:-} {Message:lj}{NewLine}{Exception}
```

**Investigation Summary:** During logging system enhancement, we investigated implementing a test-optimized output template (e.g., `[{Level:u3}] {TestClassName:-}.{TestMethodName:-} >> {Message:lj}{NewLine}{Exception}`) to make test logs more concise. However, this was not feasible with the current `InjectableTestOutput` sink implementation.

**Alternatives Considered:**
- Custom `outputTemplate` parameter: Not supported by the sink
- Custom `ITextFormatter` parameter: Not supported by the sink
- Manual logger configuration without `.ReadFrom.Configuration()`: Still requires the sink to accept formatting parameters

**Current Solution:** Test logs inherit the main application's logging configuration via `.ReadFrom.Configuration()`, ensuring consistent formatting and property enrichment across both application and test environments.

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
- [`../../Zarichney.Server/Startup/README.md`](../../Zarichney.Server/Startup/README.md) - Core logging configuration
- [`../../Zarichney.Server.Tests/README.md`](../../Zarichney.Server.Tests/README.md) - Test environment logging setup
