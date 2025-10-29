# Example: Bug Report with Comprehensive Reproduction

This example demonstrates creating a bug report using systematic context collection and the bug-report-template.

## Scenario

Production error reported: "UserService.GetUserById returns 500 Internal Server Error for all valid user IDs"

## Phase 1: Context Collection (Automated)

### 1.1 Error Message Collection

**Production logs**:
```bash
$ grep "UserService" logs/production-2025-10-25.log

[ERROR] 2025-10-25 14:32:17.823 UserService.GetUserById - NullReferenceException for userId: 12345
[ERROR] 2025-10-25 14:32:18.156 UserService.GetUserById - NullReferenceException for userId: 67890
[ERROR] 2025-10-25 14:32:19.442 UserService.GetUserById - NullReferenceException for userId: 11111
```

**Stack trace from error monitoring**:
```
System.NullReferenceException: Object reference not set to an instance of an object.
   at Zarichney.Server.Services.UserService.GetUserById(Int32 userId) in UserService.cs:line 47
   at Zarichney.Server.Controllers.UsersController.GetUser(Int32 id) in UsersController.cs:line 23
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.Execute()
```

### 1.2 Code Analysis

**Find affected files**:
```bash
$ glob "**/*UserService*.cs"

Code/Zarichney.Server/Services/UserService.cs
Code/Zarichney.Server.Tests/Services/UserServiceTests.cs
```

**Read source code at failure point**:
```csharp
// File: Code/Zarichney.Server/Services/UserService.cs
// Lines: 40-52

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public async Task<UserDto> GetUserById(int userId)
    {
        // Line 47 - NullReferenceException occurs here
        var user = await _userRepository.GetByIdAsync(userId);
        return _mapper.Map<UserDto>(user);
    }
}
```

**Check dependency injection configuration**:
```bash
$ grep -A 5 "AddScoped.*User" Code/Zarichney.Server/Program.cs

builder.Services.AddScoped<IUserService, UserService>();
// MISSING: builder.Services.AddScoped<IUserRepository, UserRepository>();
```

**ROOT CAUSE IDENTIFIED**: `IUserRepository` not registered in DI container, causing `_userRepository` to be null.

### 1.3 Recent Changes Analysis

```bash
$ gh pr list --search "UserService" --state merged --limit 5

#789 - Refactor service layer to use dependency injection [MERGED 2 days ago]
  Files changed: UserService.cs, RecipeService.cs, CookbookService.cs
  Note: This PR likely introduced the bug by refactoring to DI but missing registration
```

**Review PR #789**:
```bash
$ gh pr view 789 --json files

Files changed:
- Code/Zarichney.Server/Services/UserService.cs [MODIFIED]
- Code/Zarichney.Server/Services/RecipeService.cs [MODIFIED]
- Code/Zarichney.Server/Program.cs [MODIFIED]

Changes to Program.cs:
+ builder.Services.AddScoped<IUserService, UserService>();
+ builder.Services.AddScoped<IRecipeService, RecipeService>();
// MISSING: IUserRepository registration
```

**REGRESSION**: Introduced in PR #789, merged to main 2 days ago

### 1.4 Impact Assessment

**Error frequency**:
```bash
$ grep -c "UserService.GetUserById" logs/production-2025-10-25.log
247
```

**Affected endpoints**:
- `GET /api/users/{id}` - User profile retrieval (broken)
- `GET /api/users/me` - Current user profile (broken)
- All profile-related functionality non-functional

**User impact**: 100% of users attempting to view any profile (2,000+ users, ~500 profile views/day)

### 1.5 Reproduction Steps

**Local reproduction**:
```bash
# 1. Start application
$ dotnet run --project Code/Zarichney.Server

# 2. Call API endpoint
$ curl https://localhost:5000/api/users/12345

# 3. Observe error
HTTP 500 Internal Server Error
{
  "error": "An unexpected error occurred",
  "traceId": "abc123-def456-789ghi"
}
```

**Reproducibility**: Always - 100% failure rate for any user ID

## Phase 2: Template Selection

**Template Chosen**: Bug Report Template

**Rationale**: System functionality broken, not working as expected. Production issue with high impact.

## Phase 3: Issue Construction

### Title

```
Bug: UserService.GetUserById returns 500 for all valid user IDs
```

### Description (Using Template)

```markdown
# Bug Report: UserService.GetUserById Returns 500 Internal Server Error

## Environment

**Platform**: Production environment (Linux, Docker)

**Version**: v2.1.3, commit `abc123def`, deployed 2025-10-24

**Configuration**:
- Database: PostgreSQL 15.2 on AWS RDS
- Authentication: JWT with 1-hour expiration
- Deployment: Docker container on AWS ECS
- Feature flags: All defaults

## Expected Behavior

When a user calls `UserService.GetUserById(validUserId)` with a valid user ID that exists in the database, the service should:
1. Query the database via `IUserRepository.GetByIdAsync(userId)`
2. Map the User entity to UserDto using AutoMapper
3. Return UserDto with 200 OK status

**Example expected response**:
```json
{
  "id": 12345,
  "username": "john.doe",
  "email": "john@example.com",
  "created": "2025-09-01T00:00:00Z"
}
```

## Actual Behavior

The service throws an unhandled `System.NullReferenceException` and returns 500 Internal Server Error. The user profile is never retrieved from the database.

**Actual response**:
```json
{
  "error": "An unexpected error occurred",
  "traceId": "abc123-def456-789ghi"
}
```

**User impact**: Frontend displays generic error message: "Something went wrong. Please try again later."

## Reproduction Steps

1. Deploy application with commit `abc123def` (v2.1.3)
2. Navigate to user profile page: `https://app.zarichney.com/users/12345`
3. Observe browser network tab showing GET request: `/api/users/12345`
4. Service throws `NullReferenceException` at line 47 in `UserService.cs`
5. Frontend receives 500 status code and displays error message

**Reproducibility**: **Always** - occurs 100% of the time with any valid user ID

**Frequency**: 247 errors logged in past 24 hours (all user profile page loads)

## Error Messages

**Server logs**:
```
[ERROR] 2025-10-25 14:32:17.823 UserService.GetUserById - NullReferenceException for userId: 12345
System.NullReferenceException: Object reference not set to an instance of an object.
   at Zarichney.Server.Services.UserService.GetUserById(Int32 userId) in UserService.cs:line 47
   at Zarichney.Server.Controllers.UsersController.GetUser(Int32 id) in UsersController.cs:line 23
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.Execute()
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeActionMethodAsync()

[ERROR] 2025-10-25 14:32:17.825 Request failed: GET /api/users/12345 - 500 Internal Server Error
```

**Browser console**:
```javascript
GET https://api.zarichney.com/api/users/12345 500 (Internal Server Error)

Uncaught (in promise) Error: Failed to load user profile
    at UserProfileComponent.loadUser (user-profile.component.ts:45)
    at UserProfileComponent.ngOnInit (user-profile.component.ts:32)
```

## Impact Assessment

### Severity: **HIGH**

- **Definition**: Major functionality broken, affects all users, no workaround
- **System status**: User profile functionality completely non-functional
- **Data loss**: None - no database operations attempted
- **Security**: No security implications beyond availability

### Affected Users

**All users** attempting to view any user profile.

**Metrics**:
- Total users: 2,000 active users
- Daily profile views: ~500 views/day
- Current failures: 247 errors in 24 hours since deployment
- Impact: 100% failure rate for profile functionality

### Business Impact

**User Experience**:
- Critical feature unavailable (user profiles)
- Users cannot view their own profile or other user profiles
- Social features dependent on profiles broken

**Business Operations**:
- Customer support tickets increased 300% (45 tickets in 24 hours)
- Negative user feedback on social media (8 tweets mentioning broken profiles)
- App store reviews trending negative (3 1-star reviews mentioning bug)

**Revenue**:
- No direct revenue impact (free tier users)
- Potential churn if not resolved within 48 hours
- Estimated 10-15% user loss if issue persists >1 week

## Root Cause Analysis

**Technical explanation**:

The `UserService.GetUserById()` method attempts to access `_userRepository` field (line 47), but this field is `null` at runtime because dependency injection is not properly configured.

**Code location**:
```csharp
// File: Code/Zarichney.Server/Services/UserService.cs
// Lines: 40-52

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;  // This is NULL
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;  // userRepository parameter is NULL
        _mapper = mapper;
    }

    public async Task<UserDto> GetUserById(int userId)
    {
        // BUG: _userRepository is null, causes NullReferenceException
        var user = await _userRepository.GetByIdAsync(userId);  // Line 47
        return _mapper.Map<UserDto>(user);
    }
}
```

**Missing configuration**:
```csharp
// File: Code/Zarichney.Server/Program.cs
// Lines: 25-30

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();  // Present
// MISSING: builder.Services.AddScoped<IUserRepository, UserRepository>();
```

**Introduced by**: PR #789 - Refactor service layer (merged 2 days ago)

**Why missed**: PR reviewer focused on service refactoring, missed DI registration oversight. No integration test caught missing registration at startup.

## Suggested Fix

### Immediate Fix (Production Hotfix)

**File**: `Code/Zarichney.Server/Program.cs`

**Add missing DI registration**:
```csharp
// Line 28 (add after RecipeRepository registration)
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

**Deployment**:
1. Create hotfix branch from `main`
2. Add single-line DI registration
3. Deploy to production (5 minutes)
4. Verify all user profile endpoints functional

**Risk**: Minimal - adding missing DI registration with no behavior changes

### Long-Term Fix (Prevention)

**Add startup validation**:
```csharp
// File: Code/Zarichney.Server/Program.cs
// Add after app.Build()

#if DEBUG
// Validate all DI registrations at startup
using (var scope = app.Services.CreateScope())
{
    var requiredServices = new[]
    {
        typeof(IUserRepository),
        typeof(IUserService),
        typeof(IRecipeRepository),
        typeof(IRecipeService)
    };

    foreach (var serviceType in requiredServices)
    {
        var service = scope.ServiceProvider.GetService(serviceType);
        if (service == null)
        {
            throw new InvalidOperationException(
                $"Required service {serviceType.Name} not registered in DI container"
            );
        }
    }
}
#endif
```

**Integration test**:
```csharp
// File: Code/Zarichney.Server.Tests/Startup/DependencyInjectionTests.cs

[Fact]
public void AllRequiredServices_ShouldBeRegistered()
{
    // Arrange
    var services = new ServiceCollection();
    // Configure services as in Program.cs

    // Act
    var provider = services.BuildServiceProvider();

    // Assert
    Assert.NotNull(provider.GetService<IUserRepository>());
    Assert.NotNull(provider.GetService<IUserService>());
    // ... all required services
}
```

## Workaround

**No workaround available** for end users.

Users cannot access profile functionality until fix is deployed.

## Additional Context

**Related issues**:
- Similar bug: #456 - RecipeRepository DI issue (fixed in v2.0.1)
- Regression introduced in: PR #789 - Refactor service layer

**Related PRs**:
- #789 - Refactor service layer (MERGED - introduced regression)

**Monitoring**:
- Error tracking: 247 errors in past 24 hours
- Performance: N/A - errors occur before any processing
- User feedback: 45 support tickets, 8 social media complaints

**Screenshots**:
- [User profile error page showing "Something went wrong"]
- [Browser DevTools network tab showing 500 error]
- [Server logs with stack trace]

**Timeline**:
- 2025-10-24 10:00 - PR #789 merged to main
- 2025-10-24 14:00 - v2.1.3 deployed to production
- 2025-10-24 14:05 - First error logged
- 2025-10-24 16:00 - 50 errors logged, support tickets increasing
- 2025-10-25 09:00 - 247 total errors, bug investigation begun
- 2025-10-25 11:00 - Root cause identified, issue created

```

### Labels

**Mandatory labels**:
- `type: bug` - Broken functionality ✅
- `priority: critical` - Production down for major feature ✅
- `effort: tiny` - Single line DI registration ✅
- `component: api` - Backend service layer ✅

**Label string**:
```
type: bug,priority: critical,effort: tiny,component: api
```

### Assignees

**Primary**: @BackendSpecialist - DI configuration expertise
**Review**: @SecurityAuditor - Validate no security implications

## Phase 4: Validation & Submission

### Checks

- [x] Template complete with all sections
- [x] Reproduction steps specific and reliable
- [x] Root cause identified with code examples
- [x] Suggested fix actionable
- [x] Labels compliant (type, priority, effort, component)
- [x] No duplicates (similar issue #456 was different service)

### Submission

```bash
$ gh issue create \
  --title "Bug: UserService.GetUserById returns 500 for all valid user IDs" \
  --body "$(cat /tmp/userservice-bug.md)" \
  --label "type: bug,priority: critical,effort: tiny,component: api" \
  --assignee @BackendSpecialist

✓ Created issue #891: Bug: UserService.GetUserById returns 500 for all valid user IDs
https://github.com/Zarichney-Development/zarichney-api/issues/891
```

---

## Summary

This example demonstrates bug report creation with:

✅ **Comprehensive reproduction**: Exact steps, error messages, stack traces
✅ **Root cause analysis**: Code examination revealing missing DI registration
✅ **Impact assessment**: Quantified user impact (247 errors, 45 tickets, 300% support increase)
✅ **Immediate fix**: Single-line solution for rapid production hotfix
✅ **Long-term prevention**: Startup validation and integration tests

**Time savings**: 5 min → 1 min through automated log analysis, code search, and template population
