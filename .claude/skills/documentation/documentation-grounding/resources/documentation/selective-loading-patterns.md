# Selective Loading Patterns

**Purpose:** Quick reference patterns for common task types with optimized grounding strategies
**Target Audience:** All agents seeking fast grounding protocol selection
**Version:** 1.0.0

---

## Quick Reference Matrix

| Task Type | Tier | Phase 1 | Phase 2 | Phase 3 | Est. Tokens |
|-----------|------|---------|---------|---------|-------------|
| Typo Fix | 1 | Skip | Skip | README Sec 3 | 500-800 |
| Simple Bug Fix | 1 | Error Handling | Skip | README Sec 3, 5, 8 | 2,000-3,000 |
| Feature Implementation | 2 | 3-4 Standards | Root Skim | Full Module README | 6,000-8,000 |
| New API Endpoint | 2 | 4 Standards | Module Hierarchy | Module + Dependencies | 7,000-9,000 |
| Test Coverage | 2 | Testing (Full) + Coding | Test TDD | SUT README Sec 3, 5 | 5,000-7,000 |
| Documentation Update | 2 | Docs (Full) + Diagrams | Peer READMEs | All 8 Sections | 8,000-10,000 |
| Refactoring | 3 | All Standards | Full Architecture | Multi-Module Analysis | 12,000-15,000 |
| New Module Creation | 3 | All Standards | Peer Modules | Parent + Siblings | 13,000-16,000 |
| Security Implementation | 3 | All Standards | Security Patterns | Multi-Module Contracts | 14,000-17,000 |

---

## Pattern Catalog

### Pattern 1: Typo/Comment Fix

**Use When:** Fixing typos, updating comments, no logic changes

**Grounding Protocol:**
```yaml
Phase_1: SKIP
Phase_2: SKIP
Phase_3:
  - Module README Section 3 only (verify no contract implications)

Validation:
  - Change is purely cosmetic?
  - No interface contract affected?
  - No testing implications?

Token_Budget: 500-800 tokens
Quality_Risk: Minimal
```

---

### Pattern 2: Simple Bug Fix

**Use When:** Fixing specific bug in existing functionality

**Grounding Protocol:**
```yaml
Phase_1:
  - CodingStandards.md Section 6 (Error Handling)
  - TestingStandards.md Section 6 (if adding test for bug)

Phase_2:
  - Skip (familiar module)

Phase_3:
  - Module README Section 3 (Interface Contract - verify fix maintains contract)
  - Module README Section 5 (Known pitfalls related to bug area)
  - Module README Section 8 (Check if bug already documented)

Validation:
  - Bug reproduction understood?
  - Interface contract implications clear?
  - Test coverage for bug fix identified?

Token_Budget: 2,000-3,000 tokens
Quality_Risk: Low (targeted grounding for specific issue)
```

**Example:**
- Bug: NullReferenceException in `RecipeService.GetRecipeAsync`
- Load: CodingStandards Section 7 (Null Handling), RecipeService README Section 3 (preconditions)
- Verify: Fix maintains postconditions, add null check test

---

### Pattern 3: Feature Implementation (Backend)

**Use When:** Implementing new feature in existing backend module

**Grounding Protocol:**
```yaml
Phase_1:
  - CodingStandards.md Sections 2, 3, 6
    - Section 2: Naming, modern C# features
    - Section 3: DI patterns, SOLID principles
    - Section 6: Error handling
  - TestingStandards.md Section 6 (unit test overview)
  - DocumentationStandards.md Section 3 (if contract changes)
  - TaskManagementStandards.md (commit format)

Phase_2:
  - Root README quick skim (orientation)
  - Identify module position in hierarchy

Phase_3:
  - Target module README Sections 1-6
    - Section 3 CRITICAL (interface contracts)
    - Section 5 (testing strategy)
    - Section 6 (dependencies for integration)
  - Dependency modules: Section 1, 3 (purpose, contracts)

Validation:
  - DI patterns understood?
  - Interface contracts documented?
  - Testing strategy identified?
  - Dependencies known for mocking?

Token_Budget: 6,000-8,000 tokens
Quality_Risk: Low-Medium
```

**Example:**
- Feature: Add recipe sharing API endpoint
- Load: See `backend-specialist-grounding.md` example for complete workflow

---

### Pattern 4: Feature Implementation (Frontend)

**Use When:** Implementing new feature in Angular frontend

**Grounding Protocol:**
```yaml
Phase_1:
  - CodingStandards.md Section 2 (naming - adapt to TypeScript context)
  - TestingStandards.md Section 6 (component testing overview)
  - DocumentationStandards.md Section 3 (if component contract changes)

Phase_2:
  - Frontend module hierarchy (`Code/Zarichney.Website/`)
  - Identify component domain (shared, feature-specific)

Phase_3:
  - Target component README Sections 1-6
  - State management patterns (NgRx if applicable)
  - API integration contracts (backend dependencies)

Validation:
  - Component architecture understood?
  - State management patterns clear?
  - API contract integration identified?
  - Testing strategy for component known?

Token_Budget: 5,000-7,000 tokens
Quality_Risk: Low-Medium
```

---

### Pattern 5: New API Endpoint

**Use When:** Creating new REST API endpoint with controller and service

**Grounding Protocol:**
```yaml
Phase_1:
  - CodingStandards.md Sections 3, 4, 6
    - Section 3: DI patterns
    - Section 4: Humble Object pattern for controllers
    - Section 6: Error handling, API error responses
  - TestingStandards.md Sections 6, 7
    - Unit tests for service layer
    - Integration tests for API endpoint
  - DocumentationStandards.md Section 3 (new endpoint contract documentation)

Phase_2:
  - Controller module hierarchy
  - Service layer architecture
  - API versioning patterns

Phase_3:
  - Controller README Section 3 (existing API patterns)
  - Service README Sections 2, 3, 6
    - Architecture patterns
    - Service interface contracts
    - Dependencies for new endpoint
  - API client generation implications

Validation:
  - Humble controller pattern understood?
  - Service layer business logic separation clear?
  - API response patterns consistent?
  - Integration test requirements identified?
  - README documentation plan established?

Token_Budget: 7,000-9,000 tokens
Quality_Risk: Medium (API contracts are critical)
```

**Critical Sections:**
- CodingStandards Section 4 (Humble Object)
- Module README Section 3 (API contract documentation)
- TestingStandards Section 7 (integration test patterns)

---

### Pattern 6: Comprehensive Test Coverage

**Use When:** Adding missing test coverage for existing service or component

**Grounding Protocol:**
```yaml
Phase_1:
  - TestingStandards.md COMPLETE (all sections)
    - Critical: Section 6 (Unit Test Standards)
    - Critical: Section 7 (Integration Test Standards)
  - CodingStandards.md Sections 3, 4
    - Understand SUT design patterns
    - SOLID principles for testability

Phase_2:
  - Test project TechnicalDesignDocument.md
  - Test framework patterns (fixtures, helpers, builders)
  - Existing test patterns for similar components

Phase_3:
  - SUT module README Sections 3, 5, 6
    - Section 3: Interface contracts (what to test)
    - Section 5: Key test scenarios, known pitfalls
    - Section 6: Dependencies (mocking requirements)
  - Dependency modules: Section 3 (mock behavior understanding)

Validation:
  - AAA pattern understood?
  - FluentAssertions usage clear?
  - Moq patterns known?
  - All interface contract scenarios identified?
  - Edge cases from Section 5 and 8 incorporated?
  - Test categorization traits correct?

Token_Budget: 5,000-7,000 tokens
Quality_Risk: Low (TestingStandards comprehensive coverage)
```

**Example:**
- Target: AuthService test coverage
- Load: See `test-engineer-grounding.md` example for complete workflow

**Critical Focus:**
- TestingStandards.md Sections 6, 7 (unit and integration standards)
- SUT README Section 3 (interface contracts define test cases)
- SUT README Section 5 (key test scenarios)

---

### Pattern 7: Documentation Update (README)

**Use When:** Creating or updating module README.md

**Grounding Protocol:**
```yaml
Phase_1:
  - DocumentationStandards.md COMPLETE (all sections)
    - Critical: Section 3 (8-section template)
    - Critical: Section 4 (linking strategy)
  - DiagrammingStandards.md (if diagrams needed)
  - CodingStandards.md Sections 2-3 (understand code patterns to document)
  - TestingStandards.md (to document test strategies)

Phase_2:
  - Parent module README (navigation context)
  - Peer module READMEs (consistency patterns)
  - Root README (project overview)

Phase_3:
  - Target module code analysis (all 8 sections)
  - Dependency module READMEs (for Section 6)
  - Consumer module READMEs (for Section 6)

Validation:
  - 8-section template followed?
  - Parent link present?
  - Child links correct?
  - Dependency links use relative paths (no [cite] tags)?
  - Section 3 interface contracts thoroughly documented?
  - Diagrams embedded in Section 2 if appropriate?
  - Last Updated date current?

Token_Budget: 8,000-10,000 tokens
Quality_Risk: Low (comprehensive grounding ensures quality)
```

**Example:**
- Target: PaymentService README creation
- Load: See `documentation-maintainer-grounding.md` example for complete workflow

**Critical Sections:**
- DocumentationStandards.md Section 3 (template structure)
- Peer READMEs (consistency validation)
- Code analysis for Section 3 (interface contracts)

---

### Pattern 8: Refactoring

**Use When:** Significant refactoring of existing code

**Grounding Protocol:**
```yaml
Phase_1:
  - ALL Standards COMPLETE
    - CodingStandards.md (all patterns may be affected)
    - TestingStandards.md (tests must remain valid)
    - DocumentationStandards.md (contract changes require updates)
    - TaskManagementStandards.md (commit strategy)

Phase_2:
  - Full architecture understanding
  - Module hierarchy impact assessment
  - Dependency graph analysis

Phase_3:
  - Target module README All 8 sections
  - All dependency module READMEs (integration impacts)
  - All consumer module READMEs (change impact assessment)

Validation:
  - Refactoring maintains interface contracts?
  - All dependency implications understood?
  - All consumer impact assessed?
  - Test updates identified?
  - Documentation updates planned?
  - No architectural assumptions violated?

Token_Budget: 12,000-15,000 tokens
Quality_Risk: High (comprehensive grounding mandatory)
```

**Critical Considerations:**
- Section 3 (Interface Contracts) across multiple modules
- Section 6 (Dependencies) impact analysis
- Section 7 (Rationale) - understand why current design exists before changing

---

### Pattern 9: New Module Creation

**Use When:** Creating entirely new module with full documentation

**Grounding Protocol:**
```yaml
Phase_1:
  - ALL Standards COMPLETE
    - CodingStandards.md (all patterns apply)
    - TestingStandards.md (full test suite needed)
    - DocumentationStandards.md (complete README creation)
    - DiagrammingStandards.md (architecture diagrams)
    - TaskManagementStandards.md (branch and commit strategy)

Phase_2:
  - Root README (project integration)
  - Parent module README (where new module fits)
  - Peer module READMEs (consistency patterns)

Phase_3:
  - Parent module README All 8 sections
  - Sibling modules for architecture patterns
  - Dependency modules for integration patterns

Validation:
  - Module purpose clear and non-overlapping with existing modules?
  - Architecture aligns with project patterns?
  - All 8 README sections planned?
  - Test strategy defined?
  - Integration points identified?
  - Naming conventions followed?

Token_Budget: 13,000-16,000 tokens
Quality_Risk: High (new module sets precedent)
```

**Critical Focus:**
- DocumentationStandards.md (complete README from start)
- Peer module patterns (consistency)
- Parent module integration points

---

### Pattern 10: Security Implementation

**Use When:** Implementing authentication, authorization, or security-critical features

**Grounding Protocol:**
```yaml
Phase_1:
  - ALL Standards COMPLETE
  - Extra emphasis:
    - CodingStandards.md Section 6 (error handling - no info leakage)
    - TestingStandards.md (security test scenarios)

Phase_2:
  - Security module hierarchy
  - Authentication/authorization patterns
  - Existing security implementations

Phase_3:
  - Security module READMEs All 8 sections
  - Multi-module security contract analysis
  - Known security issues across all modules (Section 8)

Validation:
  - Security patterns understood?
  - No information leakage in errors?
  - All attack vectors considered?
  - Security test coverage comprehensive?
  - Authorization checks at all layers?
  - Sensitive data handling correct?

Token_Budget: 14,000-17,000 tokens
Quality_Risk: CRITICAL (security failures catastrophic)
```

**Zero Shortcuts Allowed:**
- Security-critical code requires Tier 3 comprehensive grounding
- All interface contracts must be analyzed for security implications
- Known issues (Section 8) across all related modules must be reviewed

---

## Pattern Selection Flowchart

```
START
│
├─ Change Type?
│  ├─ Cosmetic (typo, comment) → Pattern 1 (Typo Fix)
│  ├─ Bug Fix → Pattern 2 (Simple Bug Fix)
│  ├─ New Feature
│  │  ├─ Backend → Pattern 3 (Feature - Backend)
│  │  ├─ Frontend → Pattern 4 (Feature - Frontend)
│  │  └─ API Endpoint → Pattern 5 (New API Endpoint)
│  ├─ Testing → Pattern 6 (Test Coverage)
│  ├─ Documentation → Pattern 7 (Documentation Update)
│  ├─ Refactoring → Pattern 8 (Refactoring)
│  ├─ New Module → Pattern 9 (New Module Creation)
│  └─ Security → Pattern 10 (Security Implementation)
│
└─ Execute Selected Pattern
```

---

## Anti-Patterns to Avoid

### ❌ Anti-Pattern 1: One-Size-Fits-All
**Problem:** Using Pattern 9 (comprehensive) for all tasks
**Impact:** Token waste, slower execution, no efficiency gains
**Solution:** Match pattern to task complexity

### ❌ Anti-Pattern 2: Skipping Section 3
**Problem:** Loading module README but skipping Section 3 (Interface Contracts)
**Impact:** High risk of contract violations, rework required
**Solution:** Section 3 is ALWAYS mandatory in Phase 3

### ❌ Anti-Pattern 3: Over-Optimization
**Problem:** Using Pattern 1 (minimal) for complex changes to save tokens
**Impact:** Standards violations, rework cost exceeds token savings
**Solution:** When in doubt, use higher tier

### ❌ Anti-Pattern 4: Loading Without Purpose
**Problem:** Loading standards documents "just in case"
**Impact:** Token waste on unused context
**Solution:** Load specific sections based on task requirements

### ❌ Anti-Pattern 5: Ignoring Dependencies
**Problem:** Skipping Phase 3 dependency module review
**Impact:** Integration failures, mocking issues in tests
**Solution:** Always review Section 6 dependencies, load their Section 1 and 3

---

## Pattern Effectiveness Tracking

**After Using Pattern, Record:**

```yaml
Pattern_Used: [1-10]
Task_Description: [Brief description]
Token_Budget_Actual: [Measured tokens]
Standards_Violations: [Count]
Rework_Required: [Yes/No]
Quality_Rating: [1-5]

Retrospective:
  - Was pattern appropriate for task?
  - Should higher/lower tier have been used?
  - Were any loaded sections unused?
  - Were any missing sections needed?
```

**Continuous Improvement:**
- Track pattern effectiveness over time
- Adjust patterns based on empirical results
- Share successful optimizations with team

---

## Emergency Override

**When Pattern Unclear:**
```yaml
Default_Safe_Pattern:
  - Use Pattern 3 (Feature Implementation)
  - Tier 2 grounding
  - 6,000-8,000 token budget
  - Comprehensive enough for most tasks
  - Not wasteful for complex tasks
```

**When Time-Critical:**
```yaml
Emergency_Minimum:
  - Module README Section 3 ONLY
  - CodingStandards error handling section if applicable
  - Total: 1,500-2,000 tokens
  - Create follow-up issue for full grounding validation
```

---

**Status:** ✅ Quick reference for daily agent grounding decisions
**Version:** 1.0.0
**Last Updated:** 2025-10-25
**Skill:** documentation-grounding
