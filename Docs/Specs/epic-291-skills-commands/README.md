# Epic #291: Agent Skills & Slash Commands Integration

**Last Updated:** 2025-10-25
**Epic Status:** Specification → Ready for Implementation
**Epic Owner:** PromptEngineer (Skills/Commands development), DocumentationMaintainer (Docs reorganization)

> **Parent:** [`Specs`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Comprehensive integration of Claude Code's agent skills and slash commands capabilities into the zarichney-api project's 12-agent orchestration framework, combined with strategic documentation reorganization to establish `/Docs/` as the authoritative source of truth.

* **Key Objectives:**
  - **Context Efficiency:** Achieve 62% average reduction in agent definition context load through progressive skill loading
  - **Developer Productivity:** Streamline workflow operations through intuitive slash commands (GitHub issue creation, CI/CD monitoring, coverage analytics)
  - **Documentation Authority:** Reorganize `/Docs/` directory to serve as single source of truth with vendor-agnostic strategy
  - **Team Scalability:** Establish meta-skills framework enabling unlimited agent, skill, and command creation

* **Success Criteria:**
  - ✅ All 11 agents refactored with 60%+ context reduction while preserving effectiveness
  - ✅ CLAUDE.md optimized with 25%+ reduction, focusing on orchestration authority
  - ✅ All 5 core skills operational and adopted by agents
  - ✅ All 3 meta-skills enable PromptEngineer scalability
  - ✅ All 4 priority workflow commands functional and integrated
  - ✅ Priority 1 documentation complete (7 new guides + 4 standards updates)
  - ✅ All quality gates passing with no agent effectiveness regression
  - ✅ Comprehensive validation demonstrates ~9,864 tokens saved per session

* **Why it exists:** Current agent definitions contain extensive redundancy (~3,230 lines across 11 agents), documentation grounding protocols are duplicated, and workflow operations require manual context gathering. This epic addresses scalability constraints while establishing `/Docs/` as the authoritative knowledge repository, optimizing Claude Code performance without vendor lock-in.

### Component Specifications

* **Component A:** [`Core Skills`](./core-skills.md) - 5 foundational skills eliminating cross-cutting pattern redundancy
* **Component B:** [`Meta-Skills`](./meta-skills.md) - 3 meta-skills for scalable agent/skill/command creation
* **Component C:** [`Workflow Commands`](./workflow-commands.md) - 4 priority slash commands for developer productivity
* **Component D:** [`Documentation Reorganization`](./documentation-plan.md) - 7 new guides + standards updates
* **Component E:** [`Agent Refactoring`](./agent-refactoring.md) - 11 agents optimized with skill references

## 2. Architecture & Key Concepts

* **High-Level Design:** Progressive loading architecture where agents reference lightweight skill metadata (~100 tokens) for discovery, load full skill instructions (~2,500-5,000 tokens) on-demand, and access resources (templates, examples, documentation) only when explicitly needed. Slash commands provide CLI-style interfaces that delegate to skills for implementation logic.

* **Core Implementation Flow:**
  1. **Agent Engagement:** Claude provides context package referencing relevant skills
  2. **Skill Discovery:** Agent loads skill metadata to identify applicable capabilities
  3. **Progressive Loading:** Agent loads full skill instructions when invoked
  4. **Resource Access:** Agent accesses templates/examples/docs as needed during execution
  5. **Artifact Generation:** Agent creates skill-templated artifacts following standard protocols
  6. **Working Directory Communication:** Immediate artifact reporting per mandatory protocols

* **Key Architectural Decisions:**
  - **Decision 1: Progressive Loading Over Monolithic Definitions**
    - **Rationale:** Agent definitions containing 316-550 lines create context window pressure; extracting to skills with metadata-driven discovery achieves 62% average reduction while maintaining full capabilities through on-demand loading
    - **Implications:** All agents must adopt skill discovery patterns; PromptEngineer owns skill lifecycle; metadata schema standardization critical

  - **Decision 2: Docs as Source of Truth with Acceptable CLAUDE.md Duplication**
    - **Rationale:** Vendor-agnostic strategy requires `/Docs/` as authoritative knowledge repository; CLAUDE.md may contain orchestration-optimized duplication with clear links to Docs authority
    - **Implications:** DocumentationMaintainer owns all README.md files; PromptEngineer owns `.claude/` directory optimization; cross-references mandatory

  - **Decision 3: Command-Skill Separation of Concerns**
    - **Rationale:** Commands handle CLI interface (argument parsing, UX, error messaging); skills contain implementation logic (workflows, business rules, resources); clean boundary enables reusability
    - **Implications:** Commands are lightweight wrappers; skills are comprehensive capability bundles; integration pattern standardized

  - **Decision 4: Incremental Iteration Over Week-Based Planning**
    - **Rationale:** User requirement for flexible progression without rigid timelines; allows adaptation based on implementation learnings
    - **Implications:** Deliverables organized by priority and component; success measured by completion not duration

  - **Decision 5: GitHub Workflow Prompts Selective Integration**
    - **Rationale:** AI Sentinels in GitHub cloud environments don't require wholesale slash command duplication; only "precheck" value justifies local execution
    - **Implications:** Defer AI Sentinel precheck commands; focus workflow commands on CI/CD monitoring, coverage analytics, epic orchestration

* **Integration Points:**
  - **Agent Orchestration:** Skills referenced in Claude's context packages for agent engagement
  - **Working Directory:** Skill-generated artifacts follow mandatory communication protocols
  - **Quality Gates:** Skills respect ComplianceOfficer validation and AI Sentinel analysis
  - **CI/CD Workflows:** Commands integrate with existing GitHub Actions automation
  - **Documentation Grounding:** Systematic standards loading becomes core skill pattern

* **Architecture Diagram:**
  ```mermaid
  graph TD
      A[Agent Engagement] -->|Context Package| B{Skill Discovery};
      B -->|Metadata ~100 tokens| C[Relevant Skills?];
      C -->|Yes| D[Load Instructions ~2.5k tokens];
      C -->|No| E[Direct Execution];
      D --> F{Resources Needed?};
      F -->|Templates| G[Load Templates];
      F -->|Examples| H[Load Examples];
      F -->|Documentation| I[Load Deep Guides];
      F -->|None| J[Execute Workflow];
      G --> J;
      H --> J;
      I --> J;
      J --> K[Generate Artifact];
      K --> L[Working Directory Communication];

      M[User Invokes Command] --> N[Command Interface];
      N -->|Parse Arguments| O{Validate Input};
      O -->|Valid| P[Delegate to Skill];
      O -->|Invalid| Q[Error Message];
      P --> D;

      style B fill:#90EE90;
      style D fill:#87CEEB;
      style J fill:#FFB6C1;
      style L fill:#FFD700;
  ```

## 3. Interface Contract & Assumptions

* **Key Epic Deliverables:**

  - **Core Skills (5 foundational capabilities):**
    * **Purpose:** Eliminate cross-cutting pattern redundancy across all 11 agents
    * **Dependencies:** None - these are foundational patterns
    * **Outputs:** Standardized team coordination, documentation grounding, GitHub workflows, mission focus, authority management
    * **Quality Gates:** All 11 agents successfully adopt skills; 60%+ context reduction validated; no effectiveness regression

  - **Meta-Skills (3 scalability enablers):**
    * **Purpose:** Enable PromptEngineer to create agents, skills, commands efficiently and consistently
    * **Dependencies:** Core skills must be operational first
    * **Outputs:** Agent creation framework, skill creation framework, command creation framework
    * **Quality Gates:** PromptEngineer can create new agent in 50% less time; consistent structure; quality standards enforced

  - **Workflow Commands (4 developer productivity tools):**
    * **Purpose:** Streamline common development operations (CI/CD monitoring, coverage analytics, GitHub automation, epic consolidation)
    * **Dependencies:** Underlying skills and GitHub CLI integration
    * **Outputs:** /workflow-status, /coverage-report, /create-issue, /merge-coverage-prs functional commands
    * **Quality Gates:** Commands execute successfully; clear UX with helpful errors; workflow integration validated

  - **Documentation Reorganization (7 new guides + updates):**
    * **Purpose:** Establish /Docs/ as authoritative source of truth for all project knowledge
    * **Dependencies:** Understanding of skill/command architecture
    * **Outputs:** SkillsDevelopmentGuide.md, CommandsDevelopmentGuide.md, DocumentationGroundingProtocols.md, ContextManagementGuide.md, AgentOrchestrationGuide.md, templates, schemas
    * **Quality Gates:** Documentation enables agent/skill/command creation without external clarification; cross-references comprehensive; navigation <5 min

  - **Agent Refactoring (11 agents optimized):**
    * **Purpose:** Reduce agent definition context load by 62% average while preserving all capabilities
    * **Dependencies:** Core skills operational; documentation grounding protocols defined
    * **Outputs:** All 11 agents refactored (5,210 → 1,980 lines total)
    * **Quality Gates:** Agent effectiveness preserved 100%; orchestration integration validated; progressive loading functional

  - **CLAUDE.md Optimization (29% reduction):**
    * **Purpose:** Focus CLAUDE.md on orchestration authority with Docs references for implementation details
    * **Dependencies:** Documentation reorganization complete
    * **Outputs:** CLAUDE.md refactored (673 → 475 lines)
    * **Quality Gates:** Orchestration logic preserved 100%; skill references integrated; Docs cross-references comprehensive

* **Critical Assumptions:**
  - **Technical Assumptions:**
    - Progressive loading mechanism in Claude Code functions as architected
    - Skill metadata discovery supports filtering by category, agents, tags
    - On-demand resource loading doesn't introduce significant latency
    - Working directory communication protocols continue functioning
    - Quality gates (ComplianceOfficer, AI Sentinels) compatible with skill architecture

  - **Resource Assumptions:**
    - PromptEngineer has capacity for comprehensive .claude/ optimization
    - DocumentationMaintainer has capacity for Priority 1 documentation
    - All agents available for iterative refactoring validation
    - User available for incremental feedback and prioritization decisions

  - **External Dependencies:**
    - Claude Code CLI slash command registration mechanism understood
    - GitHub CLI (gh) available for command execution
    - Existing Scripts/*.sh continue functioning
    - AI Sentinel prompts remain in GitHub cloud environment
    - No breaking changes to agent orchestration framework during epic

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Epic-Specific Standards:**

  - **Skill Structure Convention** (per [official Claude Code docs](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/)):
    ```
    .claude/skills/category/skill-name/
    ├── SKILL.md                 # YAML frontmatter + instructions (<500 lines recommended)
    └── resources/               # Optional: On-demand progressive disclosure
        ├── templates/
        ├── examples/
        ├── documentation/
        └── scripts/
    ```

  - **Skill YAML Frontmatter** (required at top of SKILL.md):
    ```yaml
    ---
    name: skill-name
    description: Brief description of what this skill does and when to use it (max 1024 chars)
    ---
    ```
    **See:** [Official Skills Structure Specification](./official-skills-structure.md) for complete requirements

  - **Command Structure Convention (Markdown with Frontmatter):**
    ```markdown
    ---
    description: "Brief command purpose (one sentence)"
    argument-hint: "[required] [optional]"
    category: "testing|security|architecture|workflow"
    ---

    # Command Name
    [Usage examples, arguments, outputs, integration]
    ```

  - **Agent Skill Reference Pattern:**
    ```markdown
    ## [Capability] Implementation
    **SKILL REFERENCE**: `.claude/skills/category/skill-name/`

    [2-3 line summary of skill purpose]

    Key Workflow: [Step 1 | Step 2 | Step 3]

    [See skill for complete instructions]
    ```

  - **Documentation Cross-Reference Pattern:**
    ```markdown
    ## Related Documentation

    **Prerequisites:** [Standards files required for context]
    **Integration Points:** [Related development guides]
    **Orchestration Context:** [CLAUDE.md sections, agent files]
    ```

* **Technology Constraints:**
  - **Claude Code Compatibility:** Skills and commands must work with Claude Code CLI (current version)
  - **Platform Support:** Commands must function on Windows, macOS, Linux (cross-platform bash or PowerShell Core)
  - **Token Budget:** Skill metadata <150 tokens, SKILL.md 2,000-5,000 tokens, resources variable
  - **JSON Schema Validation:** All metadata.json files must pass schema validation

* **Timeline Constraints:**
  - **Incremental Iterations:** No rigid week-based schedule; priorities guide progression
  - **Validation Checkpoints:** Each component complete before dependent components begin
  - **Go/No-Go Gates:** Milestone acceptance criteria must pass before next phase
  - **User Feedback Loops:** Iterative refinement based on implementation learnings

## 5. How to Work With This Epic

* **Implementation Approach:**

  1. **Iteration 1: Foundation (Core Skills + Templates)**
     - Create working-directory-coordination, documentation-grounding, core-issue-focus, github-issue-creation skills
     - Develop SkillTemplate.md, CommandTemplate.md, metadata schemas
     - Validate progressive loading and agent integration patterns
     - **Success Criteria:** Skills can be created following templates; agent adoption successful

  2. **Iteration 2: Meta-Skills & Commands (Scalability Framework)**
     - Create agent-creation, skill-creation, command-creation meta-skills
     - Implement /workflow-status, /coverage-report, /create-issue, /merge-coverage-prs commands
     - Validate PromptEngineer efficiency improvements
     - **Success Criteria:** PromptEngineer creates new agent 50% faster; commands functional

  3. **Iteration 3: Documentation Alignment (Docs as Source of Truth)**
     - Create SkillsDevelopmentGuide.md, CommandsDevelopmentGuide.md, DocumentationGroundingProtocols.md
     - Update CodingStandards.md, TestingStandards.md, DocumentationStandards.md, TaskManagementStandards.md
     - Establish comprehensive cross-references
     - **Success Criteria:** Documentation enables autonomous creation; navigation <5 min

  4. **Iteration 4: Agent Refactoring (Incremental Context Optimization)**
     - Refactor 11 agents progressively (largest first for maximum savings)
     - Extract redundant patterns to skills with appropriate references
     - Validate agent effectiveness preservation and orchestration integration
     - **Success Criteria:** 60%+ context reduction; no effectiveness regression; all agents load skills

  5. **Iteration 5: Integration & Validation (Quality Assurance)**
     - Optimize CLAUDE.md with Docs references
     - Comprehensive integration testing across all components
     - Performance validation (token savings, loading latency)
     - Team training and documentation finalization
     - **Success Criteria:** All quality gates pass; performance targets met; team proficiency validated

* **Quality Assurance:**
  - **Testing Strategy:**
    - Unit testing: Skill metadata validation, command argument parsing
    - Integration testing: Agent skill loading, command execution, workflow coordination
    - Validation testing: Context efficiency measurement, progressive loading verification
    - Regression testing: Agent effectiveness preservation, orchestration compatibility

  - **Validation Approach:**
    - Milestone acceptance criteria: Each iteration has specific go/no-go decision points
    - Quantitative metrics: Token savings, context reduction percentages, success rates
    - Qualitative assessment: Agent usability, documentation clarity, workflow efficiency
    - User feedback integration: Iterative refinement based on implementation learnings

  - **Performance Validation:**
    - Context window savings: Target >8,000 tokens per typical multi-agent session
    - Agent definition reduction: Target 20-30% minimum, 62% average achieved
    - CLAUDE.md reduction: Target 25-30%, 29% projection validated
    - Progressive loading efficiency: Metadata discovery <150 tokens overhead

* **Common Implementation Pitfalls:**
  - **Pitfall 1: Over-extraction to Skills**
    - **Issue:** Removing essential agent identity content reduces coherence
    - **Mitigation:** Preserve core role, authority, mission in agent definition (130-240 lines); extract only deep technical patterns and redundant protocols

  - **Pitfall 2: Skill Discovery Fragmentation**
    - **Issue:** Agents unsure which skills to use when
    - **Mitigation:** Clear skill catalog with metadata-driven targeting; agent definitions reference specific skills; orchestration provides skill recommendations

  - **Pitfall 3: Breaking Orchestration Integration**
    - **Issue:** Agent refactoring disrupts Claude's coordination patterns
    - **Mitigation:** Incremental phased rollout; comprehensive testing after each agent refactor; maintain agent handoff patterns; preserve working directory protocols

  - **Pitfall 4: Documentation Navigation Gaps**
    - **Issue:** Moving content between Docs and CLAUDE.md breaks references
    - **Mitigation:** Create new Docs content first, then update CLAUDE.md references; validate all links; maintain deprecation notices in legacy locations

  - **Pitfall 5: Command Registration Confusion**
    - **Issue:** Command files exist but aren't executable (validation gap identified)
    - **Mitigation:** Phase 1 focuses on skills (production-ready); Phase 2 addresses command registration with Claude Code; document alternative execution patterns

## 6. Dependencies

* **Internal Code Dependencies:**
  - [`/.claude/agents/`](../../../.claude/agents/README.md) - All 11 agent definitions require refactoring for skill integration
  - [`CLAUDE.md`](../../../CLAUDE.md) - Orchestration guide optimization with Docs references
  - [` Scripts/`](../../../Scripts/) - Existing automation scripts leveraged by workflow commands
  - [`.github/workflows/`](../../../.github/workflows/) - CI/CD workflows referenced by commands and skills
  - [`.github/prompts/`](../../../.github/prompts/) - AI Sentinel prompts used by CI/CD skills

* **Internal Documentation Dependencies:**
  - [Documentation Standards](../../Standards/DocumentationStandards.md) - Skill metadata and resource organization requirements
  - [Testing Standards](../../Standards/TestingStandards.md) - Skills/commands testing patterns
  - [Task Management Standards](../../Standards/TaskManagementStandards.md) - Automated GitHub issue creation workflows
  - [Coding Standards](../../Standards/CodingStandards.md) - Code documentation requirements (minimal updates)

* **External Dependencies:**
  - **Claude Code CLI:** Skills and commands functionality dependent on Claude Code progressive loading mechanism
  - **GitHub CLI (gh):** Commands require gh CLI for workflow triggers, PR management, artifact retrieval
  - **Git:** Branch management and repository operations for commands
  - **.NET SDK & Tools:** API client regeneration commands require dotnet CLI and refitter
  - **Bash/PowerShell Core:** Cross-platform scripting for command execution

* **Dependent Features/Components:**
  - **Future Agent Creation:** New agents will reference core skills from inception (agent-creation meta-skill)
  - **Future Skill Development:** New skills follow skill-creation meta-skill framework
  - **Future Command Development:** New commands follow command-creation meta-skill framework
  - **Continuous Documentation:** Ongoing Docs updates leverage new guide structure
  - **Coverage Excellence Initiative:** Epic integration benefits from workflow commands and CI/CD skills

## 7. Rationale & Key Historical Context

* **Strategic Context:** This epic was prioritized to address scalability constraints in the multi-agent orchestration system. As the 11-agent team matured, agent definitions grew to 316-550 lines with ~3,230 lines of total redundancy. Context window pressure limited agent capability expansion, and workflow operations required extensive manual effort. User requirement for vendor-agnostic strategy drove `/Docs/` as source of truth while allowing Claude Code optimization.

* **Historical Evolution:**
  - **Initial Planning (2025-10-25):** ArchitecturalAnalyst provided comprehensive skills/commands analysis (42KB artifact)
  - **CI/CD Assessment:** WorkflowEngineer delivered workflow integration plan (38KB artifact) with 13 skills catalog
  - **Documentation Strategy:** DocumentationMaintainer created reorganization plan (44KB artifact) with Priority 1-3 roadmap
  - **Prompt Optimization:** PromptEngineer analyzed .claude/ directory producing 62% context reduction strategy (71KB artifact)
  - **Validation:** TestEngineer confirmed skills architecture production-ready; commands require Phase 2 integration
  - **Scope Refinement:** User clarified incremental iterations (not week-based), holistic documentation view, Docs as source of truth, PromptEngineer .claude/ ownership

* **Architectural Decision Records:**
  - **ADR 1: Progressive Loading Over Monolithic Definitions** - Metadata-driven discovery with on-demand resource access achieves 62% average context reduction while preserving full agent capabilities
  - **ADR 2: Command-Skill Separation** - Commands handle CLI interface; skills contain implementation logic; clean boundary enables reusability and testing
  - **ADR 3: Docs as Authority with Acceptable Duplication** - Vendor-agnostic `/Docs/` source of truth; CLAUDE.md may optimize for orchestration with clear cross-references
  - **ADR 4: Defer AI Sentinel Precheck Commands** - GitHub cloud AI Sentinels don't justify local precheck duplication; focus commands on CI/CD monitoring and workflow automation

* **Alternative Approaches Considered:**
  - **Option 1: Complete Agent Rewrite** - Rejected due to high risk of breaking orchestration and losing institutional knowledge
  - **Option 2: Monolithic Skill Library** - Rejected due to context window pressure; progressive loading provides better scalability
  - **Option 3: Week-Based Milestones** - Rejected per user requirement for incremental iterations with flexible progression
  - **Option 4: Wholesale GitHub Workflow Command Duplication** - Rejected due to marginal value; AI Sentinels effective in cloud environment

## 8. Known Issues & TODOs

* **Outstanding Design Decisions:**
  - [ ] **Command Registration Mechanism:** Investigate Claude Code command palette integration for slash command execution by subagents (Phase 2 requirement identified by TestEngineer validation)
  - [ ] **Skill Versioning Strategy:** Define semantic versioning approach for skills with backward compatibility requirements
  - [ ] **Cross-Skill Dependencies:** Design pattern for skills that depend on other skills (composition, inheritance)
  - [ ] **Resource Caching:** Evaluate if frequently-used resources should be cached for performance vs. context efficiency tradeoff

* **Implementation Risks:**
  - **P0 Risks:**
    - **Risk:** Breaking changes to agent orchestration during refactoring
      - **Mitigation:** Incremental phased rollout; comprehensive testing after each agent; maintain coordination patterns
    - **Risk:** Skill source trust and security
      - **Mitigation:** PromptEngineer exclusive ownership; no external skill imports; security audit for bash scripts
    - **Risk:** Command injection via arguments
      - **Mitigation:** Input validation; whitelist approach; SecurityAuditor review; avoid dynamic command construction

  - **P1 Risks:**
    - **Risk:** Context window management overhead from skill metadata
      - **Mitigation:** Progressive loading validation; <150 token metadata budget; monitor actual token usage
    - **Risk:** Agent capability fragmentation across skills
      - **Mitigation:** Clear skill categorization; agent definitions maintain core identity; skills complement not replace
    - **Risk:** Skill discovery confusion for agents
      - **Mitigation:** Metadata-driven targeting; clear skill catalog; orchestration skill recommendations

  - **P2 Risks:**
    - **Risk:** Skill loading latency impacting agent response time
      - **Mitigation:** Optimize skill file sizes; cache frequently used skills; preload critical skills
    - **Risk:** Resource file bloat in skill directories
      - **Mitigation:** Resource size limits per skill; external linking for large references; quarterly audit

* **Future Enhancements:**
  - **Phase 2 Enhancements (Post-Epic):**
    - Command registration integration with Claude Code command palette
    - Dynamic command discovery from `.claude/commands/` directory
    - Subagent command execution capability (if architecture supports)
    - AI Sentinel precheck commands for local analysis (if demand emerges)

  - **Potential Extensions:**
    - Cross-project skill sharing via skill marketplace
    - AI-assisted skill recommendations based on task context
    - Skill usage analytics and optimization insights
    - Advanced skill composition and workflow chaining
    - Session-based resource caching for performance

  - **Continuous Improvement:**
    - Quarterly skill audit and consolidation
    - Template refinement based on usage patterns
    - Documentation navigation enhancements
    - Agent feedback integration for skill usability

---

**Epic Specification Status:** ✅ **COMPLETE AND READY FOR IMPLEMENTATION**

**Next Actions:**
1. Review specification with user for approval and prioritization
2. Create GitHub epic issue linking to this specification
3. Begin Iteration 1: Foundation (Core Skills + Templates)
4. Coordinate with PromptEngineer for .claude/ optimization
5. Coordinate with DocumentationMaintainer for Priority 1 documentation

**Confidence Level:** High - comprehensive analysis across 5 specialist artifacts, validated architecture, measurable targets, clear acceptance criteria, incremental implementation approach minimizes risk.
