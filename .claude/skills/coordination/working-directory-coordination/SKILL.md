# Working Directory Coordination Skill

**Version:** 1.0.0
**Category:** Coordination
**Mandatory For:** ALL agents (no exceptions)

---

## PURPOSE

This skill defines mandatory team communication protocols and artifact management standards for `/working-dir/` usage across all agents in the zarichney-api multi-agent development team.

### Core Mission
Ensure seamless context flow, prevent communication gaps, and enable effective orchestration through comprehensive team awareness. All agent interactions with `/working-dir/` must maintain visibility across the team through immediate, standardized communication.

### Why This Matters
Multi-agent AI systems operate statelessly - agents have no inherent awareness of each other's work unless explicitly communicated. Without strict communication protocols, critical context gets lost, agents duplicate work, coordination fails, and the Codebase Manager (Claude) cannot effectively orchestrate team workflows.

### Mandatory Application
- **Universal Requirement**: Every agent, every task, every working directory interaction
- **No Exceptions**: Communication protocols apply regardless of task complexity or urgency
- **Enforcement**: Claude actively monitors compliance and intervenes when protocols are violated
- **Context Continuity**: Each agent builds upon existing team context rather than working in isolation

---

## WHEN TO USE

This skill applies in these MANDATORY scenarios:

### 1. Before Starting ANY Task (REQUIRED)
**Trigger:** Agent receives mission from Claude and is about to begin work
**Action:** Execute Pre-Work Artifact Discovery to check for existing team context
**Rationale:** Prevents duplicating analysis, identifies relevant prior work, ensures building upon rather than replacing existing context

### 2. When Creating/Updating ANY Working Directory File (MANDATORY)
**Trigger:** Agent creates or modifies any file in `/working-dir/`
**Action:** Execute Immediate Artifact Reporting using standardized format
**Rationale:** Maintains real-time team awareness, enables Claude to track progress, provides context for future agent engagements

### 3. When Building Upon Other Agents' Artifacts (REQUIRED)
**Trigger:** Agent uses another agent's working directory artifact as input for current work
**Action:** Execute Context Integration Reporting to document how prior work informs current deliverable
**Rationale:** Creates audit trail of context evolution, acknowledges team contributions, enables traceability across multi-agent workflows

### 4. During Multi-Agent Coordination (MANDATORY)
**Trigger:** Complex issue requiring sequential or parallel agent collaborations
**Action:** Execute all three communication protocols systematically across each agent engagement
**Rationale:** Prevents coordination failures, maintains context through handoffs, enables Claude to adapt orchestration strategy

---

## WORKFLOW STEPS

### Step 1: Pre-Work Artifact Discovery (REQUIRED)

**BEFORE STARTING ANY TASK**, agents must check `/working-dir/` for existing artifacts and report findings.

#### Discovery Process
1. **List Working Directory Contents**: Check all existing files in `/working-dir/`
2. **Identify Relevant Artifacts**: Determine which artifacts inform current task
3. **Analyze Context Provided**: Review relevant artifacts for insights
4. **Report Discoveries**: Use standardized discovery format

#### Standard Discovery Format
```
🔍 WORKING DIRECTORY DISCOVERY:
- Current artifacts reviewed: [list existing files checked]
- Relevant context found: [artifacts that inform current work]
- Integration opportunities: [how existing work will be built upon]
- Potential conflicts: [any overlapping concerns identified]
```

#### Discovery Checklist
- [ ] Checked `/working-dir/` for existing files
- [ ] Identified artifacts related to current mission
- [ ] Reviewed relevant artifacts for context
- [ ] Reported discoveries using standard format
- [ ] Planned how to integrate existing context into current work

#### When No Artifacts Exist
Even when `/working-dir/` is empty, report discovery attempt:
```
🔍 WORKING DIRECTORY DISCOVERY:
- Current artifacts reviewed: None found (empty working directory)
- Relevant context found: N/A - starting fresh analysis
- Integration opportunities: N/A - initial work for this issue
- Potential conflicts: None identified
```

**Resource:** See `resources/templates/artifact-discovery-template.md` for complete template

---

### Step 2: Immediate Artifact Reporting (MANDATORY)

**WHEN CREATING/UPDATING ANY WORKING DIRECTORY FILE**, agents must report immediately using standardized format.

#### Reporting Timing
- **Immediate**: Report when artifact is created, not in batches or at task completion
- **Per-File**: Each artifact gets individual reporting
- **Explicit**: Include all required fields with specific details

#### Standard Reporting Format
```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: [exact-filename-with-extension]
- Purpose: [brief description of content and intended consumers]
- Context for Team: [what other agents need to know about this artifact]
- Dependencies: [what other artifacts this builds upon or relates to]
- Next Actions: [any follow-up coordination needed]
```

#### Required Field Specifications

**Filename:**
- Exact filename with extension (e.g., `epic-291-issue-311-execution-plan.md`)
- No paths, just filename for clarity

**Purpose:**
- Brief 1-2 sentence description of artifact content
- Identify intended consumers (which agents/roles benefit from this)
- Specify artifact type (analysis, design, implementation notes, handoff context)

**Context for Team:**
- What do other agents need to know about this artifact?
- How does this artifact affect team workflows or coordination?
- What decisions or insights does this capture?

**Dependencies:**
- Which other artifacts does this build upon or reference?
- What prior work informed this artifact's creation?
- Note if artifact is independent or part of larger context chain

**Next Actions:**
- What follow-up coordination is needed?
- Which agents should engage with this artifact next?
- What decisions or validations are pending?

#### Reporting Checklist
- [ ] Reported immediately upon file creation/update
- [ ] Used exact standardized format
- [ ] Filled all required fields with specific details
- [ ] Identified intended consumers clearly
- [ ] Documented dependencies on other artifacts
- [ ] Specified next actions for team coordination

**Resource:** See `resources/templates/artifact-reporting-template.md` for complete template

---

### Step 3: Context Integration Reporting (REQUIRED)

**WHEN BUILDING UPON OTHER AGENTS' WORK**, report how existing artifacts inform current deliverable.

#### Integration Scenarios
- Using another agent's analysis to inform implementation decisions
- Building upon prior design work for next phase
- Consolidating multiple agent inputs into unified deliverable
- Evolving recommendations based on team feedback

#### Standard Integration Format
```
🔗 ARTIFACT INTEGRATION:
- Source artifacts used: [specific files that informed this work]
- Integration approach: [how existing context was incorporated]
- Value addition: [what new insights or progress this provides]
- Handoff preparation: [context prepared for future agents]
```

#### Required Field Specifications

**Source Artifacts Used:**
- List specific filenames consumed as input
- Note relevant sections or key insights from each
- Acknowledge original agent contributors

**Integration Approach:**
- How was existing context incorporated into current work?
- What synthesis or analysis was performed?
- How did prior work shape current decisions?

**Value Addition:**
- What new insights does current artifact provide beyond sources?
- How does this advance progress toward issue objectives?
- What gaps in prior context are now filled?

**Handoff Preparation:**
- What context is now ready for future agents?
- What decisions enable subsequent work?
- What dependencies or blockers are resolved?

#### Integration Checklist
- [ ] Identified all source artifacts used as input
- [ ] Explained integration approach clearly
- [ ] Documented value addition beyond prior work
- [ ] Prepared context for future agent handoffs
- [ ] Acknowledged original agent contributions

**Resource:** See `resources/templates/integration-reporting-template.md` for complete template

---

### Step 4: Communication Compliance Requirements

#### No Exceptions Policy
- **Mandatory**: All agents follow these protocols without exception
- **Universal**: Applies to analysis agents, file-editing agents, and specialists
- **Immediate**: No batching, no deferring, no "will report later"
- **Complete**: All required fields filled with specific, actionable details

#### Team Awareness Maintenance
Every agent engagement must:
- Check existing working directory artifacts before starting work
- Report any artifacts created immediately using standard format
- Document integration when building upon other agents' work
- Contribute to evolving team context rather than working in isolation

#### Claude's Enforcement Role
As orchestrator, Claude must:
- **Verify Compliance**: Confirm each agent reports artifacts using standardized formats
- **Enforce Discovery**: Ensure agents check existing artifacts before starting work
- **Track Continuity**: Monitor how artifacts build upon each other across agent engagements
- **Prevent Communication Gaps**: Intervene when agents miss reporting requirements
- **Maintain Team Awareness**: Keep comprehensive view of all working directory developments

#### Communication Failure Recovery
When agents fail to follow communication protocols:
1. **Immediate Intervention**: Claude stops agent engagement and requests proper communication
2. **Protocol Clarification**: Re-emphasize communication requirements with specific examples
3. **Compliance Verification**: Confirm agent understanding before continuing work
4. **Pattern Monitoring**: Watch for recurring communication failures across agents
5. **Escalation**: Report persistent communication issues to user for architectural review

#### Compliance Checklist
- [ ] Agent executed Pre-Work Artifact Discovery before starting task
- [ ] Agent reported all artifacts created using Immediate Artifact Reporting
- [ ] Agent documented integration when building upon other agents' work
- [ ] Claude verified compliance for each agent engagement
- [ ] No communication gaps identified in multi-agent workflow

---

## RESOURCES

This skill includes comprehensive resources for effective implementation:

### Templates (Ready-to-Use Formats)
- **artifact-discovery-template.md**: Standard format for Pre-Work Artifact Discovery
- **artifact-reporting-template.md**: Standard format for Immediate Artifact Reporting
- **integration-reporting-template.md**: Standard format for Context Integration Reporting

**Location:** `resources/templates/`
**Usage:** Copy template, fill in specific details, use verbatim in agent communication

### Examples (Reference Implementations)
- **multi-agent-coordination-example.md**: Cross-agent artifact flow demonstration
- **progressive-handoff-example.md**: Sequential agent collaboration with context continuity

**Location:** `resources/examples/`
**Usage:** Review examples for realistic scenarios showing all 4 workflow steps in action

### Documentation (Deep Dives)
- **communication-protocol-guide.md**: Philosophy and rationale behind mandatory protocols
- **troubleshooting-gaps.md**: Common communication failures and recovery strategies

**Location:** `resources/documentation/`
**Usage:** Understand protocol philosophy, troubleshoot issues, optimize communication effectiveness

---

## INTEGRATION WITH TEAM WORKFLOWS

### Multi-Agent Coordination Patterns
This skill enables:
- **Sequential Workflows**: Agent A → Agent B → Agent C with perfect context continuity
- **Parallel Workflows**: Multiple agents work simultaneously with awareness of each other's progress
- **Iterative Workflows**: Same agent re-engaged multiple times building upon own prior work
- **Cross-Domain Workflows**: Specialist agents coordinate across technical domains

### Claude's Orchestration Enhancement
Working directory communication enables Claude to:
- Track real-time progress across all agent engagements
- Adapt coordination strategy based on agent discoveries
- Identify integration opportunities between agent deliverables
- Intervene early when coordination issues emerge
- Maintain comprehensive context across complex multi-agent workflows

### Quality Gate Integration
Communication protocols integrate with:
- **ComplianceOfficer**: Pre-PR validation leveraging working directory artifacts
- **AI Sentinels**: Code review informed by implementation decision artifacts
- **TestEngineer**: Test strategy informed by feature design artifacts
- **DocumentationMaintainer**: README updates guided by contract change artifacts

---

## SUCCESS METRICS

### Communication Effectiveness
- **100% Compliance**: All agents follow protocols for all working directory interactions
- **Zero Context Gaps**: No missing context between agent engagements
- **Real-Time Awareness**: Claude maintains current view of all team developments
- **Seamless Handoffs**: Agents build upon rather than duplicate prior work

### Team Coordination Quality
- **Context Continuity**: Each agent engagement adds value to evolving context
- **Reduced Overhead**: Less clarification needed through proactive communication
- **Faster Coordination**: Claude makes decisions with complete team context
- **Higher Quality**: Deliverables benefit from integrated team insights

### Orchestration Efficiency
- **Adaptive Planning**: Claude evolves strategy based on artifact discoveries
- **Parallel Optimization**: Multiple agents work efficiently with coordination awareness
- **Early Intervention**: Issues detected through communication before becoming blockers
- **Comprehensive Integration**: Final deliverables reflect full team collaboration

---

**Skill Status:** ✅ **OPERATIONAL**
**Adoption:** Mandatory for all 11 agents
**Context Savings:** ~450 lines across agents (~3,600 tokens total)
**Progressive Loading:** metadata.json (~80 tokens) → SKILL.md (~2,500 tokens) → resources (on-demand)
