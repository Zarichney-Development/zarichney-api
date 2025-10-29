# Mission Drift Patterns & Prevention

**Purpose:** Comprehensive catalog of common scope expansion triggers, warning signs, and prevention strategies
**Audience:** All agents using core-issue-focus skill
**Usage:** Reference during Step 3 (Mission Drift Detection) to identify and prevent scope creep

---

## TABLE OF CONTENTS

1. [Common Scope Expansion Triggers](#common-scope-expansion-triggers)
2. [Warning Signs of Mission Drift](#warning-signs-of-mission-drift)
3. [Recovery Protocols](#recovery-protocols)
4. [Prevention Best Practices](#prevention-best-practices)

---

## COMMON SCOPE EXPANSION TRIGGERS

### Trigger 1: "While I'm Here" Syndrome

**Pattern Description:**
Agent modifies File A for core issue, notices improvement opportunity in File B or related code, decides to "fix it while I'm here."

**Example Scenarios:**
- Fixing API bug in UserService, notices similar bug in GetUserByEmail method
- Adding test for CreateRecipe, sees RecipeServiceTests base class could be improved
- Implementing ingredient filter, identifies autocomplete would improve UX
- Refactoring validation duplication, sees opportunity to add FluentValidation library

**Why It's Tempting:**
- Efficiency perception: "I'm already in this area of code, might as well fix everything"
- Avoidance of context switching: "If I defer this, I'll have to reload this context later"
- Completeness desire: "Why fix one instance when I could fix all similar issues?"

**Why It's Dangerous:**
- Exponential scope growth: Each "while I'm here" reveals more improvements
- Unpredictable timeline: Additional work not estimated or planned
- Testing burden: More changes = more test scenarios = longer validation
- Integration risk: Touching more code increases regression potential

**Prevention Strategy:**
- Document improvement in deferred list immediately when identified
- Create specific GitHub issue for related improvement (separate scope)
- Validate change against SCOPE_BOUNDARY from context package (is this authorized?)
- Ask: "Does this directly resolve the core blocking problem?" If no, defer

**Example (Good Discipline):**
```
Agent fixing UserService.GetUserById null profile bug notices GetUserByEmail has similar issue

❌ SCOPE CREEP: "I'll fix GetUserByEmail too while I'm here"
✅ MISSION DISCIPLINE: "Document GetUserByEmail in deferred list, create Issue #ABC for future work"

Deferred Improvement: Fix GetUserByEmail null profile handling → Issue #ABC
```

---

### Trigger 2: "Perfect is the Enemy of Done"

**Pattern Description:**
Core issue fixed and success criteria met, but agent continues improving beyond requirements because implementation "could be better."

**Example Scenarios:**
- Bug fixed, tests passing, but agent adds comprehensive logging "just in case"
- Feature MVP delivered, but agent implements 3 enhancement ideas "while I'm thinking about it"
- Validation extracted, duplication eliminated, but agent refactors validation rules "to make them more robust"
- API returns correct data, but agent optimizes query performance "for scalability"

**Why It's Tempting:**
- Craftsmanship pride: "I want this to be the best implementation possible"
- Anticipating future needs: "Someone will probably want these enhancements eventually"
- Avoiding rework: "If I don't do this now, we'll have to come back to it later"

**Why It's Dangerous:**
- Never-ending improvements: Always something that "could be better"
- Diminishing returns: Each enhancement adds less value than previous
- Premature optimization: Solving problems that may never materialize
- Delayed delivery: Core issue resolved but work continues indefinitely

**Prevention Strategy:**
- Review success criteria after implementation (all met? Mission complete!)
- Document enhancements in deferred list even if "would only take 5 more minutes"
- Validate with context package SUCCESS_CRITERIA (enhancements not listed? Out of scope)
- Ask: "Is this required for core issue resolution or nice-to-have enhancement?" If enhancement, defer

**Example (Good Discipline):**
```
Agent implemented ingredient filter MVP, all success criteria met, identifies 5 enhancements

❌ SCOPE CREEP: "Let me add autocomplete, multi-ingredient filtering, and saved filters now that basic filter works"
✅ MISSION DISCIPLINE: "Core use case enabled, all criteria met, document 5 enhancements for future iterations"

Deferred Enhancements:
1. Autocomplete for ingredient input → Frontend epic "Recipe search UX improvements"
2. Multi-ingredient filtering → Issue #XYZ "Advanced recipe filtering"
3. Saved filter preferences → Issue #GHI "User recipe search preferences"
4. Filter performance optimization → Performance epic when catalog exceeds 5,000 recipes
5. Enhanced filter UI → Requires UX design, separate feature effort
```

---

### Trigger 3: "Consistency Refactoring"

**Pattern Description:**
Agent fixes specific instance of pattern/code smell, notices same pattern elsewhere in codebase, decides to refactor all instances for consistency.

**Example Scenarios:**
- Extracts validation in RecipeService, sees UserService has same duplication
- Fixes error handling in one API endpoint, standardizes across all 30 endpoints
- Adds null check in GetUserById, refactors all service methods for consistent null handling
- Updates test pattern in one test file, applies to entire test suite

**Why It's Tempting:**
- Consistency desire: "Why have different patterns across similar code?"
- DRY principle adherence: "If duplication is bad here, it's bad everywhere"
- Proactive quality improvement: "Fix the pattern once, prevent future instances"

**Why It's Dangerous:**
- Massive scope expansion: 1 instance → N instances across codebase
- Cross-domain coordination: May require multiple agents (backend, frontend, testing)
- Unpredictable impact: Each context has unique constraints and considerations
- Regression risk: Changing many files increases chance of breaking something

**Prevention Strategy:**
- Fix specific instance mentioned in CORE_ISSUE only
- Document pattern across codebase in deferred list
- Create standardization issue/epic for broader consistency work
- Ask: "Is this inconsistency blocking current issue resolution?" If no, defer

**Example (Good Discipline):**
```
Agent extracts RecipeService validation duplication, identifies 4 other services with same smell

❌ SCOPE CREEP: "I'll extract validation duplication in UserService, IngredientService, OrderService, and ReviewService too"
✅ MISSION DISCIPLINE: "Fixed RecipeService as specified, documented pattern for standardization initiative"

Deferred Standardization:
- UserService validation extraction → Apply same pattern in separate issue
- Service validation consistency → Issue #STU "Service validation standardization across API"
- Pattern documentation → Tech debt backlog "Validation pattern guidelines"
```

---

### Trigger 4: "Opportunistic Testing"

**Pattern Description:**
Agent adds test for core issue validation, identifies test coverage gaps in related functionality, decides to add comprehensive test coverage.

**Example Scenarios:**
- Adds test for null profile handling, sees 10 other UserService methods without tests
- Creates ingredient filter test, decides to add tests for all search functionality
- Fixes bug, adds regression test, notices test suite has gaps everywhere
- Implements feature, adds integration test, refactors test infrastructure "to make future tests easier"

**Why It's Tempting:**
- Quality improvement: "More tests = better quality"
- Efficiency perception: "I'm already writing tests, might as well cover everything"
- Future productivity: "Better test infrastructure now = easier testing later"

**Why It's Dangerous:**
- Exponential test scope: One test → comprehensive test suite
- Test infrastructure tangent: Simple test → complete testing framework redesign
- Delayed completion: Core functionality working but testing never "complete"
- Diminishing validation: Tests for tests' sake rather than validating specific behavior

**Prevention Strategy:**
- Add tests validating success criteria only
- Document test coverage gaps in deferred list (TestEngineer backlog)
- Rely on existing test infrastructure (defer improvements to test infrastructure epic)
- Ask: "Does this test validate core issue resolution or general coverage gap?" If general, defer

**Example (Good Discipline):**
```
Agent adds test for GetUserById null profile handling, identifies 8 untested UserService methods

❌ SCOPE CREEP: "I'll add comprehensive test coverage for all UserService methods"
✅ MISSION DISCIPLINE: "Added test validating null profile fix, documented coverage gaps for TestEngineer"

Deferred Test Work:
- UserService comprehensive coverage → TestEngineer backlog (separate coverage issue)
- Test infrastructure improvements → Issue #VWX "UserServiceTests base class refactoring"
- Test data builder enhancements → Tech debt "Test fixture improvements"
```

---

### Trigger 5: "Infrastructure Enhancement"

**Pattern Description:**
Agent needs infrastructure/tooling for core issue resolution, decides to improve infrastructure comprehensively rather than minimally.

**Example Scenarios:**
- Needs database query for fix, decides to optimize entire database schema
- Requires logging for debugging, implements comprehensive logging framework
- Needs test fixture, refactors entire test infrastructure
- Implements CI/CD fix, redesigns complete pipeline

**Why It's Tempting:**
- Root cause focus: "Fix infrastructure, prevent future issues"
- Long-term investment: "Good infrastructure benefits all future work"
- Avoiding technical debt: "Do it right now instead of quick fix"

**Why It's Dangerous:**
- Infrastructure scope explosion: Simple tool need → complete infrastructure overhaul
- Unpredictable complexity: Infrastructure work reveals more infrastructure needs
- Delayed core fix: Core issue remains unresolved while infrastructure improves
- Over-engineering: Building infrastructure for hypothetical future needs

**Prevention Strategy:**
- Use existing infrastructure minimally (defer improvements to infrastructure epic)
- Validate infrastructure change required for core fix (FORBIDDEN SCOPE EXPANSIONS check)
- Document infrastructure limitations in deferred list (separate infrastructure work)
- Ask: "Can I resolve core issue with existing infrastructure?" If yes, use existing

**Example (Good Discipline):**
```
Agent fixing test coverage report generation issue, tempted to redesign entire CI/CD pipeline

❌ SCOPE CREEP: "I'll refactor the entire pipeline while fixing this report generation step"
✅ MISSION DISCIPLINE: "Fixed test coverage report step only, documented pipeline optimization for WorkflowEngineer"

Deferred Infrastructure:
- Pipeline optimization → WorkflowEngineer epic "CI/CD pipeline improvements"
- Workflow consolidation → Issue #ABC "Consolidate redundant GitHub Actions workflows"
- Reporting infrastructure upgrade → Tech debt "Test reporting framework migration"
```

---

### Trigger 6: "Architectural Enlightenment"

**Pattern Description:**
Agent working on specific issue, realizes better architectural approach exists, decides to refactor architecture during implementation.

**Example Scenarios:**
- Fixing service method, realizes entire service should follow clean architecture
- Implementing feature, decides to introduce CQRS pattern
- Adding validation, decides domain model should enforce invariants
- Fixing bug, realizes microservices would prevent this class of issues

**Why It's Tempting:**
- Technical excellence: "This is the right way to architect this"
- Proactive improvement: "Prevent future architectural issues"
- Learning opportunity: "Chance to apply best practices I've learned"

**Why It's Dangerous:**
- Fundamental scope change: Bug fix → architectural redesign
- Massive coordination requirement: Architecture changes affect entire team
- Unpredictable timeline: Architecture work is never "small"
- Over-engineering: Solving general problem when specific fix needed

**Prevention Strategy:**
- Implement fix using existing architecture (preserve consistency)
- Document architectural vision in deferred list (architecture epic)
- Validate with context package FORBIDDEN SCOPE EXPANSIONS (architecture changes prohibited during focused fixes)
- Ask: "Is architectural change required for core fix or future vision?" If future, defer

**Example (Good Discipline):**
```
Agent extracting validation duplication, realizes clean architecture with separate validators would be better

❌ SCOPE CREEP: "I'll refactor to clean architecture with separate validator classes"
✅ MISSION DISCIPLINE: "Extracted to private method (existing pattern), documented clean architecture vision for future epic"

Deferred Architecture:
- Clean architecture refactoring → Epic #PQR "Service layer clean architecture migration"
- Validator separation → Requires comprehensive service layer changes
- CQRS pattern introduction → Fundamental architecture shift, separate initiative
```

---

## WARNING SIGNS OF MISSION DRIFT

### Quantitative Red Flags

**File Count Expansion:**
- **Warning Sign:** Modified files exceed scope boundary definition by 2+ files
- **Example:** Scope defined 2 files (service + test), actual modified 5 files (added controller, model, configuration)
- **Action:** Review additional files - are they required for core fix or scope creep?

**Implementation Time Extension:**
- **Warning Sign:** Implementation time exceeds estimate by 50%+ without completion
- **Example:** Estimated 2 hours, 4 hours elapsed and still implementing enhancements
- **Action:** Review work completed vs. success criteria - is core issue resolved already?

**Line Count Explosion:**
- **Warning Sign:** Code changes 3x larger than estimated for surgical fix
- **Example:** Expected ~20 line change, implemented 150 line refactoring
- **Action:** Validate all changes directly resolve core blocking problem

**Dependency Addition:**
- **Warning Sign:** New NuGet packages, npm packages, or infrastructure dependencies added
- **Example:** Bug fix required FluentValidation, SignalR, or Redis dependencies
- **Action:** Validate dependency absolutely required for core fix (usually not)

**Abstraction Creation:**
- **Warning Sign:** New classes, interfaces, or architectural patterns introduced
- **Example:** Simple validation extraction became separate validator framework
- **Action:** Confirm abstractions essential for core issue (usually surgical fix doesn't require new abstractions)

### Qualitative Red Flags

**"Improvements" Not in Success Criteria:**
- **Warning Sign:** Implementing functionality not listed in success criteria
- **Example:** Success criteria: "Add ingredient filter", implementing autocomplete, multi-filter, saved preferences
- **Action:** Review success criteria - if not listed, belongs in deferred list

**Changes Touch Unrelated Code:**
- **Warning Sign:** Modifying code paths not involved in core blocking problem
- **Example:** Fixing GetUserById bug, modifying GetUserByEmail, GetUserByUsername
- **Action:** Confirm all changes necessary for core issue resolution

**Framework/Pattern Migrations:**
- **Warning Sign:** Migrating to new framework, library, or architectural pattern
- **Example:** Validation extraction became FluentValidation migration for entire API
- **Action:** Validate migration required for core fix (almost never is)

**Documentation/Infrastructure Work:**
- **Warning Sign:** Comprehensive documentation updates, test infrastructure refactoring, build process improvements
- **Example:** Simple bug fix includes README rewrite, test base class redesign, CI/CD pipeline optimization
- **Action:** Confirm documentation/infrastructure changes essential for core fix

**"While I'm Here" Statements:**
- **Warning Sign:** Agent commentary includes "while I'm here", "might as well", "would be easy to"
- **Example:** "While I'm fixing GetUserById, might as well refactor the entire UserService"
- **Action:** IMMEDIATE STOP - classic scope creep indicator, defer improvements

### Healthy Focus Indicators

**Success Criteria Guidance:**
- **Healthy Sign:** All implementation decisions guided by success criteria
- **Validation:** Ask "Does this help meet success criteria?" for each change

**Deferred List Growth:**
- **Healthy Sign:** Deferred improvements list growing as implementation proceeds
- **Validation:** Valid improvements identified but consciously deferred

**Scope Boundary Adherence:**
- **Healthy Sign:** Modified files match scope boundary definition exactly
- **Validation:** No unauthorized file modifications

**Regular Checkpoint Validation:**
- **Healthy Sign:** Periodic validation that core issue remains focus
- **Validation:** Can clearly articulate how current work resolves blocking problem

**Minimal Abstractions:**
- **Healthy Sign:** Using existing patterns rather than creating new ones
- **Validation:** No new classes/interfaces unless absolutely essential for core fix

---

## RECOVERY PROTOCOLS

### Detection: When to STOP and Refocus

**Immediate STOP Triggers:**
1. **Modified files exceed scope by 2+:** STOP, review additional files for necessity
2. **Implementation time 50%+ over estimate:** STOP, validate core issue already resolved
3. **Creating new infrastructure/abstractions:** STOP, confirm essential for core fix
4. **Implementing improvements not in success criteria:** STOP, defer to deferred list
5. **"While I'm here" thought pattern:** STOP, document improvement, return to core issue

**STOP Action Protocol:**
1. Cease current implementation work immediately
2. Save current progress (don't lose work, but don't continue either)
3. Document current state (what was implemented vs. what was planned)
4. Execute mission drift classification (Step 3 of core-issue-focus skill)

### Classification: Scope Creep Remediation

**Process:**
1. **List All Changes:** Review all file modifications made so far
2. **Classify Each Change:** CORE_ISSUE_REQUIRED vs. VALID_IMPROVEMENT_DEFERRED
3. **Identify Scope Violations:** Which changes are outside scope boundary definition?
4. **Evaluate Reversion:** Can scope creep changes be reverted without losing core fix?

**Classification Template:**
```
# Scope Creep Remediation

**Changes Made:**
1. File A - Change X: [CORE_ISSUE_REQUIRED / DEFER]
2. File B - Change Y: [CORE_ISSUE_REQUIRED / DEFER]
3. File C - Change Z: [CORE_ISSUE_REQUIRED / DEFER]

**Core Issue Changes (KEEP):**
- File A - Change X: Directly resolves blocking problem

**Scope Creep Changes (REVERT OR DEFER):**
- File B - Change Y: Valid improvement but out-of-scope → REVERT and defer to Issue #ABC
- File C - Change Z: Enhancement not required for core fix → REVERT and defer to Epic #DEF
```

### Reversion: How to Undo Scope Creep

**Strategy 1: Complete Reversion (Recommended for Major Drift)**
1. Revert all scope creep changes using version control
2. Return to state where only core issue changes present
3. Document reverted improvements in deferred list comprehensively
4. Complete core issue validation with surgical scope only

**Strategy 2: Selective Reversion (For Minor Drift)**
1. Keep core issue changes as-is
2. Revert only scope creep changes (identify specific commits/hunks)
3. Validate core functionality works without reverted improvements
4. Document reverted work in deferred list

**Strategy 3: Defer in Place (When Reversion Difficult)**
1. Complete current implementation (core + creep)
2. Document all scope creep as "technical debt incurred during issue #XYZ"
3. Create immediate follow-up issue to extract scope creep to separate work
4. Escalate to Claude for coordination (scope drift correction needed)

**Reversion Checklist:**
- [ ] Identified all scope creep changes requiring reversion
- [ ] Reverted scope creep while preserving core issue fix
- [ ] Validated core functionality works without reverted changes
- [ ] Documented all reverted improvements in deferred list
- [ ] Created GitHub issues for deferred work (captured, not lost)

### Escalation: When to Consult Claude

**Escalate Immediately When:**
1. **Uncertainty About Core Issue:** Unclear whether change is essential for core issue resolution
2. **Conflicting Priorities:** Core issue conflicts with architectural standards or quality requirements
3. **Scope Expansion Requested:** User feedback during implementation suggests expanding scope beyond original mission
4. **Multiple Drift Instances:** Recurring scope creep indicates mission unclear or scope unrealistic
5. **Reversion Complexity:** Cannot separate core changes from scope creep changes (tightly coupled)

**Escalation Format:**
```
🚨 MISSION CLARITY ESCALATION

**Core Issue:** [Original blocking problem from context package]

**Uncertainty:** [Specific question about what's in-scope vs. out-of-scope]

**Conflicting Signals:**
- Context Package Says: [Scope definition from CORE_ISSUE/SCOPE_BOUNDARY]
- Implementation Reality: [Practical need identified during work]

**Request:** [Specific guidance needed to maintain mission discipline]

**Options:**
1. [Option A]: Defer improvement, stay focused on core issue
2. [Option B]: Expand scope to include improvement (requires context package update)

**Recommendation:** [Agent's suggested approach with rationale]
```

---

## PREVENTION BEST PRACTICES

### Pre-Implementation Prevention

**1. OUT OF SCOPE List Creation (Step 2)**
- Define OUT OF SCOPE improvements proactively before implementation
- Identify tempting improvements during planning, commit to deferring
- Review OUT OF SCOPE list when tempted during implementation

**2. Success Criteria as Guard Rails**
- Use success criteria as checklist during implementation
- When considering change, ask: "Is this in success criteria?" If no, defer
- Review criteria frequently to maintain focus

**3. Context Package Alignment**
- Validate CORE_ISSUE, SCOPE_BOUNDARY, SUCCESS_CRITERIA before starting
- Refer to context package when tempted to expand scope
- Escalate to Claude if context package seems incomplete (rather than expanding scope)

**4. Deferred List Preparation**
- Create deferred improvements list at start (empty initially)
- Commit to adding improvements to list rather than implementing
- Celebrate deferred list growth (shows discipline, not laziness)

### During Implementation Prevention

**1. Continuous Drift Monitoring (Step 3)**
- Check for mission drift indicators after each file modification
- Classify improvement opportunities as they arise (CORE_REQUIRED vs. DEFER)
- Document deferred improvements immediately (capture while fresh)

**2. Regular Checkpoint Validation**
- After each significant change, ask: "Does this directly resolve core blocking problem?"
- Periodic review: "Have I stayed within scope boundary definition?"
- Frequent validation: "Am I still working toward success criteria?"

**3. Minimal Change Discipline**
- Default to existing patterns rather than creating new ones
- Use existing infrastructure rather than improving it
- Extract as-is before enhancing (separate refactoring from improvement)

**4. Time-Boxing Implementation**
- Set implementation time budget based on surgical scope estimate
- If approaching time budget without completion, STOP and review for drift
- Time over-run is drift indicator (re-evaluate scope compliance)

### Post-Implementation Prevention

**1. Comprehensive Deferred List Documentation (Step 4)**
- Review all identified improvements during validation
- Create specific GitHub issues for high-priority deferred work
- Add medium/low-priority improvements to appropriate backlogs (tech debt, epics)
- Ensure no valid improvement is lost (just properly prioritized)

**2. Scope Compliance Review**
- Compare modified files vs. scope boundary definition
- Validate all changes directly resolve core issue
- Confirm no forbidden scope expansions occurred
- Document any scope violations and remediation

**3. Lessons Learned Capture**
- What temptations were strongest during this implementation?
- Which prevention strategies were most effective?
- What could improve future mission discipline?
- Share patterns with team for collective learning

### Team Coordination Prevention

**1. Claude's Enforcement Role**
- Claude monitors agent scope compliance during orchestration
- Intervenes when agent reports show scope expansion indicators
- Requests scope compliance validation before marking mission complete
- Escalates recurring drift patterns to user for process improvement

**2. Context Package Quality**
- Clear CORE_ISSUE definition prevents ambiguity
- Specific SCOPE_BOUNDARY reduces temptation to expand
- Concrete SUCCESS_CRITERIA provides objective completion measure
- FORBIDDEN SCOPE EXPANSIONS explicitly blocks common drift patterns

**3. Working Directory Communication**
- Agents report deferred improvements through working directory artifacts
- Deferred lists visible to Claude for coordination planning
- Future agent engagements can address documented improvements
- Comprehensive team awareness of improvement backlog

---

## SUMMARY: DRIFT PREVENTION FRAMEWORK

### 6 Common Triggers (Memorize These)
1. **"While I'm Here" Syndrome:** Fixing related issues noticed during core work
2. **"Perfect is the Enemy of Done":** Continuing improvements after success criteria met
3. **"Consistency Refactoring":** Applying fix to all similar instances across codebase
4. **"Opportunistic Testing":** Comprehensive test coverage beyond core validation
5. **"Infrastructure Enhancement":** Improving tooling/infrastructure during core fix
6. **"Architectural Enlightenment":** Refactoring to better architecture during implementation

### 3 Warning Sign Categories (Monitor Continuously)
1. **Quantitative:** File count, time, line count, dependencies, abstractions
2. **Qualitative:** Improvements not in criteria, unrelated changes, migrations, "while I'm here"
3. **Healthy Focus:** Success criteria guidance, deferred list growth, scope adherence

### 3 Recovery Steps (Execute When Drift Detected)
1. **STOP:** Cease implementation immediately when drift detected
2. **CLASSIFY:** Separate core changes from scope creep changes
3. **REMEDIATE:** Revert creep, document deferred, complete core validation

### 3 Prevention Layers (Apply Throughout)
1. **Pre-Implementation:** OUT OF SCOPE list, success criteria guard rails, context package alignment
2. **During Implementation:** Continuous monitoring, regular checkpoints, minimal change discipline
3. **Post-Implementation:** Comprehensive deferred list, scope compliance review, lessons learned

---

**Document Status:** ✅ Comprehensive drift pattern catalog
**Usage:** Reference during Step 3 (Mission Drift Detection) for all agents
**Integration:** Complements core-issue-focus skill with detailed pattern recognition and prevention guidance
