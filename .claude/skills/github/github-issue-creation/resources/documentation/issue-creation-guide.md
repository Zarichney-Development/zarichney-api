# GitHub Issue Creation Guide

Complete step-by-step walkthrough of the 4-phase workflow for creating high-quality GitHub issues.

## Table of Contents

1. [Overview](#overview)
2. [Phase 1: Context Collection](#phase-1-context-collection)
3. [Phase 2: Template Selection](#phase-2-template-selection)
4. [Phase 3: Issue Construction](#phase-3-issue-construction)
5. [Phase 4: Validation & Submission](#phase-4-validation--submission)
6. [Common Patterns](#common-patterns)
7. [Troubleshooting](#troubleshooting)

---

## Overview

**Purpose**: Systematic workflow eliminating manual "hand bombing" of context into GitHub issues

**Time Savings**: 5 minutes → 1 minute (80% reduction)

**Key Benefits**:
- **Automated context collection**: grep, glob, gh CLI for discovery
- **Template-driven consistency**: All template sections completed
- **Label compliance**: GitHubLabelStandards.md enforced automatically
- **Duplicate prevention**: Systematic checking before creation

**When to Use**: Any GitHub issue creation (features, bugs, epics, technical debt, documentation)

---

## Phase 1: Context Collection

**Objective**: Gather comprehensive context automatically, eliminating manual discovery

### Step 1.1: User Requirements Analysis

**Extract from conversation**:
- What capability or fix is requested?
- What's the user value proposition?
- What pain point does this solve?
- What are the acceptance criteria?

**Example**:
```
User request: "Add recipe filtering by dietary restrictions"

Extracted:
- Capability: Multi-select filtering UI + backend endpoint
- Value: Users find safe recipes quickly without manual review
- Pain: Currently manual review is time-consuming and error-prone
- Acceptance: Filter UI, real-time updates, persistence, <200ms API
```

### Step 1.2: Codebase Analysis (Automated)

**Search for related functionality**:
```bash
# Find existing similar features
grep -r "filter" --include="*.cs" Code/Zarichney.Server/Services/

# Find relevant files
glob "**/*Recipe*.cs"

# Read code at specific locations
read Code/Zarichney.Server/Models/Recipe.cs
```

**What to collect**:
- Existing implementation patterns to follow
- Integration points (which files need modification)
- Code snippets demonstrating current behavior
- Architecture patterns (service layer, repository, etc.)

**Example output**:
```
Found: RecipeService.GetRecipesByCategory() - existing filtering pattern
Integration points: RecipeService, RecipesController, Recipe model
Pattern: Query parameter → Service method → Repository filter
```

### Step 1.3: Similar Issues Analysis (Automated)

**Search existing issues and PRs**:
```bash
# Find similar issues
gh issue list --search "keyword1 keyword2" --json number,title,state,labels

# Check related PRs
gh pr list --search "keyword" --state all --json number,title,state

# View specific issue for context
gh issue view 456
```

**What to identify**:
- Duplicate issues (avoid creating redundant issues)
- Related work (dependencies, blockers)
- Patterns and solutions from similar issues
- Lessons learned from past implementations

**Example**:
```
#456 - Add recipe category filtering [CLOSED]
  Pattern: Established filtering architecture
  Reuse: Can follow same API parameter pattern

#234 - User preferences storage [OPEN]
  Dependency: Dietary restrictions can integrate with preferences
```

### Step 1.4: Documentation Review

**Load relevant standards**:
- `/Docs/Standards/CodingStandards.md` - Implementation patterns
- `/Docs/Standards/TestingStandards.md` - Coverage requirements
- `/Docs/Standards/GitHubLabelStandards.md` - Label application

**Load module context**:
- Module-specific `README.md` files
- Architecture documentation
- API documentation (OpenAPI specs)

**Identify gaps**:
- Documentation requiring updates
- New documentation needed

### Step 1.5: Acceptance Criteria Identification

**Convert requirements to testable outcomes**:

```
Requirement: "Filter recipes by dietary restrictions"

Acceptance criteria:
✓ Users can select 1+ restrictions from UI
✓ Recipe list updates in real-time showing matching recipes
✓ API endpoint /api/recipes?restrictions=vegan,gluten-free works
✓ Restriction badges display on recipe cards
✓ Filter persists across browser sessions
✓ API response time <200ms
✓ Backend test coverage ≥75%
```

**Make criteria SMART**:
- **Specific**: Exact functionality described
- **Measurable**: Quantitative targets (<200ms, ≥75%)
- **Achievable**: Realistic with available resources
- **Relevant**: Directly supports user value proposition
- **Time-bound**: Delivery timeframe implicit in effort estimate

---

## Phase 2: Template Selection

**Decision tree for template selection**:

### Is this NEW functionality?
→ **YES**: Use **Feature Request Template**
→ **NO**: Continue...

### Is something BROKEN?
→ **YES**: Use **Bug Report Template**
→ **NO**: Continue...

### Is this a LARGE initiative (multi-issue, multi-week)?
→ **YES**: Use **Epic Template**
→ **NO**: Continue...

### Is this CODE QUALITY improvement (refactoring, technical debt)?
→ **YES**: Use **Technical Debt Template**
→ **NO**: Continue...

### Is this DOCUMENTATION missing or unclear?
→ **YES**: Use **Documentation Request Template**
→ **NO**: Unclear - default to Feature Request or create custom issue

---

### Template 1: Feature Request

**Use when**: New capability, enhancement to existing functionality

**Key sections**:
- User value proposition (As a... I want... So that...)
- Current pain point and workaround
- Proposed solution with acceptance criteria
- Technical considerations (components, dependencies, performance, security)
- Integration points (API, database, existing features)
- Success metrics (user impact, performance targets, quality gates)

**Labels**: `type: feature`, `priority: [high/medium/low]`, `effort: [estimation]`, `component: [relevant]`

---

### Template 2: Bug Report

**Use when**: Something broken, unexpected behavior, error occurring

**Key sections**:
- Environment (platform, version, configuration)
- Expected vs. actual behavior
- Reproduction steps (specific, reliable, numbered)
- Error messages (exact text, stack traces, logs)
- Impact assessment (severity, affected users, business impact)
- Root cause analysis (if known)
- Suggested fix and workaround

**Labels**: `type: bug`, `priority: [critical/high/medium/low]`, `effort: [estimation]`, `component: [relevant]`

---

### Template 3: Epic

**Use when**: Large initiative, multi-issue coordination, multi-week timeline

**Key sections**:
- Vision and strategic context (why important, business alignment)
- Success criteria (high-level outcomes)
- Component breakdown (5-8 discrete issues with dependencies)
- Dependency graph (Mermaid diagram with critical path)
- Milestones and phasing (3-4 phases over timeline)
- Risk assessment (technical, resource, integration risks with mitigation)
- Success metrics (quantitative and qualitative)

**Labels**: `type: epic`, `priority: high`, `effort: epic`, `component: [multiple]`, `status: epic-planning`

---

### Template 4: Technical Debt

**Use when**: Code quality improvement, refactoring, architectural change needed

**Key sections**:
- Current state (the problem with code examples)
- Ideal state (the goal with target architecture)
- Rationale for original decision (why implemented this way initially)
- Impact of NOT addressing (velocity, quality, performance, security)
- Proposed refactoring (approach, affected components, migration path)
- Risk mitigation (breaking changes, testing strategy, rollback plan)

**Labels**: `type: debt`, `priority: [high/medium/low]`, `effort: [estimation]`, `component: [relevant]`, `technical-debt`

---

### Template 5: Documentation Request

**Use when**: Documentation missing, unclear, or needs improvement

**Key sections**:
- Knowledge gap (what's missing or unclear)
- User impact (who needs this, what are they trying to do)
- Proposed documentation (content outline, location, format)
- Current workarounds (how people figure it out now)
- Success criteria (completeness, accuracy, accessibility)

**Labels**: `type: docs`, `priority: [high/medium/low]`, `effort: [estimation]`, `component: docs`

---

## Phase 3: Issue Construction

### Step 3.1: Craft Title

**Format**: `[Type]: [Clear, actionable description]`

**Good examples**:
- `Feature: Add recipe filtering by dietary restrictions`
- `Bug: UserService.GetUserById returns 500 for valid IDs`
- `Epic: Recipe Management Feature Set`
- `Debt: Refactor ProcessExecutor to use dependency injection`
- `Docs: Add API authentication guide for external integrators`

**Bad examples**:
- ❌ `Recipe filtering` (missing type prefix)
- ❌ `Fix the user service` (not specific enough)
- ❌ `Feature: Make the app better` (vague, not actionable)

### Step 3.2: Fill Template Sections

**Load selected template**: From `resources/templates/[template-name].md`

**Fill ALL sections**:
- Replace ALL placeholders with specific content
- Use collected context from Phase 1
- Include code snippets with file paths and line numbers
- Add cross-references to related issues/PRs
- Make acceptance criteria specific and testable

**Quality check**:
- No "[description]" or "{value}" placeholders remaining
- Code examples properly formatted with markdown syntax
- Links to related files/issues complete and valid
- Acceptance criteria SMART (Specific, Measurable, Achievable, Relevant, Time-bound)

### Step 3.3: Apply Labels (MANDATORY)

**See**: `resources/documentation/label-application-guide.md` for complete details

**Required labels (ALL 4 must be present)**:
1. **Type** (exactly one): `type: feature|bug|epic|debt|docs`
2. **Priority** (exactly one): `priority: critical|high|medium|low`
3. **Effort** (exactly one): `effort: tiny|small|medium|large|epic`
4. **Component** (at least one): `component: api|website|docs|ci-cd|database|testing|scripts`

**Optional labels** (apply when relevant):
- Epic coordination: `epic: testing-excellence`, `epic: [initiative-name]`
- Coverage phases: `coverage: phase-1` through `coverage: phase-5`
- Automation context: `automation: ci-ready`, `automation: local-only`
- Quality: `technical-debt`, `architecture`, `breaking-change`

**Label string construction**:
```bash
# Comma-separated, no spaces
"type: feature,priority: high,effort: medium,component: api,component: website"
```

### Step 3.4: Assign Milestone (if applicable)

**When to assign milestone**:
- Issue is part of larger epic initiative
- Issue targets specific release or quarter
- Issue has deadline or time-sensitive nature

**Example milestones**:
- `Recipe Management Enhancements (Q4 2025)`
- `epic/testing-excellence`
- `MVP Launch`

### Step 3.5: Identify Assignees

**Agent expertise mapping**:
- `@BackendSpecialist` - .NET/C# API, service layer, database
- `@FrontendSpecialist` - Angular/TypeScript, UI components, state management
- `@TestEngineer` - Test coverage, quality improvements, testing framework
- `@SecurityAuditor` - Security vulnerabilities, hardening, compliance
- `@WorkflowEngineer` - CI/CD, GitHub Actions, automation
- `@ArchitecturalAnalyst` - Architecture decisions, technical debt, design patterns
- `@BugInvestigator` - Complex debugging, root cause analysis
- `@DocumentationMaintainer` - Documentation updates, README files, guides

**Multiple assignees**: When issue spans multiple domains (e.g., feature requiring both backend and frontend work)

### Step 3.6: Link Related Issues

**Dependency types**:
- **Depends on**: Issues that must complete BEFORE this one
- **Blocks**: Issues that depend ON this one (can't start until this completes)
- **Related to**: Similar issues, alternative approaches, context
- **Duplicate of**: If discovered during duplicate check

**Link format**:
```markdown
**Related Issues**:
- Depends on: #234 - User preferences storage
- Blocks: #567 - Personalized recipe recommendations
- Related to: #456 - Recipe category filtering (completed - reuse pattern)
```

---

## Phase 4: Validation & Submission

### Step 4.1: Template Completeness Check

**Checklist**:
- [ ] All template sections filled with meaningful content (not placeholders)
- [ ] Code snippets properly formatted with file paths and line numbers
- [ ] Acceptance criteria specific and testable
- [ ] Related issues linked with clear dependency relationships
- [ ] Success metrics quantifiable where possible
- [ ] Technical considerations comprehensive

### Step 4.2: Label Compliance Validation

**Automated validation**:
```bash
# Verify exactly 1 type label
echo "$LABELS" | grep -c "type:" # Should be 1

# Verify exactly 1 priority label
echo "$LABELS" | grep -c "priority:" # Should be 1

# Verify exactly 1 effort label
echo "$LABELS" | grep -c "effort:" # Should be 1

# Verify at least 1 component label
echo "$LABELS" | grep -c "component:" # Should be ≥1
```

**Failure conditions**:
- ❌ Multiple type labels (only 1 allowed)
- ❌ Multiple priority labels (only 1 allowed)
- ❌ Multiple effort labels (only 1 allowed)
- ❌ Zero component labels (at least 1 required)

### Step 4.3: Duplicate Prevention

**Search for similar issues**:
```bash
# Extract title keywords (remove type prefix)
KEYWORDS="recipe filtering dietary restrictions"

# Search existing issues
gh issue list --search "$KEYWORDS" --json number,title,state,labels

# Search closed issues too (might be reopening)
gh issue list --search "$KEYWORDS" --state all --json number,title,state
```

**Decision tree**:

**Similar issue found and OPEN**:
→ Add comment to existing issue instead of creating new

**Similar issue found and CLOSED**:
→ Evaluate if truly different or should reopen
→ If creating new, reference closed issue and explain why

**No similar issues**:
→ Proceed with creation

### Step 4.4: Acceptance Criteria Clarity Check

**Each criterion must be**:
- **Specific**: Not vague (e.g., "filter works" → "filter UI shows selected restrictions")
- **Testable**: Can verify success/failure objectively
- **Quantifiable** (when possible): Include metrics (<200ms, ≥75% coverage)
- **Complete**: Covers happy path, edge cases, error scenarios

**Example transformation**:
```
❌ Vague: "Filtering should work properly"

✓ Specific: "Users can select 1+ dietary restrictions from filter sidebar"
✓ Specific: "Recipe list updates in real-time showing only matching recipes"
✓ Specific: "API endpoint /api/recipes?restrictions=vegan returns filtered results"
✓ Quantifiable: "API response time <200ms (95th percentile)"
```

### Step 4.5: Submit Issue

**Using gh CLI**:
```bash
gh issue create \
  --title "Type: Brief actionable description" \
  --body "$(cat /path/to/populated-template.md)" \
  --label "type: feature,priority: high,effort: medium,component: api" \
  --milestone "Epic or Release Name" \
  --assignee "@BackendSpecialist,@FrontendSpecialist"
```

**Expected output**:
```
✓ Created issue #890: Feature: Add recipe filtering by dietary restrictions
https://github.com/Zarichney-Development/zarichney-api/issues/890
```

### Step 4.6: Post-Submission Actions

1. **Capture issue URL**: For reference and communication
2. **Update related issues**: Add cross-links with dependency context
3. **Add to milestone/epic**: If part of larger initiative
4. **Communicate to team**: Share issue number in standup, Slack, or relevant channels
5. **Create epic branch** (if epic): Per TaskManagementStandards.md epic branch strategy

---

## Common Patterns

### Pattern 1: Feature Request from User Conversation

```
1. User conversation → Extract requirements
2. grep/glob → Find related code
3. gh issue/pr list → Check similar work
4. Feature template → Fill all sections
5. Labels: type: feature, priority: high, effort: medium, component: api
6. gh issue create → Submit
```

### Pattern 2: Bug Report from Production Error

```
1. Production logs → Error messages and stack traces
2. Read source code → Identify failure point
3. gh pr list → Find introducing change (regression analysis)
4. Bug template → Reproduction steps, root cause, suggested fix
5. Labels: type: bug, priority: critical, effort: tiny, component: api
6. gh issue create → Submit with urgency
```

### Pattern 3: Epic from Product Planning

```
1. Strategic requirements → Business objectives
2. Existing codebase → Architecture review
3. Break down → 5-8 component issues with dependencies
4. Epic template → Milestones, phasing, risk assessment
5. Labels: type: epic, priority: high, effort: epic, component: [multiple]
6. gh issue create → Create epic + 6 component issues
7. git checkout -b epic/[name] → Create epic branch
```

---

## Troubleshooting

### Problem: "I don't know which template to use"

**Solution**: Use decision tree in Phase 2

- New functionality → Feature
- Something broken → Bug
- Multi-issue initiative → Epic
- Code quality improvement → Technical Debt
- Documentation gap → Documentation Request

### Problem: "I can't find related code"

**Solution**: Broaden search patterns

```bash
# Too specific
grep -r "UserService.GetUserById" --include="*.cs"

# Better - broader pattern
grep -r "GetUserById\|GetUser" --include="*.cs"

# Find files by name
glob "**/*User*.cs"

# Search issue descriptions
gh issue list --search "user profile" --state all
```

### Problem: "I'm not sure about label priority"

**Solution**: See `resources/documentation/label-application-guide.md`

**Quick reference**:
- **Critical**: Production down, security breach, data loss
- **High**: Major functionality broken, milestone blocker
- **Medium**: Important but workaround exists
- **Low**: Nice-to-have, future consideration

### Problem: "My issue was marked as duplicate"

**Solution**: Improve duplicate checking in Phase 4.3

- Search with multiple keyword combinations
- Check both open AND closed issues
- Read similar issue descriptions carefully
- If truly different, explain distinction in new issue

### Problem: "Template sections don't all apply"

**Solution**: Mark sections as N/A with explanation

```markdown
## Performance Impact
N/A - Documentation change only, no runtime performance impact
```

**Don't skip sections entirely** - acknowledge explicitly

---

## Next Steps

**After creating issue**:
- Review `resources/examples/` for complete workflow demonstrations
- Consult `resources/documentation/label-application-guide.md` for label details
- See `resources/documentation/context-collection-patterns.md` for advanced automation
- Practice with low-priority issues to build skill
- Measure time savings vs. manual approach

**Expected proficiency**:
- 1st issue: ~3 minutes (learning template structure)
- 5th issue: ~1.5 minutes (getting comfortable with workflow)
- 10th issue: ~1 minute (full automation efficiency)

**Target**: **80% time reduction** (5 min → 1 min) through systematic workflow
