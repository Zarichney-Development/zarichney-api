# GitHub Label Application Guide

Complete guide for applying labels per GitHubLabelStandards.md with decision trees and automation patterns.

## Table of Contents

1. [Mandatory Label Requirements](#mandatory-label-requirements)
2. [Type Label Selection](#type-label-selection)
3. [Priority Label Selection](#priority-label-selection)
4. [Effort Label Selection](#effort-label-selection)
5. [Component Label Selection](#component-label-selection)
6. [Optional Labels](#optional-labels)
7. [Label Validation](#label-validation)
8. [Automation Patterns](#automation-patterns)

---

## Mandatory Label Requirements

**EVERY GitHub Issue MUST have exactly 4 mandatory labels**:

1. **Type Label** (exactly one)
2. **Priority Label** (exactly one)
3. **Effort Label** (exactly one)
4. **Component Label** (at least one, can have multiple)

**Validation failure if**:
- ❌ Any mandatory label missing
- ❌ Multiple type labels (only 1 allowed)
- ❌ Multiple priority labels (only 1 allowed)
- ❌ Multiple effort labels (only 1 allowed)
- ❌ Zero component labels (at least 1 required)

---

## Type Label Selection

**Choose exactly ONE type label** based on issue purpose.

### type: feature
**Use when**: New functionality or capability being added

**Examples**:
- Adding recipe filtering by dietary restrictions
- Implementing user profile editing
- Creating recipe sharing via social media
- Adding export recipes to PDF functionality

**Characteristics**:
- Delivers NEW user-facing or internal capability
- Enhances existing features with new options
- Enables workflows that weren't possible before

**NOT for**: Bug fixes (use `type: bug`), refactoring without new features (use `type: debt`)

---

### type: bug
**Use when**: Something is broken, not working as expected, or causing errors

**Examples**:
- UserService returns 500 error for valid user IDs
- Recipe search returns duplicate results
- Login button doesn't respond to clicks
- Database connection pool exhaustion

**Characteristics**:
- Existing functionality broken or degraded
- Error messages, exceptions, crashes
- Unexpected behavior contradicting requirements
- Regression from previous working state

**NOT for**: Missing features (use `type: feature`), intentional design limitations

---

### type: epic
**Use when**: Large initiative requiring multiple issues and coordination

**Examples**:
- Recipe Management Feature Set (6 component issues)
- User Authentication System (5 component issues)
- Backend Testing Excellence Initiative (multi-month)
- API v2 Migration (breaking changes requiring coordination)

**Characteristics**:
- Multi-issue coordination required
- Timeline: Multi-week to multi-month
- Requires epic branch strategy (per TaskManagementStandards.md)
- Multiple agents or developers involved
- Milestone-level initiative

**Component issues**: Each component uses `type: epic-task` and references parent epic

---

### type: debt
**Use when**: Code quality improvement, refactoring, or technical debt remediation

**Examples**:
- Refactor ProcessExecutor to use dependency injection
- Extract service interfaces for testability
- Remove code duplication in recipe services
- Update deprecated API patterns

**Characteristics**:
- Current implementation WORKS but is suboptimal
- Improves maintainability, testability, performance, or security
- No new user-facing functionality
- Addresses architectural or code quality issues

**Rationale required**: Explain impact of NOT addressing (velocity, quality, security)

---

### type: docs
**Use when**: Documentation is missing, unclear, or needs improvement

**Examples**:
- Add developer onboarding guide
- Update API documentation for new endpoints
- Create troubleshooting guide for common errors
- Improve README with setup instructions

**Characteristics**:
- Documentation gap or quality issue
- Knowledge not captured in written form
- Existing docs outdated or incorrect
- User or developer experience improvement through documentation

**NOT for**: Code comments (use `type: debt` for code quality)

---

### type: epic-task
**Use when**: Issue is a component of larger epic initiative

**Characteristics**:
- Part of parent epic (must reference epic issue)
- Targets epic branch, not develop
- Contributes to epic success criteria
- Coordinated with other epic components

**Additional labels**: Must include parent `epic:` label (e.g., `epic: testing-excellence`)

---

## Priority Label Selection

**Choose exactly ONE priority label** based on urgency and business impact.

### Decision Tree

```
Is this a SECURITY vulnerability or PRODUCTION DOWN issue?
├─ YES → priority: critical
└─ NO  → Continue...

Is this required for NEXT RELEASE/MILESTONE or MAJOR FUNCTIONALITY broken?
├─ YES → priority: high
└─ NO  → Continue...

Is this important but WORKAROUND EXISTS or PLANNED improvement?
├─ YES → priority: medium
└─ NO  → priority: low
```

---

### priority: critical
**Use when**: Immediate attention required, system stability at risk

**Criteria** (ANY of):
- Security vulnerability (CVE, data breach risk)
- Production system completely down
- Data loss or corruption occurring
- Blocking ALL users from critical functionality
- Compliance violation with legal/regulatory risk

**Examples**:
- Command injection vulnerability (CWE-78)
- Database backup failure causing data loss
- Authentication system broken (no users can log in)
- Payment processing down (revenue impact)

**Response expectation**: Hotfix deployed within hours, not days

**NOT for**: Important features (use `priority: high`), single-user bugs (use `priority: medium`)

---

### priority: high
**Use when**: Important for next release or major functionality impacted

**Criteria** (ANY of):
- Required for next milestone or release
- Major functionality broken (but workaround exists)
- Significant user subset affected (>20% of users)
- Architecture foundation enabling future work
- Competitive gap requiring immediate attention

**Examples**:
- Recipe filtering required for MVP launch
- User profile page broken (affects all profile views)
- Epic initiative critical to Q4 goals
- Performance degradation affecting user experience

**Response expectation**: Addressed in current sprint or next 1-2 weeks

**NOT for**: Nice-to-have features (use `priority: medium`), cosmetic issues (use `priority: low`)

---

### priority: medium
**Use when**: Normal priority within planned work

**Criteria** (ANY of):
- Planned improvement within current quarter
- Functionality degraded but workaround available
- Quality enhancement with measurable benefit
- Documentation update for clarity
- Technical debt with moderate impact

**Examples**:
- Recipe search result ranking improvements
- Test coverage increase from 70% to 75%
- Refactor service layer for better testability
- Add API documentation for new endpoints

**Response expectation**: Addressed within 1-2 months in normal backlog prioritization

---

### priority: low
**Use when**: Future consideration, minimal immediate impact

**Criteria** (ANY of):
- Future enhancement beyond current planning horizon
- Optimization with minimal measurable impact
- Experimental or research-oriented work
- Edge case bug affecting <1% of users
- Cosmetic or UI polish

**Examples**:
- Recipe suggestion algorithm exploration
- Dark mode UI theme
- Keyboard shortcuts for power users
- Minor UI alignment issues

**Response expectation**: Backlog item, addressed opportunistically or in dedicated polish sprints

---

## Effort Label Selection

**Choose exactly ONE effort label** based on estimated implementation time.

### Estimation Decision Tree

```
Can this be completed in LESS THAN 1 HOUR?
├─ YES → effort: tiny
└─ NO  → Continue...

Can this be completed in 1-4 HOURS?
├─ YES → effort: small
└─ NO  → Continue...

Can this be completed in 1-3 DAYS?
├─ YES → effort: medium
└─ NO  → Continue...

Can this be completed in 1-2 WEEKS?
├─ YES → effort: large
└─ NO  → Continue...

Is this a MULTI-WEEK or MULTI-MONTH initiative?
├─ YES → effort: epic
└─ NO  → Re-evaluate scope, likely effort: large
```

---

### effort: tiny
**Estimation**: Less than 1 hour of focused work

**Examples**:
- Single-line DI registration fix
- Typo correction in code or documentation
- Add missing import statement
- Update configuration value

**Characteristics**:
- Minimal scope, highly focused
- No testing complexity
- Trivial or no integration concerns

---

### effort: small
**Estimation**: 1-4 hours of focused work

**Examples**:
- Add API endpoint with basic validation
- Create simple UI component
- Write unit tests for single service method
- Database migration adding single column

**Characteristics**:
- Single component or file
- Straightforward testing
- Minimal integration complexity

---

### effort: medium
**Estimation**: 1-3 days of comprehensive work

**Examples**:
- Implement recipe filtering (backend + frontend)
- Add service layer with business logic
- Create UI feature with multiple components
- Comprehensive test suite for service

**Characteristics**:
- Multiple components or integration points
- Moderate testing complexity
- Cross-layer changes (API + service + repository)

---

### effort: large
**Estimation**: 1-2 weeks of extensive work

**Examples**:
- Complete frontend feature with state management
- Complex service refactoring with migration path
- Performance optimization requiring profiling and iteration
- Integration with external service (OAuth, payment gateway)

**Characteristics**:
- Multiple layers and integration points
- Significant testing requirements
- May require phased implementation
- Architecture or design decisions needed

---

### effort: epic
**Estimation**: Multi-week or multi-month initiative

**Examples**:
- Recipe Management Feature Set (4 weeks)
- Backend Testing Excellence Initiative (3 months)
- API v2 Migration (6 weeks)

**Characteristics**:
- Requires breakdown into 5-8+ component issues
- Multiple agents/developers required
- Epic branch strategy needed
- Milestone-level coordination

**Type label**: Must use `type: epic` with `effort: epic`

---

## Component Label Selection

**Select at least ONE component label** based on technical area of impact.

**Multiple component labels allowed**: When issue spans multiple technical areas.

### component: api
**Backend API and service layer changes**

**Use when**:
- ASP.NET Core controllers modified
- Service layer business logic changes
- Repository layer database access
- API endpoint additions or modifications
- Backend validation logic

**Examples**:
- Add POST /api/recipes endpoint
- Implement RecipeService filtering logic
- Refactor UserRepository to async patterns

---

### component: website
**Angular frontend application**

**Use when**:
- Angular components created or modified
- Frontend state management (NgRx)
- UI/UX changes
- Routing configuration
- Frontend validation

**Examples**:
- Create recipe filtering UI component
- Implement recipe detail view page
- Add user profile edit form

---

### component: database
**Schema, migrations, data operations**

**Use when**:
- EF Core migrations
- Database schema changes
- Index creation or modification
- Data migration scripts
- Database performance tuning

**Examples**:
- Add dietary_restrictions column to recipes table
- Create GIN index on recipe search fields
- Migrate user data to new schema

---

### component: testing
**Test framework and infrastructure**

**Use when**:
- Unit test creation or modification
- Integration test infrastructure
- E2E test framework changes
- Test coverage improvements
- Testing tool configuration

**Examples**:
- Increase RecipeService test coverage to 100%
- Add E2E tests for recipe workflows
- Configure test database setup

---

### component: ci-cd
**Build, deployment, automation**

**Use when**:
- GitHub Actions workflows
- Build configuration
- Deployment scripts
- CI/CD pipeline changes
- Automation infrastructure

**Examples**:
- Add coverage reporting to CI
- Configure automated deployment
- Implement pre-commit hooks

---

### component: docs
**Documentation and standards**

**Use when**:
- README updates
- API documentation (OpenAPI)
- Standards document changes
- Developer guides
- User documentation

**Examples**:
- Update API documentation for new endpoints
- Create developer onboarding guide
- Add troubleshooting section to README

---

### component: scripts
**Shell scripts and tooling**

**Use when**:
- Bash/PowerShell scripts
- Build scripts
- Database setup scripts
- Development tooling

**Examples**:
- Create database initialization script
- Add API client generation script
- Improve test suite execution script

---

## Optional Labels

**Apply when relevant to enhance categorization and workflow integration.**

### Epic Coordination Labels

**epic: testing-excellence**
- Use for: Backend Testing Excellence Initiative issues
- Component issues of testing coverage epic

**epic: [initiative-name]**
- Use for: Any epic initiative component issues
- Links to parent epic for coordination

---

### Coverage Phase Labels

**coverage: phase-1** through **coverage: phase-5**
- Use for: Test coverage improvement issues
- Aligns with progressive testing framework

**Decision**:
- Phase 1: 14% → 20% (Foundation - service basics)
- Phase 2: 20% → 35% (Growth - integration depth)
- Phase 3: 35% → 50% (Maturity - edge cases)
- Phase 4: 50% → 75% (Excellence - complex scenarios)
- Phase 5: 75% → comprehensive (Mastery - continuous excellence)

---

### Automation Labels

**automation: ci-ready**
- Works in unconfigured CI environment
- No external dependencies or local setup required

**automation: local-only**
- Requires external dependencies (databases, APIs)
- Local development environment needed

**automation: parallel-safe**
- Can run concurrently with other agents
- No shared resource conflicts

**automation: conflict-risk**
- Potential conflicts with parallel execution
- Coordination required

---

### Quality Labels

**technical-debt**
- Code quality improvement needed
- Often paired with `type: debt`

**architecture**
- Architectural decision or pattern
- Significant design implications

**breaking-change**
- API or interface changes
- Requires coordination and versioning

---

## Label Validation

### Automated Validation Script

```bash
#!/bin/bash

LABELS="$1"  # Comma-separated label string

# Count type labels
TYPE_COUNT=$(echo "$LABELS" | grep -o "type:" | wc -l)
if [ "$TYPE_COUNT" -ne 1 ]; then
  echo "ERROR: Must have exactly 1 type: label (found: $TYPE_COUNT)"
  exit 1
fi

# Count priority labels
PRIORITY_COUNT=$(echo "$LABELS" | grep -o "priority:" | wc -l)
if [ "$PRIORITY_COUNT" -ne 1 ]; then
  echo "ERROR: Must have exactly 1 priority: label (found: $PRIORITY_COUNT)"
  exit 1
fi

# Count effort labels
EFFORT_COUNT=$(echo "$LABELS" | grep -o "effort:" | wc -l)
if [ "$EFFORT_COUNT" -ne 1 ]; then
  echo "ERROR: Must have exactly 1 effort: label (found: $EFFORT_COUNT)"
  exit 1
fi

# Count component labels
COMPONENT_COUNT=$(echo "$LABELS" | grep -o "component:" | wc -l)
if [ "$COMPONENT_COUNT" -lt 1 ]; then
  echo "ERROR: Must have at least 1 component: label (found: $COMPONENT_COUNT)"
  exit 1
fi

echo "✓ Label validation passed"
exit 0
```

**Usage**:
```bash
./validate-labels.sh "type: feature,priority: high,effort: medium,component: api"
✓ Label validation passed

./validate-labels.sh "priority: high,effort: medium,component: api"
ERROR: Must have exactly 1 type: label (found: 0)
```

---

## Automation Patterns

### Pattern 1: Derive Labels from Context

```bash
# Type label from issue title
if echo "$TITLE" | grep -q "^Feature:"; then
  TYPE_LABEL="type: feature"
elif echo "$TITLE" | grep -q "^Bug:"; then
  TYPE_LABEL="type: bug"
# ... etc
fi

# Priority from keywords
if grep -q "critical\|security\|production down" "$DESCRIPTION"; then
  PRIORITY_LABEL="priority: critical"
# ... etc
fi

# Component from affected files
if grep -q "Services/\|Controllers/" "$FILES_CHANGED"; then
  COMPONENT_LABELS="$COMPONENT_LABELS,component: api"
fi
```

### Pattern 2: Validate Before Submission

```bash
# Before gh issue create
validate-labels.sh "$LABELS"
if [ $? -ne 0 ]; then
  echo "Label validation failed. Fix labels and retry."
  exit 1
fi

# Proceed with issue creation
gh issue create --title "$TITLE" --body "$BODY" --label "$LABELS"
```

### Pattern 3: Template-Driven Label Suggestions

Each template includes recommended labels at bottom:

```markdown
---
**Recommended Labels**:
- `type: feature`
- `priority: high`
- `effort: medium`
- `component: api`
- `component: website`
```

Agent replaces placeholders with actual selections based on context.

---

## Label String Construction

**Format**: Comma-separated, no spaces between labels

**Correct**:
```
type: feature,priority: high,effort: medium,component: api,component: website
```

**Incorrect**:
```
type: feature, priority: high, effort: medium  # Spaces after commas
type:feature,priority:high                      # No space after colon
```

**gh CLI usage**:
```bash
gh issue create \
  --label "type: feature,priority: high,effort: medium,component: api"
```

---

## Quick Reference

### Feature Request Labels
```
type: feature
priority: high|medium|low
effort: tiny|small|medium|large
component: api|website|database|testing|docs
```

### Bug Report Labels
```
type: bug
priority: critical|high|medium|low
effort: tiny|small|medium
component: api|website|ci-cd|scripts
```

### Epic Labels
```
type: epic
priority: high
effort: epic
component: [multiple]
status: epic-planning|epic-active
```

### Technical Debt Labels
```
type: debt
priority: high|medium|low
effort: small|medium|large
component: [relevant]
technical-debt
```

### Documentation Labels
```
type: docs
priority: high|medium|low
effort: tiny|small|medium
component: docs
```

---

**Next Steps**: See `resources/examples/` for complete label application demonstrations in real issues.
