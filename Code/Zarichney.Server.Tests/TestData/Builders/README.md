# Module/Directory: /Zarichney.Server.Tests/TestData/Builders

**Last Updated:** 2025-09-20

> **Parent:** [`Zarichney.Server.Tests/TestData/`](../TestData/README.md) (Assuming a parent TestData README exists or will be created)
> If `/Zarichney.Server.Tests/TestData/README.md` doesn't exist, the parent link should be:
> **Parent:** [`Zarichney.Server.Tests`](../../README.md)


## 1. Purpose & Responsibility

* **What it is:** Contains builder classes designed to facilitate the creation of complex test data objects (domain entities, DTOs) in a readable and maintainable way.
* **Key Responsibilities:**
    * Providing a fluent interface (`WithProperty(value)`) for constructing test objects.
    * Encapsulating the logic for creating valid object instances with default values.
    * Allowing easy customization of specific properties for different test scenarios.
    * Potentially integrating with `AutoFixture` (via helpers like `GetRandom`) for generating random data for non-critical properties.
* **Why it exists:** To simplify the Arrange phase of tests, improve test readability, reduce code duplication in test setup, and ensure test data is created consistently according to object constraints.

## 2. Architecture & Key Concepts

* **Base Class:** Builders typically inherit from `BaseBuilder<TBuilder, TEntity>`, which provides the core `Build()` method and fluent interface support (`Self()`).
* **Individual Builders:** Each builder class (e.g., `RecipeBuilder`) is responsible for constructing instances of a specific entity or DTO (`Recipe`).
* **Constructor / Static Factory:** Builders often have a default constructor setting baseline valid properties and potentially a static factory method (e.g., `CreateRandom()`) for creating instances with randomized data.
* **`With...` Methods:** Fluent methods (`WithId`, `WithTitle`, etc.) allow specific properties of the underlying entity to be set.
* **`Build()` Method:** Returns the fully constructed entity instance.
* **Integration with Random Data:** May use helpers like `GetRandom` (which likely uses `AutoFixture` internally) to populate properties with random data.

## 3. Interface Contract & Assumptions

* **`BuilderName()` (Constructor):** Creates a builder instance initialized with baseline valid default values for the target entity.
* `BuilderName.WithProperty(value)`: Sets a specific property on the entity being built. Returns the builder instance for chaining.
* `BuilderName.Build()`: Returns the constructed `TEntity` instance.
* **Critical Assumptions:**
    * Assumes the default values set in the builder constructor result in a valid entity instance according to domain rules.
    * Assumes the target entity class (`TEntity`) has a parameterless constructor or that the builder correctly handles required constructor parameters.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Naming:** Builders are named `[EntityName]Builder`. Fluent methods are named `With[PropertyName]`.
* **Structure:** Follows the `BaseBuilder<TBuilder, TEntity>` pattern.
* **Default Values:** Builders should aim to provide sensible, valid defaults in their constructors.

## 5. How to Work With This Code

* **Using Builders:** Instantiate a builder, chain `With...` methods to customize properties, and call `Build()` to get the final object. Example: `var recipe = new RecipeBuilder().WithTitle("Custom Title").WithServings("6").Build();`
* **Adding a New Builder:**
    1.  Create a new class `MyEntityBuilder` inheriting `BaseBuilder<MyEntityBuilder, MyEntity>`.
    2.  Add a constructor to set default values for `MyEntity`.
    3.  Add `With[PropertyName]` methods for relevant properties of `MyEntity`.
    4.  Consider adding a static `CreateRandom()` method if needed.
* **Testing:** Builders themselves are rarely unit tested directly. Their correctness is validated by the tests that use them to create data.

## 6. Dependencies

* **Internal Code Dependencies:**
    * Domain models and DTOs defined in `Zarichney.Server`: `ApiErrorResult`, `ApplicationUser`, `AuthResult`, `CheckoutSessionInfo`, `ConfigurationItemStatus`, `CookbookOrder`, `CookbookOrderSubmission`, `Customer`, `HealthCheckResponse`, `LlmConversation`, `Recipe`, `RefreshToken`, `ScrapedRecipe`, `Session`, `SynthesizedRecipe`, `TranscribeConfig`, `TranscriptionResult`, payment models, and authentication entities.
    * Test framework helpers: [`/Framework/Helpers/`](../../Framework/Helpers/README.md) for random data generators like `GetRandom`.
* **External Library Dependencies:**
    * Potentially `AutoFixture` (if used by `GetRandom`).
* **Dependents (Impact of Changes):**
    * All unit and integration tests that use these builders for test data setup. Changes to default values or `With...` methods can affect many tests.

## 7. Rationale & Key Historical Context

* The Builder pattern provides a robust and readable alternative to complex object instantiation in test setup code, especially for objects with many properties or complex validation rules. It promotes the DRY principle in test data creation.

## 8. Known Issues & TODOs

* Need to implement builders for other core entities/DTOs as identified in the TDD (e.g., `User`, `Customer`, `Order`).

---

## Enhanced Builder Patterns (Added 2025-09-10)

### EmailValidationResponseBuilder - Intention-Revealing Methods

The `EmailValidationResponseBuilder` has been enhanced with intention-revealing method names that make test scenarios more explicit:

#### New Enhanced Methods:
- **`AsValidEmail()`** - Creates a valid email validation response with safe defaults
- **`AsInvalidEmail()`** - Creates an invalid email validation response with syntax errors
- **`AsBlockedEmail()`** - Creates a blocked email validation response for restricted domains
- **`AsDisposableEmail()`** - Creates a disposable email validation response for temporary mail services
- **`AsHighRiskEmail(riskScore)`** - Creates a high-risk email validation response with configurable risk score
- **`WithTypoSuggestions(suggestions)`** - Creates an email with possible typo corrections

#### Usage Examples:
```csharp
// Clear intention for AI coders - immediately understand the test scenario
var validEmailResponse = new EmailValidationResponseBuilder()
    .AsValidEmail()
    .Build();

var blockedEmailResponse = new EmailValidationResponseBuilder()
    .AsBlockedEmail()
    .WithDomain("spammer.com")
    .Build();

var disposableEmailResponse = new EmailValidationResponseBuilder()
    .AsDisposableEmail()
    .WithRisk(85)
    .Build();

var typoEmailResponse = new EmailValidationResponseBuilder()
    .WithTypoSuggestions("john@gmail.com", "john@hotmail.com")
    .Build();
```

#### Backward Compatibility:
All original method names remain available as aliases:
- `Valid()` → `AsValidEmail()`
- `Invalid()` → `AsInvalidEmail()`
- `Blocked()` → `AsBlockedEmail()`
- `Disposable()` → `AsDisposableEmail()`
- `HighRisk()` → `AsHighRiskEmail()`
- `WithTypos()` → `WithTypoSuggestions()`

### PaymentConfigBuilder - Test Scenario Methods

The `PaymentConfigBuilder` has been enhanced with scenario-specific methods for common testing patterns:

#### New Enhanced Methods:
- **`ForDevelopmentTesting()`** - Creates PaymentConfig with standard development/testing values
- **`WithMinimalConfiguration()`** - Creates PaymentConfig with minimal valid configuration for basic testing

#### Usage Examples:
```csharp
// Development testing scenario - consistent test values
var devConfig = new PaymentConfigBuilder()
    .ForDevelopmentTesting()
    .Build();

// Minimal configuration for simple tests
var minimalConfig = new PaymentConfigBuilder()
    .WithMinimalConfiguration()
    .Build();

// Custom scenario building on standard patterns
var customConfig = new PaymentConfigBuilder()
    .ForDevelopmentTesting()
    .WithRecipePrice(4.99m)
    .WithCurrency("eur")
    .Build();
```

### Comprehensive Builder Library (Added 2025-01-20)

The builder library has been significantly expanded with 20+ builders covering all major domain entities and testing scenarios:

#### Authentication & Identity Builders
- **`ApplicationUserBuilder`** - ASP.NET Identity user creation with roles and claims
- **`AuthResultBuilder`** - Authentication result scenarios (success, failure, locked accounts)
- **`RefreshTokenBuilder`** - Token refresh testing with expiration scenarios

#### API & Error Response Builders
- **`ApiErrorResultBuilder`** - HTTP error response testing with status codes and messages
- **`ApiKeyBuilder`** - API key management testing with validation and permissions
- **`CheckoutSessionInfoBuilder`** - Payment checkout session testing with transaction data
- **`HealthCheckResponseBuilder`** - Health endpoint response testing with service status

#### Cookbook Domain Builders
- **`CookbookOrderBuilder`** - Order management testing with items and pricing
- **`CookbookOrderSubmissionBuilder`** - Order submission workflow testing
- **`CustomerBuilder`** - Customer entity testing with preferences and history
- **`RecipeBuilder`** - Recipe entity testing (significantly enhanced with 200+ lines)
- **`ScrapedRecipeBuilder`** - Web scraping result testing with parsing scenarios
- **`SynthesizedRecipeBuilder`** - AI recipe generation testing with quality metrics

#### AI & Transcription Builders
- **`LlmConversationBuilder`** - AI conversation testing with message history management
- **`LlmServiceBuilder`** - AI service configuration testing for various scenarios
- **`TranscribeConfigBuilder`** - Audio transcription configuration testing
- **`TranscriptionResultBuilder`** - Audio transcription result testing with confidence scores

#### Session & Configuration Builders
- **`SessionBuilder`** - Session management testing with lifecycle scenarios
- **`ConfigurationItemStatusBuilder`** - Configuration status testing for system health

#### Usage Examples:
```csharp
// Authentication scenario
var user = new ApplicationUserBuilder()
    .WithEmail("admin@test.com")
    .WithRole("Administrator")
    .WithClaims("CanManageUsers", "CanViewReports")
    .Build();

var authResult = new AuthResultBuilder()
    .AsSuccessful()
    .WithUser(user)
    .WithTokenExpiry(TimeSpan.FromHours(1))
    .Build();

// Order management scenario
var order = new CookbookOrderBuilder()
    .WithCustomer(customer)
    .WithItems(3)
    .WithTotalPrice(29.99m)
    .AsCompleted()
    .Build();

// AI conversation scenario
var conversation = new LlmConversationBuilder()
    .WithSessionId("test-session")
    .WithMessages(5)
    .WithLastMessage("Test completion")
    .Build();

// Error response scenario
var errorResponse = new ApiErrorResultBuilder()
    .WithStatusCode(HttpStatusCode.BadRequest)
    .WithMessage("Validation failed")
    .WithDetails("Email field is required")
    .Build();

// Payment checkout scenario
var checkoutSession = new CheckoutSessionInfoBuilder()
    .WithAmount(29.99m)
    .WithCurrency("usd")
    .WithCustomer(customer)
    .AsSuccessful()
    .Build();

// API key management scenario
var apiKey = new ApiKeyBuilder()
    .WithPermissions("read", "write")
    .WithExpiryDays(30)
    .AsActive()
    .Build();
```

### Builder Pattern Benefits for AI Assistants

The comprehensive builder library provides:

1. **Complete Domain Coverage**: Builders for all major entities enable comprehensive testing scenarios
2. **Intention-Revealing Methods**: Clear method names express test scenarios (AsSuccessful, WithItems, etc.)
3. **Scenario-Based Testing**: Pre-built patterns for common testing scenarios
4. **Consistent Test Data**: Standardized approach to test data creation across all domains
5. **Maintenance Efficiency**: Centralized test data creation reduces duplication and improves maintainability

### AI Coder Benefits

These enhanced builders provide several benefits for stateless AI assistants:

1. **Complete Entity Coverage**: 22 builders cover all major domain entities and testing scenarios
2. **Intention-Revealing Names**: Method names clearly express the test scenario being created
3. **Reduced Cognitive Load**: AI coders can quickly understand test data setup across all domains
4. **Consistent Test Patterns**: Standard scenarios ensure consistent test data across the entire test suite
5. **Backward Compatibility**: Existing tests continue to work while new tests can use enhanced methods
6. **Comprehensive Scenario Support**: Authentication, orders, AI, sessions, APIs, and configuration scenarios all supported

### Design Philosophy

The enhanced builder patterns follow these principles:

- **Explicit Over Implicit**: Method names should clearly state the intended scenario
- **Scenario-Driven**: Methods represent common testing scenarios, not just property setters
- **Backward Compatible**: All existing method names remain functional
- **AI-Friendly**: Names and patterns optimized for stateless AI assistant understanding
- **Test Clarity**: Reading a test should immediately reveal the scenario being tested