# Skills Catalog - Epic #291

**Last Updated:** 2025-10-25
**Purpose:** Complete catalog of all 8 skills with comprehensive specifications

---

## Skill Categories

- **Core Skills (5):** Foundational capabilities eliminating cross-cutting redundancy
- **Meta-Skills (3):** Scalability enablers for agent/skill/command creation

---

## Core Skills

### Skill 1: working-directory-coordination

**Category:** Coordination
**Priority:** P0 - Mandatory for all 11 agents
**Location:** `.claude/skills/coordination/working-directory-coordination/`

**Purpose:**
Standardize working directory usage, artifact management, and team communication protocols across all agents to ensure seamless context flow, prevent communication gaps, and enable effective orchestration through comprehensive team awareness.

**When to Use:**
- MANDATORY: Before starting ANY task (artifact discovery)
- MANDATORY: When creating/updating ANY working directory file
- MANDATORY: When building upon other agents' artifacts
- Universal application: All agents, all tasks, no exceptions

**Target Agents:** ALL (mandatory reference for all 11 agents)

**Workflow Steps:**
1. **Pre-Work Artifact Discovery (REQUIRED)**
   - Check /working-dir/ for existing artifacts before starting work
   - Identify relevant context from other agents
   - Report discoveries using standard format
2. **Immediate Artifact Reporting (MANDATORY)**
   - Report file creation immediately using standard format
   - Specify purpose, consumers, context for team
   - Document dependencies and next actions
3. **Context Integration Reporting (REQUIRED)**
   - Report when building upon other agents' work
   - Specify source artifacts used and integration approach
   - Document value addition and handoff preparation
4. **Communication Compliance Requirements**
   - No exceptions to reporting protocols
   - Team awareness maintained for all artifacts
   - Context continuity enforced across engagements

**Resources:**
- **Templates:**
  - artifact-discovery-template.md: Standard discovery format
  - artifact-reporting-template.md: Standard reporting format
  - integration-reporting-template.md: Standard integration format
- **Examples:**
  - multi-agent-coordination-example.md: Cross-agent artifact flow
  - progressive-handoff-example.md: Sequential agent collaboration
- **Documentation:**
  - communication-protocol-guide.md: Deep dive on protocols
  - troubleshooting-gaps.md: Common communication failures

**Token Load:**
- Frontmatter (YAML metadata): ~100 tokens
- Instructions: ~2,500 tokens
- Resources (on-demand): ~500-2,000 tokens

**Context Savings:** ~450 lines across 11 agents (~3,600 tokens total)

**Dependencies:** None - foundational pattern

**Structure:** YAML frontmatter in SKILL.md per [official specification](./official-skills-structure.md)

---

### Skill 2: documentation-grounding

**Category:** Documentation
**Priority:** P0 - Mandatory for all agents
**Location:** `.claude/skills/documentation/documentation-grounding/`

**Purpose:**
Systematic framework for loading project standards, module READMEs, and architectural patterns before agent work begins, supporting stateless AI operation with complete context.

**When to Use:**
- MANDATORY: At the start of every agent engagement
- After receiving context package from Claude
- Before modifying any code or documentation
- When switching between different modules or domains

**Target Agents:** ALL (mandatory for all 11 agents)

**Workflow Steps:**
1. **Phase 1: Standards Mastery**
   - Load CodingStandards.md for production code requirements
   - Load TestingStandards.md for test quality requirements
   - Load DocumentationStandards.md for README structure
   - Load TaskManagementStandards.md for git workflow
2. **Phase 2: Project Architecture Context**
   - Load root README.md for project overview
   - Load module-specific README hierarchy
   - Review architectural diagrams
   - Understand dependency mapping
3. **Phase 3: Domain-Specific Context**
   - Deep-dive into relevant module READMEs
   - Understand interface contracts
   - Recognize local conventions
   - Integrate historical context

**Resources:**
- **Templates:**
  - standards-loading-checklist.md: Systematic loading workflow
  - module-context-template.md: Module README analysis structure
- **Examples:**
  - backend-specialist-grounding.md: Backend-specific grounding pattern
  - test-engineer-grounding.md: Testing-specific grounding pattern
  - documentation-maintainer-grounding.md: Documentation grounding pattern
- **Documentation:**
  - grounding-optimization-guide.md: Context window optimization
  - selective-loading-patterns.md: Task-based grounding strategies

**Token Load:**
- Frontmatter (YAML metadata): ~100 tokens
- Instructions: ~3,000 tokens
- Resources (on-demand): ~1,000-3,000 tokens

**Context Savings:** ~400 lines across 11 agents (~3,200 tokens total)

**Dependencies:** /Docs/Standards/* files must exist

**Structure:** YAML frontmatter in SKILL.md per [official specification](./official-skills-structure.md)

---

### Skill 3: core-issue-focus

**Category:** Coordination
**Priority:** P1 - Mandatory for primary agents
**Location:** `.claude/skills/coordination/core-issue-focus/`

**Purpose:**
Mission discipline framework preventing scope creep and ensuring agents focus on specific blocking technical problems with surgical precision.

**When to Use:**
- When receiving mission from Claude with specific core issue
- During complex implementations with scope expansion risk
- When multiple improvements are tempting but not core
- Before expanding scope beyond original technical problem

**Target Agents:** TestEngineer, PromptEngineer, CodeChanger, BackendSpecialist, FrontendSpecialist, WorkflowEngineer

**Workflow Steps:**
1. **Identify Core Issue First**
   - What specific technical problem is blocking progress?
   - What is the minimum fix needed?
   - What are the success criteria?
2. **Surgical Scope Definition**
   - Focus solely on resolving the blocking problem
   - Defer secondary improvements
   - Document scope boundaries clearly
3. **Mission Drift Detection**
   - Monitor for scope expansion signals
   - Validate changes against core issue resolution
   - Escalate when scope expands beyond mission
4. **Core Issue Validation**
   - Test that core functionality now works
   - Validate success criteria met
   - Document any remaining improvements for future work

**Resources:**
- **Templates:**
  - core-issue-analysis-template.md: Problem identification structure
  - scope-boundary-definition.md: Scope constraint documentation
  - success-criteria-validation.md: Testable outcome specification
- **Examples:**
  - api-bug-fix-example.md: Surgical bug resolution
  - feature-implementation-focused.md: Feature with clear boundaries
  - refactoring-scoped.md: Refactoring with mission discipline
- **Documentation:**
  - mission-drift-patterns.md: Common scope expansion triggers
  - validation-checkpoints.md: When to verify core issue status

**Token Load:**
- Frontmatter (YAML metadata): ~100 tokens
- Instructions: ~2,000 tokens
- Resources (on-demand): ~500-1,500 tokens

**Context Savings:** ~200 lines across 6 agents (~1,600 tokens total)

**Dependencies:** None - coordination pattern

**Structure:** YAML frontmatter in SKILL.md per [official specification](./official-skills-structure.md)

---

### Skill 4: github-issue-creation

**Category:** GitHub Workflows
**Priority:** P1 - Critical for automation
**Location:** `.claude/skills/github/github-issue-creation/`

**Purpose:**
Streamline GitHub issue creation with automated context collection, template application, and proper labeling, eliminating manual "hand bombing" of context.

**When to Use:**
- Creating new feature requests from user requirements
- Documenting bugs with comprehensive reproduction context
- Proposing architectural improvements with analysis
- Tracking technical debt items systematically
- Creating epic milestones with progression tracking

**Target Agents:** Codebase Manager (Claude), BugInvestigator, ArchitecturalAnalyst, TestEngineer

**Workflow Steps:**
1. **Context Collection Phase**
   - Gather user requirements and pain points
   - Analyze existing codebase for related functionality
   - Review similar issues and PRs for patterns
   - Collect relevant code snippets and file paths
   - Identify acceptance criteria and success metrics
2. **Template Selection**
   - Feature Request Template (enhancement)
   - Bug Report Template (defect)
   - Epic Template (milestone/initiative)
   - Technical Debt Template (refactoring)
   - Documentation Request Template (knowledge gap)
3. **Issue Construction**
   - Title: Clear, actionable, follows naming conventions
   - Description: Comprehensive context using template structure
   - Labels: Apply per GitHubLabelStandards.md
   - Milestone: Link to epic if applicable
   - Assignees: Identify domain experts
   - Related Issues: Link dependencies
4. **Validation & Submission**
   - Verify all template sections completed
   - Validate label compliance
   - Check for duplicate issues
   - Confirm acceptance criteria clarity
   - Submit via gh CLI or API

**Resources:**
- **Templates:**
  - feature-request-template.md: Enhancement structure
  - bug-report-template.md: Defect documentation
  - epic-template.md: Initiative planning
  - technical-debt-template.md: Refactoring tracking
  - documentation-request-template.md: Knowledge gap
- **Examples:**
  - comprehensive-feature-example.md: Full feature request
  - bug-with-reproduction.md: Complete bug report
  - epic-milestone-example.md: Multi-issue initiative
- **Documentation:**
  - issue-creation-guide.md: Step-by-step process
  - label-application-guide.md: Standards compliance
  - context-collection-patterns.md: Effective gathering

**Token Load:**
- Frontmatter (YAML metadata): ~100 tokens
- Instructions: ~3,000 tokens
- Resources (on-demand): ~2,000-4,000 tokens

**Context Savings:** ~200 lines of scattered guidance consolidated

**Dependencies:** GitHubLabelStandards.md, /Docs/Templates/ issue templates

**Command Integration:** /create-issue command

**Structure:** YAML frontmatter in SKILL.md per [official specification](./official-skills-structure.md)

---

### Skill 5: flexible-authority-management

**Category:** Coordination
**Priority:** P1 - Critical for specialists
**Location:** `.claude/skills/coordination/flexible-authority-management/`

**Purpose:**
Intent recognition framework enabling specialists to distinguish query-intent (analysis) vs. command-intent (implementation) requests and respond appropriately within domain boundaries.

**When to Use:**
- When specialist receives request from Claude
- Before determining analysis vs. implementation response
- When validating domain authority boundaries
- During cross-domain coordination requirements

**Target Agents:** BackendSpecialist, FrontendSpecialist, SecurityAuditor, WorkflowEngineer, BugInvestigator

**Workflow Steps:**
1. **Intent Recognition Analysis**
   - Identify query indicators: "Analyze/Review/Assess/Evaluate"
   - Identify command indicators: "Fix/Implement/Update/Create"
   - Determine appropriate response mode
2. **Authority Boundary Validation**
   - Verify target files within domain authority
   - Check for cross-domain dependencies
   - Validate implementation permissions
3. **Response Mode Selection**
   - Query Intent → Working directory artifacts (advisory)
   - Command Intent → Direct file modifications (implementation)
   - Hybrid → Analysis + implementation within domain
4. **Domain Compliance Verification**
   - Ensure actions within flexible authority boundaries
   - Escalate cross-domain concerns to Claude
   - Document authority decisions

**Resources:**
- **Templates:**
  - intent-analysis-template.md: Query vs. command determination
  - authority-validation-checklist.md: Domain boundary verification
  - response-mode-selection.md: Action type determination
- **Examples:**
  - backend-query-intent.md: Analysis-only backend specialist engagement
  - frontend-command-intent.md: Implementation-focused frontend work
  - security-hybrid-intent.md: Analysis + remediation combined
- **Documentation:**
  - intent-recognition-guide.md: Pattern identification
  - domain-authority-boundaries.md: Specialist file permissions
  - cross-domain-coordination.md: When to escalate

**Token Load:**
- Frontmatter (YAML metadata): ~100 tokens
- Instructions: ~2,500 tokens
- Resources (on-demand): ~1,000-2,500 tokens

**Context Savings:** ~600 lines across 5 specialist agents (~4,800 tokens total)

**Dependencies:** CLAUDE.md flexible authority framework

**Structure:** YAML frontmatter in SKILL.md per [official specification](./official-skills-structure.md)

---

## Meta-Skills

### Meta-Skill 1: agent-creation

**Category:** Meta-Skills
**Priority:** P2 - Scalability enabler
**Location:** `.claude/skills/meta/agent-creation/`

**Purpose:**
Standardized framework for creating new agent definitions following prompt engineering best practices, enabling PromptEngineer to scale agent team efficiently.

**When to Use:**
- Adding new specialized agent to team
- Refactoring existing agent for authority changes
- Splitting agent responsibilities for scalability
- Migrating general-purpose patterns to specialized agents

**Target Agents:** PromptEngineer, Codebase Manager (Claude)

**Workflow Steps:**
1. **Agent Identity Design**
   - Role Definition: Clear, focused responsibility
   - Authority Boundaries: Exclusive file edit rights
   - Domain Expertise: Specific technical specialization
   - Team Integration: Coordination with existing agents
2. **Structure Template Application**
   - Required Sections: Metadata, Role, Mission, Integration, Constraints
   - Skill References: working-directory-coordination, documentation-grounding, domain skills
   - Docs References: Standards, workflows, module READMEs
   - Context Optimization: Target 130-240 lines core definition
3. **Authority Framework Design**
   - File Edit Rights: Exact file patterns and domains
   - Implementation Authority: Query vs. command intents (if specialist)
   - Coordination Requirements: When to engage other agents
   - Escalation Protocols: When to defer to Claude
4. **Skill Integration**
   - Mandatory Skills: All agents use working-directory-coordination
   - Domain Skills: Specialist-specific technical skills
   - Optional Skills: Context-dependent capabilities
5. **Context Optimization**
   - Progressive Loading: Metadata → Core → Skills → Docs
   - Token Budget: Target 130-240 lines for core definition
   - Skill References: ~20 tokens per skill
   - Doc References: ~10 tokens per link

**Resources:**
- **Templates:**
  - agent-definition-template.md: Complete structure
  - specialist-agent-template.md: Flexible authority variant
  - primary-agent-template.md: File-editing agent variant
  - advisory-agent-template.md: Working directory only variant
- **Examples:**
  - test-engineer-anatomy.md: Exemplary agent structure
  - backend-specialist-authority.md: Flexible authority example
  - compliance-officer-minimal.md: Focused, efficient agent
- **Documentation:**
  - agent-design-principles.md: Prompt engineering best practices
  - authority-framework-guide.md: Boundary design patterns
  - skill-integration-patterns.md: Progressive loading design

**Token Load:**
- Frontmatter (YAML metadata): ~100 tokens
- Instructions: ~4,000 tokens
- Resources (on-demand): ~3,000-6,000 tokens

**Efficiency Gain:** 50% faster agent creation with 100% consistency

**Dependencies:** Core skills operational, templates available

**Structure:** YAML frontmatter in SKILL.md per [official specification](./official-skills-structure.md)

---

### Meta-Skill 2: skill-creation

**Category:** Meta-Skills
**Priority:** P2 - Scalability enabler
**Location:** `.claude/skills/meta/skill-creation/`

**Purpose:**
Systematic framework for creating new skills with consistent structure, metadata, and progressive loading design, preventing skill bloat while ensuring quality.

**When to Use:**
- Extracting redundant patterns from agent definitions
- Creating new domain-specific capabilities
- Building meta-skills for team scaling
- Standardizing cross-cutting technical workflows

**Target Agents:** PromptEngineer

**Workflow Steps:**
1. **Skill Scope Definition**
   - Cross-Cutting Concern: Used by 3+ agents (coordination skill)
   - Domain Specialization: Deep technical patterns for specific agents
   - Meta-Skill: For agent/skill/command creation and management
   - Workflow Automation: Repeatable processes with clear steps
2. **Skill Structure Design**
   - SKILL.md: Purpose, When to Use, Workflow Steps, Resources
   - metadata.json: Discovery fields with token estimates
   - Resources: Templates, examples, documentation organization
3. **Progressive Loading Design**
   - Metadata Discovery (~100 tokens): Agent scans for relevant skills
   - Instructions Loading (2000-5000 tokens): On-demand when skill invoked
   - Resource Access (variable): Templates/examples loaded as needed
   - Total Efficiency: Load only what's needed, when needed
4. **Resource Organization**
   - resources/templates/: Reusable formats
   - resources/examples/: Reference implementations
   - resources/documentation/: Deep dives
5. **Agent Integration Pattern**
   - Brief 2-3 line summary of skill purpose
   - Key workflow steps
   - Reference to skill for complete instructions

**Resources:**
- **Templates:**
  - skill-template.md: Standard SKILL.md structure
  - metadata-template.json: Discovery metadata format
  - resource-organization-template/: Directory structure
- **Examples:**
  - coordination-skill-example/: working-directory-coordination anatomy
  - technical-skill-example/: Domain-specific pattern skill
  - meta-skill-example/: Self-referential skill creation
- **Documentation:**
  - progressive-loading-guide.md: Context optimization patterns
  - skill-discovery-mechanics.md: How agents find skills
  - integration-patterns.md: Agent skill reference best practices

**Quality Standards:**
- Context Efficiency: Metadata <150 tokens, Instructions 2,000-5,000 tokens
- Clarity & Usability: Clear purpose, specific triggers, achievable workflow
- Integration: No quality gate bypass, AI Sentinel compatible
- Testing: Validation examples, expected outputs

**Token Load:**
- Frontmatter (YAML metadata): ~100 tokens
- Instructions: ~3,500 tokens
- Resources (on-demand): ~2,000-5,000 tokens

**Quality Assurance:** Ensures all new skills follow optimization patterns, prevents skill bloat

**Dependencies:** Skill templates, validation framework

**Structure:** YAML frontmatter in SKILL.md per [official specification](./official-skills-structure.md)

---

### Meta-Skill 3: command-creation

**Category:** Meta-Skills
**Priority:** P2 - Scalability enabler
**Location:** `.claude/skills/meta/command-creation/`

**Purpose:**
Systematic framework for creating slash commands with consistent structure, argument handling, and skill integration, ensuring developer-friendly UX.

**When to Use:**
- Creating user-facing workflow shortcuts
- Exposing CI/CD monitoring capabilities
- Providing quick access to analysis commands
- Standardizing common development operations

**Target Agents:** PromptEngineer, WorkflowEngineer

**Workflow Steps:**
1. **Command Scope Definition**
   - User Intent: What user wants to accomplish
   - Workflow Complexity: Simple query vs. complex orchestration
   - Skill Dependency: Existing skill or new skill needed
   - Argument Requirements: Required vs. optional parameters
2. **Command Structure Template**
   - Frontmatter: description, argument-hint, category
   - Usage Examples: Multiple patterns with expected outputs
   - Arguments: Detailed specifications with defaults
   - Output: Expected results description
   - Integration: How command fits into workflows
3. **Skill Integration Design**
   - Command handles: Argument parsing, validation, user messaging
   - Skill handles: Workflow execution, business logic, resource access
   - Clear boundary: Command = interface, Skill = implementation
4. **Argument Handling Patterns**
   - Positional: /command arg1 arg2 (order matters)
   - Named: /command --format json --threshold 80 (order flexible)
   - Flags: /command --verbose --dry-run (boolean toggles)
   - Defaults: Document clearly, sensible fallbacks
5. **Error Handling & Feedback**
   - Invalid arguments: Clear error messages with examples
   - Missing dependencies: Explain requirements, suggest fixes
   - Execution failures: Actionable troubleshooting guidance
   - Success feedback: Confirm action, next steps

**Resources:**
- **Templates:**
  - command-template.md: Standard command structure
  - skill-integrated-command.md: Command + skill pattern
  - argument-parsing-patterns.md: Common arg handling
- **Examples:**
  - test-report-command.md: Comprehensive command example
  - workflow-status-command.md: CI/CD monitoring command
  - create-issue-command.md: GitHub workflow command
- **Documentation:**
  - command-design-guide.md: Best practices and patterns
  - skill-integration-guide.md: Command-skill boundaries
  - argument-handling-guide.md: Parsing and validation

**Quality Standards:**
- User Experience: Clear description, comprehensive examples, helpful errors
- Technical Excellence: Clean skill integration, robust validation, good performance
- Team Coordination: Skill dependencies documented, tool integration clear

**Token Load:**
- Frontmatter (YAML metadata): ~100 tokens
- Instructions: ~3,000 tokens
- Resources (on-demand): ~1,500-3,000 tokens

**UX Consistency:** Prevents command bloat, ensures consistent UX, clear skill integration

**Dependencies:** Command templates, skill integration patterns

**Structure:** YAML frontmatter in SKILL.md per [official specification](./official-skills-structure.md)

---

## Skill Usage Metrics (Projected)

| Skill | Agents Using | Context Savings | Priority |
|-------|--------------|-----------------|----------|
| working-directory-coordination | 11 | ~450 lines (~3,600 tokens) | P0 |
| documentation-grounding | 11 | ~400 lines (~3,200 tokens) | P0 |
| flexible-authority-management | 5 | ~600 lines (~4,800 tokens) | P1 |
| core-issue-focus | 6 | ~200 lines (~1,600 tokens) | P1 |
| github-issue-creation | 4 | ~200 lines (~1,600 tokens) | P1 |
| agent-creation | 1 | N/A (scalability) | P2 |
| skill-creation | 1 | N/A (quality) | P2 |
| command-creation | 2 | N/A (UX) | P2 |

**Total Core Skills Savings:** ~1,850 lines across agents (~14,800 tokens)

**Meta-Skills Value:** Enable unlimited scalability for agent/skill/command creation with consistent quality and efficiency

---

## Skill Discovery and Loading

**Discovery Phase (YAML Frontmatter Only):**
- Agent receives context package referencing skills
- Agent loads skill YAML frontmatter (~100 tokens each)
- Agent determines skill relevance based on name and description
- Context efficient: 98% savings vs. loading full content

**Invocation Phase (Instructions):**
- Agent loads SKILL.md when skill needed (~2,000-5,000 tokens)
- Workflow steps guide agent execution
- Progressive loading: Only instructions, not resources yet
- Context efficient: 85% savings vs. monolithic approach

**Execution Phase (Resources):**
- Agent accesses specific resources on-demand
- Templates: ~500-1,000 tokens
- Examples: ~500-2,000 tokens
- Documentation: ~1,000-3,000 tokens
- Selective loading: 60-80% savings based on needs

**Maximum Context (All Resources):**
- Full skill with all resources: ~4,000-8,000 tokens
- Still 59-70% savings vs. embedding in agent definition
- Scalable: Unlimited skills without context bloat

---

**Skills Catalog Status:** ✅ **COMPLETE**

**Next Actions:**
1. Create SKILL.md files for each skill following specifications
2. Develop metadata.json for discovery
3. Organize resources/ directories with templates, examples, documentation
4. Integrate skill references into agent definitions (Iteration 4)
5. Validate progressive loading and context efficiency
