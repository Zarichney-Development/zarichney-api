# Example: Comprehensive Feature Request Creation

This example demonstrates the complete 4-phase workflow for creating a feature request issue using the github-issue-creation skill.

## Scenario

User requests: "We need to add recipe filtering by dietary restrictions so users can find recipes that match their dietary needs."

## Phase 1: Context Collection (Automated)

### 1.1 User Requirements Analysis

**Extracted from conversation**:
- Users want to filter recipes by dietary restrictions
- Target restrictions: vegetarian, vegan, gluten-free, dairy-free, nut-free
- Multi-select capability needed (users may have multiple restrictions)
- Filter should persist across sessions

**User value proposition**:
- As a health-conscious user with dietary restrictions
- I want to filter recipes by multiple dietary needs
- So that I can quickly find safe recipes without manual review

**Pain point**:
- Currently must manually review every recipe
- Time-consuming for users with multiple restrictions
- Error-prone (users might miss restriction information)
- No way to save dietary preferences

### 1.2 Codebase Analysis

**Search for related functionality**:
```bash
# Find existing recipe filtering
$ grep -r "filter" --include="*.cs" Code/Zarichney.Server/Services/

Code/Zarichney.Server/Services/RecipeService.cs:45:    public async Task<List<RecipeDto>> GetRecipesByCategory(string category)
Code/Zarichney.Server/Services/RecipeService.cs:67:    // TODO: Add filtering by dietary restrictions
```

**Find relevant files**:
```bash
$ glob "**/*Recipe*.cs" Code/Zarichney.Server/

Code/Zarichney.Server/Services/RecipeService.cs
Code/Zarichney.Server/Controllers/RecipesController.cs
Code/Zarichney.Server/Models/Recipe.cs
Code/Zarichney.Server/Data/RecipeRepository.cs
```

**Code snippet - Current recipe model**:
```csharp
// File: Code/Zarichney.Server/Models/Recipe.cs
// Lines: 10-25

public class Recipe
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<string> Ingredients { get; set; }
    public List<string> Instructions { get; set; }
    public string Category { get; set; }
    // MISSING: DietaryRestrictions property
}
```

**Integration points identified**:
- RecipeService.cs - Add filtering method
- RecipesController.cs - Add query parameter support
- Recipe.cs - Add DietaryRestrictions property
- RecipeRepository.cs - Add database filtering logic

### 1.3 Similar Issues Analysis

```bash
$ gh issue list --search "recipe filter" --json number,title,state,labels

#456 - Add recipe category filtering [CLOSED]
  Labels: type: feature, component: api, status: done
  Notes: Established pattern for filtering, can reuse architecture

#234 - User preferences storage [OPEN]
  Labels: type: feature, component: api, priority: medium
  Notes: Can integrate dietary restrictions with user preferences

#189 - Recipe search functionality [CLOSED]
  Labels: type: feature, component: api, component: website
  Notes: Search and filter can be combined in UI
```

**Related PRs**:
```bash
$ gh pr list --search "filter" --state merged

#789 - Implement category filtering
  Merged: 2 weeks ago
  Files: RecipeService.cs, RecipesController.cs
  Pattern: Query parameter → Service method → Repository filter
```

### 1.4 Documentation Review

**Standards reviewed**:
- `/Docs/Standards/CodingStandards.md` - Service layer patterns
- `/Docs/Standards/TestingStandards.md` - Test coverage requirements (75% backend)
- `/Code/Zarichney.Server/README.md` - Architecture overview

**Documentation gaps identified**:
- API documentation needs update for new query parameter
- User guide needs dietary restrictions feature explanation

### 1.5 Acceptance Criteria Identification

**Converted from requirements**:
- [ ] Users can select 1+ dietary restrictions from filter UI
- [ ] Recipe list updates in real-time showing only matching recipes
- [ ] API endpoint supports filtering: `/api/recipes?restrictions=vegan,gluten-free`
- [ ] Dietary restriction badges display on recipe cards
- [ ] Filter selection persists across browser sessions
- [ ] Backend filtering efficient with database indexes
- [ ] All recipes tagged with applicable dietary restrictions

**Success metrics**:
- API response time <200ms for filtered queries
- Backend test coverage ≥75%
- Frontend test coverage ≥70%
- Zero performance regressions

## Phase 2: Template Selection

**Template Chosen**: Feature Request Template

**Rationale**: This is new functionality (filtering by dietary restrictions) that delivers user value. Not a bug, technical debt, or epic. Feature request template best fits requirements.

**Template loaded**: `resources/templates/feature-request-template.md`

## Phase 3: Issue Construction

### Title

```
Feature: Add recipe filtering by dietary restrictions
```

**Format**: `type: Brief actionable description`
**Follows conventions**: Yes - feature type, clear capability description

### Description (Using Template)

```markdown
# Feature Request: Add Recipe Filtering by Dietary Restrictions

## User Value Proposition

**As a** health-conscious user with dietary restrictions
**I want** to filter recipes by multiple dietary needs (vegetarian, vegan, gluten-free, dairy-free, nut-free)
**So that** I can quickly find safe recipes without manually reviewing each one

## Current Pain Point

Users must manually review every recipe to check if it meets their dietary restrictions. This is time-consuming (5-10 minutes per meal planning session) and error-prone, especially when managing multiple restrictions simultaneously. Users with common restrictions (40% of active users per survey data) report frustration with this limitation.

**Current workaround**: Users maintain personal spreadsheets of safe recipes, defeating purpose of the app.

## Proposed Solution

### High-Level Approach

Add dietary restriction tags to recipe metadata and implement multi-select filtering UI. Backend API will support filtering by multiple restrictions using AND logic (recipe must meet ALL selected restrictions).

**Component architecture**:
- Backend: Extend Recipe model, add filtering endpoint, create database index
- Frontend: Filter sidebar component with checkboxes, recipe card badges
- Database: Migration adding dietary_restrictions JSONB column

### Acceptance Criteria

- [ ] Users can select one or more dietary restrictions from filter sidebar UI
- [ ] Recipe list updates in real-time to show only recipes matching ALL selected restrictions
- [ ] Backend API endpoint `/api/recipes?restrictions=vegan,gluten-free` returns filtered results
- [ ] Dietary restriction badges display on recipe cards (e.g., "Vegan", "Gluten-Free")
- [ ] Filter selection persists across browser sessions using localStorage
- [ ] Database query performance <50ms with proper GIN indexing
- [ ] All existing recipes backfilled with restriction tags based on ingredient analysis

### Technical Considerations

**Affected Components**:
- Backend API: RecipeService, RecipesController, Recipe model, RecipeRepository
- Frontend: Recipe list component, filter sidebar, recipe card component
- Database: recipes table schema, dietary_restrictions column, GIN index

**Dependencies**:
- No new external dependencies required
- Uses existing EF Core JSONB support for PostgreSQL
- Angular Material checkbox component for filter UI

**Performance Impact**:
- Database GIN index required on dietary_restrictions column for query performance
- Frontend filtering logic runs client-side to reduce API calls after initial load
- Expected additional 50ms query time for filtered requests (baseline: 150ms → 200ms)
- Caching strategy: Recipe metadata cached in frontend state management

**Security Considerations**:
- User dietary preferences stored in authenticated user profile (existing auth)
- Input validation prevents invalid dietary restriction values (whitelist validation)
- SQL injection prevention through EF Core parameterized queries (existing protection)
- No PII collected beyond existing user profile scope

## Integration Points

### Existing Functionality

Integrates with existing recipe browsing and search:
- Recipe list component refactored to support filtering (currently only supports search)
- Existing category filtering (#456) extended with dietary restrictions dimension
- User preferences storage (#234) includes dietary restriction defaults

**Code changes**:
```csharp
// File: Code/Zarichney.Server/Models/Recipe.cs
// Add property:
public List<string> DietaryRestrictions { get; set; }

// File: Code/Zarichney.Server/Services/RecipeService.cs
// Add method:
public async Task<List<RecipeDto>> GetRecipesByDietaryRestrictions(
    List<string> restrictions,
    CancellationToken cancellationToken = default
)
```

### API Changes

**New Query Parameter**:
- Endpoint: `GET /api/recipes?restrictions=vegan,gluten-free`
- Response: Standard `List<RecipeDto>` with dietary_restrictions field

**Response Schema Extension**:
```json
{
  "id": 123,
  "title": "Mediterranean Quinoa Bowl",
  "dietary_restrictions": ["vegan", "gluten-free", "dairy-free"]
}
```

**Backward Compatible**: Yes - existing `/api/recipes` endpoint continues working without query parameter

### Database Changes

**Migration**: Add dietary_restrictions column
```sql
ALTER TABLE recipes
ADD COLUMN dietary_restrictions JSONB DEFAULT '[]'::jsonb;

CREATE INDEX idx_recipes_dietary_restrictions
ON recipes USING GIN (dietary_restrictions);
```

**Data migration script**: Analyze existing recipe ingredients and auto-tag based on rules:
- No animal products → vegan
- No meat/fish → vegetarian
- No wheat/barley/rye → gluten-free
- No dairy → dairy-free
- No nuts → nut-free

## Success Metrics

### User Impact

- **Target audience**: 40% of active users report dietary restrictions (800/2,000 users)
- **Expected usage**: 60% of recipe browsing sessions will use filtering (1,200 sessions/day)
- **User retention**: Projected 15% increase in weekly return visits
- **Time savings**: Reduce meal planning time from 10 minutes to 3 minutes per session

### Performance Targets

- **API response time**: <200ms for filtered queries (95th percentile)
- **Database query**: <50ms with GIN index
- **Frontend rendering**: <100ms for filter application
- **Concurrent users**: Support 100 concurrent filtered queries without degradation

### Quality Gates

- **Backend test coverage**: 100% for filtering logic (target: ≥75% overall)
- **Frontend test coverage**: 100% for filter component (target: ≥70% overall)
- **Integration tests**: All restriction combinations validated (5 restrictions = 32 combinations)
- **E2E tests**: Complete user journey from filter selection to recipe display
- **API documentation**: OpenAPI spec updated with restrictions parameter
- **User documentation**: Help article explaining dietary restrictions feature

## Additional Context

**User research**:
- Survey (n=500): 78% of respondents requested dietary filtering
- Top requested restrictions: Vegan (45%), Gluten-free (38%), Vegetarian (35%)
- User interviews: 9/10 users with restrictions found current app "frustrating"

**Competitive analysis**:
- 5/7 competitor apps have dietary filtering
- Average implementation: Multi-select checkbox UI + badge display
- Best practice: Persist filter preferences (3/7 competitors)

**Design mockup**: https://www.figma.com/file/abc123/recipe-filtering-mockup

**Related issues**:
- Depends on: #234 - User preferences storage (IN PROGRESS)
- Blocks: #567 - Personalized recipe recommendations
- Related to: #456 - Recipe category filtering (COMPLETED - reuse pattern)

**Related PRs**:
- #789 - Category filtering implementation (merged) - architecture pattern to follow
- #801 - User preferences API (open) - integration point

```

### Label Application

**Mandatory labels (all 4 required)**:
- `type: feature` - New functionality ✅
- `priority: high` - 40% users need this, competitive gap ✅
- `effort: medium` - 2-3 days (backend + frontend + tests) ✅
- `component: api` - Backend changes required ✅

**Additional component labels**:
- `component: website` - Frontend UI changes ✅
- `component: database` - Schema migration required ✅
- `component: testing` - Comprehensive test coverage needed ✅

**Label string for gh CLI**:
```
type: feature,priority: high,effort: medium,component: api,component: website,component: database,component: testing
```

### Milestone Assignment

**Milestone**: Recipe Management Enhancements (Q4 2025)

**Rationale**: Part of larger recipe feature improvements, coordinates with user preferences work

### Assignee Identification

**Primary**: @BackendSpecialist - API and database implementation
**Secondary**: @FrontendSpecialist - UI components and filtering logic
**Testing**: @TestEngineer - Comprehensive test coverage

### Related Issues Linking

- **Depends on**: #234 - User preferences storage must complete first for persistence
- **Blocks**: #567 - Personalized recommendations needs dietary data
- **Related to**: #456 - Category filtering (completed, reuse architecture pattern)

## Phase 4: Validation & Submission

### Template Completeness Check

- [x] User value proposition clear and specific
- [x] Current pain point articulated with user impact
- [x] Acceptance criteria specific and testable (7 criteria defined)
- [x] Technical considerations comprehensive (components, dependencies, performance, security)
- [x] Integration points identified (API, database, existing features)
- [x] Success metrics quantitative (user impact, performance, quality gates)
- [x] Additional context provided (research, competitive analysis, mockups, related issues)

### Label Compliance Validation

- [x] Exactly one `type:` label (`type: feature`)
- [x] Exactly one `priority:` label (`priority: high`)
- [x] Exactly one `effort:` label (`effort: medium`)
- [x] At least one `component:` label (4 components: api, website, database, testing)
- [x] Epic labels if applicable (N/A - standalone feature)
- [x] Coverage labels if applicable (N/A - not test coverage work)

### Duplicate Prevention

```bash
$ gh issue list --search "dietary restriction filter recipe" --json number,title,state

# No exact duplicates found

$ gh issue list --search "recipe filter" --json number,title,state

#456 - Add recipe category filtering [CLOSED]
  Notes: Different type of filtering, already completed

#189 - Recipe search functionality [CLOSED]
  Notes: Search, not filter - complementary feature
```

**Result**: No duplicates. Proceed with creation.

### Acceptance Criteria Clarity

- [x] Each criterion is specific (not vague like "filtering works")
- [x] Success metrics quantifiable (response time <200ms, coverage ≥75%)
- [x] Validation approach clear (integration tests for all combinations)
- [x] Edge cases considered (multiple restrictions, empty results, performance)

### Submission

```bash
$ gh issue create \
  --title "Feature: Add recipe filtering by dietary restrictions" \
  --body "$(cat /tmp/dietary-restrictions-feature.md)" \
  --label "type: feature,priority: high,effort: medium,component: api,component: website,component: database,component: testing" \
  --milestone "Recipe Management Enhancements" \
  --assignee @BackendSpecialist,@FrontendSpecialist

✓ Created issue #890: Feature: Add recipe filtering by dietary restrictions
https://github.com/Zarichney-Development/zarichney-api/issues/890
```

### Post-Submission Actions

1. **Capture issue URL**: https://github.com/Zarichney-Development/zarichney-api/issues/890
2. **Update related issues**:
   - Add comment to #234: "Blocked by dietary restrictions feature (#890)"
   - Add comment to #567: "Blocks personalized recommendations (#890)"
3. **Add to epic tracking**: Update Recipe Management Enhancements milestone
4. **Communicate**: Share #890 with team in standup, highlight dependency on #234

---

## Summary

This example demonstrates:

✅ **Phase 1 automation**: Systematic context collection using grep, glob, gh CLI
✅ **Phase 2 template selection**: Feature request template chosen based on issue type
✅ **Phase 3 construction**: Complete issue with all template sections, proper labels, assignees
✅ **Phase 4 validation**: Comprehensive checks before submission, duplicate prevention

**Time savings**:
- **Traditional approach**: 5 minutes (manual context gathering, formatting, label lookup)
- **Skill-automated approach**: 1 minute (automated discovery, template population, validation)
- **80% time reduction** achieved through systematic workflow
