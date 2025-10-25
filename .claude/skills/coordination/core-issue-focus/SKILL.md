---
name: core-issue-focus
description: Mission discipline framework preventing scope creep and ensuring surgical focus on specific blocking technical problems. Use when receiving complex missions, during implementations with scope expansion risk, or before expanding beyond original technical problem.
---

# Core Issue Focus Skill

**Version:** 1.0.0
**Category:** Coordination
**Target Agents:** TestEngineer, PromptEngineer, CodeChanger, BackendSpecialist, FrontendSpecialist, WorkflowEngineer

---

## PURPOSE

This skill defines mission discipline framework preventing scope creep during agent implementations through surgical focus on specific blocking technical problems.

### Core Mission
Ensure agents resolve specific blocking technical problems without scope expansion, maintain surgical focus throughout implementation, and validate completion through testable success criteria before moving to next tasks.

### Why This Matters
Agents naturally identify improvement opportunities during implementation. Without mission discipline, "fixing a bug" becomes "refactoring the entire service," delaying core issue resolution and creating scope creep. This skill provides systematic approach to maintain surgical focus while documenting valid improvements for future work.

### Target Application
**Primary Agents (Mission Discipline Required):**
- **TestEngineer**: Focus on specific test coverage gaps, not wholesale test refactoring
- **PromptEngineer**: Fix specific prompt issues, not entire agent redefinition
- **CodeChanger**: Implement specific feature/fix, not opportunistic refactoring
- **BackendSpecialist**: Resolve specific API issue, not entire service layer redesign
- **FrontendSpecialist**: Fix specific component bug, not UI/UX overhaul
- **WorkflowEngineer**: Address specific CI/CD issue, not complete pipeline redesign

---

## WHEN TO USE

This skill applies in these scenarios:

### 1. When Receiving Mission from Claude (RECOMMENDED)
**Trigger:** Agent receives context package with CORE_ISSUE field defining specific blocking problem
**Action:** Execute Step 1 (Identify Core Issue First) to validate understanding and scope boundaries
**Rationale:** Establishes surgical focus before implementation begins, preventing scope drift from start

### 2. During Complex Implementations (RECOMMENDED)
**Trigger:** Implementation involves multiple files or components with scope expansion risk
**Action:** Execute Step 2 (Surgical Scope Definition) and Step 3 (Mission Drift Detection) continuously
**Rationale:** Maintains surgical focus during implementation, identifies when improvements become scope creep

### 3. When Tempted by Improvements (CRITICAL)
**Trigger:** Agent identifies improvement opportunity not directly related to core blocking issue
**Action:** Execute Step 3 (Mission Drift Detection) to classify as deferred improvement
**Rationale:** Acknowledges valid improvements while maintaining mission discipline

### 4. Before Expanding Scope (MANDATORY)
**Trigger:** Agent considers implementing changes beyond original technical problem definition
**Action:** Execute Step 4 (Core Issue Validation) to confirm core issue resolved first
**Rationale:** Prevents scope expansion before core mission complete

---

## WORKFLOW STEPS

### Step 1: Identify Core Issue First

**BEFORE STARTING IMPLEMENTATION**, validate understanding of specific blocking technical problem.

#### Core Issue Identification Process
1. **Extract Core Issue from Context Package**: Review CORE_ISSUE field in Claude's context package
2. **Validate Blocking Nature**: Confirm what can't be done until this is fixed
3. **Define Minimum Viable Fix**: Identify minimum changes needed to resolve blocker
4. **Document Success Criteria**: Establish testable outcomes proving issue resolved

#### Core Issue Documentation Format
```
# Core Issue Analysis

**Core Blocking Problem:** [One-sentence description of the blocking issue]

**Why This Blocks Progress:** [Explanation of impact - what can't be done until this is fixed]

**Minimum Viable Fix:** [Description of surgical fix scope]

**Success Criteria:**
1. [ ] [Testable outcome 1]
2. [ ] [Testable outcome 2]
3. [ ] [Testable outcome 3]
```

#### Core Issue Validation Checklist
- [ ] Core blocking problem clearly identified (one-sentence description)
- [ ] Impact validated (confirmed what's blocked until fixed)
- [ ] Minimum viable fix defined (surgical scope, not comprehensive overhaul)
- [ ] Success criteria established (3-5 testable outcomes)
- [ ] Context package CORE_ISSUE field aligned with analysis

**Resource:** See `resources/templates/core-issue-analysis-template.md` for complete template

---

### Step 2: Surgical Scope Definition

**DURING PLANNING PHASE**, define precise boundaries of in-scope vs. out-of-scope work.

#### Scope Boundary Definition Process
1. **List In-Scope Changes**: Exact files and modifications needed to fix core issue
2. **Identify Out-of-Scope Improvements**: Valid improvements to defer to future work
3. **Document Integration Boundaries**: What other systems are affected (minimal integration)
4. **Establish Validation Approach**: How to test core functionality works after fix

#### Scope Boundary Documentation Format
```
# Scope Boundaries

**IN SCOPE (Core Issue Resolution):**
- [Specific file 1]: [Exact modification needed]
- [Specific file 2]: [Exact modification needed]
- [Validation test]: [Test demonstrating core issue resolved]

**OUT OF SCOPE (Deferred Improvements):**
- [Secondary improvement 1]: Defer to [future issue/epic]
- [Secondary improvement 2]: Defer to [future issue/epic]
- [Opportunistic refactoring]: Defer to [tech debt backlog]

**INTEGRATION BOUNDARIES:**
- [Affected system 1]: [Minimal integration required]
- [Affected system 2]: [No changes needed, validated compatibility]
```

#### Scope Definition Checklist
- [ ] In-scope changes listed with specific files and modifications
- [ ] Out-of-scope improvements identified and deferred
- [ ] Integration boundaries documented (minimal necessary)
- [ ] Validation approach established for core functionality
- [ ] Context package SCOPE_BOUNDARY field aligned with definition

**Resource:** See `resources/templates/scope-boundary-definition.md` for complete template

---

### Step 3: Mission Drift Detection

**DURING IMPLEMENTATION**, continuously monitor for scope expansion signals.

#### Common Scope Expansion Triggers
1. **"While I'm Here" Syndrome**: Modifying unrelated files because "I'm already working in this area"
2. **"Perfect is the Enemy of Done"**: Continuing improvements after success criteria met
3. **"Consistency Refactoring"**: Finding similar patterns elsewhere and refactoring all instances
4. **"Opportunistic Testing"**: Adding tests beyond core issue validation
5. **"Infrastructure Enhancement"**: Improving tooling/infrastructure while fixing core issue

#### Mission Drift Warning Signs
**Red Flags:**
- Number of modified files exceeds core issue scope definition
- Implementation time extends significantly beyond estimate
- "Improvements" not mentioned in success criteria being implemented
- Changes touch code unrelated to blocking problem
- Creating new abstractions not required for core fix

**Healthy Focus:**
- All changes directly resolve core blocking issue
- Modified files align with scope boundary definition
- Success criteria guide all implementation decisions
- Deferred list grows (documenting future improvements)
- Regular checkpoint validation confirms no drift

#### Mission Drift Classification
When improvement opportunity identified:
```
**Improvement Opportunity Detected:**
- **Description:** [What improvement was identified]
- **Classification:** [CORE_ISSUE_REQUIRED / DEFER_TO_FUTURE]
- **Rationale:** [Why this is/isn't essential for core issue resolution]
- **Action:** [IMPLEMENT_NOW / ADD_TO_DEFERRED_LIST]
```

#### Mission Drift Detection Checklist
- [ ] Monitored for "while I'm here" syndrome during implementation
- [ ] Validated all changes directly resolve core blocking issue
- [ ] Classified improvement opportunities as core-required vs. deferred
- [ ] Maintained deferred improvements list for future work
- [ ] No scope expansion beyond defined boundaries

**Resource:** See `resources/documentation/mission-drift-patterns.md` for comprehensive drift pattern catalog

---

### Step 4: Core Issue Validation

**AFTER IMPLEMENTATION**, validate core issue resolved before considering additional work.

#### Core Functionality Validation Process
1. **Test Success Criteria**: Verify all testable outcomes met
2. **Validate Core Functionality**: Confirm blocking problem no longer exists
3. **Review Scope Compliance**: Ensure no scope expansion occurred
4. **Document Deferred Improvements**: Capture all out-of-scope work for future

#### Success Criteria Validation Format
```
# Core Issue Validation

**Success Criteria Status:**
1. [✅ / ❌] [Testable outcome 1] - [Evidence/test results]
2. [✅ / ❌] [Testable outcome 2] - [Evidence/test results]
3. [✅ / ❌] [Testable outcome 3] - [Evidence/test results]

**Core Functionality Validation:**
- **Problem Before Fix:** [Description of blocking issue]
- **Problem After Fix:** [Confirmation issue resolved]
- **Validation Approach:** [How functionality was tested]
- **Result:** [CORE_ISSUE_RESOLVED / PARTIAL / UNRESOLVED]

**Scope Compliance Review:**
- **Modified Files:** [List actual files changed]
- **Scope Boundary Match:** [COMPLIANT / VIOLATION_DETECTED]
- **Mission Drift:** [NONE / DETECTED - specify if drift occurred]

**Deferred Improvements Documented:**
- [Improvement 1]: [Created issue #ABC for future work]
- [Improvement 2]: [Added to tech debt backlog]
- [Improvement 3]: [Documented in service enhancement epic]
```

#### Core Issue Validation Checklist
- [ ] All success criteria met and validated
- [ ] Core functionality works as intended (blocking problem resolved)
- [ ] Scope compliance confirmed (no unauthorized expansion)
- [ ] All deferred improvements documented for future work
- [ ] Ready to mark mission COMPLETE

**Resource:** See `resources/templates/success-criteria-validation.md` for complete template

---

## AGENT-SPECIFIC PATTERNS

### TestEngineer: Test Coverage Mission Discipline
**Pattern:** Add tests for uncovered methods/scenarios in target component only
**Scope Creep Risk:** Refactoring test infrastructure, standardizing patterns across codebase
**Example:** UserService.GetUserById missing coverage → Add GetUserById tests only, defer comprehensive UserService coverage

### PromptEngineer: Prompt Optimization Mission Discipline
**Pattern:** Modify specific prompt sections addressing identified issue only
**Scope Creep Risk:** Complete prompt rewrite, standardizing all placeholders
**Example:** DebtSentinel missing context ingestion → Add ingestion section only, defer AI Sentinel standardization

### CodeChanger: Feature Implementation Mission Discipline
**Pattern:** Minimum viable implementation satisfying user need
**Scope Creep Risk:** Feature enhancements, related features, performance optimizations
**Example:** Ingredient filter feature → Simple filter only, defer advanced combinations and performance optimization

### BackendSpecialist: API Issue Mission Discipline
**Pattern:** Resolve specific API problem with minimal changes
**Scope Creep Risk:** Service layer refactoring, async/await migration, architecture redesign
**Example:** UserService.GetUserById 500 error → Null check only, defer async refactoring and error standardization

### FrontendSpecialist: Component Bug Mission Discipline
**Pattern:** Resolve component bug with minimal UI changes
**Scope Creep Risk:** Component redesign, state management refactoring, UI/UX overhaul
**Example:** Recipe card serving size bug → Fix calculation only, defer UI redesign and state optimization

### WorkflowEngineer: CI/CD Issue Mission Discipline
**Pattern:** Resolve specific workflow problem with minimal changes
**Scope Creep Risk:** Complete pipeline redesign, workflow consolidation
**Example:** Coverage report generation failure → Fix report step only, defer pipeline optimization and consolidation

---

## INTEGRATION WITH CONTEXT PACKAGES

### CORE_ISSUE Field Mapping
Claude's context packages include CORE_ISSUE field defining specific blocking problem:
```yaml
CORE_ISSUE: "[Specific blocking technical problem to resolve]"
```

**Skill Integration:**
- Step 1 validates understanding of CORE_ISSUE field
- Documents core blocking problem, minimum viable fix, success criteria
- Ensures agent and Claude aligned on mission scope

### SCOPE_BOUNDARY Field Mapping
Context packages include SCOPE_BOUNDARY field defining exact files/areas agent can modify:
```yaml
SCOPE_BOUNDARY: "[Exact files/areas agent can modify based on intent and authority]"
```

**Skill Integration:**
- Step 2 defines surgical scope boundaries aligned with context package
- Documents in-scope changes, out-of-scope improvements, integration boundaries
- Ensures agent stays within authorized modification areas

### SUCCESS_CRITERIA Field Mapping
Context packages include SUCCESS_CRITERIA field defining testable outcomes:
```yaml
SUCCESS_CRITERIA: "[Testable outcome proving core issue resolved]"
```

**Skill Integration:**
- Step 1 establishes success criteria from context package
- Step 4 validates all criteria met before marking mission complete
- Ensures objective measurement of core issue resolution

### FORBIDDEN SCOPE EXPANSIONS Integration
Context packages include FORBIDDEN list preventing specific scope creep patterns:
```yaml
FORBIDDEN SCOPE EXPANSIONS:
- Infrastructure improvements while core issue unfixed
- Working directory protocols during syntax error fixes
- Feature additions not directly related to core problem
- Cross-agent coordination enhancements during single-issue fixes
```

**Skill Integration:**
- Step 2 incorporates forbidden patterns into out-of-scope definition
- Step 3 detects mission drift matching forbidden patterns
- Ensures common scope creep patterns prevented systematically

---

## MISSION DRIFT RECOVERY PROTOCOLS

### When to STOP and Refocus

**Detection Triggers:**
1. Modified files exceed scope boundary definition by 2+ files
2. Implementation time exceeds estimate by 50%+ without completion
3. Creating new abstractions/infrastructure not in scope definition
4. Implementing improvements not in success criteria
5. Touching code paths unrelated to core blocking problem

**Immediate Actions:**
1. **STOP CURRENT WORK**: Cease implementation immediately
2. **DOCUMENT CURRENT STATE**: Save progress, note what was implemented vs. planned
3. **EXECUTE STEP 3**: Run mission drift detection to classify all changes
4. **IDENTIFY SCOPE VIOLATIONS**: Determine which changes are out-of-scope

### How to Revert Scope Creep

**Scope Creep Remediation Process:**
1. **Classify All Changes**: Review all file modifications against scope boundary definition
2. **Separate Core from Creep**: Identify changes directly resolving core issue vs. improvements
3. **Preserve Core Changes**: Keep modifications essential for core issue resolution
4. **Revert Creep Changes**: Undo modifications that are valid improvements but out-of-scope
5. **Document Reverted Work**: Add reverted improvements to deferred list with rationale

**Reversion Format:**
```
# Scope Creep Remediation

**Changes Classified:**
- **Core Issue Changes (KEPT):**
  - [File 1]: [Modification directly resolving core issue]
  - [File 2]: [Modification directly resolving core issue]

- **Scope Creep Changes (REVERTED):**
  - [File 3]: [Valid improvement but out-of-scope] - DEFERRED to [issue/epic]
  - [File 4]: [Infrastructure enhancement] - DEFERRED to [tech debt backlog]

**Deferred Improvements:**
- [Improvement 1]: [Rationale for deferral, future work location]
- [Improvement 2]: [Rationale for deferral, future work location]
```

### Escalation Patterns to Claude

**Escalate to Claude When:**
1. **Uncertainty About Core Issue**: Unclear whether change is essential for core issue resolution
2. **Conflicting Priorities**: Core issue conflicts with architectural standards or quality requirements
3. **Scope Expansion Requested**: User feedback suggests expanding scope beyond original mission
4. **Multiple Drift Instances**: Recurring scope creep indicates mission unclear

**Escalation Format:**
```
🚨 MISSION CLARITY ESCALATION

**Core Issue:** [Original blocking problem from context package]

**Uncertainty:** [Specific question about what's in-scope vs. out-of-scope]

**Conflicting Signals:**
- Context Package Says: [Scope definition from CORE_ISSUE/SCOPE_BOUNDARY]
- Implementation Reality: [Practical need identified during work]

**Request:** [Specific guidance needed to maintain mission discipline]
```

### Documenting Deferred Improvements

**Deferred List Management:**
- Create during Step 2 (Surgical Scope Definition)
- Maintain during Step 3 (Mission Drift Detection) as improvements identified
- Finalize during Step 4 (Core Issue Validation) for handoff

**Deferred Improvement Format:**
```
# Deferred Improvements

**Issue:** [Original GitHub issue being resolved]
**Agent:** [Agent who identified improvements]

**Improvements Deferred:**
1. **[Improvement Name]**
   - **Description:** [What improvement was identified]
   - **Rationale for Deferral:** [Why this is out-of-scope for current mission]
   - **Future Work:** [Where this should be addressed - issue #, epic, backlog]
   - **Estimated Impact:** [Benefit if implemented in future]

2. **[Improvement Name]**
   - [Same structure as above]
```

---

## RESOURCES

This skill includes comprehensive resources for mission discipline implementation:

### Templates (Ready-to-Use Formats)
- **core-issue-analysis-template.md**: Standard format for Step 1 (Identify Core Issue First)
- **scope-boundary-definition.md**: Standard format for Step 2 (Surgical Scope Definition)
- **success-criteria-validation.md**: Standard format for Step 4 (Core Issue Validation)

**Location:** `resources/templates/`
**Usage:** Copy template, fill in specific details for current mission, use to maintain discipline

### Examples (Reference Implementations)
- **api-bug-fix-example.md**: Backend API bug fix with scope discipline (UserService.GetUserById)
- **feature-implementation-focused.md**: Feature implementation with MVP focus and deferred enhancements
- **refactoring-scoped.md**: Targeted refactoring without wholesale rewrite

**Location:** `resources/examples/`
**Usage:** Review examples for realistic scenarios showing all 4 workflow steps preventing mission drift

### Documentation (Deep Dives)
- **mission-drift-patterns.md**: Comprehensive catalog of common scope expansion triggers and prevention
- **validation-checkpoints.md**: When to verify core issue status during implementation milestones

**Location:** `resources/documentation/`
**Usage:** Understand mission drift psychology, identify triggers early, maintain surgical focus

---

## SUCCESS METRICS

### Mission Discipline Effectiveness
- **Surgical Focus Maintained**: All changes directly resolve core blocking issue
- **Success Criteria Met**: 100% of defined outcomes achieved
- **Scope Compliance**: Zero unauthorized scope expansion
- **Timely Completion**: Core issue resolved within estimated timeframe

### Quality Without Creep
- **Core Functionality Works**: Blocking problem no longer exists
- **Tests Validate Fix**: Success criteria proven through testing
- **No Regression**: Existing functionality unaffected
- **Deferred List Comprehensive**: All valid improvements captured for future

### Team Coordination Quality
- **Clear Mission Understanding**: Agent and Claude aligned on core issue
- **Transparent Progress**: Mission drift detected early and addressed
- **Efficient Handoffs**: Deferred improvements clearly documented for future agents
- **Adaptive Planning**: Claude can plan next steps knowing core issue resolved

---

**Skill Status:** ✅ **OPERATIONAL**
**Adoption:** Recommended for 6 primary agents (TestEngineer, PromptEngineer, CodeChanger, BackendSpecialist, FrontendSpecialist, WorkflowEngineer)
**Context Savings:** ~200 lines across agents (~1,600 tokens total)
**Progressive Loading:** frontmatter (~100 tokens) → SKILL.md (~2,500 tokens) → resources (on-demand)
