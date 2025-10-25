# Troubleshooting Communication Gaps

**Purpose:** Common communication failures, symptoms, root causes, and recovery strategies
**Audience:** All agents, Claude (Codebase Manager)
**Status:** Practical troubleshooting guide for protocol compliance

---

## Table of Contents

1. [Common Communication Failures](#common-communication-failures)
2. [Symptoms of Missing Artifact Discovery](#symptoms-of-missing-artifact-discovery)
3. [Recovery from Communication Gaps](#recovery-from-communication-gaps)
4. [Best Practices for Prevention](#best-practices-for-prevention)

---

## Common Communication Failures

### Failure 1: Agent Skips Artifact Discovery

**Scenario:**
```
Claude: "Engage TestEngineer to create coverage tests for Recipe service"
TestEngineer: [Starts creating tests immediately without checking /working-dir/]
TestEngineer: [Creates tests duplicating analysis already in recipe-service-coverage-analysis.md]
```

**Root Cause:**
- Agent didn't check /working-dir/ for existing artifacts before starting work
- Context package from Claude didn't explicitly mandate artifact discovery
- Agent assumed empty working directory without verification

**Symptoms:**
- Duplicated analysis or implementation work
- Agent unaware of relevant context from prior engagements
- Deliverables miss integration opportunities with existing artifacts

**Prevention:**
- **Claude:** Include explicit artifact discovery mandate in every context package
- **Agent:** Always execute Step 1 (Pre-Work Artifact Discovery) before starting task
- **Standard:** Make discovery reporting mandatory even when /working-dir/ is empty

**Recovery:**
```
1. Claude detects missing discovery (agent didn't report discoveries)
2. Claude stops agent engagement: "Please execute artifact discovery before continuing"
3. Agent checks /working-dir/, reports discoveries using standard format
4. Agent adjusts approach based on discovered context
5. Agent continues work building upon rather than duplicating prior artifacts
```

---

### Failure 2: Agent Creates Artifact Without Reporting

**Scenario:**
```
BackendSpecialist: [Creates /working-dir/api-design-decisions.md]
BackendSpecialist: "API implementation complete. Endpoints ready for frontend integration."
BackendSpecialist: [Terminates without reporting artifact]
Claude: [Unaware api-design-decisions.md exists]
FrontendSpecialist: [Engaged without context about design decisions artifact]
```

**Root Cause:**
- Agent created working directory artifact but didn't use Immediate Artifact Reporting protocol
- Context package didn't explicitly require artifact reporting
- Agent provided summary in natural language instead of standardized format

**Symptoms:**
- Claude unaware of artifact existence
- Subsequent agents don't discover artifact (not included in context packages)
- Coordination failures due to invisible context

**Prevention:**
- **Claude:** Include explicit artifact reporting requirement in every context package
- **Agent:** Report immediately upon file creation using standard 🗂️ format
- **Standard:** No batching - report each artifact when created, not at task end

**Recovery:**
```
1. Claude detects artifact creation without reporting (agent mentions file in summary)
2. Claude intervenes: "Please report api-design-decisions.md using standard artifact reporting format"
3. Agent provides standardized report with all required fields
4. Claude updates artifact registry and includes in next agent's context package
5. Coordination continuity restored
```

---

### Failure 3: Agent Misses Integration Reporting

**Scenario:**
```
FrontendSpecialist: [Reviews backend-api-design.md and test-coverage-gaps.md]
FrontendSpecialist: [Implements UI consuming API and addressing test gaps]
FrontendSpecialist: "UI implementation complete"
FrontendSpecialist: [Terminates without documenting which artifacts informed work]
```

**Root Cause:**
- Agent built upon other agents' artifacts but didn't execute Context Integration Reporting
- Integration was implicit in deliverable but not explicitly documented
- Agent provided work summary without acknowledging source artifacts

**Symptoms:**
- Unclear which prior artifacts informed current work
- Original agent contributors not acknowledged
- Value addition not documented (hard to distinguish synthesis from duplication)
- Handoff preparation missing (unclear what's ready for next agents)

**Prevention:**
- **Claude:** Include integration reporting requirement when prior artifacts exist
- **Agent:** Execute Step 3 (Context Integration Reporting) when building upon other agents' work
- **Standard:** Always acknowledge source artifacts and document value addition

**Recovery:**
```
1. Claude detects integration without reporting (agent clearly used prior artifacts)
2. Claude intervenes: "Please document integration with backend-api-design.md and test-coverage-gaps.md"
3. Agent provides standardized integration report with all required fields
4. Audit trail of context evolution established
5. Team contributions acknowledged and value addition documented
```

---

### Failure 4: Generic or Incomplete Reporting

**Scenario:**
```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: analysis.md
- Purpose: Analysis
- Context for Team: Some analysis done
- Dependencies: Prior work
- Next Actions: Continue
```

**Root Cause:**
- Agent used standardized format but filled fields with vague, non-actionable content
- Insufficient detail for other agents or Claude to understand artifact value
- Missing specific information about consumers, decisions, or integration points

**Symptoms:**
- Claude can't determine when to include artifact in context packages (unclear relevance)
- Subsequent agents can't assess artifact utility without opening and reading fully
- Team awareness exists but context quality too low for effective coordination

**Prevention:**
- **Agent:** Provide specific, actionable details in all required fields
- **Standard:** Include intended consumers, key decisions/insights, concrete dependencies
- **Examples:** Reference template examples for quality standards

**Recovery:**
```
1. Claude detects low-quality reporting (generic/vague content)
2. Claude requests clarification: "Please provide specific details for artifact reporting"
3. Claude provides example of quality reporting for comparison
4. Agent re-submits with specific content:
   - Filename: recipe-service-coverage-analysis.md
   - Purpose: Test coverage gap analysis for Recipe service identifying 15 missing test cases; intended for TestEngineer
   - Context for Team: Coverage gaps block feature completion - priority coordination needed
   - Dependencies: Builds upon backend-implementation.md service analysis
   - Next Actions: TestEngineer to create missing tests; BackendSpecialist to review testability
5. Quality communication restored
```

---

### Failure 5: Batched Reporting (Not Immediate)

**Scenario:**
```
PromptEngineer: [Creates metadata.json]
PromptEngineer: [Creates SKILL.md]
PromptEngineer: [Creates 3 template files]
PromptEngineer: [Creates 2 example files]
PromptEngineer: "Created 7 skill files: metadata.json, SKILL.md, templates/*, examples/*"
```

**Root Cause:**
- Agent created multiple artifacts but reported them in batch at task end
- Immediate reporting protocol violated (should report each file when created)
- Claude couldn't track progress in real-time during agent engagement

**Symptoms:**
- Claude unaware of progress until task completion
- Can't intervene early if agent goes off-track
- Subsequent agents engaged too early (before artifacts ready) or too late (blocking delays)

**Prevention:**
- **Agent:** Report each artifact immediately upon creation, not in batches
- **Standard:** "Immediate" means when file is created, not when task completes
- **Claude:** Monitor for batching pattern and intervene

**Recovery:**
```
1. Claude detects batched reporting (multiple files mentioned in single report)
2. Claude provides guidance: "Please report artifacts immediately upon creation, not in batches"
3. Agent acknowledges and adjusts behavior for future engagements
4. For current engagement: Agent provides individual reports for each artifact retroactively if critical
5. Pattern monitoring continues to ensure compliance in next engagement
```

---

## Symptoms of Missing Artifact Discovery

### Symptom 1: Duplicated Analysis

**Observable Behavior:**
```
Agent A creates analysis-report.md identifying 10 issues
Agent B (later) creates issue-analysis.md identifying same 10 issues
```

**Diagnostic Questions:**
- Did Agent B execute artifact discovery?
- Did Agent B report discoveries before starting work?
- Did Claude include analysis-report.md in Agent B's context package?

**Root Cause Likely:**
- Agent B skipped artifact discovery OR
- Claude didn't include prior artifact in context package OR
- Agent B discovered artifact but ignored it

**Fix:**
- Enforce mandatory artifact discovery before all work
- Claude must include relevant artifacts in context packages
- Agents must adjust approach based on discoveries

---

### Symptom 2: Integration Conflicts

**Observable Behavior:**
```
BackendSpecialist implements API with endpoint /api/recipes/create
FrontendSpecialist (parallel) implements UI calling /api/recipes/new
Integration fails: endpoint mismatch
```

**Diagnostic Questions:**
- Did both agents check /working-dir/ for coordination artifacts?
- Did BackendSpecialist report API design decisions?
- Did Claude create session-state.md documenting parallel work?

**Root Cause Likely:**
- Parallel agents unaware of each other's work OR
- Missing coordination artifacts for parallel workflows OR
- Artifact reporting insufficient for coordination awareness

**Fix:**
- Create session-state.md for complex multi-agent workflows
- Report all design decisions affecting other agents
- Claude monitors parallel work and ensures awareness

---

### Symptom 3: Missing Context in Handoffs

**Observable Behavior:**
```
Agent A completes work with critical decision documented in working directory
Agent B engaged for next phase without knowledge of decision
Agent B makes incompatible choice, requiring rework
```

**Diagnostic Questions:**
- Did Agent A report handoff preparation in artifact?
- Did Claude include Agent A's artifact in Agent B's context package?
- Did Agent B discover and review Agent A's artifact?

**Root Cause Likely:**
- Agent A didn't prepare handoff context OR
- Claude missed artifact when constructing context package OR
- Agent B discovered artifact but didn't integrate

**Fix:**
- Explicit handoff preparation in artifact reporting
- Claude systematically includes prior phase artifacts in context packages
- Mandatory integration reporting when building upon prior work

---

### Symptom 4: Incomplete Team Awareness

**Observable Behavior:**
```
Agent A, B, C all work on related tasks
None aware of others' progress or deliverables
Claude struggles to coordinate due to lack of visibility
```

**Diagnostic Questions:**
- Are all agents executing artifact discovery?
- Are all agents reporting artifacts created?
- Is Claude maintaining comprehensive artifact registry?

**Root Cause Likely:**
- Systematic protocol non-compliance across multiple agents OR
- Claude not enforcing communication requirements OR
- Missing session-state.md for complex workflow tracking

**Fix:**
- Claude actively enforces protocols for every agent engagement
- Create session-state.md documenting multi-agent coordination progress
- Systematic verification of communication compliance

---

## Recovery from Communication Gaps

### Recovery Strategy 1: Immediate Intervention

**When to Use:** Real-time detection of protocol violation during agent engagement

**Process:**
```
1. Detection: Claude identifies missing discovery/reporting/integration
2. Intervention: Stop agent engagement immediately
3. Clarification: Explain which protocol was missed and why it matters
4. Correction: Agent executes missing protocol using standard format
5. Continuation: Agent resumes work with complete context
```

**Example:**
```
Claude detects: TestEngineer created tests without artifact discovery
Claude: "Please execute Pre-Work Artifact Discovery before continuing test creation"
TestEngineer: [Checks /working-dir/, discovers recipe-service-coverage-analysis.md]
TestEngineer: Reports discoveries using standard format
TestEngineer: Adjusts test implementation to address identified gaps
TestEngineer: Continues with complete context
```

---

### Recovery Strategy 2: Retroactive Documentation

**When to Use:** Communication gap discovered after agent completion but before next engagement

**Process:**
```
1. Detection: Claude identifies missing artifact reporting after agent terminates
2. Context Preservation: Document gap in session-state.md
3. Next Engagement: Include instruction to review artifact and document integration
4. Integration: Next agent reports integration with previously-unreported artifact
5. Continuity: Context flow restored despite initial gap
```

**Example:**
```
BackendSpecialist completes work without reporting api-design-decisions.md
Claude discovers artifact exists but wasn't reported
Claude updates session-state.md: "BackendSpecialist created api-design-decisions.md (unreported)"
FrontendSpecialist engaged with: "Review api-design-decisions.md and report integration"
FrontendSpecialist discovers, reviews, reports integration using standard format
Context continuity restored through retroactive documentation
```

---

### Recovery Strategy 3: Communication Refresh

**When to Use:** Multiple violations across several agent engagements indicate systematic issue

**Process:**
```
1. Pattern Detection: Claude identifies recurring communication failures
2. Protocol Refresh: Re-emphasize communication requirements with examples
3. Template Provision: Provide specific templates for correct reporting
4. Verification: Confirm understanding before next agent engagement
5. Monitoring: Watch for compliance in subsequent engagements
```

**Example:**
```
Claude detects: 3 agents in a row skipped artifact discovery
Claude: "Working directory communication protocols refresher needed"
Claude: Provides artifact-discovery-template.md example
Claude: "Please execute this template before starting work"
Agents: Acknowledge and demonstrate understanding
Claude: Monitors next 3 engagements for compliance improvement
```

---

### Recovery Strategy 4: Session State Reconstruction

**When to Use:** Complex multi-agent workflow with multiple communication gaps requiring comprehensive recovery

**Process:**
```
1. Gap Analysis: Identify all missing communication instances
2. Artifact Inventory: List all working directory artifacts and creators
3. Session State Creation: Create session-state.md documenting:
   - All artifacts created (who, when, purpose)
   - All agent engagements (sequence, deliverables)
   - All integration points (who used whose artifacts)
   - Current workflow state (what's complete, what's pending)
4. Context Package Enhancement: Include session-state.md in all subsequent engagements
5. Communication Reset: All future agents follow protocols with session state as baseline
```

**Example:**
```
Complex feature implementation with 5 agent engagements, multiple communication gaps
Claude creates comprehensive-session-state.md:
  - BackendSpecialist: Created api-design.md, api-implementation.md
  - FrontendSpecialist: Created ui-components.md (used api-design.md)
  - TestEngineer: Created test-suite.md (used api-implementation.md, ui-components.md)
  - SecurityAuditor: Created security-audit.md (used api-implementation.md)
  - DocumentationMaintainer: Pending engagement
All artifacts catalogued, integrations documented, workflow state clear
DocumentationMaintainer engaged with complete session state context
Communication continuity restored, remaining work proceeds smoothly
```

---

## Best Practices for Prevention

### For Agents: Communication Checklist

**Before Starting Work:**
- [ ] Execute Pre-Work Artifact Discovery (check /working-dir/)
- [ ] Report discoveries using standard 🔍 format
- [ ] Review all relevant discovered artifacts
- [ ] Adjust approach based on discovered context

**When Creating Artifacts:**
- [ ] Report immediately upon creation (not batched, not delayed)
- [ ] Use standard 🗂️ format with all required fields
- [ ] Provide specific, actionable details (not generic)
- [ ] Identify intended consumers and dependencies clearly

**When Building on Prior Work:**
- [ ] Execute Context Integration Reporting
- [ ] Use standard 🔗 format with all required fields
- [ ] Acknowledge source artifacts and contributors
- [ ] Document value addition and handoff preparation

**At Task Completion:**
- [ ] Verify all artifacts reported
- [ ] Verify integration documented if applicable
- [ ] Confirm all communication protocols followed
- [ ] Prepare explicit handoff context for next agents

---

### For Claude: Enforcement Checklist

**Before Agent Engagement:**
- [ ] Include explicit artifact discovery mandate in context package
- [ ] Reference specific artifacts agent should discover
- [ ] Include artifact reporting requirement
- [ ] Include integration reporting requirement if building upon prior work

**During Agent Engagement:**
- [ ] Verify agent reports artifact discovery before starting work
- [ ] Monitor for artifact creation and immediate reporting
- [ ] Intervene if protocols violated (don't wait for completion)
- [ ] Provide templates and examples if agent struggles with formats

**After Agent Completion:**
- [ ] Verify all artifacts created were reported
- [ ] Verify integration reported if agent used prior artifacts
- [ ] Verify reporting quality (specific, actionable details)
- [ ] Update artifact registry for next agent engagements

**Multi-Agent Workflow Management:**
- [ ] Maintain comprehensive artifact registry
- [ ] Track agent engagement sequence and deliverables
- [ ] Create session-state.md for complex workflows
- [ ] Include relevant artifacts in each agent's context package

---

### For Multi-Agent Workflows: Coordination Patterns

**Sequential Workflows (Agent A → Agent B → Agent C):**
- Agent A prepares explicit handoff in artifact reporting ("Next Actions: Agent B should...")
- Claude includes Agent A's artifacts in Agent B's context package
- Agent B discovers Agent A's work and reports integration
- Process repeats for Agent C with cumulative context

**Parallel Workflows (Agents A, B, C working simultaneously):**
- Claude creates session-state.md documenting parallel work
- All parallel agents check session-state.md during artifact discovery
- All parallel agents update session-state.md with progress
- Claude monitors for conflicts and coordinates integration

**Iterative Workflows (Same agent engaged multiple times):**
- Agent discovers own prior artifacts in subsequent engagements
- Agent reports integration with own prior work (cumulative progress)
- Session-state.md tracks iteration progression
- Final iteration synthesizes all prior iterations

**Cross-Domain Workflows (Specialists coordinating):**
- Each specialist reports domain-specific artifacts with integration notes
- Claude identifies integration opportunities and coordinates handoffs
- Specialists discover cross-domain artifacts and report integration approach
- Final integration agent synthesizes all specialist contributions

---

## Communication Failure Impact Analysis

### Low Impact (Recoverable with Minimal Effort)

**Scenario:** Single agent misses artifact discovery, no critical dependencies
**Recovery:** Immediate intervention, agent checks artifacts, adjusts approach
**Time Cost:** ~5 minutes
**Quality Impact:** Minimal (caught early)

---

### Medium Impact (Requires Coordination Adjustment)

**Scenario:** Multiple agents miss reporting, subsequent agents lack context
**Recovery:** Retroactive documentation, session state reconstruction
**Time Cost:** ~30-60 minutes
**Quality Impact:** Moderate (some rework needed)

---

### High Impact (Major Coordination Failure)

**Scenario:** Systematic protocol non-compliance across complex multi-agent workflow
**Recovery:** Full session state reconstruction, communication refresh, potential rework
**Time Cost:** Several hours
**Quality Impact:** Significant (duplicated work, integration conflicts, delivery delays)

---

**Prevention ROI:**
- 5 minutes per engagement for protocol compliance
- vs. hours of recovery from communication failures
- **Prevention is 10-50x more efficient than recovery**

---

**Guide Status:** ✅ Complete
**Version:** 1.0
**Last Updated:** 2025-10-25
**Maintenance:** Update with new failure patterns as team identifies them through experience
