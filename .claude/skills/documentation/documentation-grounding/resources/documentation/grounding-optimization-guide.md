# Documentation Grounding Optimization Guide

**Purpose:** Strategies for optimizing context window usage while maintaining comprehensive grounding quality
**Target Audience:** All agents seeking to maximize grounding effectiveness within token constraints
**Version:** 1.0.0

---

## Table of Contents
1. [Context Window Fundamentals](#context-window-fundamentals)
2. [Progressive Loading Strategies](#progressive-loading-strategies)
3. [Selective Standards Loading](#selective-standards-loading)
4. [Module Context Optimization](#module-context-optimization)
5. [Task-Based Grounding Profiles](#task-based-grounding-profiles)
6. [Token Budget Management](#token-budget-management)
7. [Emergency Shortcuts](#emergency-shortcuts)
8. [Quality vs. Efficiency Trade-offs](#quality-vs-efficiency-trade-offs)

---

## Context Window Fundamentals

### Token Budget Reality
**Typical Agent Context Window:** 200,000 tokens
**Typical Multi-Agent Conversation:** 50,000-150,000 tokens consumed

**Context Window Allocation:**
- **Conversation History:** 30-50% (60,000-100,000 tokens)
- **Agent Definition:** 5-10% (10,000-20,000 tokens)
- **Skills (Grounding + Coordination):** 5-10% (10,000-20,000 tokens)
- **Code Reading:** 30-40% (60,000-80,000 tokens)
- **Working Directory:** 5-10% (10,000-20,000 tokens)
- **Buffer:** 10% (20,000 tokens)

**Grounding Optimization Goal:** Achieve comprehensive context within 10,000-15,000 token budget

---

## Progressive Loading Strategies

### Three-Tier Loading Model

**Tier 1: Mandatory Core (2,000-4,000 tokens)**
- Absolutely essential standards for current task
- Critical sections only (not entire documents)
- Module README Section 3 (Interface Contracts) always included

**Tier 2: Relevant Context (3,000-6,000 tokens)**
- Full standards documents relevant to task type
- Module README complete 8-section analysis
- Dependency module overviews

**Tier 3: Comprehensive Deep Dive (8,000-15,000 tokens)**
- All 5 standards documents fully loaded
- Multiple module READMEs for complex integrations
- Historical context and known issues

### When to Use Each Tier

**Tier 1 - Quick Implementation:**
- Simple bug fixes
- Minor feature additions
- Isolated changes to single module
- Time-critical hotfixes

**Tier 2 - Standard Development:**
- Feature implementation
- Test creation
- Documentation updates
- Multi-module coordination

**Tier 3 - Complex Architecture:**
- Major refactoring
- Architectural changes
- Cross-cutting concerns
- Security-critical implementations

---

## Selective Standards Loading

### Agent-Specific Prioritization

**CodeChanger (Backend/Frontend):**
```yaml
Priority 1 (ALWAYS):
  - CodingStandards.md: Sections 2-4, 6 (naming, DI, error handling)
  - Module README: Section 3 (Interface Contracts)

Priority 2 (USUALLY):
  - TestingStandards.md: Section 6 (unit test overview for testability)
  - Module README: Sections 2, 5, 6 (architecture, how to work, dependencies)

Priority 3 (SOMETIMES):
  - DocumentationStandards.md: If contract changes require README updates
  - TaskManagementStandards.md: For commit message format
```

**TestEngineer:**
```yaml
Priority 1 (ALWAYS):
  - TestingStandards.md: Complete (all sections)
  - Module README: Section 3 (Interface Contracts), Section 5 (Test Scenarios)

Priority 2 (USUALLY):
  - CodingStandards.md: Sections related to SUT understanding
  - Module README: Section 6 (Dependencies - mocking requirements)

Priority 3 (SOMETIMES):
  - DocumentationStandards.md: If test additions require README test strategy updates
```

**DocumentationMaintainer:**
```yaml
Priority 1 (ALWAYS):
  - DocumentationStandards.md: Complete (all sections)
  - DiagrammingStandards.md: If diagrams involved
  - Module README: All 8 sections (comprehensive analysis)

Priority 2 (USUALLY):
  - CodingStandards.md: To understand code patterns being documented
  - TestingStandards.md: To document test strategies accurately

Priority 3 (SOMETIMES):
  - TaskManagementStandards.md: For commit standards
```

### Section-Level Granularity

Instead of loading entire standards documents, load specific sections:

**Example: CodingStandards.md for Simple Service Change**
```yaml
Load_Only:
  - Section 2: Language & Formatting (if naming conventions relevant)
  - Section 3: Architecture & Design (dependency injection patterns)
  - Section 6: Error Handling & Logging

Skip:
  - Section 4: Designing Testable Application Logic (not modifying architecture)
  - Section 5: Asynchronous Programming (not changing async patterns)
  - Section 7-10: Not relevant to current change
```

**Token Savings:** ~40% reduction (full doc ~6,000 tokens → selective ~3,600 tokens)

---

## Module Context Optimization

### Strategic Section Loading

**For Simple Changes (Tier 1):**
```yaml
Module_README_Load:
  - Section 1: Purpose (quick orientation)
  - Section 3: Interface Contract (critical for any change)
  - Section 5: Known pitfalls (avoid common mistakes)

Skip:
  - Section 2: Architecture (not changing design)
  - Section 4: Local conventions (following existing patterns)
  - Section 6: Dependencies (not adding/removing dependencies)
  - Section 7: Rationale (not architectural decision)
  - Section 8: Known issues (unless directly fixing one)
```

**Token Savings:** ~60% reduction (full README ~4,000 tokens → selective ~1,600 tokens)

**For Standard Development (Tier 2):**
```yaml
Module_README_Load:
  - Sections 1-6: Complete (all operational context)

Skip:
  - Section 7: Rationale (only if making architectural decisions)
  - Section 8: Known issues (only if addressing specific issue)
```

**Token Savings:** ~25% reduction

**For Complex Architecture (Tier 3):**
```yaml
Module_README_Load:
  - All 8 sections: Comprehensive understanding required
```

### Dependency Module Selective Loading

When Section 6 references dependency modules:

**High Integration:**
- Load dependency README Sections 1, 3 (purpose and interface contract)

**Low Integration:**
- Read dependency README Section 1 only (understand purpose)
- Skip detailed interface unless integration point is complex

**Token Savings:** ~70% reduction per dependency module

---

## Task-Based Grounding Profiles

### Profile 1: Bug Fix (Minimal Grounding)

**Scenario:** Fix specific bug in existing functionality
**Token Budget:** 2,000-3,000 tokens

```yaml
Phase_1_Standards:
  - CodingStandards.md: Error handling section only
  - TestingStandards.md: Skip (if not fixing test)

Phase_2_Architecture:
  - Root README: Skip (familiar module)

Phase_3_Module:
  - Target module README: Sections 3, 5, 8 only
  - Known issues review for related bugs
```

### Profile 2: Feature Implementation (Standard Grounding)

**Scenario:** Implement new feature in existing module
**Token Budget:** 6,000-8,000 tokens

```yaml
Phase_1_Standards:
  - CodingStandards.md: Sections 2, 3, 6 (naming, DI, error handling)
  - TestingStandards.md: Section 6 (unit test overview)
  - DocumentationStandards.md: Section 3 (README updates)
  - TaskManagementStandards.md: Commit format

Phase_2_Architecture:
  - Root README: Quick skim for orientation

Phase_3_Module:
  - Target module README: Sections 1-6
  - Dependency modules: Section 1, 3 (purpose, contracts)
```

### Profile 3: New Module Creation (Comprehensive Grounding)

**Scenario:** Create entirely new module with documentation
**Token Budget:** 12,000-15,000 tokens

```yaml
Phase_1_Standards:
  - ALL standards: Complete loading
  - DocumentationStandards.md: Extra emphasis

Phase_2_Architecture:
  - Root README: Complete
  - Peer module READMEs: Review for consistency

Phase_3_Module:
  - Parent module README: All 8 sections
  - Sibling modules: Architecture patterns
```

### Profile 4: Test Coverage Addition (Testing Focus)

**Scenario:** Add comprehensive tests for existing service
**Token Budget:** 5,000-7,000 tokens

```yaml
Phase_1_Standards:
  - TestingStandards.md: COMPLETE (all sections)
  - CodingStandards.md: SUT understanding sections

Phase_2_Architecture:
  - Test project TechnicalDesignDocument.md

Phase_3_Module:
  - SUT module README: Sections 3, 5 (contracts, test scenarios)
  - Dependencies: Section 6 for mocking requirements
```

---

## Token Budget Management

### Measurement Techniques

**Estimate Token Count:**
- Typical markdown page: ~800-1,200 tokens
- Standards document (full): ~5,000-8,000 tokens
- Module README (full): ~3,000-5,000 tokens
- Module README section: ~400-800 tokens

**Track Cumulative Loading:**
```yaml
Grounding_Session_Tokens:
  Phase_1_Standards: 4,200 tokens
  Phase_2_Architecture: 1,800 tokens
  Phase_3_Module: 3,500 tokens
  Total_Grounding: 9,500 tokens
  Remaining_Budget: 190,500 tokens (95%)
```

### Budget Exceeded Recovery

**If Grounding Exceeds 15,000 Tokens:**

**Option 1: Section-Level Pruning**
- Revisit each loaded document
- Skip non-critical sections
- Focus on actionable guidance only

**Option 2: Tier Downgrade**
- Switch from Tier 3 to Tier 2 approach
- Load summaries instead of full sections

**Option 3: Just-In-Time Loading**
- Load minimal core upfront
- Reference additional context on-demand during implementation
- "I need to check CodingStandards Section 5 for async patterns"

---

## Emergency Shortcuts (Use Sparingly)

### When Severely Token-Constrained

**Absolute Minimum Grounding:**
```yaml
Phase_1: CodingStandards.md Section 3 only (DI patterns)
Phase_2: Skip entirely
Phase_3: Module README Section 3 only (Interface Contracts)

Total_Tokens: ~1,500 tokens
Risk: Missing critical context, potential standards violations
Use_Case: Emergency hotfix under extreme time pressure
```

**Recovery Plan:**
- Complete full grounding after emergency resolved
- Create follow-up issue to validate emergency implementation against standards

### When to Escalate vs. Shortcut

**Escalate to Claude When:**
- Complex architectural changes requiring comprehensive context
- Security-critical implementations
- Multi-module integrations
- You're unfamiliar with the module

**Acceptable to Shortcut When:**
- Trivial changes (typo fixes, comment updates)
- Extremely familiar module (recent repeated work)
- Changes confined to isolated utility functions
- Time-critical production incident

---

## Quality vs. Efficiency Trade-offs

### Quality Impact Matrix

**Comprehensive Grounding (Tier 3):**
- **Quality:** 95-100% standards compliance, minimal rework
- **Efficiency:** Higher upfront token cost, faster overall delivery
- **Best For:** Complex changes, unfamiliar modules, architectural work

**Standard Grounding (Tier 2):**
- **Quality:** 85-90% standards compliance, occasional minor corrections
- **Efficiency:** Balanced token usage, good delivery speed
- **Best For:** Most development tasks, feature implementation, testing

**Minimal Grounding (Tier 1):**
- **Quality:** 70-80% standards compliance, higher rework risk
- **Efficiency:** Lowest token cost, but potential rework negates savings
- **Best For:** Simple bug fixes, trivial changes, emergency hotfixes

### ROI Analysis

**Token Investment:**
- Tier 1: 2,000-4,000 tokens
- Tier 2: 6,000-8,000 tokens
- Tier 3: 12,000-15,000 tokens

**Rework Cost:**
- Tier 1: 30% chance of rework (~5,000 tokens average)
- Tier 2: 10% chance of rework (~2,000 tokens average)
- Tier 3: <5% chance of rework (~500 tokens average)

**Expected Total Cost:**
- Tier 1: 2,000-4,000 + (0.30 × 5,000) = 3,500-5,500 tokens
- Tier 2: 6,000-8,000 + (0.10 × 2,000) = 6,200-8,200 tokens
- Tier 3: 12,000-15,000 + (0.05 × 500) = 12,025-15,025 tokens

**Insight:** Tier 2 optimal for most tasks. Tier 3 justified for complex/critical work.

---

## Optimization Best Practices

### 1. Front-Load Critical Context
Load Section 3 (Interface Contracts) first - it's always critical

### 2. Skip Redundant Loading
If you loaded CodingStandards.md for previous task in same conversation, reference it rather than re-reading

### 3. Leverage Examples
Examples often contain concentrated actionable patterns (higher value per token)

### 4. Use Templates as Checklists
Templates provide structure without narrative overhead

### 5. Defer Historical Context
Section 7 (Rationale) is lowest priority unless making architectural changes

### 6. Batch Dependency Reviews
When multiple dependencies, load Section 1 for all first, then Section 3 only for high-integration dependencies

### 7. Progressive Depth
Start with Tier 1, expand to Tier 2 if complexity emerges during implementation

---

## Continuous Improvement

### After Each Agent Engagement, Reflect:

**Questions:**
1. Did I load any sections I didn't actually use?
2. Did I encounter issues that better grounding would have prevented?
3. Could I have achieved same quality with less context?
4. What was my actual token budget remaining?

**Adjust Future Profile:**
- If excess context unused: Dial back to lower tier
- If standards violations occurred: Upgrade to higher tier
- If perfect execution: Current tier optimal

---

**Status:** ✅ Living document - update with empirical optimization patterns
**Version:** 1.0.0
**Last Updated:** 2025-10-25
**Skill:** documentation-grounding
