# Working Directory Coordination Skill - Integration Testing Plan

**Issue:** #311 - Iteration 1.1: Core Skills - Working Directory Coordination
**Subtask:** 6 - Integration Testing with 2 Agents
**Created:** 2025-10-25
**Status:** Ready for Execution

---

## Testing Objectives

### Primary Goals
1. **Progressive Loading Validation:** Verify metadata → instructions → resources loading pattern works
2. **Agent Adoption Verification:** Confirm at least 2 agents can successfully load and use the skill
3. **Protocol Compliance Testing:** Validate communication protocols are enforceable and effective
4. **Resource Accessibility:** Ensure templates, examples, and documentation are accessible on-demand

### Success Criteria (from Issue #311)
- ✅ At least 2 agents successfully load and use working-directory-coordination skill
- ✅ Progressive loading validated: metadata → instructions → resources pattern works
- ✅ Communication protocols enforceable
- ✅ No integration issues discovered

---

## Test Agent Selection

### Agent 1: TestEngineer
**Rationale:**
- Primary file-editing agent with direct authority over test files
- Complex workflows requiring artifact coordination
- Natural fit for coverage analysis → test implementation patterns

**Test Scenario:**
- Engage TestEngineer to create a test coverage analysis
- TestEngineer loads working-directory-coordination skill
- Validates metadata discovery (loads ~80 tokens)
- Loads SKILL.md instructions when needed (~2,500 tokens)
- Uses artifact-discovery-template.md from resources
- Executes all 4 workflow steps (discovery, reporting, integration, compliance)

---

### Agent 2: DocumentationMaintainer
**Rationale:**
- Primary file-editing agent with documentation expertise
- Multi-agent coordination experience (integrates with all agents' work)
- Natural fit for README updates following implementation

**Test Scenario:**
- Engage DocumentationMaintainer to update documentation
- DocumentationMaintainer loads working-directory-coordination skill
- Validates progressive loading (metadata → instructions)
- Accesses templates from resources on-demand
- Executes artifact discovery and reporting workflows
- Validates skill integration with existing documentation standards

---

## Progressive Loading Validation

### Phase 1: Metadata Discovery (~80 tokens)
**Test:**
- Agent receives context package mentioning working-directory-coordination skill
- Agent loads metadata.json to understand skill purpose and applicability
- Agent determines skill relevance based on category, agents, tags

**Validation Criteria:**
- ✅ Metadata loads successfully
- ✅ Token count ~80 tokens (within target)
- ✅ Agent understands skill purpose from metadata description
- ✅ Agent determines applicability (agents: ["ALL"] means mandatory)

---

### Phase 2: Instructions Loading (~2,500 tokens)
**Test:**
- Agent invokes skill to access complete workflow instructions
- Agent loads SKILL.md with 4-step workflow, when to use, integration guidance
- Agent applies instructions to current task

**Validation Criteria:**
- ✅ SKILL.md loads successfully
- ✅ Token count ~2,500 tokens (within 2,000-5,000 target)
- ✅ Agent understands mandatory workflow steps
- ✅ Agent can execute all 4 steps based on instructions

---

### Phase 3: Resource Access (On-Demand)
**Test:**
- Agent needs template for artifact discovery
- Agent accesses resources/templates/artifact-discovery-template.md
- Agent uses template to format communication
- Agent references examples for quality standards

**Validation Criteria:**
- ✅ Templates accessible on-demand
- ✅ Examples provide clear guidance
- ✅ Documentation available for deep dives
- ✅ Resources loadable separately (not all at once)

---

## Communication Protocol Testing

### Test 1: Pre-Work Artifact Discovery
**Scenario:** Agent starts task, working directory has existing artifacts

**Expected Agent Behavior:**
1. Check /working-dir/ for existing files before starting work
2. Report discoveries using standard 🔍 format from template
3. Identify integration opportunities with discovered artifacts
4. Adjust approach based on discovered context

**Validation:**
- ✅ Agent reports artifact discovery before starting work
- ✅ Agent uses standardized format (matches template)
- ✅ Agent provides specific details (not generic)
- ✅ Agent demonstrates context awareness from discoveries

---

### Test 2: Immediate Artifact Reporting
**Scenario:** Agent creates working directory artifact during task

**Expected Agent Behavior:**
1. Create working directory file
2. Immediately report using standard 🗂️ format from template
3. Include all required fields (filename, purpose, context, dependencies, next actions)
4. Provide specific, actionable details

**Validation:**
- ✅ Agent reports immediately upon creation (not batched)
- ✅ Agent uses standardized format (matches template)
- ✅ All required fields filled with specific details
- ✅ Intended consumers and next actions clear

---

### Test 3: Context Integration Reporting
**Scenario:** Agent builds upon another agent's artifact

**Expected Agent Behavior:**
1. Use prior artifact as input for current work
2. Report integration using standard 🔗 format from template
3. Acknowledge source artifacts and value addition
4. Prepare handoff context for future agents

**Validation:**
- ✅ Agent reports integration when building upon prior work
- ✅ Agent uses standardized format (matches template)
- ✅ Source artifacts acknowledged clearly
- ✅ Value addition and handoff preparation documented

---

### Test 4: Communication Compliance
**Scenario:** Claude monitors agent communication throughout engagement

**Expected Claude Behavior:**
1. Verify agent executes artifact discovery before starting
2. Monitor for immediate artifact reporting
3. Confirm integration reporting when applicable
4. Intervene if protocols violated

**Validation:**
- ✅ Claude can verify protocol compliance
- ✅ Claude can detect missing communication
- ✅ Protocols enforceable through intervention
- ✅ Recovery strategies effective when needed

---

## Testing Execution Plan

### Step 1: TestEngineer Validation
**Task:** Create test coverage analysis for a sample service
**Skill Integration:**
- Load working-directory-coordination skill
- Execute Pre-Work Artifact Discovery (Step 1)
- Create coverage-analysis.md artifact
- Execute Immediate Artifact Reporting (Step 2)
- Verify progressive loading worked correctly
- Document integration experience

**Expected Deliverable:**
- TestEngineer completion report
- Artifact created with proper reporting
- Progressive loading validation confirmed
- Integration experience documented

---

### Step 2: DocumentationMaintainer Validation
**Task:** Update README documentation based on TestEngineer's analysis
**Skill Integration:**
- Load working-directory-coordination skill
- Execute Pre-Work Artifact Discovery (discover TestEngineer's artifact)
- Update documentation based on analysis
- Execute Context Integration Reporting (Step 3)
- Access templates from resources as needed
- Document integration experience

**Expected Deliverable:**
- DocumentationMaintainer completion report
- Integration with TestEngineer's artifact demonstrated
- Resource access validated (templates used)
- Progressive loading validation confirmed

---

### Step 3: Results Documentation
**Deliverable:** Integration testing results report
**Contents:**
- Progressive loading validation results (all 3 phases)
- Communication protocol testing results (all 4 tests)
- Agent adoption verification (2 agents confirmed)
- Integration issues discovered (if any)
- Recommendations for refinement

---

## Integration Issues Tracking

### Potential Issues to Monitor

**Progressive Loading:**
- [ ] Metadata token count exceeds 150 tokens
- [ ] SKILL.md token count outside 2,000-5,000 range
- [ ] Resource files not accessible on-demand
- [ ] Loading latency unacceptable (>1 second)

**Communication Protocols:**
- [ ] Templates difficult to find or use
- [ ] Standardized formats unclear or ambiguous
- [ ] Required fields confusing or redundant
- [ ] Examples not representative of real scenarios

**Agent Adoption:**
- [ ] Skill instructions unclear for agents
- [ ] Workflow steps too complex or numerous
- [ ] Integration with existing workflows problematic
- [ ] Resource organization confusing

**Resolution Approach:**
- Document all discovered issues
- Determine severity (blocking vs. enhancement)
- Implement fixes before Subtask 7 validation
- Re-test if critical issues found

---

## Success Metrics

### Quantitative Metrics
- ✅ 2 agents successfully load skill
- ✅ Metadata <150 tokens
- ✅ SKILL.md ~2,500 tokens (2,000-5,000 range)
- ✅ Progressive loading <1 second latency
- ✅ 100% protocol compliance during testing

### Qualitative Metrics
- ✅ Agents find skill instructions clear and actionable
- ✅ Templates provide effective communication formats
- ✅ Examples demonstrate realistic scenarios
- ✅ Documentation accessible for troubleshooting
- ✅ Integration with existing workflows seamless

---

## Testing Timeline

**Step 1:** TestEngineer validation (~1 hour)
**Step 2:** DocumentationMaintainer validation (~1 hour)
**Step 3:** Results documentation (~30 minutes)
**Total:** ~2.5 hours

---

## Next Actions After Testing

### If All Tests Pass:
- Document validation success in completion report
- Proceed to Subtask 7 (Validation & Documentation)
- Measure context savings vs. baseline
- Create integration snippets for agent refactoring

### If Issues Discovered:
- Document all issues with severity assessment
- Implement fixes for critical issues
- Re-test affected validation scenarios
- Update skill files as needed before proceeding

---

**Testing Plan Status:** ✅ Ready for Execution
**Dependencies:** All 9 skill files complete (metadata, SKILL.md, 3 templates, 2 examples, 2 documentation)
**Blocking:** Subtask 7 (Validation & Documentation) awaits testing results
