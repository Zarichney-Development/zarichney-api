# Module Context Template

Use this template to systematically analyze module-specific README files and establish comprehensive domain-specific context.

---

## Module Identification

**Module Name:** `[Module name]`
**Module Path:** `[Full path from repository root]`
**README Location:** `[Path to module README.md]`
**Last Updated:** `[Date from README header]`
**Agent Role:** `[Your agent name]`
**Task Context:** `[Brief description of current task]`

---

## Section 1: Purpose & Responsibility

### Functional Role
**What does this module do?**
```
[Summary of module's functional purpose]
```

### Separation Rationale
**Why does it exist as a separate unit?**
```
[Explanation of why this is its own module]
```

### Child Modules
**What sub-modules does this contain?**
```
- [Child module 1]: [Purpose] - Link: [relative path to README]
- [Child module 2]: [Purpose] - Link: [relative path to README]
```

**Impact on Current Task:**
```
[How understanding this module's purpose informs your work]
```

---

## Section 2: Architecture & Key Concepts

### Internal Design Patterns
**What design patterns are used internally?**
```
[Key patterns: Repository, Service Layer, Factory, Strategy, etc.]
```

### Key Components
**What are the main components?**
```
- [Component 1]: [Role and responsibility]
- [Component 2]: [Role and responsibility]
- [Component 3]: [Role and responsibility]
```

### Data Structures
**What are the critical data structures?**
```
- [Data structure 1]: [Purpose and usage]
- [Data structure 2]: [Purpose and usage]
```

### Embedded Diagrams
**What architectural diagrams are present?**
```
- [Diagram 1 type]: [What it illustrates]
- [Diagram 2 type]: [What it illustrates]
```

**Diagram Insights:**
```
[Key insights from visual architecture representations]
```

**Impact on Current Task:**
```
[How understanding this architecture informs your implementation approach]
```

---

## Section 3: Interface Contract & Assumptions ⚠️ CRITICAL

### Preconditions
**What input states/values are required?**
```
- [Precondition 1]: [Required state or value]
- [Precondition 2]: [Required state or value]
```

### Postconditions
**What output states/values/side effects are expected?**
```
- [Postcondition 1]: [Expected outcome]
- [Postcondition 2]: [Expected side effect]
```

### Error Handling
**What specific exceptions are thrown under what conditions?**
```
- [Exception type 1]: [Condition that triggers it]
- [Exception type 2]: [Condition that triggers it]
```

### Invariants
**What must always be true?**
```
- [Invariant 1]: [Condition that must hold]
- [Invariant 2]: [Condition that must hold]
```

### Critical Assumptions
**What assumptions does the code make?**
```
- [Assumption 1]: [About inputs, dependencies, or environment]
- [Assumption 2]: [About inputs, dependencies, or environment]
```

**Behavioral Contracts Beyond Signatures:**
```
[Non-obvious behaviors not evident from method signatures alone]
```

**Impact on Current Task:**
```
[How these contracts constrain or inform your modifications]
[What preconditions must you maintain?]
[What postconditions must you ensure?]
```

---

## Section 4: Local Conventions & Constraints

### Deviations from Global Standards
**What module-specific rules augment or override global standards?**
```
- [Deviation 1]: [Specific local convention]
- [Deviation 2]: [Specific local convention]
```

### Configuration Requirements
**What configuration values are critical?**
```
- [Config 1]: [Purpose and required/optional status]
- [Config 2]: [Purpose and required/optional status]
```

### Environmental Dependencies
**What environmental conditions are required?**
```
- [Dependency 1]: [Environmental requirement]
- [Dependency 2]: [Environmental requirement]
```

### Testing Setup Specifics
**What special testing setup is needed for this module?**
```
- [Setup requirement 1]: [For test execution or validation]
- [Setup requirement 2]: [For test execution or validation]
```

**Impact on Current Task:**
```
[How these local conventions affect your implementation approach]
```

---

## Section 5: How to Work With This Code

### Setup Steps
**What unique setup is required for this module?**
```
1. [Setup step 1]
2. [Setup step 2]
3. [Setup step 3]
```

### Module-Specific Testing Strategy
**What testing approach does this module use?**
```
[Primary testing approach: unit vs integration, why, coverage expectations]
```

### Key Test Scenarios
**What scenarios MUST be covered by tests?**
```
- [Scenario 1]: [Why it's critical]
- [Scenario 2]: [Why it's critical]
- [Scenario 3]: [Why it's critical]
```

### Test Data Considerations
**What test data strategies are relevant?**
```
[Specific data types, generation strategies, edge cases]
```

### Module-Specific Commands
**What commands run tests for this module?**
```bash
# [Command 1]: [Description]
# [Command 2]: [Description]
```

### Known Pitfalls and Gotchas
**What non-obvious behaviors could affect work?**
```
- [Gotcha 1]: [Description and how to handle]
- [Gotcha 2]: [Description and how to handle]
```

**Impact on Current Task:**
```
[How this guidance informs your implementation and testing approach]
```

---

## Section 6: Dependencies

### Internal Modules Consumed
**What internal modules does this module directly use?**
```
- [Module 1]: [Purpose] - README: [relative link]
  - Testing implication: [Mocking requirements]
- [Module 2]: [Purpose] - README: [relative link]
  - Testing implication: [Mocking requirements]
```

### Internal Consumers
**What internal modules directly use this module?**
```
- [Module 1]: [How it uses this module] - README: [relative link]
  - Impact consideration: [Change impact]
- [Module 2]: [How it uses this module] - README: [relative link]
  - Impact consideration: [Change impact]
```

### External Libraries and Services
**What external dependencies are key?**
```
- [Library 1]: [Purpose and version]
  - Testing implication: [Virtualization or mocking needs]
- [Service 1]: [Purpose]
  - Testing implication: [Virtualization requirements]
```

### Dependency README Review
**Which dependency READMEs should be reviewed?**
```
- [ ] [Dependency 1 README]: [Sections to focus on]
- [ ] [Dependency 2 README]: [Sections to focus on]
```

**Impact on Current Task:**
```
[How these dependencies constrain your modifications]
[What mocking or virtualization will be needed for testing?]
[What change impact assessment is required?]
```

---

## Section 7: Rationale & Key Historical Context

### Design Decision Explanations
**Why was the current design chosen?**
```
[Non-obvious design choices and their rationale]
```

### Historical Notes Illuminating Current State
**What historical context is relevant to current design?**
```
[Historical decisions that explain why things are the way they are]
```

**Impact on Current Task:**
```
[How this historical context informs your approach]
[What design constraints are rooted in this history?]
```

---

## Section 8: Known Issues & TODOs

### Current Limitations
**What are this module's known limitations?**
```
- [Limitation 1]: [Description]
- [Limitation 2]: [Description]
```

### Planned Work
**What planned improvements or technical debt items exist?**
```
- [TODO 1]: [Description and priority]
- [TODO 2]: [Description and priority]
```

### Integration Concerns
**What integration issues are known?**
```
[Issues with dependencies or consumers]
```

**Impact on Current Task:**
```
[How known issues affect your work]
[What workarounds might be needed?]
[What improvements could be incorporated?]
```

---

## Context Integration Summary

### Critical Insights for Current Task
```
1. [Most important insight from grounding]
2. [Second most important insight]
3. [Third most important insight]
```

### Constraints to Respect
```
- [Constraint 1]: [From interface contracts]
- [Constraint 2]: [From local conventions]
- [Constraint 3]: [From dependencies]
```

### Testing Requirements Identified
```
- [Test requirement 1]: [From Section 5 key scenarios]
- [Test requirement 2]: [From Section 3 interface contracts]
- [Test requirement 3]: [From Section 6 dependencies]
```

### Implementation Approach Informed by Grounding
```
[How this comprehensive module context shapes your implementation strategy]
```

---

## Grounding Completion Validation

- [ ] All 8 README sections analyzed
- [ ] Section 3 (Interface Contracts) thoroughly understood
- [ ] Dependencies mapped and relevant READMEs identified
- [ ] Testing strategy and key scenarios documented
- [ ] Known issues and limitations acknowledged
- [ ] Critical insights extracted for current task
- [ ] Implementation approach informed by comprehensive context

**Grounding Status:** ✅ COMPLETE / ⚠️ IN PROGRESS / ❌ INCOMPLETE

---

**Usage Notes:**
- Complete this template for each module you're modifying
- Focus extra attention on Section 3 (Interface Contracts) - it's critical
- Review dependency module READMEs when integration points are complex
- Extract actionable insights in the summary section
- Include this context in your completion report

---

**Template Version:** 1.0.0
**Last Updated:** 2025-10-25
**Skill:** documentation-grounding
