# Context Collection Patterns

Advanced strategies for automated context gathering using grep, glob, gh CLI, and documentation analysis.

## Table of Contents

1. [Overview](#overview)
2. [Code Discovery Patterns](#code-discovery-patterns)
3. [Issue & PR Analysis Patterns](#issue--pr-analysis-patterns)
4. [Documentation Loading Patterns](#documentation-loading-patterns)
5. [Acceptance Criteria Extraction](#acceptance-criteria-extraction)
6. [Error Analysis Patterns](#error-analysis-patterns)
7. [Performance Benchmarking](#performance-benchmarking)

---

## Overview

**Goal**: Eliminate manual "hand bombing" of context through systematic automation

**Key tools**:
- **Grep**: Content search within files (ripgrep syntax)
- **Glob**: File pattern matching and discovery
- **gh CLI**: GitHub issue/PR querying
- **Read**: Documentation and code analysis

**Time savings**: 4-5 minutes of manual discovery → 30-60 seconds automated

---

## Code Discovery Patterns

### Pattern 1: Find Related Functionality

**Use case**: Identifying similar existing features to understand patterns

**Grep for similar functionality**:
```bash
# Search for similar feature by keyword
grep -r "filter\|search" --include="*.cs" Code/Zarichney.Server/Services/

# Search for specific method patterns
grep -r "async Task<.*>.*Filter" --include="*.cs" Code/Zarichney.Server/

# Find existing API endpoints
grep -r "MapPost\|MapGet\|MapPut\|MapDelete" --include="*.cs" Code/Zarichney.Server/
```

**Example output analysis**:
```
Code/Zarichney.Server/Services/RecipeService.cs:45:    public async Task<List<RecipeDto>> GetRecipesByCategory(string category)

INTERPRETATION:
- Existing filtering pattern: GetRecipesByCategory()
- Can reuse architecture: Service method → Repository query
- Integration point: RecipeService class
```

---

### Pattern 2: Locate Integration Points

**Use case**: Identifying which files need modification

**Glob for relevant files**:
```bash
# Find all service files
glob "**/*Service.cs" Code/Zarichney.Server/

# Find all controller files
glob "**/*Controller.cs" Code/Zarichney.Server/

# Find model/entity files
glob "**/Models/*.cs" Code/Zarichney.Server/

# Find repository files
glob "**/*Repository.cs" Code/Zarichney.Server/
```

**Cross-reference with grep**:
```bash
# After finding RecipeService.cs, check dependencies
grep -r "IRecipeService\|RecipeService" --include="*.cs" Code/Zarichney.Server/

# Find usages of Recipe model
grep -r "Recipe\s+\w+\s*=" --include="*.cs" Code/Zarichney.Server/
```

**Example integration map**:
```
RecipeService.cs [MODIFY - add filtering method]
  ↓ used by
RecipesController.cs [MODIFY - add query parameter]
  ↓ depends on
Recipe.cs [MODIFY - add DietaryRestrictions property]
  ↓ persisted by
RecipeRepository.cs [MODIFY - add database filtering]
```

---

### Pattern 3: Extract Code Context

**Use case**: Understanding current implementation to inform issue description

**Read specific files**:
```bash
# Read entire file for context
read Code/Zarichney.Server/Services/RecipeService.cs

# Read with line offset for specific method
read Code/Zarichney.Server/Services/RecipeService.cs --offset 40 --limit 20
```

**Grep for specific patterns**:
```bash
# Find method signature
grep -A 10 "public.*GetRecipesByCategory" Code/Zarichney.Server/Services/RecipeService.cs

# Find property definitions
grep "public.*{.*get;.*set;" Code/Zarichney.Server/Models/Recipe.cs
```

**Example code snippet extraction**:
```csharp
// File: Code/Zarichney.Server/Models/Recipe.cs
// Lines: 10-18

public class Recipe
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<string> Ingredients { get; set; }
    // MISSING: DietaryRestrictions property
}
```

---

### Pattern 4: Dependency Analysis

**Use case**: Identifying dependencies and required changes

**Search for dependency injection**:
```bash
# Find DI registrations
grep -r "AddScoped\|AddSingleton\|AddTransient" Code/Zarichney.Server/Program.cs

# Find constructor injection
grep -A 5 "public.*constructor" Code/Zarichney.Server/Services/*.cs
```

**Identify missing registrations**:
```bash
# Check if IUserRepository registered
grep "AddScoped.*IUserRepository" Code/Zarichney.Server/Program.cs

# If no results, this is a bug - DI not configured
```

**Example dependency map**:
```
RecipeService (EXISTING)
  ↓ depends on
IRecipeRepository (REGISTERED ✓)
  ↓ implemented by
RecipeRepository (EXISTS ✓)

UserService (EXISTING)
  ↓ depends on
IUserRepository (NOT REGISTERED ❌)  ← BUG IDENTIFIED
  ↓ implemented by
UserRepository (EXISTS but not injected)
```

---

## Issue & PR Analysis Patterns

### Pattern 1: Find Similar Issues

**Use case**: Duplicate prevention and pattern identification

**Search by keywords**:
```bash
# Search issue titles and bodies
gh issue list --search "recipe filter" --json number,title,state,labels

# Search in specific state
gh issue list --search "recipe" --state open --json number,title,labels

# Search closed issues for patterns
gh issue list --search "filter" --state closed --limit 10
```

**Advanced search operators**:
```bash
# Search by label
gh issue list --label "type: feature" --search "recipe"

# Search by milestone
gh issue list --milestone "MVP Launch" --json number,title,state

# Search by date range
gh issue list --search "created:>2025-10-01 recipe"
```

**Example analysis**:
```
Results for "recipe filter":

#456 - Add recipe category filtering [CLOSED]
  Labels: type: feature, component: api, status: done
  PATTERN: Established filtering architecture
  ACTION: Review PR #789 for implementation approach

#234 - User preferences storage [OPEN]
  Labels: type: feature, component: api, priority: medium
  DEPENDENCY: Dietary restrictions can integrate with preferences
  ACTION: Link as dependency (depends on #234)

No exact duplicate found - safe to create new issue
```

---

### Pattern 2: Analyze Related PRs

**Use case**: Learning from past implementations, finding introducing changes

**Search merged PRs**:
```bash
# Find PRs implementing similar features
gh pr list --search "filter" --state merged --limit 5 --json number,title,mergedAt

# Find recent PRs to specific files
gh pr list --search "UserService.cs" --state all --json number,title,state
```

**View PR details**:
```bash
# Get PR file changes
gh pr view 789 --json files

# Get PR diff for specific file
gh pr diff 789 --name-only | grep UserService.cs
```

**Regression analysis example**:
```bash
# Bug reported: UserService broken
# Find recent PRs modifying UserService

$ gh pr list --search "UserService" --state merged --limit 5

#789 - Refactor service layer [MERGED 2 days ago]
  Files: UserService.cs, Program.cs, ...

# Review PR #789
$ gh pr view 789

IDENTIFIED: PR #789 refactored to DI but missed IUserRepository registration
CAUSE: Regression introduced in PR #789
ACTION: Reference in bug issue, suggest reverting or hotfixing
```

---

### Pattern 3: Extract Lessons Learned

**Use case**: Understanding patterns and avoiding past mistakes

**Review issue comments**:
```bash
# View issue with all comments
gh issue view 456

# Search for specific patterns in comments
gh issue view 456 | grep -i "lesson\|mistake\|problem"
```

**Identify success patterns**:
```bash
# Find recently completed similar features
gh issue list --label "type: feature" --state closed --search "filter" --limit 3

# Review implementation approach
gh issue view 456 --comments
```

**Example lessons extraction**:
```
Issue #456 - Recipe category filtering:

Comment by @BackendSpecialist:
"Performance was critical - added GIN index on category column.
Without index, queries took 800ms. With index: 45ms.
LESSON: Always benchmark and index filter columns."

Comment by @TestEngineer:
"Integration tests caught edge case: empty filter results.
Added specific test coverage for empty state.
LESSON: Test boundary conditions explicitly."

ACTION: Apply lessons to new dietary restrictions filter
- Plan GIN index on dietary_restrictions column
- Add integration test for empty filter results
```

---

## Documentation Loading Patterns

### Pattern 1: Standards Review

**Use case**: Ensuring compliance with project standards

**Read relevant standards**:
```bash
# Coding standards for implementation patterns
read /home/zarichney/workspace/zarichney-api/Docs/Standards/CodingStandards.md

# Testing standards for coverage requirements
read /home/zarichney/workspace/zarichney-api/Docs/Standards/TestingStandards.md

# Task management for epic branch strategy
read /home/zarichney/workspace/zarichney-api/Docs/Standards/TaskManagementStandards.md

# GitHub labels for proper categorization
read /home/zarichney/workspace/zarichney-api/Docs/Standards/GitHubLabelStandards.md
```

**Extract key requirements**:
```bash
# Find coverage requirements
grep "coverage.*75\|75.*coverage" Docs/Standards/TestingStandards.md

# Find epic branch patterns
grep -A 10 "Epic Branch" Docs/Standards/TaskManagementStandards.md
```

---

### Pattern 2: Module Context

**Use case**: Understanding module-specific architecture and patterns

**Read module READMEs**:
```bash
# Backend module overview
read Code/Zarichney.Server/README.md

# Frontend module overview
read Code/Zarichney.Website/README.md

# Specific service documentation
read Code/Zarichney.Server/Services/README.md
```

**Extract architecture patterns**:
```bash
# Find service layer pattern
grep -A 5 "Service Layer" Code/Zarichney.Server/README.md

# Find repository pattern usage
grep -A 5 "Repository Pattern" Code/Zarichney.Server/README.md
```

---

### Pattern 3: API Documentation

**Use case**: Understanding existing API contracts and integration points

**Find OpenAPI specs**:
```bash
# Locate Swagger/OpenAPI documentation
glob "**/*swagger*.json" Code/Zarichney.Server/
glob "**/*openapi*.yaml" Code/Zarichney.Server/

# Read API documentation
read Code/Zarichney.Server/apidocs.json
```

**Extract endpoint patterns**:
```bash
# Find existing recipe endpoints
grep -A 3 "\"\/api\/recipes" Code/Zarichney.Server/apidocs.json

# Identify query parameter patterns
grep "query.*parameters" Code/Zarichney.Server/apidocs.json
```

---

## Acceptance Criteria Extraction

### Pattern 1: User Story to Criteria

**Use case**: Converting conversational requirements to testable outcomes

**Input (user conversation)**:
```
"Users need to filter recipes by dietary restrictions.
They should be able to select multiple restrictions like vegan and gluten-free.
The filter should be fast and save their preferences."
```

**Extraction process**:
```
1. Identify capabilities:
   - "filter recipes" → Filtering functionality
   - "select multiple restrictions" → Multi-select UI
   - "fast" → Performance requirement
   - "save preferences" → Persistence

2. Convert to SMART criteria:
   - [ ] Users can select 1+ dietary restrictions from filter UI
   - [ ] Recipe list updates showing only matching recipes
   - [ ] API endpoint /api/recipes?restrictions=vegan,gluten-free works
   - [ ] API response time <200ms (95th percentile)
   - [ ] Filter selection persists across browser sessions

3. Add technical validation:
   - [ ] Backend test coverage ≥75%
   - [ ] Frontend test coverage ≥70%
   - [ ] Database query performance <50ms
```

---

### Pattern 2: Bug to Acceptance Criteria

**Use case**: Defining what "fixed" means for bug reports

**Bug description**:
```
"UserService.GetUserById returns 500 error for all valid user IDs"
```

**Acceptance criteria**:
```
- [ ] UserService.GetUserById(validUserId) returns UserDto (not exception)
- [ ] API endpoint /api/users/{id} returns 200 OK for valid IDs
- [ ] No NullReferenceException in production logs
- [ ] All existing user profile page loads succeed
- [ ] Integration test added preventing regression
- [ ] DI registration validation added to prevent future issues
```

---

### Pattern 3: Epic to Component Criteria

**Use case**: Breaking down epic success into component deliverables

**Epic vision**:
```
"Recipe Management Feature Set enabling core MVP functionality"
```

**Component-level criteria extraction**:
```
Epic success criteria:
- [ ] All 6 component issues completed

Component issue criteria (Issue 1 - API Endpoints):
- [ ] POST /api/recipes endpoint functional
- [ ] GET /api/recipes/{id} endpoint functional
- [ ] PUT /api/recipes/{id} endpoint functional
- [ ] DELETE /api/recipes/{id} endpoint functional
- [ ] API documentation updated (OpenAPI spec)
- [ ] Input validation working (400 errors for invalid data)

Component issue criteria (Issue 2 - Database Schema):
- [ ] Recipe table created with migration
- [ ] Foreign key relationship to users table
- [ ] Indexes created for query performance
- [ ] 20+ sample recipes seeded for testing
```

---

## Error Analysis Patterns

### Pattern 1: Log Analysis

**Use case**: Extracting error context from production logs

**Search logs for errors**:
```bash
# Find specific error occurrences
grep "UserService.GetUserById" logs/production-2025-10-25.log

# Count error frequency
grep -c "NullReferenceException" logs/production-2025-10-25.log

# Extract stack traces
grep -A 10 "NullReferenceException" logs/production-2025-10-25.log
```

**Example log parsing**:
```
[ERROR] 2025-10-25 14:32:17.823 UserService.GetUserById - NullReferenceException for userId: 12345
System.NullReferenceException: Object reference not set to an instance of an object.
   at Zarichney.Server.Services.UserService.GetUserById(Int32 userId) in UserService.cs:line 47

EXTRACTED CONTEXT:
- Error: NullReferenceException
- Location: UserService.cs, line 47
- Method: GetUserById
- Frequency: 247 errors in 24 hours (from grep -c)
- Impact: All user profile page loads
```

---

### Pattern 2: Stack Trace Analysis

**Use case**: Identifying root cause from call stack

**Parse stack trace**:
```
Stack trace:
   at UserService.GetUserById(Int32 userId) in UserService.cs:line 47
   at UsersController.GetUser(Int32 id) in UsersController.cs:line 23
   at ActionMethodExecutor.Execute()

ANALYSIS:
1. Entry point: UsersController.GetUser (line 23)
2. Calls: UserService.GetUserById
3. Failure point: UserService.cs line 47

ACTION: Read UserService.cs starting at line 40 to understand context
```

**Read source at failure point**:
```bash
read Code/Zarichney.Server/Services/UserService.cs --offset 40 --limit 15
```

---

### Pattern 3: Reproduction Context

**Use case**: Building reliable reproduction steps from error reports

**Error report elements**:
```
User report: "I can't view any user profiles, getting error message"

CONTEXT COLLECTION:
1. Which page? → "User profile page at /users/{id}"
2. Which users? → "Any user ID, tried multiple"
3. When started? → "Since this morning" (correlate with deployment time)
4. Browser/device? → "Chrome on desktop"
5. Logged in? → "Yes, authenticated user"

REPRODUCTION STEPS:
1. Navigate to https://app.example.com/users/12345
2. Observe browser network tab: GET /api/users/12345
3. Service throws NullReferenceException (line 47)
4. Frontend displays "Something went wrong"

REPRODUCIBILITY: Always (100% failure rate per logs)
```

---

## Performance Benchmarking

### Pattern 1: Query Performance Analysis

**Use case**: Establishing performance baselines and targets

**Benchmark current performance**:
```bash
# Time database queries
psql -d zarichney_db -c "EXPLAIN ANALYZE SELECT * FROM recipes WHERE category = 'dinner';"

# API response time testing
curl -w "@curl-format.txt" https://api.example.com/api/recipes?category=dinner
```

**Example benchmarking**:
```
Current performance (without filter):
- Database query: 45ms
- API endpoint: 150ms (95th percentile)

Expected performance (with dietary filter):
- Database query: <50ms (requires GIN index)
- API endpoint: <200ms (95th percentile)

ACCEPTANCE CRITERIA:
- [ ] Database query execution <50ms with GIN index
- [ ] API response time <200ms (95th percentile)
- [ ] No performance regression on existing endpoints
```

---

### Pattern 2: Load Testing Context

**Use case**: Defining scalability requirements

**Current load metrics**:
```bash
# Check concurrent user count from monitoring
# Average daily requests from logs
grep -c "GET /api/recipes" logs/access-2025-10-25.log
```

**Example load requirements**:
```
Current metrics:
- Daily requests: ~5,000 recipe requests
- Concurrent users: ~50 peak
- Average response time: 150ms

Target metrics (with filtering):
- Daily requests: ~7,500 (50% increase expected)
- Concurrent users: ~100 peak
- Response time target: <200ms (25% degradation acceptable)

ACCEPTANCE CRITERIA:
- [ ] Support 100 concurrent filtered queries without degradation
- [ ] API response time <200ms under load (95th percentile)
```

---

## Complete Example Workflow

**Scenario**: Create feature request for recipe dietary filtering

### Step-by-Step Context Collection

```bash
# 1. Find related code
grep -r "filter\|search" --include="*.cs" Code/Zarichney.Server/Services/

# 2. Locate relevant files
glob "**/*Recipe*.cs" Code/Zarichney.Server/

# 3. Read current implementation
read Code/Zarichney.Server/Models/Recipe.cs

# 4. Find similar issues
gh issue list --search "recipe filter" --json number,title,state

# 5. Find implementation patterns from PRs
gh pr list --search "filter" --state merged --limit 3

# 6. Load standards
read Docs/Standards/GitHubLabelStandards.md
read Docs/Standards/TestingStandards.md

# 7. Load module context
read Code/Zarichney.Server/README.md

# 8. Extract acceptance criteria from conversation
# (Manual step - convert user requirements to SMART criteria)
```

**Time**: ~60 seconds automated discovery vs. 4-5 minutes manual

**Result**: Comprehensive context for issue creation with zero manual "hand bombing"

---

## Best Practices

1. **Start broad, narrow down**: Begin with general searches, refine based on results
2. **Cross-reference findings**: Validate grep results with glob, gh CLI with read
3. **Document discoveries**: Include file paths, line numbers, related issues in notes
4. **Automate repetitive patterns**: Create bash functions for common search combinations
5. **Verify before including**: Read actual code/docs rather than assuming from filenames

**Efficiency target**: <1 minute context collection for typical feature/bug issues

---

**Next Steps**: See `resources/examples/` for complete context collection demonstrations in comprehensive feature, bug, and epic examples.
