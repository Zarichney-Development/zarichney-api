# BackendSpecialist Grounding Example

**Scenario:** BackendSpecialist engaged to implement new API endpoint for recipe sharing feature
**Issue:** #456 - Add recipe sharing API endpoint with access control
**Module:** `Code/Zarichney.Server/Cookbook/RecipesController.cs` and related services

---

## Phase 1: Standards Mastery

### CodingStandards.md (Comprehensive Review)

**Focus Areas for Backend API Work:**

✅ **Dependency Injection Patterns**
- Constructor injection for all dependencies (controllers, services, repositories)
- Interface definitions: `IRecipeService`, `IRecipeSharingService`
- Service lifetime: Scoped for services with DbContext dependencies

✅ **Asynchronous Programming**
- All database calls via EF Core use `async/await`
- API controller actions return `Task<IActionResult>`
- CancellationToken passed through service layers

✅ **Error Handling**
- try/catch around database operations
- Specific exceptions: `RecipeNotFoundException`, `UnauthorizedAccessException`
- Structured logging: `_logger.LogError(ex, "Error sharing recipe {RecipeId} with user {Email}", recipeId, email)`

✅ **API Design Patterns**
- DTOs for request/response models: `ShareRecipeRequest`, `ShareRecipeResponse`
- Humble Controller pattern: validate → delegate to service → map response
- Consistent error responses using `ApiErrorResult`

**Key Takeaways:**
- Controller should be thin - all business logic in `RecipeSharingService`
- Use primary constructors for controllers
- Ensure all dependencies injectable for testing

---

### TestingStandards.md (Design for Testability Focus)

✅ **Unit Test Requirements**
- Service layer (`RecipeSharingService`) must be unit testable with mocked dependencies
- Mock `IRecipeRepository`, `IUserRepository`, `ILogger<RecipeSharingService>`
- Use FluentAssertions for all assertions with reason messages

✅ **Integration Test Requirements**
- API endpoint integration test via Refit client
- Use `DatabaseFixture` for test database
- Test authentication/authorization via `TestAuthHandler`
- Verify database state changes via repository queries

✅ **Test Categorization**
- Unit tests: `[Trait("Category", "Unit")]`
- Integration tests: `[Trait("Category", "Integration")]`, `[Trait("Category", "Database")]`

**Key Takeaways:**
- Design `RecipeSharingService` for easy mocking
- API response patterns: extract content to variable before assertions
- Integration tests must verify authorization rules

---

### DocumentationStandards.md (README Update Requirements)

✅ **Interface Contract Updates**
- Section 3 must document new `ShareRecipe` endpoint contract
- Preconditions: authenticated user, recipe ownership or share permission
- Postconditions: recipe access granted, audit log created
- Error cases: RecipeNotFound, Unauthorized, EmailInvalid

✅ **Dependency Documentation**
- Update Section 6 with new `IRecipeSharingService` dependency
- Link to RecipeSharingService README

**Key Takeaways:**
- README.md update required for API contract changes
- Update "Last Updated" date
- Document security assumptions in Section 3

---

### TaskManagementStandards.md

✅ **Branch Strategy**
- Create `feature/issue-456-recipe-sharing-api`
- Conventional commit: `feat: add recipe sharing API endpoint (#456)`

✅ **PR Requirements**
- Comprehensive description of API contract
- AI Sentinels will analyze on PR to develop

**Key Takeaways:**
- Feature branch from develop
- AI analysis triggers on PR creation

---

## Phase 2: Project Architecture Context

### Root README Analysis

✅ **Technology Stack**
- ASP.NET Core 8 with EF Core
- PostgreSQL database
- MediatR for CQR if applicable (or direct service layer)

✅ **Module Hierarchy**
- `Code/Zarichney.Server/` - main application
- `Code/Zarichney.Server/Cookbook/` - recipe domain
- `Code/Zarichney.Server/Services/Auth/` - authentication services

✅ **Architectural Patterns**
- Repository pattern for data access
- Service layer for business logic
- Controller as thin orchestration layer

**Key Insights:**
- Recipe sharing fits in Cookbook domain
- May need to coordinate with Auth services for access control
- Follow existing API patterns in RecipesController

---

## Phase 3: Domain-Specific Context

### Module: `Code/Zarichney.Server/Cookbook/README.md`

**Section 1: Purpose & Responsibility**
- Cookbook module manages recipe creation, storage, retrieval, and sharing
- Responsible for recipe lifecycle and access control

**Section 2: Architecture & Key Concepts**
- Repository pattern: `RecipeRepository` for data access
- Service layer: `RecipeService` for business logic
- Controller: `RecipesController` for API endpoints
- Embedded diagram showing recipe data flow

**Section 3: Interface Contract & Assumptions** ⚠️ CRITICAL
- **Existing Preconditions:**
  - Authenticated user for all non-public endpoints
  - Recipe ownership for modifications
  - Valid recipe IDs (non-zero, positive integers)

- **Postconditions:**
  - Database transactions committed or rolled back atomically
  - Audit logs created for all modifications
  - Cache invalidated on recipe updates

- **Error Handling:**
  - `RecipeNotFoundException` for invalid recipe IDs
  - `UnauthorizedAccessException` for permission violations
  - `ValidationException` for invalid inputs

- **Assumptions:**
  - User authentication handled by AuthService
  - Recipe IDs are stable and never reused

**New Endpoint Contract to Document:**
```
POST /api/recipes/{id}/share
Authorization: Bearer token required
Request: ShareRecipeRequest { Email: string, AccessLevel: enum }
Response: ShareRecipeResponse { Success: bool, SharedWithEmail: string }
Preconditions:
  - User owns recipe OR has admin role
  - Target email is valid and registered user
Postconditions:
  - RecipeAccess record created in database
  - Audit log entry created
  - Email notification sent to shared user (future)
Errors:
  - 404 RecipeNotFound if recipe doesn't exist
  - 401 Unauthorized if user lacks permission
  - 400 BadRequest if email invalid
```

**Section 5: How to Work With This Code**
- **Setup:** Configure PostgreSQL connection string
- **Testing Strategy:** Unit test services, integration test API endpoints
- **Key Scenarios:**
  - Recipe ownership validation
  - Access control enforcement
  - Cascading delete behavior

**Section 6: Dependencies**
- **Internal Consumed:**
  - `Services/Auth/` for user authentication
  - `Services/Email/` for notifications (future)
- **Internal Consumers:**
  - API clients (Website)
- **External:**
  - EF Core for database access
  - FluentValidation for input validation

**Section 7: Rationale**
- Repository pattern chosen for testability and data access abstraction
- Service layer separation enables business logic reuse

**Section 8: Known Issues**
- Recipe sharing currently limited to single users (no groups)
- Audit log lacks detailed access history

---

## Context Integration Summary

### Critical Insights for Recipe Sharing Implementation

1. **Follow Humble Controller Pattern:**
   - Controller validates request DTO
   - Delegates to `RecipeSharingService` for business logic
   - Maps service response to API response

2. **Access Control Requirements:**
   - Verify user owns recipe OR has admin role
   - Validate target user email exists in system
   - Enforce access level constraints (read-only vs. edit)

3. **Testing Requirements:**
   - **Unit Tests:**
     - `RecipeSharingService` with mocked `IRecipeRepository`, `IUserRepository`
     - Test scenarios: successful share, unauthorized, recipe not found, invalid email
   - **Integration Tests:**
     - Full API endpoint via Refit client
     - Database verification of RecipeAccess record creation
     - Authorization testing (valid token, invalid token, wrong user)

4. **Documentation Updates:**
   - Update `Cookbook/README.md` Section 3 with new endpoint contract
   - Update Section 6 with `IRecipeSharingService` dependency
   - Update "Last Updated" date

### Implementation Approach

**Step 1:** Create `ShareRecipeRequest` and `ShareRecipeResponse` DTOs
- Follow existing DTO patterns (record types)
- Use FluentValidation for email validation

**Step 2:** Implement `IRecipeSharingService` and `RecipeSharingService`
- Constructor inject `IRecipeRepository`, `IUserRepository`, `ILogger<RecipeSharingService>`
- Register as Scoped service in Program.cs
- Implement `ShareRecipeAsync(int recipeId, string email, AccessLevel level, CancellationToken ct)`

**Step 3:** Add controller action to `RecipesController`
- `[HttpPost("{id}/share")]`
- `[Authorize]` attribute for authentication
- Validate request → call service → map response

**Step 4:** Create unit tests for `RecipeSharingService`
- Mock all dependencies
- Test success path, error paths, edge cases
- Use FluentAssertions with reason messages

**Step 5:** Create integration test for API endpoint
- Use `IRecipesApi` Refit interface
- Setup test user with owned recipe
- Test share endpoint with various scenarios

**Step 6:** Update documentation
- Cookbook README Section 3 and Section 6
- Commit message: `feat: add recipe sharing API endpoint (#456)`

---

## Grounding Completion Validation

- ✅ Phase 1: All 4 standards documents reviewed with backend API focus
- ✅ Phase 2: Project architecture and module hierarchy understood
- ✅ Phase 3: Cookbook module README analyzed (all 8 sections)
- ✅ Critical interface contracts documented for new endpoint
- ✅ Testing requirements identified (unit + integration)
- ✅ Implementation approach informed by comprehensive context

**Grounding Status:** ✅ **COMPLETE**

---

## How Grounding Improved Work Quality

**Without Grounding:**
- Might have put business logic in controller (violating Humble Object pattern)
- Could have missed async/await patterns for database calls
- Might not have registered service with correct lifetime
- Could have forgotten README documentation updates
- Might have missed critical authorization checks

**With Grounding:**
- Clear separation: thin controller, testable service layer
- All async patterns correctly applied
- Service registered as Scoped (correct for DbContext dependencies)
- README Section 3 updated with complete interface contract
- Authorization requirements explicitly understood and tested

**Outcome:**
- First implementation meets all standards
- Tests pass immediately
- Documentation complete
- AI Sentinels approve on first review
- Zero rework required

---

**Example Status:** ✅ Demonstrates complete 3-phase grounding for backend API work
**Agent:** BackendSpecialist
**Skill:** documentation-grounding v1.0.0
