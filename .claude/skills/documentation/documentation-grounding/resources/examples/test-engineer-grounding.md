# TestEngineer Grounding Example

**Scenario:** TestEngineer engaged to create comprehensive test coverage for AuthService
**Issue:** #512 - Add missing test coverage for authentication service edge cases
**Module:** `Code/Zarichney.Server/Services/Auth/AuthService.cs`
**Test Target:** 85% branch coverage with all edge cases covered

---

## Phase 1: Standards Mastery

### TestingStandards.md (Comprehensive Mastery)

✅ **Core Testing Philosophy**
- Test behavior, not implementation
- AAA pattern (Arrange-Act-Assert) mandatory
- Complete isolation for unit tests
- Determinism - tests must be repeatable

✅ **Required Tooling**
- xUnit as test framework
- FluentAssertions (mandatory) with reason messages
- Moq (mandatory) for all mocking
- AutoFixture for test data generation

✅ **Test Categorization**
- Unit tests: `[Trait("Category", "Unit")]`
- Service layer tests: `[Trait("Category", "Service")]` if applicable
- Database dependency: `[Trait("Category", "Database")]` if integration test needed

✅ **Test Structure & Naming**
- Class: `AuthServiceTests.cs`
- Method: `Login_IncorrectPassword_ReturnsBadRequest`
- Folder: `Zarichney.Server.Tests/Unit/Services/Auth/`

✅ **Unit Test Standards**
- Mock ALL dependencies (IUserRepository, IPasswordHasher, ILogger, etc.)
- Verify behavior via method return values and mock verifications
- Use FluentAssertions: `result.Should().BeOfType<BadRequestResult>("because login with incorrect password should fail")`

✅ **AutoFixture Usage**
- Use for generating test data where specific values don't matter
- Create custom builders for complex objects with required relationships
- AutoFixture customizations in `Framework/TestData/AutoFixtureCustomizations/`

**Key Takeaways:**
- Every public method in AuthService needs test coverage
- Edge cases: null inputs, empty strings, invalid formats, boundary conditions
- Mock verification for side effects (logging, repository calls)

---

### CodingStandards.md (SUT Understanding Focus)

✅ **Understanding System Under Test (SUT)**
- AuthService likely uses constructor injection
- Dependencies: `IUserRepository`, `IPasswordHasher`, `ITokenGenerator`, `ILogger<AuthService>`
- Asynchronous methods return `Task<TResult>`
- Error handling: specific exceptions vs. result objects

✅ **SOLID Principles for Testability**
- SRP: AuthService should focus only on authentication logic
- DIP: All dependencies are interfaces (mockable)
- ISP: Interfaces should be lean (easy to mock)

**Key Takeaways:**
- Understand AuthService's actual dependencies before creating mocks
- Know what exceptions it throws vs. returns error results
- Understand async patterns to test correctly

---

### DocumentationStandards.md (Test Coverage Documentation)

✅ **Module README Section 5**
- Look for documented test scenarios
- Identify known edge cases
- Understand testing strategy

**Key Takeaways:**
- AuthService README may document critical test scenarios
- Section 3 (Interface Contract) defines what to test
- Section 8 (Known Issues) may reveal edge cases

---

### TaskManagementStandards.md

✅ **Branch Strategy**
- Create `test/issue-512-auth-service-coverage`
- Conventional commit: `test: add comprehensive AuthService test coverage (#512)`

---

## Phase 2: Project Architecture Context

### Test Project Structure

✅ **Test Project Organization**
- Location: `Zarichney.Server.Tests/`
- Review: `TechnicalDesignDocument.md` for test architecture

✅ **Test Framework Components**
- `Framework/Fixtures/` - Not needed for unit tests (pure isolation)
- `Framework/Helpers/` - May have reusable test utilities
- `Framework/TestData/Builders/` - Custom builders for complex objects

✅ **Existing Test Patterns**
- Review existing service tests in `Unit/Services/` for patterns
- Look for established mocking patterns
- Understand common test data setup approaches

**Key Insights:**
- Unit tests go in `Unit/Services/Auth/AuthServiceTests.cs`
- Mirror production code structure
- May leverage existing user builders from `TestData/Builders/`

---

## Phase 3: Domain-Specific Context

### Module: `Code/Zarichney.Server/Services/Auth/README.md`

**Section 1: Purpose & Responsibility**
- AuthService responsible for user authentication, login, logout, token generation
- Core security component - testing is CRITICAL

**Section 2: Architecture & Key Concepts**
- Uses password hashing for security (IPasswordHasher)
- Token-based authentication (ITokenGenerator)
- User repository for user lookup (IUserRepository)
- Stateless authentication pattern

**Section 3: Interface Contract & Assumptions** ⚠️ CRITICAL FOR TESTING

**Login Method:**
```csharp
Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct)
```

**Preconditions:**
- email: non-null, non-empty, valid email format
- password: non-null, non-empty
- CancellationToken may be default or valid token

**Postconditions:**
- Success case: LoginResult with Success=true, Token populated, User info
- Failure cases: LoginResult with Success=false, Error message describing failure
- Side effects: Log login attempts (success and failure)

**Error Scenarios:**
- User not found: Success=false, Error="User not found"
- Incorrect password: Success=false, Error="Invalid credentials"
- Account locked: Success=false, Error="Account locked"
- Invalid email format: ArgumentException (validate before calling)

**Invariants:**
- Password never logged or returned in responses
- Failed login attempts should be rate-limited (future)

**Section 4: Local Conventions**
- Password hashing algorithm: BCrypt via IPasswordHasher
- Token format: JWT with specific claims
- Token expiration: 1 hour (configurable)

**Section 5: How to Work With This Code**

**Testing Strategy:**
- **Unit tests:** All business logic with mocked dependencies
- **Integration tests:** Token generation and validation with real JWT library

**Key Test Scenarios:**
1. Successful login with valid credentials
2. Failed login - user not found
3. Failed login - incorrect password
4. Failed login - account locked (if implemented)
5. Null/empty email handling
6. Null/empty password handling
7. Invalid email format handling
8. Token generation verification
9. Logging verification (success and failure)
10. Cancellation token handling

**Section 6: Dependencies**
- `IUserRepository` - User lookup (MOCK in unit tests)
- `IPasswordHasher` - Password verification (MOCK in unit tests)
- `ITokenGenerator` - JWT token creation (MOCK in unit tests)
- `ILogger<AuthService>` - Structured logging (MOCK in unit tests)

**Testing Implication:** All dependencies are interfaces - perfect for Moq

**Section 8: Known Issues**
- No rate limiting on failed login attempts (potential brute force vulnerability)
- Account locking not yet implemented
- Password complexity requirements minimal

**Edge Cases to Test:**
- Concurrent login attempts
- Extremely long email/password strings
- Special characters in passwords
- Case sensitivity of email

---

## Context Integration Summary

### Comprehensive Test Plan for AuthService

**Test Class:** `AuthServiceTests.cs`
**Location:** `Zarichney.Server.Tests/Unit/Services/Auth/`

**Setup (Arrange):**
```csharp
private readonly Mock<IUserRepository> _userRepositoryMock;
private readonly Mock<IPasswordHasher> _passwordHasherMock;
private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
private readonly Mock<ILogger<AuthService>> _loggerMock;
private readonly AuthService _sut;

public AuthServiceTests()
{
    _userRepositoryMock = new Mock<IUserRepository>();
    _passwordHasherMock = new Mock<IPasswordHasher>();
    _tokenGeneratorMock = new Mock<ITokenGenerator>();
    _loggerMock = new Mock<ILogger<AuthService>>();

    _sut = new AuthService(
        _userRepositoryMock.Object,
        _passwordHasherMock.Object,
        _tokenGeneratorMock.Object,
        _loggerMock.Object
    );
}
```

### Test Methods to Create

**1. Success Scenarios:**

```csharp
[Fact]
[Trait("Category", "Unit")]
public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithToken()
{
    // Arrange
    var email = "user@example.com";
    var password = "ValidPassword123";
    var user = new User { Email = email, PasswordHash = "hashedPassword" };

    _userRepositoryMock
        .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
        .ReturnsAsync(user);

    _passwordHasherMock
        .Setup(x => x.VerifyPassword(password, user.PasswordHash))
        .Returns(true);

    _tokenGeneratorMock
        .Setup(x => x.GenerateToken(user))
        .Returns("valid-jwt-token");

    // Act
    var result = await _sut.LoginAsync(email, password, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue("because valid credentials should authenticate successfully");
    result.Token.Should().NotBeNullOrEmpty("because successful login should return a token");
    result.User.Should().NotBeNull("because successful login should return user info");
    result.User.Email.Should().Be(email, "because returned user should match login email");

    // Verify logging
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("successful login")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once,
        "because successful login should be logged");
}
```

**2. Error Scenarios:**

```csharp
[Fact]
[Trait("Category", "Unit")]
public async Task LoginAsync_UserNotFound_ReturnsFailure()
{
    // Arrange
    var email = "nonexistent@example.com";
    var password = "AnyPassword";

    _userRepositoryMock
        .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
        .ReturnsAsync((User)null);

    // Act
    var result = await _sut.LoginAsync(email, password, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because login should fail when user does not exist");
    result.Error.Should().Be("User not found", "because error message should indicate user not found");
    result.Token.Should().BeNullOrEmpty("because failed login should not return a token");

    // Verify password hasher was NOT called (optimization - don't hash if user doesn't exist)
    _passwordHasherMock.Verify(
        x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
        Times.Never,
        "because password verification should be skipped when user does not exist");
}

[Fact]
[Trait("Category", "Unit")]
public async Task LoginAsync_IncorrectPassword_ReturnsFailure()
{
    // Arrange
    var email = "user@example.com";
    var password = "WrongPassword";
    var user = new User { Email = email, PasswordHash = "hashedPassword" };

    _userRepositoryMock
        .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
        .ReturnsAsync(user);

    _passwordHasherMock
        .Setup(x => x.VerifyPassword(password, user.PasswordHash))
        .Returns(false);

    // Act
    var result = await _sut.LoginAsync(email, password, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because login should fail with incorrect password");
    result.Error.Should().Be("Invalid credentials", "because error message should indicate authentication failure");
    result.Token.Should().BeNullOrEmpty("because failed login should not return a token");

    // Verify token generator was NOT called
    _tokenGeneratorMock.Verify(
        x => x.GenerateToken(It.IsAny<User>()),
        Times.Never,
        "because token should not be generated for failed login");
}
```

**3. Edge Case Scenarios:**

```csharp
[Theory]
[Trait("Category", "Unit")]
[InlineData(null, "ValidPassword")]
[InlineData("", "ValidPassword")]
[InlineData("   ", "ValidPassword")]
public async Task LoginAsync_InvalidEmail_ThrowsArgumentException(string email, string password)
{
    // Act & Assert
    await FluentActions.Invoking(async () =>
        await _sut.LoginAsync(email, password, CancellationToken.None))
        .Should().ThrowAsync<ArgumentException>("because email is required and must not be null or empty")
        .WithParameterName("email");
}

[Theory]
[Trait("Category", "Unit")]
[InlineData("user@example.com", null)]
[InlineData("user@example.com", "")]
[InlineData("user@example.com", "   ")]
public async Task LoginAsync_InvalidPassword_ThrowsArgumentException(string email, string password)
{
    // Act & Assert
    await FluentActions.Invoking(async () =>
        await _sut.LoginAsync(email, password, CancellationToken.None))
        .Should().ThrowAsync<ArgumentException>("because password is required and must not be null or empty")
        .WithParameterName("password");
}

[Fact]
[Trait("Category", "Unit")]
public async Task LoginAsync_CancellationRequested_ThrowsOperationCanceledException()
{
    // Arrange
    var email = "user@example.com";
    var password = "ValidPassword";
    var cts = new CancellationTokenSource();
    cts.Cancel();

    // Act & Assert
    await FluentActions.Invoking(async () =>
        await _sut.LoginAsync(email, password, cts.Token))
        .Should().ThrowAsync<OperationCanceledException>("because operation should respect cancellation token");
}
```

**4. Logging Verification:**

```csharp
[Fact]
[Trait("Category", "Unit")]
public async Task LoginAsync_FailedLogin_LogsWarning()
{
    // Arrange
    var email = "user@example.com";
    var password = "WrongPassword";
    var user = new User { Email = email, PasswordHash = "hashedPassword" };

    _userRepositoryMock
        .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
        .ReturnsAsync(user);

    _passwordHasherMock
        .Setup(x => x.VerifyPassword(password, user.PasswordHash))
        .Returns(false);

    // Act
    await _sut.LoginAsync(email, password, CancellationToken.None);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("failed login attempt")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once,
        "because failed login attempts should be logged as warnings for security monitoring");
}
```

### Coverage Goals

**Target:** 85% branch coverage minimum
**Expected:** 95%+ with comprehensive edge case testing

**Branches Covered:**
1. User found vs. not found
2. Password correct vs. incorrect
3. Null/empty email variations
4. Null/empty password variations
5. Cancellation token respected
6. Success logging
7. Failure logging

---

## Grounding Completion Validation

- ✅ Phase 1: TestingStandards.md comprehensively mastered
- ✅ Phase 1: CodingStandards.md reviewed for SUT understanding
- ✅ Phase 2: Test project structure and patterns understood
- ✅ Phase 3: AuthService README analyzed (Section 3 interface contracts critical)
- ✅ All test scenarios identified from interface contract
- ✅ Edge cases documented from Section 8 known issues
- ✅ Mocking strategy clear for all dependencies
- ✅ Logging verification patterns understood

**Grounding Status:** ✅ **COMPLETE**

---

## How Grounding Improved Work Quality

**Without Grounding:**
- Might have missed edge cases like null/empty strings
- Could have forgotten to verify logging calls
- Might not have known about account locking edge case
- Could have used wrong test categorization traits
- Might have forgotten FluentAssertions reason messages

**With Grounding:**
- All 10 critical test scenarios identified from Section 5
- Logging verification included based on CodingStandards requirements
- Edge cases from Section 8 known issues incorporated
- Correct test categorization: `[Trait("Category", "Unit")]`
- All assertions include reason messages per TestingStandards.md

**Outcome:**
- 95% branch coverage achieved
- All edge cases covered
- Tests are maintainable and well-documented
- Zero flaky tests (proper mocking)
- ComplianceOfficer approval on first review

---

**Example Status:** ✅ Demonstrates complete 3-phase grounding for comprehensive testing
**Agent:** TestEngineer
**Skill:** documentation-grounding v1.0.0
