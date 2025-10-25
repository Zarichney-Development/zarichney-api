# Bug Report: [Brief Description]

## Environment

**Platform**: [Operating System, Browser, Device]

**Example**: Windows 11, Chrome 120.0.6099, Desktop

**Version**: [Application version, commit hash, deployment environment]

**Example**: v2.1.3, commit `abc123def`, production environment

**Configuration**: [Relevant settings, environment variables, feature flags]

**Example**:
- Database: PostgreSQL 15.2
- Authentication: JWT with 1-hour expiration
- Feature flags: `ENABLE_RECIPE_CACHING=true`

## Expected Behavior

[What should happen according to requirements or user expectations?]

**Example**: When a user calls `UserService.GetUserById(validUserId)` with a valid user ID that exists in the database, the service should return a `UserDto` object containing the user's profile information with a 200 OK response.

## Actual Behavior

[What actually happens? Be specific about the incorrect behavior]

**Example**: The service throws an unhandled `NullReferenceException` and returns a 500 Internal Server Error. The user profile is never retrieved, and the frontend displays a generic error message.

## Reproduction Steps

Provide detailed, numbered steps that reliably reproduce the bug:

1. [Step 1 with specific details, values, and actions]
2. [Step 2 with specific details, values, and actions]
3. [Step 3 - the point where the bug occurs]

**Example**:
1. Navigate to user profile page: `https://app.example.com/users/12345`
2. Observe browser network tab showing GET request: `/api/users/12345`
3. Service throws `NullReferenceException` at line 47 in `UserService.cs`
4. Frontend receives 500 status code and displays "Something went wrong"

**Reproducibility**: [Always / Sometimes / Rare]

**Example**: Always - occurs 100% of the time with any valid user ID

**Frequency**: [How often does this occur? Percentage or ratio]

**Example**: Affects all user profile page loads. Observed 247 errors in past 24 hours.

## Error Messages

Provide exact error text, stack traces, and relevant log entries:

```
[Paste exact error messages, stack traces, logs here]
```

**Example**:
```
System.NullReferenceException: Object reference not set to an instance of an object.
   at Zarichney.Server.Services.UserService.GetUserById(Int32 userId) in UserService.cs:line 47
   at Zarichney.Server.Controllers.UsersController.GetUser(Int32 id) in UsersController.cs:line 23
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.Execute()

[ERROR] 2025-10-25 14:32:17.823 UserService.GetUserById - NullReferenceException for userId: 12345
[ERROR] 2025-10-25 14:32:17.825 Request failed: GET /api/users/12345 - 500 Internal Server Error
```

**Browser Console** (if applicable):
```javascript
POST https://api.example.com/users/12345 500 (Internal Server Error)
Error: Failed to load user profile
    at UserProfileComponent.loadUser (user-profile.component.ts:45)
```

## Impact Assessment

### Severity

**[Critical / High / Medium / Low]**

**Severity Definitions**:
- **Critical**: System completely down, data loss, security breach, affects all users
- **High**: Major functionality broken, no workaround, affects significant user subset
- **Medium**: Functionality degraded but workaround exists, affects limited users
- **Low**: Cosmetic issue, edge case, minimal user impact

**Example**: **High** - Major functionality (user profiles) completely broken. No workaround available.

### Affected Users

[All users / Specific user segment / Edge case scenario]

**Example**: All users attempting to view any user profile. Affects approximately 2,000 daily profile views.

### Business Impact

[Revenue, operations, reputation, compliance implications]

**Example**:
- **User Experience**: Critical feature unavailable, users cannot view profiles
- **Business Operations**: Customer support tickets increased 300% in past 24 hours
- **Revenue**: Potential churn if not resolved within 48 hours
- **Reputation**: 15 negative reviews mentioning broken profiles

## Root Cause Analysis

[Technical explanation of why this bug occurs - if known]

**Example**: The `UserService.GetUserById()` method attempts to access the `_userRepository` field, but dependency injection is not properly configured in `Program.cs`. The repository is `null` at runtime, causing the `NullReferenceException` when any database query is attempted.

**Code Location**:
```csharp
// File: Code/Zarichney.Server/Services/UserService.cs
// Lines: 45-50

public async Task<UserDto> GetUserById(int userId)
{
    // BUG: _userRepository is null because DI not configured
    var user = await _userRepository.GetByIdAsync(userId); // Line 47 - NullReferenceException
    return _mapper.Map<UserDto>(user);
}
```

**Root Cause**: Missing service registration in `Program.cs`:
```csharp
// MISSING: builder.Services.AddScoped<IUserRepository, UserRepository>();
```

## Suggested Fix

[Proposed solution with code examples if available]

**Example**:

**Short-Term Fix** (immediate mitigation):
```csharp
// Add null check with error handling
public async Task<UserDto> GetUserById(int userId)
{
    if (_userRepository == null)
    {
        _logger.LogError("UserRepository is null - dependency injection not configured");
        throw new InvalidOperationException("UserRepository not available");
    }

    var user = await _userRepository.GetByIdAsync(userId);
    return _mapper.Map<UserDto>(user);
}
```

**Proper Fix** (correct root cause):
```csharp
// File: Code/Zarichney.Server/Program.cs
// Add missing dependency injection registration

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
```

**Testing Strategy**:
- Add integration test verifying DI configuration
- Add unit test with mock repository to catch future DI issues
- Validate all service dependencies registered on application startup

## Workaround

[Temporary solution users can apply while bug is being fixed]

**Example**: No workaround available. Users cannot view profiles until fix is deployed.

**Alternative** (if workaround exists):
Users can access basic profile information through the search results page, which uses a different service endpoint (`/api/search/users`) that is not affected by this bug.

## Additional Context

[Screenshots, videos, related issues, browser DevTools output, network traces]

**Screenshots**:
- [Screenshot 1: Error message displayed to user]
- [Screenshot 2: Browser DevTools showing 500 error]
- [Screenshot 3: Application logs with stack trace]

**Related Issues**:
- Similar bug: #456 - RecipeService dependency injection issue (fixed in v2.0.1)
- Regression introduced in: PR #789 - Refactor service layer
- Related documentation: `/Docs/Development/DependencyInjection.md`

**Video Recording**: [Link to Loom/screen recording demonstrating bug]

**Network Trace**:
```
Request URL: https://api.example.com/api/users/12345
Request Method: GET
Status Code: 500 Internal Server Error
Response Headers:
  Content-Type: application/json
  Date: Fri, 25 Oct 2025 14:32:17 GMT
Response Body:
{
  "error": "An unexpected error occurred",
  "traceId": "abc123-def456-789ghi"
}
```

---

**Recommended Labels**:
- `type: bug`
- `priority: high` (adjust based on severity: `critical` for system-down bugs)
- `effort: small` (or `tiny`/`medium` based on fix complexity)
- `component: api`

**Assignee**: @BackendSpecialist (or relevant domain expert)

**Related Issues**:
- Caused by: #[issue-number] - [Description of introducing change]
- Blocks: #[issue-number] - [Dependent functionality]
- Duplicate of: #[issue-number] - [If this bug was already reported]
