# Evaluation-First Skill Development Workflow

**Purpose:** Testing methodology ensuring skills improve Claude's actual performance
**Source:** Official Anthropic best practices
**Target Audience:** PromptEngineer creating new skills

---

## CORE METHODOLOGY

**Best Practice (Official Anthropic):**
> "Build evaluations first before extensive documentation."

### Why Evaluation-First Matters

**Anti-Pattern:** Write 1,000+ lines of skill content → Deploy → Discover Claude doesn't use it effectively

**Correct Pattern:** Create test scenarios → Validate improvement → Then expand documentation

**Benefits:**
- Validates skill actually improves Claude's performance (not assumed)
- Identifies specific gaps in Claude's baseline behavior
- Prevents over-documentation of unneeded context
- Enables iterative refinement based on real behavior
- Saves token budget by focusing on what actually helps

---

## 6-STEP EVALUATION-FIRST PROCESS

### Step 1: Identify Performance Gaps

**Question:** Where does Claude struggle without this skill?

**Process:**
1. Observe Claude executing tasks in target domain
2. Note specific failures, confusion, or suboptimal approaches
3. Document exact scenarios where guidance would help
4. Distinguish between "Claude doesn't know" vs. "Claude knows but I want different approach"

**Example (working-directory-coordination):**
- **Gap Observed:** Agents create working directory artifacts but don't report to orchestrator
- **Specific Failure:** Claude unaware of agent discoveries, can't coordinate handoffs
- **Skill Need:** Communication protocol ensuring artifact reporting

**Example (api-design-patterns):**
- **Gap Observed:** Backend generates inconsistent error responses across endpoints
- **Specific Failure:** No standardized format for validation errors vs. server errors
- **Skill Need:** Consistent error handling patterns

**Output:** 3-5 specific scenarios where Claude's baseline behavior needs improvement

---

### Step 2: Create Test Scenarios

**Requirement (Best Practices):**
> "Minimum three evaluations created"

**Process:**
1. Write 3+ realistic test scenarios representing real zarichney-api tasks
2. Each scenario should trigger the performance gap identified
3. Include input context, expected output, and success criteria
4. Cover typical case, edge case, and complex case

**Test Scenario Template:**
```markdown
## Test Scenario [N]: [Scenario Name]

**Input Context:**
- Task description
- Files available
- Expected workflow

**Current Behavior (Baseline without skill):**
- What Claude does
- Where Claude struggles
- What output produced

**Desired Behavior (With skill):**
- What Claude should do
- How skill guides Claude
- What improved output expected

**Success Criteria:**
- [ ] [Measurable criterion 1]
- [ ] [Measurable criterion 2]
- [ ] [Measurable criterion 3]
```

**Example Test Scenario (working-directory-coordination):**
```markdown
## Test Scenario 1: Backend Specialist Creates Artifact

**Input Context:**
- BackendSpecialist completes API endpoint design
- Creates `/working-dir/api-contracts.md` with endpoint specifications
- Orchestrator needs to know about this for Frontend coordination

**Current Behavior (Baseline):**
- BackendSpecialist creates file silently
- Orchestrator unaware of artifact
- Frontend specialist misses critical context
- Coordination gap results

**Desired Behavior (With skill):**
- BackendSpecialist creates file AND reports immediately
- Report includes: file path, purpose, intended consumers
- Orchestrator sees report, coordinates Frontend handoff
- Seamless context flow

**Success Criteria:**
- [ ] Artifact reported immediately after creation
- [ ] Report format includes path, purpose, consumers
- [ ] Orchestrator acknowledges artifact in next coordination
```

**Output:** 3+ documented test scenarios with clear success criteria

---

### Step 3: Establish Baseline Behavior

**Purpose:** Measure Claude's performance WITHOUT skill to validate improvement

**Process:**
1. Execute each test scenario with fresh Claude session (no skill available)
2. Document exact behavior, outputs, and failures
3. Note token usage for baseline comparison
4. Identify patterns in baseline struggles

**Baseline Documentation Template:**
```markdown
## Baseline Test: [Scenario Name]

**Model:** Claude Sonnet 4.5
**Session:** Fresh (no skill context)
**Date:** YYYY-MM-DD

**Execution:**
[Exact prompts given to Claude]

**Claude's Response:**
[Full output from Claude]

**Observed Issues:**
- [Issue 1: Specific failure or suboptimal behavior]
- [Issue 2: Another gap]
- [Issue 3: Edge case missed]

**Token Usage:** [Approximate tokens]

**Success Criteria Met:** X/Y criteria passed
```

**Example Baseline (working-directory-coordination):**
```markdown
## Baseline Test: Backend Specialist Creates Artifact

**Model:** Claude Sonnet 4.5
**Session:** Fresh (no skill)
**Date:** 2025-11-01

**Execution:**
"Complete API endpoint design for Recipe management. Create artifact documenting contracts."

**Claude's Response:**
[Creates `/working-dir/api-contracts.md` with excellent technical content]
[No mention of artifact in completion report]
[No communication protocol followed]

**Observed Issues:**
- No immediate reporting after artifact creation
- Completion report omits working directory activity
- Orchestrator would miss this critical context

**Token Usage:** ~2,500 tokens

**Success Criteria Met:** 0/3 criteria passed
```

**Output:** Baseline performance documentation for each test scenario

---

### Step 4: Write Minimal Skill Instructions

**Principle:** Start with smallest possible skill that might address gaps

**Process:**
1. Write bare minimum SKILL.md addressing identified gaps
2. Focus on WHAT to do, not lengthy WHY explanations
3. Use 200-500 tokens initially (expand only if tests show need)
4. Include only essential workflow steps

**Minimal Skill Template:**
```markdown
---
name: skill-name
description: [Concise what + when - 2 sentences max]
---

# Skill Name

[1 sentence purpose]

## Workflow

### Step 1: [Critical Action]
[2-3 line instruction]

### Step 2: [Critical Action]
[2-3 line instruction]

**Format:**
[Template if needed]
```

**Example Minimal Skill (working-directory-coordination v0.1):**
```markdown
---
name: working-directory-coordination
description: Report working directory artifacts immediately after creation. Use when creating files in /working-dir/.
---

# Working Directory Coordination

Report all working directory artifacts for team awareness.

## Workflow

### Step 1: Create Artifact
Create file in `/working-dir/` with clear naming.

### Step 2: Report Immediately
After creation, report:
- File path
- Purpose
- Intended consumers

**Format:**
Artifact Created: [path]
Purpose: [description]
Consumers: [agents]
```

**Output:** Minimal SKILL.md (~200-500 tokens) targeting identified gaps

---

### Step 5: Test Against Evaluations

**Purpose:** Validate skill actually improves Claude's performance

**Process:**
1. Execute each test scenario with fresh Claude session (skill available)
2. Compare behavior to baseline: improvements? regressions?
3. Document success criteria met
4. Note any unexpected behaviors or new issues
5. Measure token overhead of loading skill

**Skill Test Documentation Template:**
```markdown
## Skill Test: [Scenario Name]

**Model:** Claude Sonnet 4.5
**Skill Version:** v0.1 (minimal)
**Session:** Fresh (skill available)
**Date:** YYYY-MM-DD

**Execution:**
[Same prompts as baseline test]

**Claude's Response:**
[Full output from Claude]

**Improvements vs. Baseline:**
- ✅ [Improvement 1: Specific behavior now correct]
- ✅ [Improvement 2: Another gap closed]
- ⚠️ [Partial improvement or new issue]

**Token Usage:**
- Skill loading: ~[X] tokens
- Total session: ~[Y] tokens
- Overhead vs. baseline: +[Z] tokens

**Success Criteria Met:** X/Y criteria passed

**Iteration Needs:**
- [What to add/refine in next version]
```

**Example Skill Test (working-directory-coordination v0.1):**
```markdown
## Skill Test: Backend Specialist Creates Artifact

**Model:** Claude Sonnet 4.5
**Skill Version:** v0.1 (minimal)
**Session:** Fresh (skill available)
**Date:** 2025-11-01

**Execution:**
"Complete API endpoint design for Recipe management. Create artifact documenting contracts."

**Claude's Response:**
[Creates `/working-dir/api-contracts.md`]
[Immediately reports in completion report:]

"Artifact Created: /working-dir/api-contracts.md
Purpose: API contracts for Recipe management endpoints
Consumers: FrontendSpecialist for service integration"

**Improvements vs. Baseline:**
- ✅ Immediate reporting after creation (was missing)
- ✅ Format includes path, purpose, consumers (was missing)
- ✅ Orchestrator can now coordinate handoff (was communication gap)

**Token Usage:**
- Skill loading: ~500 tokens
- Total session: ~3,000 tokens
- Overhead vs. baseline: +500 tokens (20% increase for 100% communication improvement)

**Success Criteria Met:** 3/3 criteria passed

**Iteration Needs:**
- Add pre-work artifact discovery protocol (not yet tested)
- Expand format template for complex artifacts
```

**Output:** Performance comparison showing improvement (or lack thereof)

---

### Step 6: Iterate Based on Results

**Decision Tree:**

**If success criteria fully met (all tests pass):**
→ Expand documentation incrementally
→ Add resources (templates/examples) as needed
→ Test with additional edge cases
→ Consider deployment

**If success criteria partially met (some tests pass):**
→ Refine specific workflow steps that failed
→ Add clarifying examples for confused areas
→ Re-test failed scenarios
→ Do NOT expand successful parts yet

**If success criteria not met (tests fail):**
→ Re-evaluate skill need: Is this actually helpful?
→ Revise approach based on Claude's actual struggles
→ Consider different framing or structure
→ Re-test with simplified version

**If skill causes regressions (new issues appear):**
→ Identify conflicting instructions
→ Simplify skill to remove confusion
→ Test isolation: Does skill interfere with other capabilities?

**Iteration Pattern:**
1. Analyze test results against success criteria
2. Identify 1-2 specific improvements needed
3. Make targeted changes to SKILL.md (keep minimal)
4. Re-run failing test scenarios
5. Repeat until all success criteria met
6. THEN expand documentation, resources

**Example Iteration (working-directory-coordination):**
```markdown
## Iteration 1 → 2

**Test Results:**
- ✅ Scenario 1 (immediate reporting): PASS
- ✅ Scenario 2 (format compliance): PASS
- ❌ Scenario 3 (pre-work discovery): FAIL - Claude doesn't check for existing artifacts

**Analysis:**
Skill addresses reporting but not discovery protocol.

**Changes for v0.2:**
Add Step 0: Pre-Work Artifact Discovery
- Check `/working-dir/` before starting task
- Review existing artifacts for context

**Re-test:**
Scenario 3 only (others already passing)

**Result:**
- ✅ Scenario 3 now passes
- All 3/3 success criteria met across scenarios

**Next Steps:**
Expand documentation with examples
Create artifact-reporting-template.md resource
Test with 2+ additional agents
```

---

## CROSS-MODEL TESTING

**Requirement (Best Practices):**
> "Test Skills across Claude Haiku, Sonnet, and Opus since effectiveness depends on the underlying model."

### Why Cross-Model Testing Matters

**Model Differences:**
- **Haiku:** Speed-optimized, may miss nuanced instructions, needs explicit guidance
- **Sonnet:** Balanced, production baseline, handles moderate complexity
- **Opus:** Complex reasoning, catches edge cases, may over-analyze

**Skill Behavior Variations:**
- Haiku might need simpler language, more explicit steps
- Sonnet handles moderate abstraction well
- Opus can work with conceptual guidance, fewer examples

### Cross-Model Test Matrix

```markdown
## Test Scenario [N]: [Scenario Name]

### Haiku Test
**Version:** Claude Haiku 3.5
**Execution:** [Same prompt as baseline]
**Result:** [Pass/Fail with specifics]
**Observations:** [Haiku-specific behaviors]
**Adjustments Needed:** [If Haiku struggled, what to add]

### Sonnet Test
**Version:** Claude Sonnet 4.5
**Execution:** [Same prompt as baseline]
**Result:** [Pass/Fail with specifics]
**Observations:** [Sonnet-specific behaviors]
**Baseline:** [This is production model]

### Opus Test
**Version:** Claude Opus 3
**Execution:** [Same prompt as baseline]
**Result:** [Pass/Fail with specifics]
**Observations:** [Opus-specific behaviors]
**Adjustments Needed:** [If Opus over-analyzed, what to simplify]
```

**Adjustment Strategy:**
- If Haiku fails but Sonnet/Opus pass → Add more explicit steps or examples
- If all three fail → Skill design issue, revise fundamentally
- If Opus passes but Haiku/Sonnet fail → Skill too conceptual, add concrete guidance
- If all three pass but Opus over-analyzes → Add scope boundaries

---

## TWO-CLAUDE TESTING APPROACH

**Best Practice (Anthropic):**
> "Use one Claude instance to create/refine skill, another to test it (fresh session)"

### Why Separate Sessions Matter

**Problem:** Same Claude instance that created skill has contaminated context
**Solution:** Test skill with fresh Claude session seeing skill exactly as users will

**Process:**
1. **Claude A (Creator):** Write/refine skill based on evaluation results
2. **Claude B (Tester):** Test skill on scenarios (fresh session, no creation context)
3. **Observe:** Note failures, confusion, misactivation in Claude B
4. **Return to Claude A:** Provide specific observations for refinement
5. **Refine:** Update skill based on actual behavior
6. **Re-test with Claude B:** Validate improvements

**Why This Works:**
- Claude B sees skill exactly as agents will (no contamination)
- Reveals real-world activation and usage patterns
- Identifies confusing wording or unclear instructions
- Validates skill discovery mechanism (name/description)

---

## EVALUATION DOCUMENTATION TEMPLATE

Use this template to document complete evaluation-first process:

```markdown
# [Skill Name] Evaluation Report

**Date:** YYYY-MM-DD
**PromptEngineer:** [Name]
**Skill Version:** v[X.Y]

---

## Step 1: Performance Gaps Identified

### Gap 1: [Description]
**Observation:** [Where Claude struggled]
**Impact:** [Consequence of gap]

### Gap 2: [Description]
**Observation:** [Where Claude struggled]
**Impact:** [Consequence of gap]

### Gap 3: [Description]
**Observation:** [Where Claude struggled]
**Impact:** [Consequence of gap]

---

## Step 2: Test Scenarios Created

### Scenario 1: [Name]
[Full scenario with input, baseline expected, desired behavior, success criteria]

### Scenario 2: [Name]
[Full scenario details]

### Scenario 3: [Name]
[Full scenario details]

---

## Step 3: Baseline Performance

### Scenario 1 Baseline
**Success Criteria Met:** X/Y
**Key Issues:** [Failures observed]
**Token Usage:** ~[X] tokens

### Scenario 2 Baseline
**Success Criteria Met:** X/Y
**Key Issues:** [Failures observed]
**Token Usage:** ~[X] tokens

### Scenario 3 Baseline
**Success Criteria Met:** X/Y
**Key Issues:** [Failures observed]
**Token Usage:** ~[X] tokens

**Baseline Summary:** X/Y total criteria met across scenarios

---

## Step 4: Minimal Skill Written

**Initial Version:** v0.1
**Token Count:** ~[X] tokens
**Structure:** [Brief description of minimal approach]
**Targets:** [Which gaps addressed]

---

## Step 5: Skill Test Results

### Scenario 1 with Skill
**Success Criteria Met:** X/Y
**Improvements:** [What improved vs. baseline]
**Token Overhead:** +[X] tokens

### Scenario 2 with Skill
**Success Criteria Met:** X/Y
**Improvements:** [What improved vs. baseline]
**Token Overhead:** +[X] tokens

### Scenario 3 with Skill
**Success Criteria Met:** X/Y
**Improvements:** [What improved vs. baseline]
**Token Overhead:** +[X] tokens

**Skill Test Summary:** X/Y total criteria met across scenarios

---

## Step 6: Iterations

### Iteration 1 → 2
**Issues Found:** [What failed or needs improvement]
**Changes Made:** [Specific refinements]
**Re-test Results:** [New success criteria met]

### Iteration 2 → 3
[Continue documenting iterations]

---

## Cross-Model Validation

### Haiku Results
**Scenarios Passed:** X/Y
**Adjustments Needed:** [Haiku-specific needs]

### Sonnet Results
**Scenarios Passed:** X/Y (production baseline)

### Opus Results
**Scenarios Passed:** X/Y
**Adjustments Needed:** [Opus-specific needs]

---

## Final Validation

**All Scenarios Pass:** ✅/❌
**Cross-Model Compatibility:** ✅/❌
**Token Efficiency Acceptable:** ✅/❌
**Ready for Deployment:** ✅/❌

**Deployment Recommendation:** [Deploy/Iterate further/Reconsider need]
```

---

## SUCCESS CRITERIA

**Skill Ready for Deployment When:**
- [ ] 3+ test scenarios created and documented
- [ ] Baseline performance measured for all scenarios
- [ ] Minimal skill version tested against scenarios
- [ ] All success criteria met across test scenarios
- [ ] Cross-model testing completed (Haiku/Sonnet/Opus)
- [ ] Two-Claude testing validated (creator vs. tester sessions)
- [ ] Token overhead justified by performance improvement
- [ ] No regressions or new issues introduced
- [ ] Iteration process documented with clear improvements

**Only then:** Expand documentation, create resources, integrate with agents

---

## REFERENCES

**Official Sources:**
- https://docs.claude.com/en/docs/agents-and-tools/agent-skills/best-practices

**Related Resources:**
- `resources/documentation/anti-patterns.md` - Common evaluation mistakes
- `resources/documentation/token-optimization-techniques.md` - Budget tracking
- `resources/examples/coordination-skill-example.md` - Real evaluation process example
