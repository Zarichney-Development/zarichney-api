# Documentation Reorganization Plan - Epic #291

**Last Updated:** 2025-10-25
**Purpose:** Comprehensive documentation reorganization establishing /Docs/ as authoritative source of truth

---

## Overview

**Objective:** Create comprehensive documentation infrastructure that enables stateless AI assistants, supports epic progression tracking, ensures organizational knowledge preservation, and establishes /Docs/ as single source of truth with vendor-agnostic strategy.

**Scope:** 7 new documentation files + 4 standards updates + comprehensive cross-references

---

## Priority 1: Critical Epic Documentation (Must-Have)

### 1. SkillsDevelopmentGuide.md

**Location:** `/Docs/Development/SkillsDevelopmentGuide.md`
**Owner:** DocumentationMaintainer
**Estimated Size:** ~6,000-8,000 words

**Purpose:**
Comprehensive guide for creating, organizing, and maintaining agent skills with progressive loading architecture.

**Required Sections:**
```markdown
# Skills Development Guide

## 1. Purpose & Philosophy
- Progressive loading strategy for context management
- Metadata-driven skill discovery mechanisms
- Resource bundling patterns and organization
- Integration with agent orchestration framework

## 2. Skills Architecture
- Skill definition structure and components
- Metadata schema and discovery mechanics
- Resource bundling and loading strategies
- Progressive loading best practices (metadata → instructions → resources)

## 3. Creating New Skills
- Skill creation workflow (5-phase process)
- Metadata configuration and validation
- Resource organization (templates, examples, documentation)
- Testing and validation procedures

## 4. Skill Categories
- Coordination Skills: Cross-cutting team patterns
- Technical Skills: Domain-specific expertise
- Meta-Skills: Agent/skill/command creation
- Workflow Skills: Repeatable processes

## 5. Integration with Orchestration
- Agent skill loading patterns and references
- Context window optimization techniques
- Skill dependency management
- Dynamic skill composition patterns

## 6. Best Practices
- Granularity guidelines (when to create new skill)
- Metadata design patterns for discovery
- Resource organization strategies
- Performance optimization techniques
- Quality standards and validation

## 7. Examples
- Coordination skill anatomy: working-directory-coordination
- Technical skill example: backend-architecture-excellence
- Meta-skill example: skill-creation
- Integration examples: Agent skill references
```

**Cross-References:**
- DocumentationStandards.md: Metadata requirements
- SkillTemplate.md: Skill creation template
- skill-metadata.schema.json: Metadata validation
- Agent files in .claude/agents/: Integration examples

**Acceptance Criteria:**
- ✅ Enables skill creation without external clarification
- ✅ All sections complete with clear, actionable guidance
- ✅ Examples demonstrate all skill types
- ✅ Integration patterns comprehensive
- ✅ Cross-references to standards and templates functional

---

### 2. CommandsDevelopmentGuide.md

**Location:** `/Docs/Development/CommandsDevelopmentGuide.md`
**Owner:** DocumentationMaintainer
**Estimated Size:** ~5,000-7,000 words

**Purpose:**
Comprehensive guide for creating slash commands with consistent UX, argument handling, and skill integration.

**Required Sections:**
```markdown
# Commands Development Guide

## 1. Purpose & Philosophy
- Slash command architecture and design principles
- Command discovery and registration mechanics
- Integration with Claude Code workflows
- User experience design principles for CLI commands

## 2. Commands Architecture
- Command definition structure (frontmatter + sections)
- Argument parsing and validation patterns
- Error handling and user feedback standards
- Output formatting conventions

## 3. Creating New Commands
- Command creation workflow (5-phase process)
- Registration and discovery mechanisms
- Argument configuration best practices
- Testing and validation procedures

## 4. Command Categories
- Workflow Commands: CI/CD monitoring, automation triggers
- Testing Commands: Coverage analytics, test execution
- GitHub Commands: Issue creation, PR management
- Documentation Commands: Generation, validation

## 5. Integration with Skills
- Command-driven skill loading patterns
- Context packaging for command execution
- Skill composition within commands
- Performance optimization (command = interface, skill = logic)

## 6. GitHub Issue Creation Workflows
- Automated issue creation patterns
- Template-driven issue generation (5 types)
- Label and milestone management automation
- Epic coordination integration

## 7. Best Practices
- Command naming conventions (kebab-case)
- Argument design guidelines (positional, named, flags)
- Error message standards (helpful, actionable)
- Performance considerations (async, caching)

## 8. Examples
- Workflow command: /workflow-status
- Testing command: /coverage-report
- GitHub command: /create-issue
- Epic command: /merge-coverage-prs
```

**Cross-References:**
- CommandTemplate.md: Command creation template
- command-definition.schema.json: Frontmatter validation
- SkillsDevelopmentGuide.md: Skill integration patterns
- TaskManagementStandards.md: GitHub workflows

**Acceptance Criteria:**
- ✅ Enables command creation without external clarification
- ✅ Command-skill boundary crystal clear
- ✅ Argument handling patterns comprehensive
- ✅ GitHub automation documented thoroughly
- ✅ Examples demonstrate all command types

---

### 3. DocumentationGroundingProtocols.md

**Location:** `/Docs/Development/DocumentationGroundingProtocols.md`
**Owner:** DocumentationMaintainer
**Estimated Size:** ~4,000-6,000 words

**Purpose:**
Systematic documentation loading protocols enabling stateless AI operation with complete context.

**Required Sections:**
```markdown
# Documentation Grounding Protocols

## 1. Purpose & Philosophy
- Self-contained knowledge philosophy for stateless AI
- Stateless AI assistant design principles
- Systematic context loading patterns
- Standards-driven agent operation

## 2. Agent Grounding Architecture
- Standards loading sequence (mandatory order)
- Module README discovery patterns
- Architectural pattern recognition
- Integration point validation

## 3. Grounding Protocol Phases

### Phase 1: Standards Mastery
- CodingStandards.md: Production code requirements
- TestingStandards.md: Test quality requirements
- DocumentationStandards.md: README structure
- TaskManagementStandards.md: Git workflow
- DiagrammingStandards.md: Mermaid conventions

### Phase 2: Project Architecture Context
- Root README.md: Project overview navigation
- Module-specific README hierarchy discovery
- Architectural diagram integration
- Dependency mapping and understanding

### Phase 3: Domain-Specific Context
- Relevant module deep-dive (local README.md)
- Interface contract understanding
- Local convention recognition
- Historical context integration (Section 7)

## 4. Agent-Specific Grounding Patterns
- CodeChanger: Production code patterns and conventions
- TestEngineer: Testing standards and frameworks
- DocumentationMaintainer: Documentation templates and philosophy
- BackendSpecialist: .NET architectural patterns
- FrontendSpecialist: Angular component patterns
- SecurityAuditor: Security patterns and compliance
- [Pattern for each of 11 agents]

## 5. Skills Integration
- Progressive loading through documentation-grounding skill
- Metadata-driven context discovery
- Resource bundling for grounding materials
- Dynamic grounding based on task context

## 6. Optimization Strategies
- Context window management techniques
- Selective loading based on task scope
- Incremental grounding for iterative work
- Caching and reuse patterns

## 7. Quality Validation
- Grounding completeness checks
- Context accuracy validation
- Integration point verification
- Standards compliance confirmation
```

**Content Migration:**
- Moves detailed grounding from CLAUDE.md to authoritative Docs location
- CLAUDE.md references this guide for complete protocols
- Preserves orchestration context while establishing Docs authority

**Cross-References:**
- All /Docs/Standards/ files: Comprehensive grounding sources
- SkillsDevelopmentGuide.md: documentation-grounding skill
- Agent files: Agent-specific patterns
- CLAUDE.md: Orchestration integration

**Acceptance Criteria:**
- ✅ 3-phase grounding workflow clear and actionable
- ✅ All 11 agent patterns specified
- ✅ Skills integration explained comprehensively
- ✅ CLAUDE.md content successfully migrated
- ✅ Optimization strategies practical

---

## Priority 2: Enhancement Documentation (Should-Have)

### 4. ContextManagementGuide.md

**Location:** `/Docs/Development/ContextManagementGuide.md`
**Owner:** DocumentationMaintainer
**Estimated Size:** ~3,000-5,000 words

**Purpose:**
Context window optimization strategies, progressive loading patterns, and resource management.

**Required Sections:**
```markdown
# Context Management Guide

## 1. Purpose & Philosophy
- Context window optimization strategies
- Progressive loading patterns and benefits
- Metadata-driven discovery efficiency
- Resource bundling approaches

## 2. Context Window Challenges
- Token budget management (200k limit Claude Sonnet 4.5)
- Information prioritization techniques
- Incremental context loading strategies
- Context preservation across agent engagements

## 3. Progressive Loading Strategies
- Skills-based progressive loading (metadata → instructions → resources)
- Metadata-driven discovery (98.6% savings during browsing)
- Lazy loading patterns (resources on-demand)
- Context caching and reuse opportunities

## 4. Resource Bundling
- Bundling strategies for related content
- Metadata for efficient bundle discovery
- Dynamic bundle composition patterns
- Performance optimization techniques

## 5. Integration with Orchestration
- Agent context packaging by Claude
- Working directory for context sharing
- Multi-agent context coordination
- Context handoff protocols between agents

## 6. Measurement and Optimization
- Token usage measurement techniques
- Context efficiency metrics (before/after)
- Performance benchmarking
- Continuous optimization patterns

## 7. Best Practices
- Granularity guidelines (skill vs. documentation)
- Metadata design for discovery efficiency
- Performance optimization techniques
- Context validation strategies
```

**Cross-References:**
- SkillsDevelopmentGuide.md: Progressive loading implementation
- DocumentationGroundingProtocols.md: Systematic loading
- CLAUDE.md: Orchestration context

**Acceptance Criteria:**
- ✅ Progressive loading strategies clear
- ✅ Token savings quantified and validated
- ✅ Optimization techniques actionable
- ✅ Integration with orchestration explained

---

### 5. AgentOrchestrationGuide.md

**Location:** `/Docs/Development/AgentOrchestrationGuide.md`
**Owner:** DocumentationMaintainer
**Estimated Size:** ~4,000-6,000 words

**Purpose:**
Enhanced orchestration patterns extracted from CLAUDE.md for comprehensive agent coordination documentation.

**Required Sections:**
```markdown
# Agent Orchestration Guide

## 1. Purpose & Philosophy
- Multi-agent coordination principles
- Delegation-only orchestration model
- Stateless agent engagement patterns
- Quality gate integration

## 2. Orchestration Architecture
- 12-agent team structure and specializations
- Agent authority framework (file-editing, specialists, advisory)
- Flexible authority with intent recognition
- Working directory communication protocols

## 3. Delegation Patterns
- Iterative adaptive planning process
- Context package template construction
- Agent reporting format standards
- Mission-focused result processing

## 4. Multi-Agent Coordination
- Cross-agent workflow orchestration
- Handoff protocols between agents
- Shared context management
- Integration dependencies

## 5. Working Directory Integration
- Artifact discovery mandate (before work)
- Immediate artifact reporting (during work)
- Context integration reporting (building upon others)
- Session state tracking

## 6. Quality Gate Protocols
- ComplianceOfficer pre-PR validation
- AI Sentinels integration (5 sentinels)
- Coverage Excellence tracking
- Standards compliance enforcement

## 7. Emergency Protocols
- Delegation failure escalation
- Violation recovery procedures
- Agent coordination conflicts
- Rollback and remediation

## 8. Best Practices
- Context package construction efficiency
- Agent selection optimization
- Iterative planning adaptation
- Communication excellence patterns
```

**Content Migration:**
- Extracts enhanced orchestration from CLAUDE.md
- Establishes Docs as comprehensive orchestration reference
- CLAUDE.md retains core orchestration, references guide for depth

**Cross-References:**
- CLAUDE.md: Core orchestration authority
- All agent files: Integration patterns
- Working directory protocols: Communication standards
- ComplianceOfficer: Quality gates

**Acceptance Criteria:**
- ✅ Orchestration patterns comprehensive
- ✅ Multi-agent coordination clear
- ✅ Quality gate integration documented
- ✅ CLAUDE.md content successfully extracted

---

## Templates & Schemas

### 6. SkillTemplate.md

**Location:** `/Docs/Templates/SkillTemplate.md`
**Owner:** DocumentationMaintainer
**Estimated Size:** ~1,500-2,500 words

**Purpose:**
Standardized template for creating new skills with consistent structure.

**Structure:**
```markdown
# [Skill Name]

**Version:** X.Y.Z
**Category:** [coordination|technical|meta|workflow]
**Agents:** [agent1, agent2, ALL]
**Token Load**: Metadata ~XXX tokens, Instructions ~XXXX tokens

## Purpose
[1-2 sentences: What problem does this solve?]

## When to Use
[3-5 bullet points: Clear usage triggers]

## Workflow Steps
[5-10 numbered steps: Progressive complexity, actionable guidance]

## Resources

### Templates
[List of template files in resources/templates/]

### Examples
[List of example files in resources/examples/]

### Documentation
[List of deep-dive files in resources/documentation/]

## Agent-Specific Instructions
[Tailored guidance for each target agent]

## Integration Points
[Working directory, quality gates, cross-agent coordination]

## Validation Criteria
[How to verify skill execution success]

## Expected Outputs
[Deliverable specifications]
```

**Acceptance Criteria:**
- ✅ All required sections present
- ✅ Clear placeholder guidance
- ✅ Examples demonstrate structure
- ✅ Integration with metadata.json explained

---

### 7. CommandTemplate.md

**Location:** `/Docs/Templates/CommandTemplate.md`
**Owner:** DocumentationMaintainer
**Estimated Size:** ~1,000-2,000 words

**Purpose:**
Standardized template for creating slash commands with consistent UX.

**Structure:**
```markdown
---
description: "Brief command purpose (one sentence)"
argument-hint: "[required] [optional]"
category: "testing|security|architecture|workflow"
requires-skills: ["optional-skill-dependency"]
---

# [Command Name]

Brief expansion of purpose and value proposition.

## What this command does:
1. [High-level step 1]
2. [High-level step 2]
3. [High-level step 3]

## Usage Examples:
\`\`\`bash
/command-name arg1 arg2
/command-name --option value
/command-name (no args for default behavior)
\`\`\`

## Arguments:
- **arg1** (required): Purpose and constraints
- **arg2** (optional): Purpose and default value
- **--option**: Flag behavior description

## Output Includes:
- [Expected output 1]
- [Expected output 2]

## Technical Implementation:
This command loads the \`skill-name\` skill for execution logic.
[Brief architecture note if relevant]

Perfect for:
- [Use case 1]
- [Use case 2]
```

**Acceptance Criteria:**
- ✅ Frontmatter structure clear
- ✅ All sections templated
- ✅ Skill integration pattern documented
- ✅ Examples comprehensive

---

### 8. JSON Schemas

**Location:** `/Docs/Templates/schemas/`
**Owner:** WorkflowEngineer (schemas), DocumentationMaintainer (documentation)

**Files:**
- `skill-metadata.schema.json`: Metadata validation schema
- `command-definition.schema.json`: Command frontmatter schema

**skill-metadata.schema.json Structure:**
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "Agent Skill Metadata",
  "type": "object",
  "required": ["name", "version", "category", "agents", "description"],
  "properties": {
    "name": {
      "type": "string",
      "pattern": "^[a-z0-9-]+$",
      "description": "Kebab-case skill name"
    },
    "version": {
      "type": "string",
      "pattern": "^\\d+\\.\\d+\\.\\d+$",
      "description": "Semantic version"
    },
    "category": {
      "type": "string",
      "enum": ["coordination", "technical", "meta", "workflow"]
    },
    "agents": {
      "type": "array",
      "items": {"type": "string"},
      "minItems": 1
    },
    "description": {
      "type": "string",
      "maxLength": 200
    },
    "tags": {
      "type": "array",
      "items": {"type": "string"}
    },
    "dependencies": {
      "type": "array",
      "items": {"type": "string"}
    },
    "token_estimate": {
      "type": "object",
      "properties": {
        "metadata": {"type": "integer", "maximum": 150},
        "instructions": {"type": "integer", "minimum": 2000, "maximum": 5000}
      }
    }
  }
}
```

**Acceptance Criteria:**
- ✅ Schemas validate required fields
- ✅ Semantic versioning enforced
- ✅ Token budgets validated
- ✅ Integration with validation scripts

---

## Standards Updates

### 9. DocumentationStandards.md Update

**Location:** `/Docs/Standards/DocumentationStandards.md`
**Owner:** DocumentationMaintainer

**New Section to Add:**
```markdown
## Skills Documentation Requirements

### Metadata Standards
- All skills MUST have metadata.json with required fields
- Token estimates MUST be provided for metadata and instructions
- Category and tags MUST enable accurate discovery
- Agent targeting MUST specify applicable agents

### Resource Organization
- Templates in resources/templates/ for reusable formats
- Examples in resources/examples/ for reference implementations
- Documentation in resources/documentation/ for deep dives
- File naming: kebab-case, descriptive, no version numbers in filenames

### Discovery Mechanism Documentation
- README.md in each skill category directory
- Skill catalog in .claude/skills/README.md
- Cross-references from agent definitions
- Integration examples in documentation

### Progressive Loading Design
- Metadata optimized for discovery (<150 tokens)
- Instructions focused and actionable (2,000-5,000 tokens)
- Resources loaded on-demand, not preloaded
- Clear separation: metadata → instructions → resources
```

**Acceptance Criteria:**
- ✅ Section integrates seamlessly with existing standards
- ✅ No duplication with SkillsDevelopmentGuide.md (guide is comprehensive, standard is requirement)
- ✅ Cross-references to guide and templates clear

---

### 10. TestingStandards.md Update

**Location:** `/Docs/Standards/TestingStandards.md`
**Owner:** DocumentationMaintainer (standards), TestEngineer (validation)

**New Section to Add:**
```markdown
## Skills and Commands Testing

### Skill Testing Requirements
- All skills MUST include validation examples in resources/
- Skill workflows MUST be testable with clear success criteria
- Resource files MUST be validated for accessibility
- Metadata.json MUST pass JSON schema validation

### Command Testing Requirements
- All commands MUST have comprehensive usage examples tested
- Argument parsing MUST be validated with invalid inputs
- Error messages MUST be tested for clarity and actionability
- Integration with skills MUST be validated end-to-end

### Validation Approach
- Pre-commit hooks validate metadata.json and frontmatter
- CI workflows enforce schema validation
- Integration tests verify skill loading and command execution
- Performance tests measure progressive loading efficiency

### Quality Metrics
- Skill discovery overhead: <150 tokens metadata
- Skill loading latency: <1 second for SKILL.md
- Command execution success rate: >95%
- Error message clarity: User can resolve without documentation
```

**Acceptance Criteria:**
- ✅ Testing requirements clear and testable
- ✅ Integration with validation framework documented
- ✅ Quality metrics measurable

---

### 11. TaskManagementStandards.md Update

**Location:** `/Docs/Standards/TaskManagementStandards.md`
**Owner:** DocumentationMaintainer

**New Section to Add:**
```markdown
## Automated Issue Creation Workflows

### GitHub Issue Automation
- Use /create-issue command for consistent issue creation
- Automated context collection eliminates manual effort
- Template selection based on issue type (feature/bug/epic/debt/docs)
- Label application automated per GitHubLabelStandards.md

### Issue Types and Templates
- Feature Request: Enhancement with user value proposition
- Bug Report: Defect with reproduction steps and expected behavior
- Epic: Milestone with component breakdown and acceptance criteria
- Technical Debt: Refactoring with rationale and scope
- Documentation: Knowledge gap with proposed content

### Automation Workflow
1. Identify issue type and gather context
2. Invoke /create-issue <type> "<title>"
3. Skill collects additional context automatically
4. Template applied with proper structure
5. Labels, milestone, assignees automated
6. Issue created via gh CLI

### Quality Standards
- Titles: Clear, actionable, <80 characters
- Descriptions: Complete context using template sections
- Labels: Automated compliance with standards
- Related issues: Automated discovery and linking
```

**Acceptance Criteria:**
- ✅ Automation workflow documented clearly
- ✅ Integration with github-issue-creation skill explained
- ✅ Quality standards for automated issues specified

---

### 12. CodingStandards.md Update (Minimal)

**Location:** `/Docs/Standards/CodingStandards.md`
**Owner:** DocumentationMaintainer

**Brief Note to Add (in Documentation Requirements section):**
```markdown
## Skills and Commands Documentation
For detailed guidance on creating agent skills and slash commands, see:
- [SkillsDevelopmentGuide.md](../Development/SkillsDevelopmentGuide.md)
- [CommandsDevelopmentGuide.md](../Development/CommandsDevelopmentGuide.md)

Skills are documentation/orchestration concerns, not coding standards per se, but all skill resources must follow coding standards when containing code examples.
```

**Acceptance Criteria:**
- ✅ Minimal, non-intrusive addition
- ✅ Cross-reference clear and helpful

---

## Documentation Cross-Reference Strategy

### Phase 1: Epic Documentation Cross-References (Priority 1)
```
SkillsDevelopmentGuide.md ↔ CommandsDevelopmentGuide.md
   (command-skill integration patterns)

DocumentationGroundingProtocols.md ↔ All Standards files
   (systematic standards loading references)

ContextManagementGuide.md ↔ SkillsDevelopmentGuide.md
   (progressive loading implementation)

Templates ↔ Development guides
   (template usage examples and guidance)
```

### Phase 2: Standards Integration (Priority 2)
```
DocumentationStandards.md → SkillsDevelopmentGuide.md
   (metadata requirements reference)

TestingStandards.md → CommandsDevelopmentGuide.md
   (command testing requirements)

TaskManagementStandards.md → CommandsDevelopmentGuide.md
   (GitHub automation workflows)
```

### Phase 3: Comprehensive Navigation (Priority 3)
```
All major files:
- Add "Related Documentation" section
- Bidirectional linking
- Visual documentation map in DOCUMENTATION_INDEX.md
- Enhanced README files with epic documentation references
- CLAUDE.md references to new Development guides
```

**Cross-Reference Pattern Template:**
```markdown
## Related Documentation

**Prerequisites:**
- [DocumentationStandards.md](../Standards/DocumentationStandards.md) - Metadata requirements
- [TaskManagementStandards.md](../Standards/TaskManagementStandards.md) - GitHub workflows

**Integration Points:**
- [CommandsDevelopmentGuide.md](./CommandsDevelopmentGuide.md) - Command-driven skill loading
- [ContextManagementGuide.md](./ContextManagementGuide.md) - Progressive loading patterns

**Orchestration Context:**
- [CLAUDE.md](../../CLAUDE.md) - Multi-agent coordination
- [/.claude/agents/](../../.claude/agents/) - Agent grounding protocols
```

---

## Legacy Workflow Archival (Priority 3)

### Create /Docs/Development/Legacy/ Subdirectory

**Files to Archive:**
1. `CodingPlannerAssistant.md` → `/Docs/Development/Legacy/`
   - Deprecation notice: "Superseded by multi-agent orchestration framework. See CLAUDE.md for current agent coordination patterns."
2. `StandardWorkflow.md` → `/Docs/Development/Legacy/`
   - Deprecation notice: "Superseded by AutomatedTestingCoverageWorkflow.md and multi-agent delegation. See Development/README.md for current workflows."
3. `ComplexTaskWorkflow.md` → `/Docs/Development/Legacy/`
   - Deprecation notice: "Superseded by multi-agent orchestration with iterative adaptive planning. See AgentOrchestrationGuide.md."
4. `TestCoverageWorkflow.md` → `/Docs/Development/Legacy/`
   - Deprecation notice: "Superseded by AutomatedTestingCoverageWorkflow.md with AI-powered coverage generation."

**Update Development README.md:**
```markdown
## Legacy Workflows (Archived)
The following workflows are preserved for historical reference but have been superseded:
- [CodingPlannerAssistant.md](./Legacy/CodingPlannerAssistant.md) - See CLAUDE.md orchestration
- [StandardWorkflow.md](./Legacy/StandardWorkflow.md) - See current automation workflows
- [ComplexTaskWorkflow.md](./Legacy/ComplexTaskWorkflow.md) - See AgentOrchestrationGuide.md
- [TestCoverageWorkflow.md](./Legacy/TestCoverageWorkflow.md) - See AutomatedTestingCoverageWorkflow.md

**Current Workflows:** See above sections for active development patterns.
```

---

## DOCUMENTATION_INDEX.md Update

**Location:** `/Docs/DOCUMENTATION_INDEX.md`
**Owner:** DocumentationMaintainer

**New Sections to Add:**
```markdown
## Epic #291: Skills & Commands Documentation

### Development Guides
- [SkillsDevelopmentGuide.md](./Development/SkillsDevelopmentGuide.md) - Agent skills creation and architecture
- [CommandsDevelopmentGuide.md](./Development/CommandsDevelopmentGuide.md) - Slash commands development
- [DocumentationGroundingProtocols.md](./Development/DocumentationGroundingProtocols.md) - Systematic context loading
- [ContextManagementGuide.md](./Development/ContextManagementGuide.md) - Progressive loading and optimization
- [AgentOrchestrationGuide.md](./Development/AgentOrchestrationGuide.md) - Multi-agent coordination patterns

### Templates
- [SkillTemplate.md](./Templates/SkillTemplate.md) - Standard skill structure
- [CommandTemplate.md](./Templates/CommandTemplate.md) - Standard command structure
- [skill-metadata.schema.json](./Templates/schemas/skill-metadata.schema.json) - Metadata validation
- [command-definition.schema.json](./Templates/schemas/command-definition.schema.json) - Frontmatter validation

### Specifications
- [Epic #291 README](./Specs/epic-291-skills-commands/README.md) - Complete epic specification
- [Implementation Iterations](./Specs/epic-291-skills-commands/implementation-iterations.md) - Detailed iteration breakdown
- [Skills Catalog](./Specs/epic-291-skills-commands/skills-catalog.md) - All 8 skills specifications
- [Commands Catalog](./Specs/epic-291-skills-commands/commands-catalog.md) - All 4 commands specifications
```

**Visual Documentation Map:**
```
Epic #291 Documentation Ecosystem:
┌─────────────────────────────────────────────────────┐
│ Development Guides (Comprehensive How-To)           │
│ ├─ SkillsDevelopmentGuide.md                        │
│ ├─ CommandsDevelopmentGuide.md                      │
│ ├─ DocumentationGroundingProtocols.md               │
│ ├─ ContextManagementGuide.md                        │
│ └─ AgentOrchestrationGuide.md                       │
└─────────────────────────────────────────────────────┘
           ↓ references
┌─────────────────────────────────────────────────────┐
│ Templates (Structural Patterns)                     │
│ ├─ SkillTemplate.md                                 │
│ ├─ CommandTemplate.md                               │
│ └─ schemas/ (JSON validation)                       │
└─────────────────────────────────────────────────────┘
           ↓ validates
┌─────────────────────────────────────────────────────┐
│ Standards (Requirements)                            │
│ ├─ DocumentationStandards.md (updated)              │
│ ├─ TestingStandards.md (updated)                    │
│ ├─ TaskManagementStandards.md (updated)             │
│ └─ CodingStandards.md (minimal update)              │
└─────────────────────────────────────────────────────┘
           ↓ enforces
┌─────────────────────────────────────────────────────┐
│ Specifications (Epic Details)                       │
│ └─ Specs/epic-291-skills-commands/                  │
│    ├─ README.md (Epic overview)                     │
│    ├─ implementation-iterations.md                  │
│    ├─ skills-catalog.md                             │
│    └─ commands-catalog.md                           │
└─────────────────────────────────────────────────────┘
```

---

## Implementation Timeline

### Iteration 3: Documentation Alignment (Priority 1)

**Days 1-4: Core Development Guides**
- Day 1-2: SkillsDevelopmentGuide.md (comprehensive, ~8k words)
- Day 3-4: CommandsDevelopmentGuide.md (comprehensive, ~7k words)

**Days 5-6: Grounding and Context**
- Day 5: DocumentationGroundingProtocols.md (move from CLAUDE.md, ~6k words)
- Day 6: Templates (SkillTemplate.md, CommandTemplate.md, ~3k words total)

**Days 7-8: Standards Updates**
- Day 7: Update 4 standards files (DocumentationStandards, TestingStandards, TaskManagementStandards, CodingStandards)
- Day 8: JSON schemas creation and validation script integration

**Total Effort:** 8-9 days for Priority 1 documentation

### Priority 2: Enhancement Guides (Post-Iteration 3)

**Days 9-11: Advanced Guides**
- Day 9-10: ContextManagementGuide.md (~5k words)
- Day 11: AgentOrchestrationGuide.md (extract from CLAUDE.md, ~6k words)

### Priority 3: Navigation and Legacy (Final Polish)

**Days 12-13: Cross-References and Archive**
- Day 12: Comprehensive cross-references across all files
- Day 13: Legacy workflow archival, DOCUMENTATION_INDEX.md update, visual map creation

---

## Success Criteria

### Documentation Completeness
- ✅ All 7 new guides created with comprehensive content
- ✅ All 4 standards updated appropriately
- ✅ All 2 templates usable for creation without clarification
- ✅ All 2 JSON schemas validate metadata and frontmatter
- ✅ Cross-references comprehensive and bidirectional
- ✅ Navigation <5 minutes from any entry point

### Docs as Source of Truth
- ✅ CLAUDE.md simplified with clear references to Docs
- ✅ Agent definitions reference Docs for comprehensive details
- ✅ No duplication of standards content across Docs and .claude/
- ✅ Acceptable optimization duplication in CLAUDE.md with links to Docs authority
- ✅ Vendor-agnostic strategy preserved

### Usability Validation
- ✅ Skills can be created following SkillsDevelopmentGuide.md alone
- ✅ Commands can be created following CommandsDevelopmentGuide.md alone
- ✅ Agents can load documentation following DocumentationGroundingProtocols.md
- ✅ Templates reduce creation effort by 50%+
- ✅ Schemas prevent invalid metadata/frontmatter commits

---

**Documentation Plan Status:** ✅ **COMPLETE**

**Next Actions:**
1. Begin Iteration 3 documentation creation
2. Draft SkillsDevelopmentGuide.md and CommandsDevelopmentGuide.md
3. Move DocumentationGroundingProtocols.md content from CLAUDE.md
4. Create templates and schemas
5. Update standards files appropriately
6. Establish comprehensive cross-references
