# Agent Identity Design Template (Phase 1)

**Purpose:** Systematic questionnaire for defining agent identity before selecting structural template. Complete this before applying specialist/primary/advisory templates.

**Usage:** Answer each question section thoroughly. Your responses guide template selection and customization in Phase 2.

---

## 1. ROLE DEFINITION

### Primary Purpose
**Question:** What specific problem does this agent solve that the current 12-agent team doesn't address?

**Answer:**
- [Describe gap in current team capabilities]
- [Explain business need or workflow efficiency improvement]
- [Identify user pain point this agent resolves]

### Core Mission Statement
**Question:** In 2-3 sentences, what is this agent's primary mission?

**Answer:**
[Draft mission statement focusing on unique value and deliverables]

### Agent Classification Decision
**Question:** Which classification best fits this agent's role?

**Options:**
- [ ] **Specialist Agent** - Domain expert with flexible authority (query intent = analysis, command intent = implementation)
- [ ] **Primary File-Editing Agent** - Exclusive direct modification authority over specific file types
- [ ] **Advisory Agent** - Analysis-only through working directory artifacts, no direct file modifications

**Selection Rationale:**
[Explain why this classification fits the agent's purpose]

**Decision Tree Guidance:**
```
Does agent need to modify files directly?
├─ NO → Advisory Agent (working directory only)
└─ YES
   └─ Does agent need both analysis AND implementation capabilities?
      ├─ YES → Specialist Agent (flexible authority with intent recognition)
      └─ NO → Primary File-Editing Agent (exclusive direct modification)
```

---

## 2. AUTHORITY BOUNDARY DESIGN

### File Edit Rights Specification
**Question:** What files will this agent modify? (Use glob patterns for precision)

**Answer:**
```yaml
DIRECT_MODIFICATION_AUTHORITY:
  - "[Pattern 1 - e.g., Code/Zarichney.Server/**/*.cs excluding tests]"
  - "[Pattern 2 - e.g., config/**/*.json]"
  - "[Pattern 3 - additional file patterns]"

FORBIDDEN_MODIFICATIONS:
  - "[Other agent exclusive files - e.g., **/*Tests.cs (TestEngineer)]"
  - "[Additional boundaries - e.g., **/*.md (DocumentationMaintainer)]"
```

**Validation Check:**
- [ ] No overlap with existing agent exclusive authorities
- [ ] Glob patterns unambiguous (no interpretation needed)
- [ ] Forbidden zones clearly documented

### Intent Recognition Framework (Specialists Only)
**Question:** How does this agent distinguish between analysis requests vs. implementation requests?

**Answer (skip if Primary or Advisory):**
```yaml
QUERY_INTENT_INDICATORS:
  - "[Indicator 1 - e.g., 'Analyze backend performance']"
  - "[Indicator 2 - e.g., 'Review API architecture']"
  - "[Indicator 3 - additional query patterns]"

COMMAND_INTENT_INDICATORS:
  - "[Indicator 1 - e.g., 'Implement new API endpoint']"
  - "[Indicator 2 - e.g., 'Fix database query performance']"
  - "[Indicator 3 - additional command patterns]"
```

### Working Directory Usage (Advisory Agents Only)
**Question:** What types of artifacts will this agent create for other agents?

**Answer (skip if Primary or Specialist):**
```yaml
ADVISORY_ARTIFACTS:
  - "[Artifact Type 1 - e.g., Architectural Decision Records (ADRs)]"
  - "[Artifact Type 2 - e.g., Compliance validation reports]"
  - "[Artifact Type 3 - additional analysis deliverables]"
```

---

## 3. DOMAIN EXPERTISE SCOPE

### Technical Specialization
**Question:** What specific technical areas does this agent master?

**Answer:**
```yaml
CORE_COMPETENCIES:
  - "[Competency 1 - e.g., .NET 8, C#, ASP.NET Core]"
  - "[Competency 2 - e.g., Entity Framework Core, database design]"
  - "[Competency 3 - e.g., RESTful API architecture]"
  - "[Competency 4 - additional technical capabilities]"

EXPERTISE_DEPTH:
  - Level: [Junior/Mid/Senior/Principal equivalent years]
  - Standards Mastery: [Relevant project standards this agent must know]
  - Project Context: [Project-specific knowledge requirements]
```

### Knowledge Requirements
**Question:** What documentation and standards must this agent load before work?

**Answer:**
```yaml
REQUIRED_STANDARDS:
  - "[Standard 1 - e.g., /Docs/Standards/CodingStandards.md]"
  - "[Standard 2 - e.g., /Docs/Standards/TestingStandards.md]"
  - "[Additional standards relevant to domain]"

MODULE_CONTEXT:
  - "[Context 1 - e.g., Code/Zarichney.Server/README.md]"
  - "[Context 2 - e.g., Domain-specific module documentation]"
```

---

## 4. TEAM INTEGRATION PLANNING

### Agent Coordination Mapping
**Question:** Which existing agents will this agent coordinate with most frequently?

**Answer:**
```yaml
PRIMARY_COORDINATION:
  Agent: [Agent Name - e.g., CodeChanger]
  Pattern: [How coordination works - e.g., "Implements testability requirements"]
  Frequency: [How often - e.g., "Every feature implementation"]

SECONDARY_COORDINATION:
  Agent: [Agent Name - e.g., DocumentationMaintainer]
  Pattern: [Coordination type]
  Frequency: [Cadence]

TERTIARY_COORDINATION:
  Agent: [Agent Name - e.g., SecurityAuditor]
  Pattern: [Coordination scenario]
  Frequency: [When triggered]
```

### Sequential Workflow Position
**Question:** Where does this agent typically fit in development workflows?

**Answer:**
Typical progression:
1. [Step 1 - e.g., "DocumentationMaintainer: Load module context"]
2. [Step 2 - e.g., "CodeChanger: Implement feature"]
3. **[This Agent]:** [Your agent's role in sequence]
4. [Step 4 - e.g., "TestEngineer: Create tests"]
5. [Step 5 - e.g., "ComplianceOfficer: Pre-PR validation"]

### Handoff Protocols
**Question:** What artifacts or deliverables does this agent provide to other agents?

**Answer:**
```yaml
PROVIDES_TO:
  [Agent Name]:
    - "[Deliverable 1 - e.g., Implementation code ready for testing]"
    - "[Deliverable 2 - e.g., Working directory analysis report]"

  [Another Agent]:
    - "[Deliverable - e.g., Updated configuration files]"
```

**Question:** What artifacts or context does this agent consume from other agents?

**Answer:**
```yaml
CONSUMES_FROM:
  [Agent Name]:
    - "[Input 1 - e.g., Test coverage reports]"
    - "[Input 2 - e.g., Architectural recommendations]"

  [Another Agent]:
    - "[Input - e.g., Security analysis findings]"
```

---

## 5. ESCALATION SCENARIOS

### Autonomous Action Boundaries
**Question:** When can this agent proceed independently without coordination?

**Answer:**
```yaml
AUTONOMOUS_SCENARIOS:
  - "[Scenario 1 - e.g., File modifications within exclusive authority]"
  - "[Scenario 2 - e.g., Standards compliance validation within domain]"
  - "[Scenario 3 - e.g., Working directory artifact creation]"
```

### Coordination Triggers
**Question:** When must this agent coordinate with other agents before proceeding?

**Answer:**
```yaml
COORDINATION_REQUIRED:
  - "[Trigger 1 - e.g., Cross-domain implementations affecting multiple agents]"
  - "[Trigger 2 - e.g., Security-sensitive modifications requiring SecurityAuditor]"
  - "[Trigger 3 - e.g., Architecture changes affecting system design]"
```

### Escalation to Claude
**Question:** What scenarios require escalation to Codebase Manager (Claude)?

**Answer:**
```yaml
ESCALATION_SCENARIOS:
  - "[Scenario 1 - e.g., Authority boundary conflicts between agents]"
  - "[Scenario 2 - e.g., Standards ambiguity or conflicts]"
  - "[Scenario 3 - e.g., Multi-agent coordination failures]"
  - "[Scenario 4 - e.g., Complexity overflow beyond agent scope]"
```

---

## 6. QUALITY STANDARDS

### Quality Gates
**Question:** What quality criteria must this agent's work meet?

**Answer:**
```yaml
QUALITY_METRICS:
  - "[Metric 1 - e.g., 100% executable test pass rate]"
  - "[Metric 2 - e.g., Standards compliance validation]"
  - "[Metric 3 - e.g., Comprehensive coverage of edge cases]"

VALIDATION_APPROACH:
  - "[Validation 1 - e.g., Execute test suite after modifications]"
  - "[Validation 2 - e.g., ComplianceOfficer pre-PR review]"
```

### Standards Compliance
**Question:** Which project standards govern this agent's work quality?

**Answer:**
- [Standard 1 - e.g., "CodingStandards.md for all code modifications"]
- [Standard 2 - e.g., "TestingStandards.md for test coverage requirements"]
- [Additional standards relevant to domain]

---

## 7. SKILLS INTEGRATION

### Mandatory Skills (All Agents)
**Validation:** Confirm these will be integrated:
- [x] **working-directory-coordination** - Team communication protocols
- [x] **documentation-grounding** - Standards loading before work

### Domain-Specific Skills
**Question:** What domain-specific skills should this agent reference?

**Answer:**
```yaml
DOMAIN_SKILLS:
  - "[Skill 1 - e.g., api-design-patterns for BackendSpecialist]"
  - "[Skill 2 - e.g., test-architecture for TestEngineer]"
  - "[Skill 3 - additional domain-relevant skills]"
```

### Optional Skills
**Question:** What contextual or secondary skills might this agent need?

**Answer:**
- [Optional Skill 1 - e.g., "core-issue-focus for implementation agents"]
- [Optional Skill 2 - e.g., "github-issue-creation for tracking technical debt"]

---

## 8. TEMPLATE SELECTION DECISION

### Based on Your Answers Above

**Recommended Template:**
- [ ] **specialist-agent-template.md** - Flexible authority with intent recognition (query vs. command)
- [ ] **primary-agent-template.md** - Exclusive file-editing focus with direct modification authority
- [ ] **advisory-agent-template.md** - Working directory only, analysis and recommendations

**Selection Reasoning:**
[Explain why this template matches answers from sections 1-7]

**Next Steps:**
1. Copy recommended template file
2. Replace all `[PLACEHOLDER]` markers with answers from this identity design
3. Customize domain-specific sections using your expertise scope answers
4. Integrate mandatory and domain-specific skills from section 7
5. Validate progressive loading efficiency (target 130-240 lines core definition)

---

## IDENTITY DESIGN VALIDATION CHECKLIST

Before proceeding to Phase 2 (Structure Template Application):

- [ ] **Role Classification:** Clear choice between specialist/primary/advisory with rationale
- [ ] **Authority Boundaries:** File edit rights specified with unambiguous glob patterns
- [ ] **Domain Expertise:** Technical specialization areas articulated with depth
- [ ] **Team Coordination:** Mapped coordination patterns with other 11 agents
- [ ] **Intent Recognition:** Defined (for specialists) or confirmed N/A (for primary/advisory)
- [ ] **Escalation Scenarios:** Identified when to coordinate vs. escalate vs. act autonomously
- [ ] **Quality Standards:** Established measurable quality gates and validation approaches
- [ ] **Skills Integration:** Identified mandatory + domain-specific skills to reference

**Validation Status:** [ ] COMPLETE - Ready for Phase 2 template application

---

**Usage Note:** This template helps PromptEngineer systematically design agent identity before structural implementation. Complete answers guide efficient Phase 2 customization and ensure no critical design elements overlooked.
