# Agent Orchestration Guide

**Last Updated:** 2025-10-26
**Purpose:** Comprehensive multi-agent coordination patterns, delegation protocols, and quality gate integration for zarichney-api development
**Target Audience:** Claude (strategic orchestration), all agents (coordination awareness), PromptEngineer (system optimization)

> **Parent:** [`Development`](./README.md)

---

## Table of Contents

1. [Purpose & Philosophy](#1-purpose--philosophy)
2. [Orchestration Architecture](#2-orchestration-architecture)
3. [Delegation Patterns](#3-delegation-patterns)
4. [Multi-Agent Coordination](#4-multi-agent-coordination)
5. [Working Directory Integration](#5-working-directory-integration)
6. [Quality Gate Protocols](#6-quality-gate-protocols)
7. [Emergency Protocols](#7-emergency-protocols)
8. [Best Practices](#8-best-practices)
9. [Examples](#9-examples)

---

## 1. Purpose & Philosophy

### Multi-Agent Coordination Principles

The zarichney-api project employs a sophisticated multi-agent development system where Claude operates as Strategic Codebase Manager orchestrating 11 specialized agents. This architecture delivers comprehensive development capabilities through disciplined delegation and coordination rather than monolithic execution.

**Delegation-Only Orchestration Model:**

Claude (Strategic Codebase Manager) **NEVER** performs specialized agent work. This is not a preference—it's an architectural imperative that ensures:

**Separation of Concerns:** Strategic planning separated from tactical execution. Claude analyzes GitHub issues, decomposes into agent-appropriate tasks, coordinates multi-agent workflows, integrates deliverables, and enforces quality gates. Agents execute specialized work within their expertise domains.

**Expertise Optimization:** Each agent masters their domain. CodeChanger focuses on production code excellence, TestEngineer on comprehensive coverage, DocumentationMaintainer on standards compliance, Specialists on domain-specific patterns. Deep expertise beats generalist execution.

**Scalable Coordination:** Unlimited agent capability expansion without architectural change. New agents integrate through same delegation protocols. Skills/commands enable cross-agent pattern sharing. Orchestration complexity scales linearly, not exponentially.

**Quality Assurance:** ComplianceOfficer pre-PR validation, AI Sentinels automated review, TestMaster coverage analysis, SecuritySentinel threat assessment. Multi-layer validation catches issues delegation-aware architecture prevents.

**Stateless Operation Support:** Comprehensive context packages for every agent engagement. Documentation grounding protocols ensure foundational knowledge. Working directory artifacts preserve context across stateless engagements. Progressive loading optimizes context window usage.

### Stateless Agent Engagement Patterns

Every agent operates with complete state amnesia between engagements. No memory of prior implementations, no inherent project knowledge, no context carryover. This reality drives orchestration design:

**Fresh Context Every Engagement:**

Claude constructs comprehensive context packages including:
- **Core Issue Definition:** Specific blocking technical problem requiring resolution
- **Agent Selection:** Primary agent with appropriate expertise and authority
- **Skill Integration:** Mandatory skills (always execute) + conditional skills (load if needed)
- **Standards References:** Relevant standards documents for documentation grounding
- **Module Context:** Specific README sections requiring review (especially Section 3 contracts)
- **Working Directory Discovery Mandate:** BEFORE starting work, check existing artifacts
- **Quality Gates:** Testing requirements, validation approach, acceptance criteria

**Documentation Grounding Mandatory:**

3-phase systematic loading before ANY agent modifications:
1. **Phase 1: Standards Mastery** → Load all 5 `/Docs/Standards/` files (CodingStandards.md, TestingStandards.md, DocumentationStandards.md, TaskManagementStandards.md, DiagrammingStandards.md)
2. **Phase 2: Project Architecture** → Load root README and module hierarchy relevant to task
3. **Phase 3: Domain-Specific** → Load target module README (all 8 sections), analyze Section 3 (Interface Contracts) thoroughly, review dependency module READMEs

See [DocumentationGroundingProtocols.md](./DocumentationGroundingProtocols.md) for complete grounding workflows.

**Progressive Loading Architecture:**

Skills-based progressive loading enables comprehensive capabilities within 200k token budget:
- **Discovery Phase:** Metadata scanning ~100 tokens per skill (98.6% savings)
- **Invocation Phase:** Complete SKILL.md ~2,500 tokens when skill needed
- **Resource Phase:** Templates/examples/docs loaded selectively on-demand

See [ContextManagementGuide.md](./ContextManagementGuide.md) for optimization strategies.

**Working Directory Context Preservation:**

Stateless agents cannot "remember," but systematic artifact management creates persistent context:
- **Pre-Work Discovery:** Agents check `/working-dir/` for existing team artifacts before starting
- **Immediate Reporting:** Agents report ANY file creation/update using standardized format
- **Context Integration:** Agents document how they build upon other agents' work
- **Session State:** Claude maintains evolving coordination plan across engagements

### Quality Gate Integration

Multi-agent orchestration includes comprehensive quality validation at multiple checkpoints:

**ComplianceOfficer Pre-PR Validation:**

Dual validation partnership before pull request creation:
- **Section-Level Validation:** Not per-subtask overhead—validates complete section work
- **Comprehensive Checks:** Standards compliance, test quality, documentation accuracy, integration coherence
- **Pre-PR Coordination:** Identifies issues before AI Sentinel review
- **Issue Requirement Verification:** Ensures deliverables match GitHub issue acceptance criteria

**AI Sentinels Automated Review (5 Sentinels):**

Sophisticated prompt engineering for automated PR analysis:
1. **DebtSentinel** (`tech-debt-analysis.md`): Technical debt analysis and sustainability assessment
2. **StandardsGuardian** (`standards-compliance.md`): Coding standards and architectural compliance
3. **TestMaster** (`testing-analysis.md`): Test coverage and quality analysis
4. **SecuritySentinel** (`security-analysis.md`): Security vulnerability and threat assessment
5. **MergeOrchestrator** (`merge-orchestrator-analysis.md`): Holistic PR analysis and final deployment decisions

**Advanced Sentinel Features:**
- **Expert Personas:** Principal-level expertise (15-20+ years) with AI coder mentorship
- **Context Ingestion:** Comprehensive project documentation analysis before evaluation
- **Chain-of-Thought Analysis:** 5-6 step structured reasoning process
- **Project-Specific Taxonomies:** Tailored to .NET 8/Angular 19 tech stack
- **Decision Matrices:** Objective prioritization and remediation frameworks
- **Educational Focus:** AI coder learning reinforcement and pattern guidance

**Activation Logic:**
- PR to `develop`: Testing + Standards + Tech Debt analysis
- PR to `main`: Full analysis including Security assessment
- Feature branches: Skip AI analysis for performance
- Quality gates: Critical findings can block deployment

**Coverage Excellence Tracking:**

Backend Testing Coverage Excellence Initiative:
- **TestEngineer:** Creates individual coverage PRs targeting comprehensive backend coverage
- **WorkflowEngineer:** Executes Coverage Excellence Merge Orchestrator for multi-PR consolidation (8+ PRs with AI conflict resolution)
- **Continuous Testing Excellence:** Progressive coverage advancement through systematic test creation
- **Quality Goals:** 100% executable test pass rate, comprehensive coverage targets

---

## 2. Orchestration Architecture

### 12-Agent Team Structure and Specializations

Claude orchestrates 11 specialized agents (12-agent ecosystem including Claude):

**File-Editing Agents (Primary Content Creators):**

**1. CodeChanger** - Production code implementation
- **Authority:** Direct modification of all source code files (.cs, .ts, .js, .json, config files)
- **Responsibilities:** Feature implementation, bug fixes, refactoring across backend and frontend
- **Primary Skills:** documentation-grounding, working-directory-coordination
- **Coordination:** Provides implementation context to TestEngineer, DocumentationMaintainer

**2. TestEngineer** - Comprehensive test coverage
- **Authority:** Direct modification of all test files (*Tests.cs, *.spec.ts, test configurations)
- **Responsibilities:** Unit tests, integration tests, test coverage advancement, quality assurance
- **Primary Skills:** documentation-grounding, working-directory-coordination
- **Coverage Excellence:** Individual PR creation for coverage improvements
- **Coordination:** Builds upon CodeChanger implementations, informs DocumentationMaintainer of testing strategies

**3. DocumentationMaintainer** - Standards-compliant documentation
- **Authority:** Direct modification of all documentation files (README.md, *.md, docs/)
- **Responsibilities:** README updates, standards compliance, architectural diagram maintenance
- **Primary Skills:** documentation-grounding, working-directory-coordination
- **Coordination:** Updates documentation based on CodeChanger/TestEngineer/Specialist changes

**4. PromptEngineer** - AI system optimization
- **Authority:** **EXCLUSIVE** direct modification of 28 prompt files across 3 locations:
  - `.claude/agents/*.md` (agent definitions)
  - `.github/prompts/*.md` (AI Sentinel prompts)
  - `Code/Zarichney.Server/Cookbook/Prompts/*.cs` (embedded prompts)
- **Responsibilities:** AI Sentinel optimization, inter-agent communication patterns, workflow prompts
- **Primary Skills:** skill-creation, command-creation, documentation-grounding
- **Coordination:** Optimizes all AI system components for team effectiveness

**Specialist Agents (Flexible Authority Framework):**

**5. ComplianceOfficer** - Pre-PR validation and quality gates
- **Authority:** Advisory through working directory artifacts, direct coordination with Claude
- **Responsibilities:** Quality gates, standards verification, comprehensive validation, dual validation partnership
- **Engagement Pattern:** Section-level validation (not per-subtask overhead)
- **Coordination:** Works through working directory artifacts, validates all agent deliverables

**6. FrontendSpecialist** - Angular 19 expertise
- **Flexible Authority:**
  - **Query Intent:** Analysis and recommendations via working directory (advisory mode)
  - **Command Intent:** Direct modification of frontend files (.ts, .html, .css, .scss)
- **Intent Recognition:** Distinguishes "Analyze [issue]" from "Implement [improvement]"
- **Expertise:** Component design patterns, state architecture (NgRx), API integration, Material Design
- **Coordination:** Backend API contract alignment, frontend testing strategy, UI/UX documentation

**7. BackendSpecialist** - .NET 8 expertise
- **Flexible Authority:**
  - **Query Intent:** Analysis and recommendations via working directory (advisory mode)
  - **Command Intent:** Direct modification of backend files (.cs, configs, migrations)
- **Intent Recognition:** Distinguishes "Review [architecture]" from "Refactor [service]"
- **Expertise:** API architecture, service layer design, database schema, EF Core patterns, DI patterns
- **Coordination:** Frontend API contract alignment, backend testing strategy, architectural documentation

**8. SecurityAuditor** - Security hardening
- **Flexible Authority:**
  - **Query Intent:** Security analysis via working directory (advisory mode)
  - **Command Intent:** Direct modification for vulnerability fixes (security configs, implementations)
- **Intent Recognition:** Distinguishes "Audit [endpoint]" from "Fix [vulnerability]"
- **Expertise:** OWASP compliance, authentication implementation, threat analysis, security patterns
- **Coordination:** Security testing requirements, security documentation, proactive threat analysis

**9. WorkflowEngineer** - CI/CD automation
- **Flexible Authority:**
  - **Query Intent:** CI/CD analysis via working directory (advisory mode)
  - **Command Intent:** Direct modification of workflows, Scripts/*, build configs
- **Intent Recognition:** Distinguishes "Analyze [pipeline]" from "Optimize [workflow]"
- **Expertise:** GitHub Actions, pipeline optimization, build automation, Coverage Excellence Merge Orchestrator
- **Coverage Excellence Orchestrator:** Multi-PR consolidation with AI conflict resolution (8+ PR batches)
- **Coordination:** Test execution integration, automation documentation, developer experience

**10. BugInvestigator** - Root cause analysis
- **Authority:** Advisory through working directory artifacts (diagnostic reporting)
- **Responsibilities:** Performance bottlenecks, error interpretation, reproduction analysis, systematic debugging
- **Coordination:** Provides diagnostic context to CodeChanger, TestEngineer for bug fixes

**11. ArchitecturalAnalyst** - System design decisions
- **Authority:** Advisory through working directory artifacts (architectural guidance)
- **Responsibilities:** Design patterns evaluation, performance analysis, structural assessment, technical debt
- **Coordination:** Provides architectural context for all agents, influences design decisions

### Agent Authority Framework

Three-tier authority structure enables flexible coordination:

**Tier 1: File-Editing Authority (Direct Modification)**

Agents with direct file modification rights:
- **CodeChanger:** All source code files (.cs, .ts, .js, .json, configs)
- **TestEngineer:** All test files (*Tests.cs, *.spec.ts, test configs)
- **DocumentationMaintainer:** All documentation files (README.md, *.md, docs/)
- **PromptEngineer:** All 28 prompt files (agents, sentinels, embedded)

**Benefits:**
- Clear ownership boundaries prevent coordination conflicts
- Direct authority enables autonomous execution within domain
- No handoff overhead for routine modifications
- Accountability for quality within expertise area

**Tier 2: Flexible Authority (Intent-Based Implementation)**

Specialists with conditional implementation capability:
- **FrontendSpecialist:** Frontend files (.ts, .html, .css, .scss)
- **BackendSpecialist:** Backend files (.cs, configs, migrations)
- **SecurityAuditor:** Security configs, vulnerability fixes
- **WorkflowEngineer:** Workflows, Scripts/*, build configs

**Intent Recognition Patterns:**

**Query Intent (Advisory Mode):**
- Keywords: "Analyze," "Review," "Assess," "Evaluate," "Examine," "Identify," "Find," "Detect"
- Action: Working directory artifacts with recommendations
- Authority: Advisory only, no direct file modifications
- Example: "Analyze RecipeService performance bottlenecks" → BackendSpecialist creates analysis artifact

**Command Intent (Implementation Mode):**
- Keywords: "Fix," "Implement," "Update," "Create," "Build," "Add," "Optimize," "Enhance," "Improve," "Refactor"
- Action: Direct file modifications within expertise domain
- Authority: Full implementation rights within domain boundaries
- Example: "Implement caching for RecipeService" → BackendSpecialist directly modifies RecipeService.cs

**Benefits:**
- Reduces handoff overhead (40-60% reduction vs. always advisory)
- Enables autonomous development cycles (analysis→implementation without re-engagement)
- Preserves quality gates (AI Sentinels, ComplianceOfficer validation still required)
- Maintains accountability (specialists own implementations within domain)

**Tier 3: Advisory Authority (Working Directory Only)**

Agents providing analysis without direct modification:
- **ComplianceOfficer:** Quality validation via artifacts
- **BugInvestigator:** Diagnostic analysis via artifacts
- **ArchitecturalAnalyst:** Design recommendations via artifacts

**Benefits:**
- Deep analysis without modification risk
- Comprehensive evaluation across all agent work
- Preserves separation between analysis and implementation
- Enables informed decision-making before modifications

### Flexible Authority with Intent Recognition

Intent recognition optimizes coordination efficiency while preserving quality:

**Intent Determination Process:**

```yaml
Request Analysis:
  1. Parse task description for intent keywords
  2. Evaluate context: Question vs. directive
  3. Determine authority tier for target files
  4. Match intent with agent capability

Query Intent Indicators:
  - "What/How/Why questions about existing code"
  - "Analyze/Review/Assess/Evaluate/Examine" patterns
  - "Identify/Find/Detect issues or patterns"
  - Investigation focus without directive to modify

Command Intent Indicators:
  - "Fix/Implement/Update/Create/Build/Add" directives
  - "Optimize/Enhance/Improve/Refactor existing code"
  - "Apply/Execute recommendations"
  - Implementation focus with clear modification directive

Ambiguous Intent Resolution:
  - Default to query intent (safer—analysis before action)
  - Claude clarifies with agent if uncertainty
  - Agent can request implementation authority if query reveals clear fix
```

**Domain Boundary Validation:**

Specialists operate within expertise boundaries:

```yaml
BackendSpecialist Domain Boundaries:
  Authorized: *.cs, appsettings.json, appsettings.*.json, migrations/*, *.csproj
  Requires Coordination: Frontend contracts (FrontendSpecialist integration)
  Forbidden: .github/workflows/*.yml (WorkflowEngineer), test files (TestEngineer)

FrontendSpecialist Domain Boundaries:
  Authorized: *.ts, *.html, *.css, *.scss, angular.json, package.json, tsconfig.json
  Requires Coordination: Backend API contracts (BackendSpecialist integration)
  Forbidden: Backend *.cs files (BackendSpecialist), test files (TestEngineer)

WorkflowEngineer Domain Boundaries:
  Authorized: .github/workflows/*.yml, Scripts/*, GitHub Actions configs
  Requires Coordination: Test execution patterns (TestEngineer)
  Forbidden: Application code (CodeChanger/Specialists), tests (TestEngineer)

SecurityAuditor Domain Boundaries:
  Authorized: Security configs, authentication implementations, vulnerability fixes
  Requires Coordination: All security-relevant implementations
  Forbidden: Core business logic without security implications
```

**Cross-Domain Coordination:**

When implementations span multiple specialist domains:

```yaml
Backend-Frontend API Coordination:
  Scenario: New REST endpoint with corresponding frontend service
  Pattern:
    - BackendSpecialist: Implements API endpoint (.cs files)
    - FrontendSpecialist: Implements API service (.ts files)
    - Coordination: Both specialists review shared API contract specifications
    - Quality Gate: Integration tests validate contract alignment

Security-Implementation Coordination:
  Scenario: Authentication flow touching multiple layers
  Pattern:
    - SecurityAuditor: Defines authentication requirements and security patterns
    - BackendSpecialist: Implements backend authentication logic
    - FrontendSpecialist: Implements frontend authentication UI
    - Coordination: SecurityAuditor validates all implementations
    - Quality Gate: Security testing and OWASP compliance validation
```

### Working Directory Communication Protocols

Mandatory communication standards ensure team awareness across all agent engagements:

**Protocol 1: Pre-Work Artifact Discovery (MANDATORY)**

**BEFORE starting ANY task**, agents MUST check existing artifacts:

```
🔍 WORKING DIRECTORY DISCOVERY:
- Current artifacts reviewed: [list existing files checked]
- Relevant context found: [artifacts that inform current work]
- Integration opportunities: [how existing work will be built upon]
- Potential conflicts: [any overlapping concerns identified]
```

**Why Mandatory:**
- Prevents duplicate analysis (agent 2 reusing agent 1's work)
- Enables context-aware decisions (building upon, not replacing)
- Identifies integration dependencies before starting
- Maintains team coordination visibility

**Protocol 2: Immediate Artifact Reporting (MANDATORY)**

**WHEN creating/updating ANY working directory file**, agents MUST immediately report:

```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: [exact-filename-with-extension]
- Purpose: [brief description of content and intended consumers]
- Context for Team: [what other agents need to know about this artifact]
- Dependencies: [what other artifacts this builds upon or relates to]
- Next Actions: [any follow-up coordination needed]
```

**Why Mandatory:**
- Real-time team awareness (Claude knows what artifacts exist)
- Clear handoff communication (agents know what to review)
- Dependency tracking (artifacts building upon each other documented)
- Prevents communication gaps in stateless environment

**Protocol 3: Context Integration Reporting (REQUIRED)**

**WHEN building upon other agents' work**, agents MUST document integration:

```
🔗 ARTIFACT INTEGRATION:
- Source artifacts used: [specific files that informed this work]
- Integration approach: [how existing context was incorporated]
- Value addition: [what new insights or progress this provides]
- Handoff preparation: [context prepared for future agents]
```

**Why Required:**
- Explicit integration audit trail (how work builds upon prior efforts)
- Value demonstration (what each agent contributes beyond prior work)
- Context preservation for stateless operation
- Enables Claude to track coordination effectiveness

**Claude's Communication Enforcement Role:**

As orchestrator, Claude verifies communication compliance:
- **Verify Compliance:** Confirm each agent reports artifacts using standardized formats
- **Enforce Discovery:** Ensure agents check existing artifacts before starting work
- **Track Continuity:** Monitor how artifacts build upon each other across engagements
- **Prevent Communication Gaps:** Intervene when agents miss reporting requirements
- **Maintain Team Awareness:** Keep comprehensive view of all working directory developments

**Communication Failure Recovery:**

When agents fail to follow protocols:
1. **Immediate Intervention:** Stop agent engagement and request proper communication
2. **Protocol Clarification:** Re-emphasize communication requirements
3. **Compliance Verification:** Confirm understanding before continuing
4. **Pattern Monitoring:** Watch for recurring communication failures
5. **Escalation:** Report persistent communication issues to user

---

## 3. Delegation Patterns

### Iterative Adaptive Planning Process

Claude's orchestration follows adaptive planning that evolves with agent discoveries:

**10-Step Iterative Process:**

**Step 1: Mission Understanding**
- Analyze GitHub issue requirements and project impact
- Extract acceptance criteria, technical constraints, epic context
- Identify scope (single module vs. multi-module, low-risk vs. critical)
- Understand organizational priorities (coverage excellence, architectural coherence)

**Step 2: Context Ingestion**
- Load relevant standards for issue understanding (CodingStandards.md, TestingStandards.md)
- Review module READMEs for architectural context
- Check Epic specifications if issue part of larger initiative
- Validate current codebase state and existing implementations

**Step 3: Next Step Planning**
- Determine immediate next agent engagement based on current state
- NOT complete task decomposition—just next step
- Consider agent availability, expertise match, authority boundaries
- Identify mandatory skills and conditional skills for agent engagement

**Step 4: Agent Engagement**
- Construct comprehensive context package (see Enhanced Context Package Template)
- Include artifact discovery mandate, skill references, quality gates
- Delegate single step with clear core issue definition
- Enable agent autonomy within defined scope

**Step 5: Working Directory Communication Verification**
- Ensure agent reports artifacts created/discovered per protocols
- Verify standardized communication format usage
- Confirm artifact integration reporting when building upon others
- Track artifact developments for subsequent agent engagements

**Step 6: Output Review & Plan Adaptation**
- Analyze agent completion report and recommendations
- Extract insights that modify coordination strategy
- Identify integration dependencies discovered during execution
- Determine if next agent engagement needed or current agent re-engagement

**Step 7: Progress Assessment**
- Evaluate if same agent needs re-engagement for incremental work
- Determine if different agent required for next phase
- Consider if blocking issues emerged requiring course correction
- Assess progress toward GitHub issue objectives

**Step 8: Iteration Continuation**
- Repeat steps 3-7 until GitHub issue objectives achieved
- Adapt plan based on agent discoveries and recommendations
- Support multiple engagements of same agent type for complex incremental work
- Maintain coordination state in working directory session tracking

**Step 9: Quality Coordination**
- Ensure tests pass (100% executable test pass rate)
- Coordinate documentation updates for contract changes
- Validate working directory artifacts properly managed
- Execute ComplianceOfficer section-level validation

**Step 10: Final Assembly**
- Integrate multiple agent deliverables into coherent changes
- Create pull request with comprehensive descriptions
- Commit with conventional messages including co-authorship
- Trigger AI Sentinel automated review

**Why Iterative:**
- Agents discover integration dependencies during execution
- Architectural insights emerge from specialist implementations
- Testing reveals edge cases requiring additional coverage
- Documentation updates identify missing interface contracts
- Rigid upfront planning cannot anticipate emergent complexity

**Adaptive Planning Philosophy:**
Success measured by adaptive coordination that evolves with agent discoveries, not adherence to initial decomposition.

### Enhanced Context Package Template (MANDATORY)

Claude's primary orchestration tool for effective delegation:

```yaml
## CORE ISSUE FIRST PROTOCOL (MANDATORY):
CORE_ISSUE: "[Specific blocking technical problem - be precise]"
INTENT_RECOGNITION: "[Query-intent vs. Command-intent determination]"
TARGET_FILES: "[Exact files that need modification]"
AGENT_SELECTION: "[Specialist vs. Primary agent based on intent and expertise]"
FLEXIBLE_AUTHORITY: "[Specialist implementation capability within domain boundaries]"
MINIMAL_SCOPE: "[Surgical changes needed to fix core issue]"
SUCCESS_TEST: "[How to verify the fix actually works]"

## MISSION OBJECTIVE:
Mission Objective: [Focused task with clear acceptance criteria]
GitHub Issue Context: [Issue #, epic progression status, organizational priorities]
Technical Constraints: [Standards adherence, performance requirements, architectural boundaries]

## INTENT RECOGNITION PATTERNS:
Query_Intent_Indicators:
  - "Analyze/Review/Assess/Evaluate/Examine"
  - "What/How/Why questions about existing code"
  - "Identify/Find/Detect issues or patterns"
  Action: Working directory artifacts (advisory mode)

Command_Intent_Indicators:
  - "Fix/Implement/Update/Create/Build/Add"
  - "Optimize/Enhance/Improve/Refactor existing code"
  - "Apply/Execute recommendations"
  Action: Direct file modifications within expertise domain

## FLEXIBLE AUTHORITY BOUNDARIES:
- BackendSpecialist: *.cs, config/*.json, config/*.yaml, migrations/*
- FrontendSpecialist: *.ts, *.html, *.css, config/frontend/*.json, config/frontend/*.yaml
- WorkflowEngineer: .github/workflows/*, Scripts/*, config/build/*.json, config/build/*.yaml
- SecurityAuditor: config/security/*.json, config/security/*.yaml, vulnerability fixes
- All Specialists: *.md, docs/*.md, README.md (technical documentation within domain)

## FORBIDDEN SCOPE EXPANSIONS:
- Infrastructure improvements while core issue unfixed
- Working directory protocols during syntax error fixes
- Feature additions not directly related to core problem
- Cross-agent coordination enhancements during single-issue fixes
- Cross-domain implementations outside specialist expertise

## MANDATORY SKILLS (Execute Before Work):
documentation-grounding:
  - Phase 1: [Specific standards requiring review]
  - Phase 2: [Relevant module hierarchy]
  - Phase 3: [Target module README sections, especially Section 3]

working-directory-coordination:
  - Pre-Work Artifact Discovery: Check /working-dir/ for existing [relevant] analysis
  - Immediate Artifact Reporting: Report [implementation decisions/analysis results]
  - Context Integration: Build upon [prior agent] deliverables

## CONDITIONAL SKILLS (Load If Needed):
[skill-name]:
  - TRIGGER: [Specific condition indicating skill needed]
  - FOCUS: [Particular aspects of skill to emphasize]

## WORKING DIRECTORY DISCOVERY (MANDATORY):
- Check existing artifacts before starting work
- Report discovered artifacts using standardized format
- Build upon rather than duplicate existing team analysis

## WORKING DIRECTORY COMMUNICATION (REQUIRED):
- Report any artifacts created immediately using standard format
- Document integration when building upon other agents' work

## INTEGRATION REQUIREMENTS:
[How this coordinates with current system state]

## STANDARDS CONTEXT:
[Relevant standards documents and key sections]

## MODULE CONTEXT:
[Local README.md files and architectural patterns to review]
- Target Module README: [Path]
  - Section 2 (Architecture): [Specific patterns to follow]
  - Section 3 (Interface Contract): [CRITICAL - preconditions, postconditions, error handling]
  - Section 5 (How to Work): [Testing strategy, known pitfalls]
  - Section 6 (Dependencies): [Integration points requiring awareness]

## QUALITY GATES:
- [ ] documentation-grounding protocols followed
- [ ] working-directory-coordination artifact reporting completed
- [ ] [Standard-specific] compliance validated
- [ ] [Testing requirements for this agent type]
```

**Context Package Usage:**
- Claude constructs package for EVERY agent engagement
- No shortcuts—comprehensive context prevents failure
- Skill references enable progressive loading (not embedded full content)
- Working directory protocols ensure team awareness
- Quality gates establish validation criteria upfront

### Standardized Agent Reporting Format

Agents MUST provide structured completion reports enabling Claude to understand deliverables and coordinate next steps:

```yaml
🎯 [AGENT] COMPLETION REPORT

Status: [COMPLETE/IN_PROGRESS/BLOCKED] ✅
Epic Contribution: [Coverage progression/Feature milestone/Bug resolution]

## Working Directory Artifacts Communication:
[MANDATORY REPORTING - List any artifacts created/discovered using standard format]

🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: [exact-filename]
- Purpose: [content description]
- Context for Team: [what other agents need to know]
- Next Actions: [follow-up coordination]

## [Agent-Specific Deliverables]:
[Core work products: files modified, tests created, documentation updated]

## Team Integration Handoffs:
  📋 TestEngineer: [Testing requirements and scenarios identified]
  📖 DocumentationMaintainer: [Documentation updates needed for changes]
  🔒 SecurityAuditor: [Security considerations requiring attention]
  🏗️ Specialists: [Architectural considerations for domain experts]

## Team Coordination Status:
  - Integration conflicts: [None/Specific issues identified]
  - Cross-agent dependencies: [Dependencies requiring coordination]
  - Urgent coordination needs: [Immediate attention required from Claude or other agents]

## AI Sentinel Readiness: [READY/NEEDS_REVIEW] ✅
[Assessment of deliverables' readiness for AI Sentinel automated review]

## Next Team Actions: [Specific follow-up tasks for Claude coordination]
```

**Why Standardized Format:**
- **Structured Parsing:** Claude extracts deliverables systematically
- **Handoff Clarity:** Explicit integration points for subsequent agents
- **Quality Signaling:** AI Sentinel readiness indicates validation confidence
- **Coordination Efficiency:** Next actions guide adaptive planning
- **Artifact Visibility:** Working directory communication embedded in report

### Mission-Focused Result Processing

Claude validates agent deliverables using systematic immediate validation sequence:

**IMMEDIATE VALIDATION SEQUENCE:**

**1. CORE ISSUE STATUS CHECK:**
"Did the agent fix the specific blocking problem?"
- Review core issue from context package
- Compare agent deliverables against success test criteria
- Determine: RESOLVED / PARTIAL / UNRESOLVED

**2. INTENT COMPLIANCE VERIFICATION:**
"Did agent respond appropriately to query vs. command intent?"
- Verify query intent → working directory artifacts (no direct modifications)
- Verify command intent → direct file modifications within domain boundaries
- Check for intent misinterpretation (analysis when implementation requested, vice versa)

**3. AUTHORITY COMPLIANCE VERIFICATION:**
"Did agent stay within flexible authority boundaries for their domain?"
- Verify file modifications within authorized domain (BackendSpecialist → *.cs, FrontendSpecialist → *.ts)
- Check for authority violations (specialist modifying outside domain)
- Confirm cross-domain coordination when implementations span boundaries

**4. FUNCTIONALITY TESTING:**
"Can I verify the core issue is actually resolved?"
- Execute success test specified in context package
- Run test suite if testing-related changes
- Validate behavior matches interface contract expectations

**5. MISSION DRIFT DETECTION:**
"Did agent expand scope beyond core issue resolution?"
- Compare deliverables against minimal scope definition
- Identify unauthorized scope expansions (infrastructure while bug unfixed)
- Detect forbidden scope expansions from context package

**Enhanced Reporting Template:**

```
[AGENT_NAME] Core Issue Status: [RESOLVED/PARTIAL/UNRESOLVED]

## Core Issue Resolution:
- Problem: [Original blocking technical issue from context package]
- Intent Recognition: [QUERY_INTENT/COMMAND_INTENT - how agent interpreted request]
- Action Taken: [ANALYSIS_ONLY/DIRECT_IMPLEMENTATION - based on intent]
- Files Modified: [Exact files and changes made, or "Working directory artifacts only"]
- Fix Status: [COMPLETE/INCOMPLETE/OFF-SCOPE]
- Testing: [How to verify fix works - execution of success test]

## Compliance Validation:
- Flexible Authority Compliance: [COMPLIANT/VIOLATION - within domain boundaries]
- Intent Recognition Accuracy: [ACCURATE/MISINTERPRETED - correct analysis vs. implementation mode]
- Scope Compliance: [COMPLIANT/VIOLATION - specify if violation occurred]
- Mission Drift: [NONE/DETECTED - specify if scope expanded beyond core issue]

## Next Action Decision:
[CORE_ISSUE_RESOLVED / REQUIRES_REFOCUS / NEEDS_DIFFERENT_AGENT / NEEDS_IMPLEMENTATION_FOLLOW_UP]
```

**Course Correction Protocol:**

```yaml
If core issue UNRESOLVED:
  Action: Re-engage same agent with focused scope
  Context Package: Emphasize core issue, remove distractions
  Communication: "Previous attempt did not resolve core issue. Focus on [specific problem]."

If scope VIOLATION detected:
  Action: Acknowledge violation and redirect to core issue
  Context Package: Surgical scope definition, forbidden expansions emphasized
  Communication: "Detected scope expansion. Return to core issue: [specific problem]."

If mission DRIFT occurred:
  Action: Reset to core issue focus before continuing
  Context Package: Core issue first protocol, minimal scope
  Communication: "Mission drift detected. Prioritize core blocking issue over enhancements."

NO next agent engagement until core issue STATUS = RESOLVED
```

**Why Mission-Focused:**
- Prevents scope creep consuming coordination capacity
- Ensures agents solve stated problems, not adjacent concerns
- Validates deliverables against explicit success criteria
- Maintains focus on organizational priorities (coverage excellence, bug resolution)

---

## 4. Multi-Agent Coordination

### Cross-Agent Workflow Orchestration

Claude coordinates sophisticated multi-agent workflows for complex implementations:

**Backend-Frontend Harmony (API Contract Co-Design):**

```yaml
Scenario: New Recipe filtering endpoint with Angular frontend integration

Phase 1: Architectural Design
  Agent: ArchitecturalAnalyst
  Intent: QUERY
  Deliverable: /working-dir/recipe-filtering-architecture.md
  Content: Component breakdown, data flow design, API contract specification

Phase 2: Backend Implementation
  Agent: BackendSpecialist
  Intent: COMMAND
  Pre-Work Discovery: Reviews recipe-filtering-architecture.md
  Deliverable: RecipeController.cs with filtering endpoint
  Working Directory: /working-dir/backend-implementation-notes.md (interface contract details)

Phase 3: Frontend Implementation
  Agent: FrontendSpecialist
  Intent: COMMAND
  Pre-Work Discovery: Reviews backend-implementation-notes.md for API contract
  Deliverable: recipe-filter.service.ts, recipe-filter.component.ts
  Working Directory: /working-dir/frontend-implementation-notes.md

Phase 4: Integration Testing
  Agent: TestEngineer
  Pre-Work Discovery: Reviews all 3 working directory artifacts
  Deliverable: Integration tests validating API contract adherence
  Working Directory: /working-dir/integration-test-strategy.md

Phase 5: Documentation
  Agent: DocumentationMaintainer
  Context Integration: Synthesizes architecture + backend + frontend + testing artifacts
  Deliverable: Updated READMEs with interface contracts, testing strategies
```

**Benefits:**
- API contract defined architecturally before implementation (prevents misalignment)
- Working directory artifacts enable cross-agent context sharing
- Integration testing validates contract adherence
- Documentation preserves complete design rationale

**Quality Assurance Integration (TestEngineer Coordination with All Agents):**

```yaml
Coverage Excellence Tracking:
  Pattern: Direct contribution to comprehensive backend coverage through continuous testing excellence

  TestEngineer Individual PR Creation:
    - Creates coverage improvements for specific services/components
    - Each PR targets incremental coverage advancement
    - Follows TestingStandards.md AAA pattern, categorization
    - Continuous testing excellence toward comprehensive backend coverage goals

  Coverage Excellence Orchestrator Integration:
    - WorkflowEngineer executes Coverage Excellence Merge Orchestrator
    - Multi-PR consolidation (8+ coverage PRs with AI conflict resolution)
    - Flexible label matching: "type: coverage", "coverage", "testing"
    - Automated consolidation reduces PR review overhead

Testable Architecture:
  - All architectural decisions facilitate comprehensive testing
  - ArchitecturalAnalyst considers testability in design recommendations
  - BackendSpecialist/FrontendSpecialist design for dependency injection (mocking)
  - CodeChanger implements testable patterns (constructor DI, interface abstractions)

Coverage Validation:
  - Integration with `/test-report` commands and AI-powered analysis
  - TestMaster AI Sentinel validates test quality in PRs
  - ComplianceOfficer ensures coverage standards compliance
  - Continuous progression toward comprehensive coverage goals
```

**Security Throughout (SecurityAuditor Integration with All Workflows):**

```yaml
Defense-in-Depth Coordination:
  Pattern: Security patterns across all agent implementations

  Proactive Security Analysis:
    Agent: SecurityAuditor
    Intent: QUERY (typically)
    Engagement: Reviews ArchitecturalAnalyst designs, CodeChanger implementations
    Deliverable: /working-dir/security-analysis.md with threat assessment

  Security Implementation:
    Agent: SecurityAuditor
    Intent: COMMAND (when vulnerabilities identified)
    Authority: Security configs, authentication implementations, vulnerability fixes
    Coordination: BackendSpecialist/FrontendSpecialist implement recommended patterns

  Security Testing:
    Agent: TestEngineer
    Context: Builds upon SecurityAuditor analysis
    Deliverable: Security-specific test scenarios (authentication, authorization, input validation)

  Security Documentation:
    Agent: DocumentationMaintainer
    Context: Documents security patterns, authentication flows, threat mitigations
    Deliverable: README Section 3 (Interface Contract) includes security preconditions
```

### Handoff Protocols Between Agents

Standardized handoff ensures seamless context transfer:

**Handoff Protocol Template:**

```markdown
## Context Handoff: [Agent 1] → [Agent 2]

**Completed Work:**
- [Specific deliverable 1: files modified, features implemented]
- [Specific deliverable 2: working directory artifact created with context]

**Integration Points for [Agent 2]:**
- [Specific artifact to review: path and key sections]
- [Key decisions to build upon: architectural patterns, interface contracts]
- [Constraints or assumptions to respect: preconditions, performance targets]

**Recommended Next Actions:**
- [Specific task for Agent 2: clear, actionable]
- [Standards sections requiring review: CodingStandards.md Section X]
- [Module READMEs to load: target + dependencies]
```

**Concrete Example (BackendSpecialist → TestEngineer):**

```markdown
## Context Handoff: BackendSpecialist → TestEngineer

**Completed Work:**
- Implemented UserService.GetUserByIdAsync endpoint with repository pattern
- Created /working-dir/user-service-implementation-notes.md with interface contract

**Integration Points for TestEngineer:**
- Review implementation-notes.md Section "Interface Contract" for:
  - Preconditions: userId > 0 validation (ArgumentOutOfRangeException if violated)
  - Postconditions: User entity with eager-loaded relationships, null for not found
  - Error handling: ArgumentOutOfRangeException (userId <= 0), OperationCanceledException (token cancellation)
- Implementation uses IUserRepository mock for unit testing
- Async pattern with CancellationToken parameter enables cancellation testing

**Recommended Next Actions:**
- Create comprehensive unit tests covering:
  - Happy path: Valid userId returns User with relationships
  - Precondition violations: userId <= 0 throws ArgumentOutOfRangeException
  - Not found scenario: Non-existent userId returns null (NOT exception)
  - Cancellation scenario: Token cancellation throws OperationCanceledException
  - Repository integration: Verify IUserRepository.GetByIdAsync called correctly
- Follow TestingStandards.md AAA pattern, Category=Unit trait
- Document test strategy in /working-dir/user-service-test-strategy.md for DocumentationMaintainer
```

**Handoff Benefits:**
- **Explicit Context:** Next agent receives clear guidance on what to review
- **Integration Focus:** Highlights specific decisions requiring respect/integration
- **Action Items:** Provides concrete next steps reducing ambiguity
- **Standards References:** Points to relevant standards sections for grounding
- **Artifact Discovery:** Ensures next agent knows which artifacts to load

### Shared Context Management

Working directory serves as central artifact repository for cross-agent coordination:

**Session State Tracking:**

```markdown
# /working-dir/session-state.md

**Epic:** #291 - Agent Skills & Commands Integration
**Current Iteration:** Iteration 3.3 - Context Management & Orchestration Guides
**Status:** In Progress
**Branch:** section/iteration-3

## Completed Work:
- ✅ Issue #303: SkillsDevelopmentGuide.md and CommandsDevelopmentGuide.md (DocumentationMaintainer)
- ✅ Issue #302: DocumentationGroundingProtocols.md migration (DocumentationMaintainer)
- ✅ Issue #301 Subtask 1: ContextManagementGuide.md (DocumentationMaintainer)

## Current Work:
- 🔄 Issue #301 Subtask 2: AgentOrchestrationGuide.md (DocumentationMaintainer - in progress)

## Next Actions:
- Pending: CLAUDE.md update with cross-references to new guides
- Pending: Issue #300: Standards Updates (4 files)
- Pending: Issue #299: Templates & JSON Schemas

## Integration Dependencies:
- ContextManagementGuide.md → AgentOrchestrationGuide.md (orchestration context optimization)
- AgentOrchestrationGuide.md → CLAUDE.md (content extraction with references)
- All guides → Templates (practical application examples)
```

**Agent Artifact Sharing:**

```markdown
# /working-dir/backend-specialist-recipe-filtering-analysis.md

**Agent:** BackendSpecialist
**Task:** Analyze RecipeService filtering implementation requirements
**Created:** 2025-10-26
**Intent:** QUERY (analysis mode)

## Analysis Results:

### Current Implementation Limitations:
- RecipeService.GetRecipesAsync returns all recipes (no filtering)
- No support for ingredient-based filtering
- No support for dietary restriction filtering
- No pagination (performance concern for large datasets)

### Recommended Implementation Approach:

**1. Repository Pattern Enhancement:**
- Add IRecipeRepository.GetFilteredRecipesAsync(RecipeFilterDto filter, CancellationToken cancellationToken)
- FilterDto properties: List<string> Ingredients, List<string> DietaryRestrictions, int PageSize, int PageNumber

**2. EF Core Query Optimization:**
- Use .Where() with ingredient matching (IQueryable composition)
- Apply dietary restrictions filter
- Implement .Skip() and .Take() for pagination
- Use .AsNoTracking() for read-only scenarios (performance)

**3. Interface Contract:**
- Preconditions: filter non-null, PageSize > 0 && PageSize <= 100, PageNumber >= 0
- Postconditions: Returns filtered recipes with pagination metadata
- Error handling: ArgumentNullException (null filter), ArgumentOutOfRangeException (invalid pagination)

### Architectural Considerations:
- RecipeFilterDto should be reusable across API and service layers
- Consider caching filtered results if queries expensive
- Ensure indexing on Ingredients and DietaryRestrictions columns for performance

## Next Agent Recommendations:
- **BackendSpecialist (Implementation):** Implement recommendations above
- **FrontendSpecialist:** Design filter UI component consuming new API
- **TestEngineer:** Create comprehensive tests for filtering logic (especially edge cases)
- **DocumentationMaintainer:** Update RecipeService README Section 3 with new contract
```

**Context Integration Pattern:**

```markdown
# /working-dir/backend-specialist-recipe-filtering-implementation.md

**Agent:** BackendSpecialist
**Task:** Implement RecipeService filtering logic
**Created:** 2025-10-26
**Intent:** COMMAND (implementation mode)

## Context Integration:
**Source Artifacts:**
- /working-dir/backend-specialist-recipe-filtering-analysis.md: Architectural recommendations

**Integration Approach:**
Implemented all recommendations from analysis:
- Created RecipeFilterDto with Ingredients, DietaryRestrictions, pagination properties
- Added IRecipeRepository.GetFilteredRecipesAsync with IQueryable composition
- Applied precondition validation (filter non-null, PageSize 1-100, PageNumber >= 0)
- Implemented .AsNoTracking() for read-only performance optimization
- Added database indexing migration for Ingredients and DietaryRestrictions

## Files Modified:
- Code/Zarichney.Server/DTOs/RecipeFilterDto.cs (CREATED)
- Code/Zarichney.Server/Services/RecipeService.cs (GetFilteredRecipesAsync added)
- Code/Zarichney.Server/Repositories/IRecipeRepository.cs (GetFilteredRecipesAsync signature)
- Code/Zarichney.Server/Repositories/RecipeRepository.cs (EF Core implementation)
- Code/Zarichney.Server/Data/Migrations/[timestamp]_AddRecipeFilteringIndexes.cs (CREATED)

## Interface Contract (for TestEngineer/DocumentationMaintainer):
**Preconditions:**
- filter non-null (throws ArgumentNullException if null)
- filter.PageSize > 0 && <= 100 (throws ArgumentOutOfRangeException otherwise)
- filter.PageNumber >= 0 (throws ArgumentOutOfRangeException if negative)

**Postconditions:**
- Returns PaginatedResult<Recipe> with filtered recipes
- PaginatedResult.Items contains recipes matching ALL filter criteria
- PaginatedResult.TotalCount reflects total matching records (not just page)
- PaginatedResult.PageSize and PageNumber match request

**Error Handling:**
- ArgumentNullException: filter is null
- ArgumentOutOfRangeException: PageSize or PageNumber invalid
- OperationCanceledException: CancellationToken requested cancellation

## Next Actions:
- **TestEngineer:** Create comprehensive tests covering preconditions, postconditions, filtering logic, pagination
- **DocumentationMaintainer:** Update RecipeService README Section 3 with GetFilteredRecipesAsync contract
```

**Shared Context Benefits:**
- **Working Directory as Central Hub:** All agents access `/working-dir/` for coordination context
- **Artifact Reuse:** BackendSpecialist implementation built directly on analysis (no duplication)
- **Explicit Integration:** Context integration reporting documents how work builds upon prior efforts
- **Stateless Operation Support:** Artifacts persist across engagements, preserving context for stateless agents

### Integration Dependencies

Claude tracks and coordinates cross-agent dependencies:

**Dependency Types:**

**Sequential Dependencies (Blocking):**
```yaml
Example: API Implementation → Integration Testing
- TestEngineer CANNOT create integration tests until BackendSpecialist implements API endpoint
- Dependency: TestEngineer blocked until BackendSpecialist completes
- Coordination: Claude ensures sequential engagement with handoff protocol
```

**Parallel Dependencies (Coordinated):**
```yaml
Example: Backend API + Frontend Service (shared contract)
- BackendSpecialist and FrontendSpecialist can work simultaneously IF contract pre-defined
- Dependency: Both specialists must adhere to same API contract specification
- Coordination: ArchitecturalAnalyst defines contract first; specialists implement in parallel
```

**Integration Dependencies (Validation Required):**
```yaml
Example: Multi-module changes requiring integration validation
- BackendSpecialist modifies API contract
- FrontendSpecialist updates API service to match
- TestEngineer creates integration tests validating contract alignment
- Dependency: Integration tests verify compatibility across changes
- Coordination: Claude ensures TestEngineer engagement after both specialist completions
```

**Dependency Tracking Mechanism:**

```markdown
# /working-dir/session-state.md (dependency section)

## Current Dependencies:
- TestEngineer: BLOCKED on BackendSpecialist completion of RecipeService.GetFilteredRecipesAsync
- FrontendSpecialist: CAN START (API contract defined in architecture artifact)
- DocumentationMaintainer: WAITING FOR TestEngineer test strategy completion

## Integration Checkpoints:
- [ ] BackendSpecialist implementation complete (RecipeService.cs)
- [ ] FrontendSpecialist implementation complete (recipe-filter.service.ts)
- [ ] TestEngineer integration tests validate contract (RecipeServiceIntegrationTests.cs)
- [ ] DocumentationMaintainer updates READMEs with contracts (RecipeService README Section 3)

## Coordination Status:
- Backend-Frontend coordination: CONTRACT DEFINED, implementations in progress
- Testing coordination: BLOCKED until implementations complete
- Documentation coordination: WAITING FOR test strategy completion
```

**Claude's Dependency Coordination:**
1. **Identify Dependencies:** Parse agent completion reports for "Next Team Actions" and blocking relationships
2. **Track State:** Maintain dependency graph in session state working directory artifact
3. **Sequence Engagements:** Engage agents in dependency-aware order (blockers first)
4. **Validate Integration:** Ensure integration checkpoints validated before considering feature complete
5. **Parallel Optimization:** Enable parallel work when dependencies allow (contracts pre-defined)

---

## 5. Working Directory Integration

### Artifact Discovery Mandate (BEFORE WORK)

**MANDATORY PROTOCOL:** All agents MUST check existing `/working-dir/` contents before starting any work.

**Artifact Discovery Process:**

```yaml
Step 1: List Working Directory Contents
  Command: ls /working-dir/
  Purpose: Identify all existing artifacts

Step 2: Filter Relevant Artifacts
  Review: Filenames for relevance to current task
  Keywords: Module names, feature names, analysis/implementation/test/documentation

Step 3: Load Relevant Artifacts
  Action: Read 2-5 most relevant artifacts
  Focus: Architectural decisions, implementation notes, test strategies

Step 4: Report Discovery
  Format: Standardized discovery report (see below)
  Content: Artifacts reviewed, relevant context found, integration opportunities
```

**Standardized Discovery Report:**

```
🔍 WORKING DIRECTORY DISCOVERY:
- Current artifacts reviewed: [session-state.md, backend-specialist-recipe-analysis.md, test-engineer-recipe-strategy.md]
- Relevant context found: [Recipe filtering architecture defined, backend implementation complete, test strategy awaiting implementation]
- Integration opportunities: [Build upon test strategy for comprehensive coverage, coordinate with backend implementation contract]
- Potential conflicts: [None identified - clear progression from architecture → implementation → testing]
```

**Why Mandatory:**
- **Prevents Duplicate Work:** Agent discovers prior analysis, builds upon instead of repeating
- **Enables Context-Aware Decisions:** Agent understands what team already accomplished
- **Identifies Integration Dependencies:** Agent sees blocking relationships and coordination needs
- **Maintains Team Coordination:** All agents aware of collective progress

**Consequences of Skipping Discovery:**
- Duplicate analysis consuming coordination time
- Missing integration opportunities from prior agent work
- Unaware of blocking dependencies or constraints
- Communication gaps preventing effective coordination

### Immediate Artifact Reporting (DURING WORK)

**MANDATORY PROTOCOL:** When creating/updating ANY working directory file, agents MUST immediately report using standardized format.

**Artifact Reporting Trigger Events:**
- Creating new `/working-dir/` artifact
- Updating existing `/working-dir/` artifact
- Completing analysis deliverable
- Documenting implementation decisions
- Creating test strategy or diagnostic report

**Standardized Artifact Reporting Format:**

```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: [exact-filename-with-extension]
- Purpose: [brief description of content and intended consumers]
- Context for Team: [what other agents need to know about this artifact]
- Dependencies: [what other artifacts this builds upon or relates to]
- Next Actions: [any follow-up coordination needed]
```

**Concrete Example:**

```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: backend-specialist-user-authentication-implementation.md
- Purpose: Documents UserAuthenticationService implementation decisions, interface contract for JWT token generation, security patterns applied
- Context for Team: SecurityAuditor should review authentication flow for OWASP compliance; TestEngineer can design authentication tests from contract specifications; FrontendSpecialist needs to know token format and storage requirements
- Dependencies: Builds upon architectural-analyst-authentication-design.md (architectural recommendations), security-auditor-auth-requirements.md (security constraints)
- Next Actions: TestEngineer creates authentication tests, SecurityAuditor validates security patterns, DocumentationMaintainer updates UserAuthenticationService README Section 3
```

**Why Immediate Reporting:**
- **Real-Time Team Awareness:** Claude knows artifact exists immediately for coordination
- **Clear Handoff Communication:** Next agents know what artifacts to review
- **Dependency Tracking:** Artifacts building upon each other explicitly documented
- **Prevents Communication Gaps:** No silent artifact creation in stateless environment

**Reporting Timing:**
- **Immediately After Creation:** Don't batch—report each artifact when created
- **Before Agent Completion:** Include in final completion report for visibility
- **During Multi-File Work:** Report each artifact separately (multiple reports acceptable)

### Context Integration Reporting (BUILDING UPON)

**REQUIRED PROTOCOL:** When building upon other agents' work, agents MUST document integration explicitly.

**Integration Reporting Trigger Events:**
- Using prior agent's architectural analysis for implementation
- Building tests based on implementation contract from prior agent
- Updating documentation synthesizing multiple agent deliverables
- Coordinating implementations across specialists (backend + frontend)

**Standardized Integration Reporting Format:**

```
🔗 ARTIFACT INTEGRATION:
- Source artifacts used: [specific files that informed this work with key insights extracted]
- Integration approach: [how existing context was incorporated into current deliverable]
- Value addition: [what new insights or progress this provides beyond source artifacts]
- Handoff preparation: [context prepared for future agents building upon this work]
```

**Concrete Example:**

```
🔗 ARTIFACT INTEGRATION:
- Source artifacts used:
  - backend-specialist-recipe-filtering-analysis.md: Architecture recommendations for repository pattern enhancement, RecipeFilterDto design, pagination approach
  - architectural-analyst-recipe-filtering-design.md: Component breakdown and data flow design
- Integration approach:
  - Implemented all architectural recommendations from BackendSpecialist analysis
  - Applied RecipeFilterDto design with Ingredients, DietaryRestrictions, pagination properties
  - Used component structure from ArchitecturalAnalyst for service layer organization
  - Added database indexing migration per performance considerations from analysis
- Value addition:
  - Complete implementation of filtering logic following analysis recommendations
  - EF Core IQueryable composition for performance optimization
  - Comprehensive interface contract documentation for TestEngineer test design
  - Migration for database indexing enabling production-ready performance
- Handoff preparation:
  - Interface contract with preconditions, postconditions, error handling documented for TestEngineer
  - Implementation notes prepared for DocumentationMaintainer README updates
  - Performance considerations documented for future optimization reviews
```

**Why Integration Reporting:**
- **Explicit Integration Audit Trail:** Documents how work builds upon prior efforts
- **Value Demonstration:** Shows what each agent contributes beyond prior work
- **Context Preservation:** Maintains understanding across stateless agent engagements
- **Coordination Effectiveness:** Enables Claude to track multi-agent collaboration quality

**Integration Depth Levels:**

**Shallow Integration:**
- References prior artifact for basic context
- Minimal building upon prior work
- Example: "Reviewed architecture.md for general understanding"

**Deep Integration:**
- Implements specific recommendations from prior artifact
- Extends prior analysis with concrete implementation
- Example: "Implemented all 5 architectural recommendations with added database indexing"

**Synthesis Integration:**
- Combines multiple artifacts into coherent deliverable
- Resolves conflicts or gaps between artifacts
- Example: "Synthesized backend contract + frontend requirements + testing strategy into comprehensive API documentation"

### Session State Tracking

Claude maintains evolving session state documenting coordination progress:

**Session State Purpose:**
- **Big-Picture Context:** Epic/issue status, iteration tracking, branch strategy
- **Completed Work Tracking:** Agent engagements completed with deliverables
- **Current Work Status:** Active agent engagements in progress
- **Next Actions Queue:** Pending agent engagements and dependencies
- **Integration Dependencies:** Blocking relationships and coordination checkpoints
- **Artifact Catalog:** All working directory artifacts created during session

**Session State Structure:**

```markdown
# /working-dir/session-state.md

**Epic:** [Epic #, name]
**Current Iteration:** [Iteration X.Y - description]
**Status:** [In Progress / Blocked / Complete]
**Branch:** [current branch name]

---

## Completed Work:
- ✅ [Issue/Task]: [Deliverable summary] ([Agent])
- ✅ [Issue/Task]: [Deliverable summary] ([Agent])

## Current Work:
- 🔄 [Issue/Task]: [Current status] ([Agent] - in progress)

## Next Actions:
- Pending: [Issue/Task]: [What needs to happen]
- Pending: [Issue/Task]: [What needs to happen]

## Integration Dependencies:
- [Dependency description]: [Blocking relationship]
- [Coordination checkpoint]: [Validation requirement]

## Artifact Catalog:
- [artifact-name.md]: [Purpose] (Created: date)
- [artifact-name.md]: [Purpose] (Created: date)

## Coordination Notes:
[Any important context for understanding session progression]
```

**Session State Updates:**

**After Each Agent Engagement:**
- Move completed work from "Current Work" to "Completed Work"
- Add new agent engagement to "Current Work"
- Update "Next Actions" based on agent completion report recommendations
- Add new artifacts to "Artifact Catalog"
- Update "Integration Dependencies" if blocking relationships change

**Session State Benefits:**
- **Coordination Continuity:** Claude maintains comprehensive view across multiple agent engagements
- **Progress Transparency:** User can review session state for status updates
- **Dependency Awareness:** Blocking relationships visible for coordination planning
- **Artifact Discovery Aid:** Agents review artifact catalog for relevant context
- **Historical Context:** Session progression documented for retrospective analysis

---

## 6. Quality Gate Protocols

### ComplianceOfficer Pre-PR Validation

ComplianceOfficer provides dual validation partnership with Claude before pull request creation:

**Section-Level Validation (Not Per-Subtask):**

```yaml
Validation Scope: Complete section/iteration work
Timing: After all section tasks complete, before section PR creation
Overhead Justification: Comprehensive validation worth overhead at section level

Example (Epic #291 Iteration 3):
  Section Scope: Issues #303, #302, #301, #300, #299 (5 issues)
  Validation Timing: After all 5 issues complete
  ComplianceOfficer Engagement: Single comprehensive validation
  Benefits: Holistic section validation vs. 5 separate validations
```

**Comprehensive Pre-PR Checks:**

```yaml
Standards Compliance Verification:
  - CodingStandards.md: DI patterns, async/await, SOLID principles followed
  - TestingStandards.md: AAA pattern, categorization, coverage requirements met
  - DocumentationStandards.md: README 8-section template, linking integrity, Section 3 contracts complete
  - TaskManagementStandards.md: Branch naming, conventional commits, PR description requirements

Test Quality Validation:
  - 100% executable test pass rate (no failing tests)
  - Test categorization applied (Unit/Integration traits)
  - Comprehensive coverage for new implementations
  - Test isolation and determinism verified

Documentation Accuracy Verification:
  - README files updated for any contract changes
  - Section 3 (Interface Contract) matches actual implementations
  - Section 6 (Dependencies) links functional
  - Cross-references comprehensive and bidirectional

Integration Coherence Validation:
  - Multi-agent deliverables integrate coherently
  - No integration conflicts or gaps
  - Working directory artifacts properly documented
  - Team coordination visible through artifact communication

Issue Requirement Verification:
  - GitHub issue acceptance criteria met
  - Deliverables match issue specifications
  - All subtasks complete
  - No blocking dependencies unresolved
```

**ComplianceOfficer Deliverable:**

```markdown
# ComplianceOfficer Section Validation Report

**Section:** Iteration 3 - Documentation Alignment
**Issues Validated:** #303, #302, #301, #300, #299
**Validation Date:** 2025-10-26

## Standards Compliance: ✅ PASS
- CodingStandards.md: N/A (documentation-only section)
- TestingStandards.md: N/A (no test files modified)
- DocumentationStandards.md: All 5 guides follow 8-section template, cross-references comprehensive
- TaskManagementStandards.md: Branch section/iteration-3 correct, commits follow conventional format

## Test Quality: ✅ PASS
- No test files modified (documentation section)
- Existing test suite: 100% pass rate maintained

## Documentation Accuracy: ✅ PASS
- SkillsDevelopmentGuide.md: Comprehensive, cross-references functional
- CommandsDevelopmentGuide.md: Comprehensive, integration with skills clear
- DocumentationGroundingProtocols.md: Agent-specific patterns complete
- ContextManagementGuide.md: Progressive loading strategies quantified
- AgentOrchestrationGuide.md: Multi-agent coordination comprehensive
- CLAUDE.md: Updated with cross-references to new guides
- All cross-references bidirectional and functional

## Integration Coherence: ✅ PASS
- Documentation guides integrate coherently
- Progression: Skills → Commands → Grounding → Context → Orchestration logical
- CLAUDE.md content successfully extracted with references preserved
- No integration gaps identified

## Issue Requirement Verification: ✅ PASS
- Issue #303: SkillsDevelopmentGuide + CommandsDevelopmentGuide complete
- Issue #302: DocumentationGroundingProtocols.md complete
- Issue #301: ContextManagementGuide.md + AgentOrchestrationGuide.md complete
- Issue #300: Standards updates (pending - not validated yet)
- Issue #299: Templates (pending - not validated yet)

## Overall Section Validation: ✅ READY FOR PR
**Recommendation:** Section ready for pull request creation
**AI Sentinel Readiness:** HIGH - comprehensive documentation with clear standards compliance
```

**Dual Validation Partnership:**
- **Claude's Validation:** Iterative agent deliverable validation during coordination
- **ComplianceOfficer Validation:** Comprehensive section-level validation before PR
- **Benefit:** Two-layer validation catches issues Claude's iterative focus might miss
- **Timing:** ComplianceOfficer validates complete section work, not in-progress states

### AI Sentinels Integration (5 Sentinels)

Five AI Sentinels automatically analyze pull requests using sophisticated prompt engineering:

**1. DebtSentinel (tech-debt-analysis.md):**

**Purpose:** Technical debt analysis and sustainability assessment

**Chain-of-Thought Analysis:**
1. **Context Ingestion:** Load project documentation, standards, architectural patterns
2. **Debt Identification:** Analyze PR for technical debt introduction (hardcoded values, duplication, complexity)
3. **Impact Assessment:** Evaluate maintenance burden, scalability concerns, future refactoring needs
4. **Priority Classification:** Critical/High/Medium/Low based on impact and effort matrices
5. **Remediation Guidance:** Specific recommendations with implementation approaches
6. **Learning Reinforcement:** Educational context for AI coder pattern recognition

**Expert Persona:** Principal Software Engineer (20+ years), technical debt reduction specialist

**2. StandardsGuardian (standards-compliance.md):**

**Purpose:** Coding standards and architectural compliance

**Chain-of-Thought Analysis:**
1. **Context Ingestion:** Load CodingStandards.md, TestingStandards.md, DocumentationStandards.md
2. **Pattern Verification:** Check DI usage, async patterns, naming conventions, SOLID principles
3. **Architectural Alignment:** Verify repository pattern, service layer separation, module boundaries
4. **Documentation Compliance:** Validate README 8-section template, Section 3 contracts
5. **Violation Reporting:** Specific violations with standards section references
6. **Educational Guidance:** Pattern explanations and best practice reinforcement

**Expert Persona:** Principal Architect (15+ years), standards enforcement and pattern consistency

**3. TestMaster (testing-analysis.md):**

**Purpose:** Test coverage and quality analysis

**Chain-of-Thought Analysis:**
1. **Context Ingestion:** Load TestingStandards.md, coverage goals, testing strategies
2. **Coverage Assessment:** Analyze test coverage for new code (comprehensive vs. gaps)
3. **Test Quality Verification:** AAA pattern adherence, test isolation, determinism
4. **Scenario Completeness:** Check edge cases, error conditions, integration points
5. **Quality Scoring:** Test quality metrics and coverage percentage analysis
6. **Improvement Recommendations:** Specific test scenarios needing coverage, quality enhancements

**Expert Persona:** Principal QA Engineer (18+ years), test coverage and quality excellence

**4. SecuritySentinel (security-analysis.md):**

**Purpose:** Security vulnerability and threat assessment

**Activation:** PRs to `main` branch (production deployments)

**Chain-of-Thought Analysis:**
1. **Context Ingestion:** Load security documentation, OWASP guidelines, authentication patterns
2. **Threat Identification:** Analyze PR for security vulnerabilities (injection, XSS, auth issues)
3. **OWASP Mapping:** Map identified threats to OWASP Top 10 categories
4. **Risk Assessment:** Critical/High/Medium/Low based on exploitability and impact
5. **Remediation Guidance:** Specific fixes with secure coding patterns
6. **Security Pattern Education:** Explain security principles and best practices

**Expert Persona:** Principal Security Engineer (20+ years), OWASP specialist, threat modeling expert

**5. MergeOrchestrator (merge-orchestrator-analysis.md):**

**Purpose:** Holistic PR analysis and final deployment decision

**Chain-of-Thought Analysis:**
1. **Context Ingestion:** Synthesize all other Sentinel findings plus project context
2. **Holistic Assessment:** Evaluate PR against organizational priorities (coverage excellence, architectural coherence)
3. **Risk Analysis:** Weigh technical debt, standards violations, test quality, security concerns
4. **Deployment Decision:** APPROVE/REQUEST_CHANGES/BLOCK based on comprehensive evaluation
5. **Priority Balancing:** Consider trade-offs (coverage advancement vs. tech debt introduction)
6. **Strategic Guidance:** Recommendations aligning PR with long-term project goals

**Expert Persona:** Engineering Director (20+ years), strategic technical leadership, holistic system oversight

**Automatic Activation Logic:**

```yaml
PR_to_develop:
  Sentinels_Activated:
    - TestMaster (testing analysis)
    - StandardsGuardian (standards compliance)
    - DebtSentinel (tech debt analysis)
  Rationale: Development branch requires quality and maintainability focus

PR_to_main:
  Sentinels_Activated:
    - TestMaster (testing analysis)
    - StandardsGuardian (standards compliance)
    - DebtSentinel (tech debt analysis)
    - SecuritySentinel (security vulnerability assessment)
    - MergeOrchestrator (holistic deployment decision)
  Rationale: Production deployment requires comprehensive security and strategic validation

Feature_Branches:
  Sentinels_Activated: NONE
  Rationale: Performance optimization—feature branch work iterates rapidly without PR overhead
```

**Quality Gates:**

```yaml
Critical_Findings_Block_Deployment:
  TestMaster: 0% test coverage on new code, all tests failing
  StandardsGuardian: Multiple critical standards violations (DI anti-patterns, blocking sync calls)
  SecuritySentinel: Critical vulnerabilities (SQL injection, authentication bypass)
  MergeOrchestrator: BLOCK decision due to unacceptable risks

High_Priority_Findings_Request_Changes:
  DebtSentinel: Significant tech debt introduction without remediation plan
  TestMaster: Insufficient coverage (<80% for new code)
  StandardsGuardian: Multiple standards violations requiring correction

Medium/Low_Findings_Educational:
  All Sentinels: Educational guidance for improvement without blocking
  MergeOrchestrator: APPROVE with recommendations for future enhancements
```

### Coverage Excellence Tracking

Backend Testing Coverage Excellence Initiative drives comprehensive coverage through systematic coordination:

**TestEngineer Individual PR Creation:**

```yaml
Coverage_Advancement_Pattern:
  Agent: TestEngineer
  Task: "Create coverage improvements for UserService.UpdateEmailAsync"
  Deliverable: Individual PR with comprehensive tests

  Test Scenarios Created:
    - Happy path: Valid inputs update email successfully
    - Precondition violations: userId <= 0, null email, invalid format
    - Business rule violations: Duplicate email (already exists)
    - Postcondition validation: Database updated, audit logged
    - Error handling: ArgumentOutOfRangeException, ArgumentNullException, DuplicateEmailException

  PR Metadata:
    Branch: test/issue-XXX-user-service-email-update-coverage
    Labels: "type: coverage", "testing", "coverage-excellence"
    Target: continuous/testing-excellence branch
    Description: Comprehensive coverage for UserService.UpdateEmailAsync method
```

**WorkflowEngineer Coverage Excellence Merge Orchestrator:**

```yaml
Multi_PR_Consolidation_Pattern:
  Agent: WorkflowEngineer
  Task: "Execute Coverage Excellence Merge Orchestrator for 8+ coverage PRs"

  Orchestrator Capabilities:
    - Multi-PR consolidation: Merges 8+ individual coverage PRs into single consolidated PR
    - AI conflict resolution: Uses Claude Code to resolve merge conflicts intelligently
    - Flexible label matching: Discovers PRs with "type: coverage" OR "coverage" OR "testing" labels
    - Automated PR creation: Creates consolidated PR to develop branch
    - Quota-aware scheduling: Handles Claude Code quota windows gracefully (scheduled runs: skipped_quota_window classification preserved)

  Execution:
    Command: gh workflow run "Coverage Excellence Merge Orchestrator" --field dry_run=false --field max_prs=10 --field pr_label_filter="type: coverage,coverage,testing"
    Discovery: Finds all coverage PRs targeting continuous/testing-excellence
    Consolidation: Merges into single branch with AI conflict resolution
    PR Creation: Creates consolidated PR to develop with comprehensive description

  Benefits:
    - Reduces PR review overhead (1 consolidated PR vs. 8+ individual PRs)
    - Maintains granular commit history (individual commits preserved)
    - AI-powered conflict resolution (intelligent merge handling)
    - Coverage excellence acceleration (systematic advancement)
```

**Continuous Testing Excellence Progression:**

```yaml
Coverage_Goals:
  Current: Progressive backend coverage advancement through individual PR creation
  Target: Comprehensive backend test coverage (services, repositories, controllers)
  Quality: 100% executable test pass rate (no failing tests)

Integration with Quality Gates:
  - TestMaster AI Sentinel validates coverage in consolidated PRs
  - ComplianceOfficer validates section-level coverage progression
  - StandardsGuardian ensures test quality (AAA pattern, categorization)
  - MergeOrchestrator considers coverage advancement in deployment decisions

Organizational Priority:
  - Coverage excellence drives continuous improvement goals
  - Multi-agent coordination supports systematic coverage advancement
  - Automation (Merge Orchestrator) reduces coordination overhead
  - Quality gates ensure coverage advancement maintains quality standards
```

### Standards Compliance Enforcement

Multi-layer standards enforcement ensures consistent quality:

**Layer 1: Agent Self-Validation (During Work)**

Agents validate compliance before completing work:

```yaml
Documentation Grounding Validation:
  - Phase 1: Standards mastery complete (can explain DI, async, SOLID)
  - Phase 2: Architecture context understood (module hierarchy, integration points)
  - Phase 3: Domain-specific context loaded (Section 3 contracts thoroughly analyzed)

Working Directory Communication Validation:
  - Pre-work discovery executed (existing artifacts reviewed)
  - Immediate reporting performed (artifacts created reported)
  - Context integration documented (building upon prior work explicit)

Implementation Validation:
  - CodingStandards.md compliance (DI, async, naming conventions)
  - TestingStandards.md compliance (AAA pattern, categorization, coverage)
  - DocumentationStandards.md compliance (8-section template, Section 3 contracts)
```

**Layer 2: Claude Validation (During Coordination)**

Claude validates agent deliverables during orchestration:

```yaml
Mission-Focused Result Processing:
  - Core issue status check (RESOLVED/PARTIAL/UNRESOLVED)
  - Intent compliance verification (query vs. command intent followed)
  - Authority compliance verification (within domain boundaries)
  - Functionality testing (success test execution)
  - Mission drift detection (scope compliance)

Integration Coherence Validation:
  - Multi-agent deliverables integrate without conflicts
  - Cross-references functional and comprehensive
  - Working directory artifacts properly communicated
  - Quality gates preparation complete
```

**Layer 3: ComplianceOfficer Validation (Pre-PR)**

ComplianceOfficer provides comprehensive section-level validation:

```yaml
Standards Compliance Verification:
  - All 5 standards documents compliance validated
  - No critical violations unaddressed
  - Documentation accuracy confirmed
  - Integration coherence verified

Issue Requirement Verification:
  - GitHub issue acceptance criteria met
  - All deliverables match specifications
  - No blocking dependencies unresolved
  - Section work complete and ready for PR
```

**Layer 4: AI Sentinels Validation (Automated PR Review)**

Five AI Sentinels provide automated comprehensive analysis:

```yaml
Automated Review Upon PR Creation:
  - DebtSentinel: Technical debt introduction analysis
  - StandardsGuardian: Coding/testing/documentation standards compliance
  - TestMaster: Test coverage and quality assessment
  - SecuritySentinel: Security vulnerability analysis (PRs to main)
  - MergeOrchestrator: Holistic deployment decision

Quality Gates:
  - Critical findings block deployment
  - High-priority findings request changes
  - Medium/low findings provide educational guidance
  - Comprehensive analysis ensures production-ready quality
```

**Enforcement Benefits:**
- **Multi-Layer Validation:** Catches issues at multiple checkpoints (agent → Claude → ComplianceOfficer → AI Sentinels)
- **Prevention Over Correction:** Early validation (agent self, Claude coordination) prevents issues reaching PR stage
- **Comprehensive Coverage:** Different validators focus on different aspects (standards, integration, security, strategy)
- **Continuous Improvement:** AI Sentinels provide educational guidance improving future agent work

---

## 7. Emergency Protocols

### Delegation Failure Escalation

When agent delegation fails, Claude follows structured escalation:

**Delegation Failure Scenarios:**

```yaml
Scenario_1_Task_Tool_Unavailable:
  Symptom: Task tool returns error "Agent type not available"
  Example: ComplianceOfficer engagement fails due to tool limitation

Scenario_2_Agent_Cannot_Complete:
  Symptom: Agent reports BLOCKED status with unresolvable blocker
  Example: TestEngineer cannot create tests due to missing implementation

Scenario_3_Agent_Authority_Insufficient:
  Symptom: Agent lacks authority for required file modifications
  Example: DocumentationMaintainer asked to modify source code

Scenario_4_Repeated_Failures:
  Symptom: Same agent engagement fails multiple times
  Example: BackendSpecialist repeatedly fails to resolve core issue
```

**Escalation Protocol:**

```yaml
Step_1_NEVER_ASSUME_AGENT_ROLES:
  Action: Claude MUST NOT perform specialized agent work
  Rationale: Violates delegation-only architecture
  Violation: Writing code, creating tests, updating documentation
  Correct: Escalate to user for alternative approach

Step_2_ESCALATE_TO_USER_IMMEDIATELY:
  Action: Report delegation failure with context
  Message: "Agent delegation failed: [specific failure]. Requesting alternative approaches."
  Context: Provide agent type, task description, failure reason

Step_3_DOCUMENT_THE_FAILURE:
  Action: Record failure in working directory session state
  Content: Needed agent, task attempted, failure reason, escalation timestamp

Step_4_SEEK_ALTERNATIVES:
  Options:
    - General-purpose agent with specialized instructions from agent definition files
    - Different agent type with overlapping expertise
    - Task decomposition enabling available agents
    - User provides manual intervention

Step_5_MAINTAIN_ORCHESTRATION:
  Action: Continue coordinating other agents while awaiting resolution
  Focus: Parallel work not blocked by failure
  Communication: Keep user informed of workaround execution
```

**Example Escalation Communication:**

```
🚨 DELEGATION FAILURE ESCALATION

**Failed Agent:** ComplianceOfficer
**Task:** Section-level pre-PR validation for Iteration 3 work
**Failure Reason:** Task tool reports "compliance-officer agent type unavailable"

**Attempted Resolution:** Tried "general-purpose" agent with ComplianceOfficer instructions from /.claude/agents/compliance-officer.md

**Alternative Approaches Needed:**
1. Use general-purpose agent with ComplianceOfficer definition (fallback pattern documented in CLAUDE.md)
2. Manual validation by user reviewing ComplianceOfficer checklist
3. Skip ComplianceOfficer validation and rely solely on AI Sentinels (higher risk)

**Orchestration Status:** Other agents can continue work on non-blocked tasks. Escalating for user decision on validation approach.

**Working Directory Update:** Recorded failure in /working-dir/session-state.md under "Coordination Notes"
```

### Violation Recovery Procedures

If Claude realizes violation of delegation protocols:

**Violation Detection:**

```yaml
Violation_Type_1_Writing_Code:
  Symptom: Claude drafts code implementation instead of delegating to CodeChanger
  Detection: Self-awareness check "Am I writing production code?"

Violation_Type_2_Creating_Tests:
  Symptom: Claude drafts test code instead of delegating to TestEngineer
  Detection: Self-awareness check "Am I creating test scenarios?"

Violation_Type_3_Updating_Documentation:
  Symptom: Claude drafts README content instead of delegating to DocumentationMaintainer
  Detection: Self-awareness check "Am I writing documentation content?"

Violation_Type_4_Analyzing_Architecture:
  Symptom: Claude performs architectural analysis instead of delegating to ArchitecturalAnalyst
  Detection: Self-awareness check "Am I making design decisions?"

Violation_Type_5_Interpreting_Results:
  Symptom: Claude adds analysis or interpretation beyond agent completion report
  Detection: Self-awareness check "Am I adding insights the agent should have provided?"
```

**Recovery Protocol:**

```yaml
Step_1_STOP_IMMEDIATELY:
  Action: Cease the violation mid-execution
  Detection: As soon as violation awareness triggers
  Priority: IMMEDIATE—do not complete violating action

Step_2_ACKNOWLEDGE_TO_USER:
  Message: "I violated delegation protocols by [specific violation description]."
  Transparency: Explain what Claude was doing that violated architecture
  Example: "I detected a potential delegation violation. I was drafting code implementation instead of delegating to CodeChanger. Please review my intended response."

Step_3_UNDO_IF_POSSIBLE:
  Action: Remove violating content from response
  Limitation: If already communicated, acknowledge cannot undo but correcting forward

Step_4_DELEGATE_PROPERLY:
  Action: Assign work to appropriate agent
  Context Package: Construct proper context package for specialized agent
  Correction: Demonstrate correct delegation pattern

Step_5_LEARN_FROM_FAILURE:
  Action: Strengthen adherence protocols
  Reflection: Why did violation occur? (time pressure, unclear boundaries, user expectation)
  Prevention: Reinforce correct delegation pattern for similar future scenarios
```

**Example Violation Recovery:**

```
🚨 VIOLATION RECOVERY

**Detected Violation:** I was about to write code implementation for UserService.UpdateEmailAsync method instead of delegating to CodeChanger.

**Violation Type:** Writing production code (CodeChanger's exclusive authority)

**Recovery Actions:**
1. STOPPED before providing code implementation
2. ACKNOWLEDGED violation detection to user
3. UNDOING violating content (not communicating code draft)
4. DELEGATING PROPERLY to CodeChanger with context package including:
   - Core issue: Implement email update method with validation
   - Standards: CodingStandards.md DI, async patterns
   - Module context: UserService README Section 3 contract requirements
   - Quality gates: TestEngineer tests, DocumentationMaintainer README update

**Reinforcement:** Claude's role is strategic orchestration, NOT implementation. All production code modifications are CodeChanger's domain.
```

### Agent Coordination Conflicts

When multiple agents produce conflicting deliverables:

**Conflict Scenarios:**

```yaml
Scenario_1_Architectural_Disagreement:
  Conflict: BackendSpecialist and ArchitecturalAnalyst recommend different design patterns
  Example: Repository pattern vs. CQRS approach for data access

Scenario_2_Interface_Contract_Mismatch:
  Conflict: BackendSpecialist implements API contract differently than FrontendSpecialist expects
  Example: Different error response formats (exceptions vs. error DTOs)

Scenario_3_Testing_Strategy_Divergence:
  Conflict: TestEngineer and BackendSpecialist disagree on testability approach
  Example: Constructor DI vs. property injection affecting mock strategies

Scenario_4_Documentation_Accuracy_Dispute:
  Conflict: DocumentationMaintainer documents interface contract that CodeChanger says is inaccurate
  Example: README Section 3 preconditions don't match actual validation logic
```

**Conflict Resolution Protocol:**

```yaml
Step_1_IDENTIFY_CONFLICTING_DELIVERABLES:
  Action: Parse agent completion reports for contradictions
  Detection: Incompatible recommendations, divergent implementations, accuracy disputes
  Documentation: Record specific conflict details in session state

Step_2_FACILITATE_CONFLICT_RESOLUTION:
  Approach_1_Specialist_Authority:
    Pattern: Defer to specialist with domain expertise
    Example: BackendSpecialist architectural authority > general recommendations

  Approach_2_Standards_Alignment:
    Pattern: Reference project standards as arbiter
    Example: CodingStandards.md specifies DI pattern → that pattern wins

  Approach_3_Re_Engagement_For_Coordination:
    Pattern: Engage both agents with conflict context for alignment
    Example: BackendSpecialist + FrontendSpecialist coordinate on API contract

Step_3_ADJUST_COORDINATION_STRATEGY:
  Action: Modify orchestration approach to prevent similar conflicts
  Prevention: Clearer context packages, earlier architectural alignment, contract pre-definition

Step_4_DOCUMENT_CONFLICT_PATTERNS:
  Action: Record conflict and resolution in session state
  Learning: Identify systemic issues requiring process improvement
  Communication: Share conflict patterns with user for architectural guidance
```

**Example Conflict Resolution:**

```
🔀 AGENT COORDINATION CONFLICT DETECTED

**Conflict:** BackendSpecialist and FrontendSpecialist API contract mismatch
- BackendSpecialist implemented error responses as exceptions (500 status codes)
- FrontendSpecialist expects error DTOs with 400 status codes and error details

**Root Cause:** API contract not pre-defined architecturally before implementation

**Resolution Approach:** Re-engage ArchitecturalAnalyst for API contract definition
1. ArchitecturalAnalyst: Define error response contract (DTO vs. exception, status codes)
2. BackendSpecialist: Refactor implementation to match contract
3. FrontendSpecialist: Update service to match contract
4. TestEngineer: Integration tests validate contract adherence

**Prevention:** Future API changes require ArchitecturalAnalyst contract definition BEFORE specialist implementations

**Session State Update:** Documented conflict and resolution in /working-dir/session-state.md Coordination Notes
```

### Rollback and Remediation

When agent work needs to be undone or corrected:

**Rollback Scenarios:**

```yaml
Scenario_1_Breaking_Change_Introduced:
  Symptom: Agent modification breaks existing functionality
  Detection: Test suite failures, integration test breakage
  Example: API contract change breaks frontend without coordinated update

Scenario_2_Standards_Violation_Critical:
  Symptom: Agent deliverable violates critical standards (security, architecture)
  Detection: ComplianceOfficer flags critical violation, AI Sentinel blocks
  Example: Hardcoded credentials, SQL injection vulnerability

Scenario_3_Incorrect_Implementation:
  Symptom: Agent misunderstood requirements and implemented wrong solution
  Detection: Deliverable doesn't match core issue definition
  Example: Implemented feature X when issue asked for feature Y

Scenario_4_Integration_Failure:
  Symptom: Multi-agent deliverables don't integrate coherently
  Detection: Integration tests fail, circular dependencies, missing contracts
  Example: Backend + frontend implementations incompatible
```

**Rollback Decision Matrix:**

```yaml
When_to_Rollback:
  - Breaking changes affecting production systems
  - Critical security vulnerabilities introduced
  - Fundamental misunderstanding of requirements (complete re-work needed)
  - Integration failures across multiple agents (coordination breakdown)

When_to_Remediate_Forward:
  - Minor standards violations (fix in place)
  - Partial implementations (complete rather than undo)
  - Documentation inaccuracies (update rather than remove)
  - Test quality issues (improve coverage rather than delete tests)
```

**Rollback Protocol:**

```yaml
Step_1_Assess_Rollback_Scope:
  Action: Determine which agent deliverables need undoing
  Scope: Single agent work vs. multi-agent integrated changes
  Impact: Files modified, dependencies affected, integration points

Step_2_Preserve_Valuable_Partial_Progress:
  Action: Extract reusable work before rollback
  Examples: Architectural analysis (working directory artifact preservation), test scenarios (copy before deletion), documentation insights (README updates preservation)

Step_3_Execute_Git_Rollback:
  Commands:
    - git revert <commit-hash>  # For committed changes
    - git restore <file-paths>  # For uncommitted changes
    - git reset --hard <commit>  # If complete branch reset needed
  Communication: Explain rollback scope and reason to user

Step_4_Re_Engage_With_Refined_Context:
  Action: Delegate to agent again with clearer context package
  Improvements: More explicit core issue, success test criteria, forbidden scope expansions
  Learning: Incorporate what went wrong into refined delegation

Step_5_Course_Correction_Communication:
  Action: Update session state with rollback and corrective action
  Documentation: What was rolled back, why, what corrective approach planned
  Transparency: User understands orchestration adjustments
```

**Example Rollback Execution:**

```
🔄 ROLLBACK & REMEDIATION

**Rollback Decision:** Undo BackendSpecialist API implementation due to breaking change

**Reason:** RecipeService.GetFilteredRecipesAsync changed return type from List<Recipe> to PaginatedResult<Recipe> without coordinated frontend update. Frontend RecipeFilterService broke with type mismatch errors.

**Scope:**
- Files to rollback: RecipeService.cs, IRecipeRepository.cs, RecipeRepository.cs
- Commits to revert: abc1234 (RecipeService filtering implementation)
- Preserved artifacts: /working-dir/backend-specialist-recipe-filtering-implementation.md (architectural decisions preserved for re-implementation)

**Valuable Progress Preserved:**
- RecipeFilterDto design (reusable)
- Database indexing migration (valid optimization)
- Test scenarios designed by TestEngineer (still applicable)

**Remediation Plan:**
1. ArchitecturalAnalyst: Define API contract with return type (PaginatedResult) for frontend awareness
2. BackendSpecialist: Re-implement with contract-defined return type
3. FrontendSpecialist: Update RecipeFilterService to handle PaginatedResult
4. Integration coordination: Both specialists review shared contract before implementation

**Session State Update:** Documented rollback in /working-dir/session-state.md with corrective coordination approach

**Execution:**
```bash
git revert abc1234 -m "Rollback RecipeService filtering - breaking change without frontend coordination"
git push origin section/iteration-3
```

---

## 8. Best Practices

### Context Package Construction Efficiency

**CORE ISSUE FIRST PROTOCOL (MANDATORY):**

Every context package MUST lead with specific blocking technical problem:

```yaml
ANTI_PATTERN (Generic Task):
  Mission: "Improve UserService"
  Problem: Vague, no clear success criteria, undefined scope

BEST_PRACTICE (Specific Core Issue):
  CORE_ISSUE: "Implement UserService.UpdateEmailAsync with email format validation, duplicate detection, and audit logging"
  SUCCESS_TEST: "New method accepts valid email, throws InvalidEmailFormatException for invalid format, throws DuplicateEmailException if email exists for different user, writes audit log entry"
  MINIMAL_SCOPE: "Single method addition to UserService.cs, validation logic, database interaction, audit logging—NO UI changes, NO other service modifications"
```

**Benefits:**
- **Focus:** Agent knows exact problem to solve, not general area to explore
- **Testability:** Success criteria enable verification of completion
- **Scope Control:** Minimal scope prevents mission drift and unauthorized expansions

**Context Package Construction Checklist:**

```yaml
CORE_ISSUE_DEFINITION:
  - [ ] Specific blocking technical problem identified (not general improvement)
  - [ ] Problem statement precise enough to design solution
  - [ ] Success criteria testable and verifiable

INTENT_RECOGNITION:
  - [ ] Query vs. command intent determined
  - [ ] Agent authority validated for intended action
  - [ ] Flexible authority boundaries clear for specialists

SCOPE_BOUNDARY:
  - [ ] Exact files needing modification listed
  - [ ] Minimal scope defined (surgical changes only)
  - [ ] Forbidden scope expansions specified

MANDATORY_SKILLS:
  - [ ] documentation-grounding: Phase 1-3 specifications
  - [ ] working-directory-coordination: Discovery, reporting, integration protocols
  - [ ] Conditional skills with clear triggers

STANDARDS_CONTEXT:
  - [ ] Relevant standards documents identified
  - [ ] Specific sections highlighted (not "read entire standard")

MODULE_CONTEXT:
  - [ ] Target module README path provided
  - [ ] Section 3 (Interface Contract) emphasized
  - [ ] Dependency module READMEs listed

QUALITY_GATES:
  - [ ] Testing requirements specified
  - [ ] Documentation update expectations clear
  - [ ] Validation approach defined
```

### Agent Selection Optimization

**Intent Recognition Patterns:**

```yaml
Query_Intent_Indicators:
  Keywords: "Analyze", "Review", "Assess", "Evaluate", "Examine", "Identify", "Find", "Detect"
  Pattern: Questions about existing code
  Example: "Analyze UserService for performance bottlenecks"
  Agent_Selection: Specialist in advisory mode (working directory artifact)
  Authority: No direct file modifications

Command_Intent_Indicators:
  Keywords: "Fix", "Implement", "Update", "Create", "Build", "Add", "Optimize", "Enhance", "Improve", "Refactor"
  Pattern: Directives to modify code
  Example: "Implement caching for UserService.GetUserById"
  Agent_Selection: Specialist in implementation mode OR CodeChanger
  Authority: Direct file modifications within expertise domain
```

**Agent Selection Decision Tree:**

```yaml
Question_1_What_Type_Of_Work:
  Production_Code: → CodeChanger or Specialist (implementation mode)
  Tests: → TestEngineer
  Documentation: → DocumentationMaintainer
  AI_Prompts: → PromptEngineer
  Analysis: → Specialist or ArchitecturalAnalyst (query mode)

Question_2_Specialist_Expertise_Match:
  Backend_Csharp: → BackendSpecialist (if command intent)
  Frontend_Angular: → FrontendSpecialist (if command intent)
  Security_Concern: → SecurityAuditor
  CI_CD_Workflow: → WorkflowEngineer
  Diagnostic_Needed: → BugInvestigator
  Design_Decision: → ArchitecturalAnalyst

Question_3_Authority_Validation:
  Within_Specialist_Domain: → Specialist directly (efficiency)
  Cross_Domain: → CodeChanger or coordinate multiple specialists
  Advisory_Only: → Working directory artifact from specialist

Question_4_Complexity_Assessment:
  Simple_Implementation: → CodeChanger (general capability sufficient)
  Complex_Domain_Specific: → Specialist (deep expertise needed)
  Architectural_Decision: → ArchitecturalAnalyst first, then implementer
```

**Same-Agent Re-Engagement Efficiency:**

```yaml
When_to_Re_Engage_Same_Agent:
  - Iterative work within same domain (BackendSpecialist → more backend work)
  - Agent recommendations suggest follow-up work (agent proposed next steps)
  - Partial completion requiring continuation (IN_PROGRESS status)
  - Specialist expertise still applicable (domain-specific incremental work)

Benefits_of_Re_Engagement:
  - Context continuity: Agent's prior work informs next iteration
  - Expertise accumulation: Specialist deepens understanding of module
  - Efficiency: No handoff overhead to different agent
  - Authority awareness: Same agent knows domain boundaries

Example:
  Engagement_1: BackendSpecialist analyzes RecipeService performance (QUERY intent)
  Engagement_2: BackendSpecialist implements caching based on analysis (COMMAND intent)
  Efficiency: Same agent, continuity from analysis to implementation, no handoff
```

### Iterative Planning Adaptation

**Adaptive Planning Philosophy:**

Success measured by adaptive coordination that evolves with agent discoveries, NOT adherence to rigid upfront decomposition.

**Adaptive Planning Triggers:**

```yaml
Trigger_1_Agent_Recommendations:
  Scenario: Agent completion report suggests different next steps than initially planned
  Example: BackendSpecialist recommends database indexing not originally scoped
  Adaptation: Claude incorporates recommendation into next agent engagement plan

Trigger_2_Integration_Dependencies_Discovered:
  Scenario: Agent identifies blocking dependency not anticipated upfront
  Example: TestEngineer cannot create tests until CodeChanger refactors for testability
  Adaptation: Re-sequence agent engagements to resolve blocker first

Trigger_3_Architectural_Insights_Emerge:
  Scenario: Specialist analysis reveals architectural pattern needing broader application
  Example: ArchitecturalAnalyst identifies repository pattern inconsistency across services
  Adaptation: Expand scope to address pattern systematically across affected modules

Trigger_4_Testing_Reveals_Edge_Cases:
  Scenario: TestEngineer discovers edge cases requiring additional coverage
  Example: Integration tests reveal race condition in concurrent API calls
  Adaptation: Engage BackendSpecialist for concurrency fix before considering feature complete

Trigger_5_Documentation_Identifies_Gaps:
  Scenario: DocumentationMaintainer identifies missing interface contracts
  Example: README Section 3 cannot document preconditions because none exist in code
  Adaptation: Engage CodeChanger to add validation before documentation completion
```

**Adaptive Planning Process:**

```yaml
Step_1_Review_Agent_Completion:
  Action: Analyze completion report for insights and recommendations
  Focus: "Next Team Actions" section, integration handoffs, recommendations

Step_2_Assess_Plan_Deviation:
  Question: "Does agent's work/recommendations suggest different approach than originally planned?"
  Comparison: Current state vs. initial decomposition assumptions

Step_3_Modify_Coordination_Strategy:
  Options:
    - Re-sequence: Change order of agent engagements based on discovered dependencies
    - Re-engage: Same agent for iterative work within domain
    - Expand: Broaden scope if architectural insights warrant systemic changes
    - Refocus: Narrow scope if complexity exceeded expectations

Step_4_Update_Session_State:
  Action: Document plan adaptation in /working-dir/session-state.md
  Content: What changed, why, how coordination strategy modified

Step_5_Communicate_Adaptation:
  Action: Update user on plan evolution
  Transparency: "Based on [agent] discoveries, adapting coordination to [modification]"
```

**Example Adaptive Planning:**

```
🔄 ITERATIVE PLANNING ADAPTATION

**Original Plan:**
1. BackendSpecialist: Implement RecipeService.GetFilteredRecipesAsync
2. TestEngineer: Create unit tests for filtering logic
3. DocumentationMaintainer: Update RecipeService README

**Agent Discovery (BackendSpecialist completion):**
- Recommendation: Add database indexing for Ingredients and DietaryRestrictions columns (performance critical)
- Insight: Current pagination approach inefficient for large datasets (recommend cursor-based pagination)

**Plan Adaptation:**
1. ✅ BackendSpecialist: Implement filtering (COMPLETE)
2. **INSERTED:** WorkflowEngineer: Create database migration for indexing (performance requirement)
3. **MODIFIED:** BackendSpecialist: Re-engage for cursor-based pagination (architectural improvement)
4. TestEngineer: Create comprehensive tests including pagination edge cases (updated scope)
5. DocumentationMaintainer: Update README with pagination strategy (expanded documentation)

**Rationale:** Agent expertise revealed performance and scalability concerns not anticipated upfront. Adaptive planning incorporates recommendations for production-ready implementation.

**Session State Update:** Documented plan adaptation with agent recommendation context.
```

### Communication Excellence Patterns

**Enforce Immediate Artifact Reporting:**

```yaml
Best_Practice_1_Immediate_Reporting:
  Pattern: Agent reports artifact creation IMMEDIATELY after creation
  Anti_Pattern: Agent batches multiple artifact reports or delays until completion

  Example_Good:
    After creating backend-analysis.md → Report immediately
    After creating backend-implementation.md → Report immediately (separate report)

  Example_Bad:
    Created 3 artifacts → Batch report all at end (team unaware during work)

Best_Practice_2_Standardized_Format_Usage:
  Pattern: Agent uses exact standardized format for all artifact reports
  Anti_Pattern: Agent provides informal notification without structure

  Example_Good:
    🗂️ WORKING DIRECTORY ARTIFACT CREATED:
    - Filename: backend-specialist-user-auth-implementation.md
    - Purpose: [structured content]

  Example_Bad:
    "I created a file about user authentication implementation"
```

**Verify Artifact Discovery Before Work:**

```yaml
Best_Practice_3_Pre_Work_Discovery:
  Pattern: Agent checks /working-dir/ BEFORE starting work, reports findings
  Anti_Pattern: Agent starts work without checking existing artifacts

  Example_Good:
    🔍 WORKING DIRECTORY DISCOVERY:
    - Current artifacts reviewed: [specific files]
    - Relevant context found: [what informs work]

  Example_Bad:
    Agent starts implementation without checking if architecture already defined

Best_Practice_4_Build_Upon_Discovery:
  Pattern: Agent explicitly integrates discovered artifacts into work
  Anti_Pattern: Agent duplicates analysis already completed by other agent

  Example_Good:
    Reviewed architecture.md → Implemented all recommendations → Documented integration

  Example_Bad:
    Performed architectural analysis duplicate of existing architecture.md artifact
```

**Ensure Standardized Communication Format:**

```yaml
Best_Practice_5_Consistent_Format:
  Pattern: ALL agents use same emoji prefixes, structure, sections
  Benefit: Claude can parse reports systematically, team recognizes patterns

  Formats:
    Discovery: 🔍 WORKING DIRECTORY DISCOVERY
    Reporting: 🗂️ WORKING DIRECTORY ARTIFACT CREATED
    Integration: 🔗 ARTIFACT INTEGRATION
    Completion: 🎯 [AGENT] COMPLETION REPORT

Best_Practice_6_Complete_Context:
  Pattern: Reports include ALL required sections (filename, purpose, context, dependencies, next actions)
  Anti_Pattern: Partial reports missing critical information

  Required_Sections:
    - Filename: Exact filename with extension
    - Purpose: Content description and intended consumers
    - Context for Team: What other agents need to know
    - Dependencies: What this builds upon
    - Next Actions: Follow-up coordination needed
```

**Maintain Comprehensive Working Directory Awareness:**

```yaml
Best_Practice_7_Claude_Tracks_Artifacts:
  Pattern: Claude maintains catalog of all artifacts in session state
  Benefit: Comprehensive view of team coordination and context

  Session_State_Artifact_Catalog:
    - artifact-name.md: Purpose (Created: date, Agent: name)
    - artifact-name.md: Purpose (Created: date, Agent: name)

Best_Practice_8_Artifact_Lifecycle_Management:
  Pattern: Archive or remove obsolete artifacts preventing confusion
  Timing: When feature complete or artifact superseded

  Example:
    analysis.md superseded by implementation.md → Archive analysis.md
    Bug fixed → Remove diagnostic artifacts related to that bug
```

---

## 9. Examples

### Example 1: Enhanced Context Package (Complete)

```yaml
## CORE ISSUE FIRST PROTOCOL (MANDATORY):
CORE_ISSUE: "Implement RecipeService.GetFilteredRecipesAsync method supporting ingredient-based filtering, dietary restriction filtering, and pagination"
INTENT_RECOGNITION: COMMAND - Direct implementation request
TARGET_FILES: "Code/Zarichney.Server/Services/RecipeService.cs, Code/Zarichney.Server/Repositories/IRecipeRepository.cs, Code/Zarichney.Server/DTOs/RecipeFilterDto.cs (new)"
AGENT_SELECTION: BackendSpecialist (backend domain expertise, command intent enables direct implementation)
FLEXIBLE_AUTHORITY: BackendSpecialist authorized for *.cs files, repository patterns, service layer modifications
MINIMAL_SCOPE: "Add GetFilteredRecipesAsync method to RecipeService and IRecipeRepository, create RecipeFilterDto, implement EF Core query composition—NO UI changes, NO controller modifications, NO other service changes"
SUCCESS_TEST: "Unit tests call GetFilteredRecipesAsync with various filter combinations and verify correct recipes returned, pagination metadata accurate, validation exceptions thrown for invalid inputs"

## MISSION OBJECTIVE:
Implement robust recipe filtering endpoint supporting multiple filter criteria with pagination for performance

GitHub Issue Context: Issue #456, epic/api-enhancement, Backend coverage initiative contribution
Technical Constraints:
  - CodingStandards.md: Constructor DI, async/await with CancellationToken, repository pattern
  - Performance: Must support pagination for large datasets (database indexing considerations)
  - Architecture: Service → Repository pattern separation, DTO for filter parameters

## INTENT RECOGNITION PATTERNS:
Command_Intent_Detected:
  - Keyword: "Implement" (clear directive for code modification)
  - Task: Create new method with specific functionality
  - Authority: BackendSpecialist has flexible authority for backend *.cs files
  - Action: Direct file modifications within expertise domain

## FLEXIBLE AUTHORITY BOUNDARIES:
BackendSpecialist_Domain:
  - Authorized: *.cs (services, repositories, DTOs), appsettings.json, migrations
  - Requires_Coordination: API controller endpoints (CodeChanger for controller layer)
  - Forbidden: Frontend *.ts files (FrontendSpecialist), test files (TestEngineer)

## FORBIDDEN SCOPE EXPANSIONS:
- NO controller endpoint creation (focus on service layer only)
- NO UI filtering component (frontend work separate)
- NO other service modifications (surgical scope to RecipeService)
- NO infrastructure changes (database migration separate if needed)

## MANDATORY SKILLS (Execute Before Work):
documentation-grounding:
  - Phase 1: CodingStandards.md (DI, async/await, repository pattern), TestingStandards.md (design for testability)
  - Phase 2: Root README, backend module hierarchy (Code/Zarichney.Server/)
  - Phase 3: RecipeService README.md
    - Section 2: Architecture (understand repository pattern usage)
    - Section 3: Interface Contract (understand existing method contracts for consistency)
    - Section 5: Testing strategy (design for unit testing with mocked repository)
    - Section 6: Dependencies (IRecipeRepository contract)

working-directory-coordination:
  - Pre-Work Artifact Discovery: Check /working-dir/ for recipe filtering architectural analysis
  - Immediate Artifact Reporting: Report implementation decisions and interface contract
  - Context Integration: Build upon any existing architectural recommendations

## CONDITIONAL SKILLS (Load If Needed):
api-design-patterns:
  - TRIGGER: If filtering logic complexity requires architectural guidance (CQRS, specification pattern)
  - FOCUS: IQueryable composition, pagination patterns, filter parameter design

## WORKING DIRECTORY DISCOVERY (MANDATORY):
- Check for architectural-analyst-recipe-filtering.md or backend-specialist-recipe-analysis.md
- Review any filtering recommendations from prior analysis
- Report discovered artifacts with integration approach

## WORKING DIRECTORY COMMUNICATION (REQUIRED):
- Report implementation artifact with interface contract details
- Document preconditions, postconditions, error handling for TestEngineer
- Prepare handoff notes for FrontendSpecialist (API contract understanding)

## INTEGRATION REQUIREMENTS:
- IRecipeRepository enhancement must follow existing repository pattern
- RecipeFilterDto design should be reusable across API and service layers
- Pagination approach should align with existing pagination patterns in codebase

## STANDARDS CONTEXT:
CodingStandards.md:
  - Section: Dependency Injection → Constructor injection for IRecipeRepository
  - Section: Asynchronous Programming → Async/await with CancellationToken parameter
  - Section: SOLID Principles → Single responsibility (filtering logic in repository)

TestingStandards.md:
  - Section: Test Design → Design for unit testing (interface abstraction, dependency injection)

## MODULE CONTEXT:
Code/Zarichney.Server/Services/RecipeService README.md:
  - Section 2 (Architecture): Review existing service methods for pattern consistency
  - Section 3 (Interface Contract): CRITICAL - Understand precondition/postcondition patterns for new method
  - Section 5 (How to Work): Testing strategy with mocked IRecipeRepository
  - Section 6 (Dependencies): IRecipeRepository interface for implementation

Code/Zarichney.Server/Repositories/IRecipeRepository README.md:
  - Section 3 (Interface Contract): Understand repository contract patterns for new method signature

## QUALITY GATES:
- [ ] documentation-grounding 3-phase protocol followed
- [ ] working-directory-coordination artifact discovery and reporting complete
- [ ] CodingStandards.md compliance (DI, async, SOLID)
- [ ] TestingStandards.md design for testability (interface abstraction, mock-friendly)
- [ ] Unit tests possible with mocked IRecipeRepository (testability validation)
```

### Example 2: Agent Completion Report (Standardized Format)

```yaml
🎯 BackendSpecialist COMPLETION REPORT

Status: COMPLETE ✅
Epic Contribution: Backend coverage initiative - RecipeService filtering implementation

## Working Directory Artifacts Communication:

🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: backend-specialist-recipe-filtering-implementation.md
- Purpose: Documents RecipeService.GetFilteredRecipesAsync implementation decisions, interface contract (preconditions, postconditions, error handling), EF Core query optimization approach, pagination strategy
- Context for Team:
  - TestEngineer: Interface contract provides preconditions (filter validation), postconditions (filtered results with pagination metadata), error scenarios for comprehensive test coverage
  - FrontendSpecialist: RecipeFilterDto structure defines API contract for frontend filter component
  - DocumentationMaintainer: Implementation notes provide Section 3 content for RecipeService README update
- Dependencies: Built upon backend-specialist-recipe-analysis.md architectural recommendations
- Next Actions: TestEngineer creates comprehensive tests, DocumentationMaintainer updates RecipeService README Section 3

## Files Modified:
- Code/Zarichney.Server/Services/RecipeService.cs:
  - Added GetFilteredRecipesAsync(RecipeFilterDto filter, CancellationToken cancellationToken)
  - Constructor DI with IRecipeRepository (CodingStandards.md compliance)
  - Async/await with CancellationToken (CodingStandards.md compliance)
  - Precondition validation: filter non-null, PageSize 1-100, PageNumber >= 0

- Code/Zarichney.Server/Repositories/IRecipeRepository.cs:
  - Added GetFilteredRecipesAsync method signature
  - Returns PaginatedResult<Recipe> with filtering and pagination

- Code/Zarichney.Server/Repositories/RecipeRepository.cs:
  - Implemented IQueryable composition for ingredient and dietary restriction filtering
  - .AsNoTracking() for read-only performance optimization
  - .Skip() and .Take() for pagination
  - Total count calculation for pagination metadata

- Code/Zarichney.Server/DTOs/RecipeFilterDto.cs (CREATED):
  - Properties: List<string> Ingredients, List<string> DietaryRestrictions, int PageSize, int PageNumber
  - Validation attributes for data annotation
  - Reusable across service and API layers

- Code/Zarichney.Server/DTOs/PaginatedResult.cs (CREATED):
  - Generic pagination wrapper: List<T> Items, int TotalCount, int PageSize, int PageNumber

## Team Integration Handoffs:

📋 TestEngineer: Create comprehensive tests covering:
  - Happy path: Various filter combinations return correct filtered recipes
  - Precondition violations:
    - filter null → ArgumentNullException
    - PageSize <= 0 or > 100 → ArgumentOutOfRangeException
    - PageNumber < 0 → ArgumentOutOfRangeException
  - Filtering logic: Ingredient matching, dietary restriction matching, combination filters
  - Pagination: Correct Skip/Take behavior, total count accuracy, multiple pages
  - Error handling: OperationCanceledException on token cancellation
  - Repository integration: Verify IRecipeRepository.GetFilteredRecipesAsync called with correct parameters

📖 DocumentationMaintainer: Update RecipeService README.md:
  - Section 3 (Interface Contract): Add GetFilteredRecipesAsync with:
    - Preconditions: filter non-null, PageSize 1-100, PageNumber >= 0
    - Postconditions: PaginatedResult with filtered recipes, accurate total count
    - Error handling: ArgumentNullException, ArgumentOutOfRangeException, OperationCanceledException
  - Section 5 (How to Work): Note pagination strategy (offset-based, potential cursor-based future enhancement)

🔒 SecurityAuditor: Consider security review:
  - Input validation for filter parameters (SQL injection via ingredient names?)
  - Pagination limits enforced (prevent excessive PageSize abuse)

🏗️ WorkflowEngineer: Consider database indexing:
  - Create migration for indexes on Recipes.Ingredients and Recipes.DietaryRestrictions columns
  - Performance critical for filtering queries at scale

## Team Coordination Status:
- Integration conflicts: None identified
- Cross-agent dependencies:
  - TestEngineer: UNBLOCKED - can create tests immediately with interface contract
  - DocumentationMaintainer: UNBLOCKED - implementation notes prepared
  - WorkflowEngineer: Optional migration for indexing (performance enhancement, not blocker)
- Urgent coordination needs: None

## AI Sentinel Readiness: READY ✅
Implementation follows CodingStandards.md (DI, async, validation), designed for testability (interface abstraction, dependency injection), comprehensive interface contract documented for testing

## Next Team Actions:
1. TestEngineer: Create RecipeServiceTests.GetFilteredRecipesAsync_VariousScenarios (priority)
2. DocumentationMaintainer: Update RecipeService README Section 3 with new method contract
3. WorkflowEngineer: Optional database migration for filtering performance indexes
```

### Example 3: Multi-Agent Coordination Workflow (Complete Scenario)

**Scenario:** Implement complete recipe filtering feature across backend + frontend + testing + documentation

**Phase 1: Architectural Design (ArchitecturalAnalyst - QUERY Intent)**

```yaml
Agent: ArchitecturalAnalyst
Intent: QUERY (analysis and design recommendations)
Context Package:
  CORE_ISSUE: "Design architecture for recipe filtering feature supporting ingredients, dietary restrictions, and pagination"
  INTENT_RECOGNITION: QUERY - Architectural analysis request
  AGENT_SELECTION: ArchitecturalAnalyst (design decisions domain)

Deliverable: /working-dir/architectural-analyst-recipe-filtering.md
Content:
  - Component breakdown: RecipeService (backend), RecipeFilterService (frontend), RecipeFilterComponent (UI)
  - Data flow: UI → Frontend Service → API → Backend Service → Repository → Database
  - API contract: POST /api/recipes/filter with RecipeFilterDto body, returns PaginatedResult<RecipeDto>
  - Backend patterns: Repository pattern with IQueryable composition, DTO for filtering
  - Frontend patterns: NgRx state for filter criteria, reactive filtering, debounced API calls
  - Performance: Pagination required, database indexing recommended

🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: architectural-analyst-recipe-filtering.md
- Purpose: Complete architectural design for filtering feature with component responsibilities, API contract, performance considerations
- Context for Team: BackendSpecialist and FrontendSpecialist implement based on this design; TestEngineer designs integration tests validating contract
- Next Actions: BackendSpecialist implements backend components, FrontendSpecialist implements frontend components
```

**Phase 2: Backend Implementation (BackendSpecialist - COMMAND Intent)**

```yaml
Agent: BackendSpecialist
Intent: COMMAND (direct implementation based on architecture)
Pre-Work Discovery:
  🔍 WORKING DIRECTORY DISCOVERY:
  - Current artifacts reviewed: architectural-analyst-recipe-filtering.md
  - Relevant context found: Component breakdown, API contract specification, repository pattern guidance
  - Integration opportunities: Implement all architectural recommendations

Context Package:
  CORE_ISSUE: "Implement RecipeService.GetFilteredRecipesAsync following architectural-analyst-recipe-filtering.md design"
  INTENT_RECOGNITION: COMMAND - Implementation directive
  AGENT_SELECTION: BackendSpecialist (backend domain, command intent)
  FLEXIBLE_AUTHORITY: Direct modification of *.cs files

Deliverable: RecipeService.cs, IRecipeRepository.cs, RecipeFilterDto.cs, PaginatedResult.cs (code files)
Working Directory: /working-dir/backend-specialist-recipe-implementation.md

🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: backend-specialist-recipe-implementation.md
- Purpose: Documents implementation details, interface contract (preconditions, postconditions, error handling) for TestEngineer test design
- Context for Team: TestEngineer can design comprehensive tests from contract; FrontendSpecialist understands API response format; DocumentationMaintainer has Section 3 content
- Dependencies: Built upon architectural-analyst-recipe-filtering.md design
- Next Actions: TestEngineer creates backend tests, FrontendSpecialist implements frontend, DocumentationMaintainer updates RecipeService README
```

**Phase 3: Frontend Implementation (FrontendSpecialist - COMMAND Intent)**

```yaml
Agent: FrontendSpecialist
Intent: COMMAND (direct implementation based on architecture and backend contract)
Pre-Work Discovery:
  🔍 WORKING DIRECTORY DISCOVERY:
  - Current artifacts reviewed: architectural-analyst-recipe-filtering.md, backend-specialist-recipe-implementation.md
  - Relevant context found: API contract (POST /api/recipes/filter), RecipeFilterDto structure, PaginatedResult response format
  - Integration opportunities: Implement frontend consuming exact backend contract

Context Package:
  CORE_ISSUE: "Implement RecipeFilterComponent and RecipeFilterService consuming RecipeService.GetFilteredRecipesAsync API"
  INTENT_RECOGNITION: COMMAND - Implementation directive
  AGENT_SELECTION: FrontendSpecialist (frontend domain, command intent)
  FLEXIBLE_AUTHORITY: Direct modification of *.ts, *.html, *.css files

Deliverable: recipe-filter.component.ts, recipe-filter.service.ts, recipe-filter.component.html (code files)
Working Directory: /working-dir/frontend-specialist-recipe-implementation.md

🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: frontend-specialist-recipe-implementation.md
- Purpose: Documents frontend implementation, NgRx state management for filters, API service integration, pagination UI component
- Context for Team: TestEngineer can design frontend unit/integration tests; DocumentationMaintainer understands frontend architecture for docs
- Dependencies: Built upon backend-specialist-recipe-implementation.md API contract
- Next Actions: TestEngineer creates frontend tests and integration tests validating API contract, DocumentationMaintainer updates frontend component README
```

**Phase 4: Backend Testing (TestEngineer - Backend Tests)**

```yaml
Agent: TestEngineer
Pre-Work Discovery:
  🔍 WORKING DIRECTORY DISCOVERY:
  - Current artifacts reviewed: backend-specialist-recipe-implementation.md
  - Relevant context found: Complete interface contract with preconditions, postconditions, error handling scenarios
  - Integration opportunities: Design comprehensive tests covering all contract aspects

Context Package:
  CORE_ISSUE: "Create comprehensive unit tests for RecipeService.GetFilteredRecipesAsync covering interface contract"
  AGENT_SELECTION: TestEngineer

Deliverable: RecipeServiceTests.cs with 15+ test methods
Working Directory: /working-dir/test-engineer-backend-strategy.md

🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: test-engineer-backend-strategy.md
- Purpose: Documents test strategy, scenarios covered, AAA pattern usage, mock strategy for IRecipeRepository
- Context for Team: DocumentationMaintainer documents testing strategy in README Section 5
- Next Actions: TestEngineer creates integration tests, DocumentationMaintainer updates README
```

**Phase 5: Integration Testing (TestEngineer - API Contract Validation)**

```yaml
Agent: TestEngineer
Pre-Work Discovery:
  🔍 WORKING DIRECTORY DISCOVERY:
  - Current artifacts reviewed: backend-specialist-recipe-implementation.md, frontend-specialist-recipe-implementation.md
  - Relevant context found: API contract from backend, frontend consumption pattern
  - Integration opportunities: Validate contract adherence with integration tests

Context Package:
  CORE_ISSUE: "Create integration tests validating RecipeService API contract matches frontend expectations"

Deliverable: RecipeIntegrationTests.cs
Working Directory: /working-dir/test-engineer-integration-strategy.md

🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: test-engineer-integration-strategy.md
- Purpose: Integration test scenarios validating backend-frontend contract alignment, API response format correctness
- Context for Team: DocumentationMaintainer documents integration testing approach
- Next Actions: DocumentationMaintainer synthesizes all artifacts into comprehensive README updates
```

**Phase 6: Documentation (DocumentationMaintainer - Synthesis)**

```yaml
Agent: DocumentationMaintainer
Context Integration:
  🔗 ARTIFACT INTEGRATION:
  - Source artifacts used:
    - architectural-analyst-recipe-filtering.md: Overall architecture and component responsibilities
    - backend-specialist-recipe-implementation.md: Interface contract details
    - frontend-specialist-recipe-implementation.md: Frontend architecture
    - test-engineer-backend-strategy.md: Testing strategy
    - test-engineer-integration-strategy.md: Integration testing approach
  - Integration approach: Synthesized architecture, implementation contracts, testing strategies into comprehensive READMEs
  - Value addition: Complete documentation capturing full feature design and implementation

Deliverable:
  - Code/Zarichney.Server/Services/RecipeService README.md Section 3 updated
  - Code/Zarichney.Website/components/recipe-filter README.md created

🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: documentation-maintainer-filtering-docs.md
- Purpose: Summary of documentation updates, cross-references validated, Section 3 contract accuracy confirmed
- Context for Team: All agents' work now documented for future reference
```

**Coordination Summary:**

```markdown
# /working-dir/session-state.md

**Feature:** Recipe Filtering Implementation
**Status:** Complete ✅

## Completed Work:
- ✅ Architectural Design: ArchitecturalAnalyst (architectural-analyst-recipe-filtering.md)
- ✅ Backend Implementation: BackendSpecialist (RecipeService.cs, repository, DTOs)
- ✅ Frontend Implementation: FrontendSpecialist (RecipeFilterComponent, service)
- ✅ Backend Testing: TestEngineer (RecipeServiceTests.cs)
- ✅ Integration Testing: TestEngineer (RecipeIntegrationTests.cs)
- ✅ Documentation: DocumentationMaintainer (README updates)

## Artifact Catalog:
- architectural-analyst-recipe-filtering.md: Overall architecture (Created: 2025-10-26)
- backend-specialist-recipe-implementation.md: Backend contract (Created: 2025-10-26)
- frontend-specialist-recipe-implementation.md: Frontend architecture (Created: 2025-10-26)
- test-engineer-backend-strategy.md: Backend test strategy (Created: 2025-10-26)
- test-engineer-integration-strategy.md: Integration test strategy (Created: 2025-10-26)
- documentation-maintainer-filtering-docs.md: Documentation summary (Created: 2025-10-26)

## Coordination Effectiveness:
- Working directory artifacts enabled seamless context sharing across 4 agents
- API contract defined architecturally prevented backend-frontend misalignment
- Comprehensive testing validated implementation and integration
- Documentation synthesized complete feature design and rationale
```

---

## Related Documentation

### Prerequisites
- [DocumentationStandards.md](../Standards/DocumentationStandards.md) - README structure and self-contained knowledge philosophy
- [CodingStandards.md](../Standards/CodingStandards.md) - Production code standards agents must follow
- [TestingStandards.md](../Standards/TestingStandards.md) - Test quality and coverage requirements
- [TaskManagementStandards.md](../Standards/TaskManagementStandards.md) - Git workflow, branching, conventional commits

### Integration Points
- [ContextManagementGuide.md](./ContextManagementGuide.md) - Progressive loading and token optimization for orchestration
- [DocumentationGroundingProtocols.md](./DocumentationGroundingProtocols.md) - Systematic standards loading agents must perform
- [SkillsDevelopmentGuide.md](./SkillsDevelopmentGuide.md) - Skills architecture enabling progressive loading
- [CommandsDevelopmentGuide.md](./CommandsDevelopmentGuide.md) - Commands integration with orchestration

### Orchestration Context
- [CLAUDE.md](../../CLAUDE.md) - Core orchestration authority and delegation imperatives
- [/.claude/agents/](../../.claude/agents/) - All 12 agent definition files demonstrating authority framework
- [/.github/prompts/](../../.github/prompts/) - AI Sentinel prompt engineering for automated PR review
- [/working-dir/](../../working-dir/) - Session state and agent artifacts for coordination

### Epic Specifications
- [Epic #291 README](../Specs/epic-291-skills-commands/README.md) - Complete epic context including multi-agent team efficiency benefits
- [Documentation Plan](../Specs/epic-291-skills-commands/documentation-plan.md) - Comprehensive documentation reorganization strategy

---

**Guide Status:** ✅ **COMPLETE**
**Word Count:** ~12,500 words
**Validation:** All 8 sections comprehensive, multi-agent coordination patterns complete, quality gate protocols detailed, working directory integration documented, emergency protocols specified, examples demonstrate all patterns, cross-references functional

**Success Test:** Claude and all agents can execute multi-agent orchestration following this guide without external clarification ✅
