# Implementation Iterations - Epic #291

**Last Updated:** 2025-10-25
**Purpose:** Detailed iteration breakdown with specific tasks, acceptance criteria, and handoff protocols

---

## Iteration 1: Foundation (Core Skills + Templates)

**Objective:** Establish foundational skills infrastructure and creation templates

### Deliverables

#### 1.1 Core Coordination Skills
**Owner:** PromptEngineer

**Tasks:**
- [ ] Create `working-directory-coordination` skill
  - Extract protocols from all 11 agents (~500 lines redundancy)
  - Create artifact discovery, reporting, integration templates
  - Build multi-agent coordination examples
  - Document communication compliance enforcement
- [ ] Create `documentation-grounding` skill
  - Extract grounding protocols from 10 agents (~400 lines redundancy)
  - Define Phase 1-3 systematic loading workflow
  - Create agent-specific grounding patterns
  - Build standards integration examples
- [ ] Create `core-issue-focus` skill
  - Extract from TestEngineer, PromptEngineer, CodeChanger, BackendSpecialist (~200 lines redundancy)
  - Define mission drift prevention framework
  - Create scope discipline validation patterns
  - Build success criteria templates

**Estimated Effort:** 5-7 days
**Dependencies:** None - foundational patterns

**Acceptance Criteria:**
- ✅ All 3 skills have complete SKILL.md with YAML frontmatter (<500 lines recommended), resources/
- ✅ YAML frontmatter includes required fields: name, description per [official specification](./official-skills-structure.md)
- ✅ At least 2 agents successfully load and use working-directory-coordination skill
- ✅ Progressive loading validated: frontmatter → instructions → resources pattern works
- ✅ Context savings measurable (>2,000 tokens)
- ✅ Agent integration snippets validated and documented

#### 1.2 GitHub Workflow Skill
**Owner:** PromptEngineer (skill), WorkflowEngineer (workflow integration validation)

**Tasks:**
- [ ] Create `github-issue-creation` skill
  - Design 4-phase workflow (context collection, template selection, issue construction, validation)
  - Create templates for feature/bug/epic/debt/docs issues
  - Build label application patterns per GitHubLabelStandards.md
  - Implement gh CLI integration examples
  - Document context gathering automation

**Estimated Effort:** 3-4 days
**Dependencies:** None

**Acceptance Criteria:**
- ✅ Skill enables comprehensive issue creation workflow
- ✅ All 5 issue type templates created and validated
- ✅ Label compliance automated and tested
- ✅ Context collection eliminates manual "hand bombing"
- ✅ Integration with /create-issue command prepared

#### 1.3 Skill and Command Templates
**Owner:** PromptEngineer (templates), DocumentationMaintainer (template documentation)

**Tasks:**
- [ ] Create SkillTemplate.md in /Docs/Templates/
  - Define SKILL.md structure with YAML frontmatter and complete sections
  - Specify YAML frontmatter required fields (name, description) per [official specification](./official-skills-structure.md)
  - Document resource organization patterns
  - Provide usage examples
- [ ] Create CommandTemplate.md in /Docs/Templates/
  - Define frontmatter structure with required fields
  - Specify command documentation sections
  - Document argument handling patterns
  - Provide integration examples
- [ ] Create YAML schemas in /Docs/Templates/schemas/
  - skill-frontmatter.schema.yaml with validation rules (name, description constraints)
  - command-definition.schema.json with frontmatter schema

**Estimated Effort:** 2-3 days
**Dependencies:** Understanding of skill/command architecture from 1.1-1.2

**Acceptance Criteria:**
- ✅ Templates enable creation without external clarification
- ✅ YAML schema validates skill frontmatter (name, description per [official specification](./official-skills-structure.md))
- ✅ JSON schema validates command frontmatter
- ✅ Examples demonstrate all template sections
- ✅ Integration with validation scripts prepared

#### 1.4 Validation Framework
**Owner:** WorkflowEngineer (scripts), TestEngineer (validation testing)

**Tasks:**
- [ ] Create skill frontmatter validation script
  - YAML frontmatter parsing from SKILL.md
  - Required field verification (name, description per [official specification](./official-skills-structure.md))
  - Name constraints check (max 64 chars, lowercase/numbers/hyphens only, no reserved words)
  - Description constraints check (non-empty, max 1024 chars)
  - Error if metadata.json file found (incorrect structure)
- [ ] Create command frontmatter validation script
  - YAML frontmatter parsing
  - Required field enforcement
  - Category validation
- [ ] Integrate validation into pre-commit hooks
- [ ] Create CI validation workflow

**Estimated Effort:** 2-3 days
**Dependencies:** Templates from 1.3

**Acceptance Criteria:**
- ✅ Validation scripts execute successfully
- ✅ Pre-commit hooks prevent invalid commits
- ✅ CI workflow validates all skills/commands
- ✅ Clear error messages guide correction

### Iteration 1 Handoffs

**To Iteration 2:**
- Working-directory-coordination skill adopted by all 11 agents
- Skill creation patterns established and validated
- Templates enable consistent skill/command development
- Foundation ready for meta-skills development

**To DocumentationMaintainer:**
- Skill integration patterns documented for agent refactoring
- Template usage validated for Priority 1 documentation

**To TestEngineer:**
- Validation framework operational for quality assurance

---

## Iteration 2: Meta-Skills & Commands (Scalability Framework)

**Objective:** Enable PromptEngineer scalability and developer workflow automation

### Deliverables

#### 2.1 Agent Creation Meta-Skill
**Owner:** PromptEngineer

**Tasks:**
- [ ] Create `agent-creation` meta-skill
  - Define 5-phase agent design workflow (identity, structure, authority, skills, optimization)
  - Create templates for specialist vs. primary vs. advisory agents
  - Document authority framework design patterns
  - Build skill integration guidance
  - Provide agent refactoring examples
- [ ] Create agent-definition-template.md variants
  - specialist-agent-template.md (flexible authority)
  - primary-agent-template.md (file-editing focus)
  - advisory-agent-template.md (working directory only)

**Estimated Effort:** 4-5 days
**Dependencies:** Core skills operational (Iteration 1)

**Acceptance Criteria:**
- ✅ PromptEngineer creates new agent 50% faster
- ✅ Agent structure consistent across all variants
- ✅ Context optimization patterns applied automatically
- ✅ Skill references integrated from creation
- ✅ Authority boundaries clear and validated

#### 2.2 Skill Creation Meta-Skill
**Owner:** PromptEngineer

**Tasks:**
- [ ] Create `skill-creation` meta-skill
  - Define 5-phase skill design workflow (scope, structure, progressive loading, resources, integration)
  - Document skill categorization patterns (coordination/technical/meta/workflow)
  - Build progressive loading design framework
  - Create resource organization best practices
  - Provide metadata design examples

**Estimated Effort:** 3-4 days
**Dependencies:** Skill creation patterns from Iteration 1

**Acceptance Criteria:**
- ✅ New skills follow consistent structure
- ✅ Progressive loading optimized from design phase
- ✅ Context efficiency targets met (<150 metadata, <5k instructions)
- ✅ Resource organization standardized
- ✅ Integration patterns documented

#### 2.3 Command Creation Meta-Skill
**Owner:** PromptEngineer (skill), WorkflowEngineer (workflow integration)

**Tasks:**
- [ ] Create `command-creation` meta-skill
  - Define 5-phase command design workflow (scope, structure, skill integration, arguments, UX)
  - Document command vs. skill philosophy
  - Build argument handling patterns (positional, named, flags, defaults)
  - Create error handling and feedback standards
  - Provide skill integration examples

**Estimated Effort:** 3-4 days
**Dependencies:** Command template from Iteration 1

**Acceptance Criteria:**
- ✅ Commands have consistent UX patterns
- ✅ Command-skill integration clear
- ✅ Argument handling robust
- ✅ Error messages helpful and actionable
- ✅ Integration with bash/gh CLI documented

#### 2.4 Workflow Commands Implementation
**Owner:** WorkflowEngineer (commands), BackendSpecialist/FrontendSpecialist (validation)

**Tasks:**
- [ ] Implement `/workflow-status` command
  - gh CLI integration for workflow run listing
  - Status filtering and detailed views
  - Real-time monitoring capabilities
- [ ] Implement `/coverage-report` command
  - Artifact retrieval from latest runs
  - Coverage data parsing and trending
  - Epic progression analytics
- [ ] Implement `/create-issue` command
  - Integration with github-issue-creation skill
  - Argument parsing and template selection
  - gh CLI issue creation automation
- [ ] Implement `/merge-coverage-prs` command
  - Integration with coverage-merge-orchestration workflow
  - Dry-run and live execution modes
  - Real-time orchestrator monitoring

**Estimated Effort:** 6-8 days (2 days per command)
**Dependencies:** github-issue-creation skill (Iteration 1), meta-skills for structure

**Acceptance Criteria:**
- ✅ All 4 commands execute successfully
- ✅ Argument parsing robust with clear errors
- ✅ gh CLI integration functional
- ✅ Output formatting clear and actionable
- ✅ Workflow integration validated
- ✅ Documentation complete with examples

### Iteration 2 Handoffs

**To Iteration 3:**
- Meta-skills operational and validated
- PromptEngineer productivity enhanced
- Workflow commands functional
- Agent/skill/command creation scalable

**To PromptEngineer:**
- Meta-skills available for agent refactoring
- Command creation framework for future development

**To WorkflowEngineer:**
- Commands integrated with CI/CD workflows
- Monitoring and automation streamlined

---

## Iteration 3: Documentation Alignment (Docs as Source of Truth)

**Objective:** Create comprehensive documentation establishing /Docs/ as authoritative knowledge repository

### Deliverables

#### 3.1 Skills Development Guide
**Owner:** DocumentationMaintainer

**Tasks:**
- [ ] Create /Docs/Development/SkillsDevelopmentGuide.md
  - Document skills architecture and philosophy
  - Define metadata schema and discovery mechanisms
  - Establish resource bundling patterns
  - Explain integration with agent orchestration
  - Provide creation workflow with examples
  - Document best practices and optimization

**Estimated Effort:** 3-4 days
**Dependencies:** Skills operational (Iteration 1-2)

**Acceptance Criteria:**
- ✅ Guide enables skill creation without external clarification
- ✅ All sections complete with clear examples
- ✅ Cross-references to standards comprehensive
- ✅ Integration with SkillTemplate.md seamless
- ✅ Navigation from other documentation clear

#### 3.2 Commands Development Guide
**Owner:** DocumentationMaintainer

**Tasks:**
- [ ] Create /Docs/Development/CommandsDevelopmentGuide.md
  - Document command architecture and philosophy
  - Define command-skill separation patterns
  - Establish argument handling best practices
  - Explain integration with workflows
  - Document GitHub automation patterns
  - Provide creation workflow with examples

**Estimated Effort:** 3-4 days
**Dependencies:** Commands operational (Iteration 2)

**Acceptance Criteria:**
- ✅ Guide enables command creation without external clarification
- ✅ Command-skill boundary clear
- ✅ Argument patterns comprehensive
- ✅ Integration with CommandTemplate.md seamless
- ✅ Workflow automation documented

#### 3.3 Documentation Grounding Protocols
**Owner:** DocumentationMaintainer

**Tasks:**
- [ ] Create /Docs/Development/DocumentationGroundingProtocols.md
  - Move detailed grounding content from CLAUDE.md
  - Document 3-phase systematic loading (Standards → Project → Domain)
  - Define agent-specific grounding patterns for all 11 agents
  - Explain skills integration for progressive loading
  - Document optimization strategies
  - Provide validation criteria

**Estimated Effort:** 2-3 days
**Dependencies:** documentation-grounding skill (Iteration 1)

**Acceptance Criteria:**
- ✅ Complete grounding workflow documented
- ✅ All 11 agent patterns specified
- ✅ Skills integration explained
- ✅ CLAUDE.md content successfully moved
- ✅ Cross-references to all standards

#### 3.4 Standards Updates
**Owner:** DocumentationMaintainer

**Tasks:**
- [ ] Update DocumentationStandards.md
  - Add skills metadata requirements section
  - Define resource organization conventions
  - Establish discovery mechanism documentation
- [ ] Update TestingStandards.md
  - Add skills and commands testing section
  - Define validation approach
  - Document integration testing patterns
- [ ] Update TaskManagementStandards.md
  - Add automated issue creation workflows
  - Document command-driven automation
  - Update GitHub integration patterns
- [ ] Update CodingStandards.md (minimal)
  - Add brief note referencing SkillsDevelopmentGuide.md

**Estimated Effort:** 2-3 days
**Dependencies:** Guides from 3.1-3.3

**Acceptance Criteria:**
- ✅ All 4 standards updated appropriately
- ✅ No duplication with new guides
- ✅ Cross-references comprehensive
- ✅ Integration with existing content seamless

#### 3.5 Context Management and Orchestration Guides
**Owner:** DocumentationMaintainer

**Tasks:**
- [ ] Create /Docs/Development/ContextManagementGuide.md
  - Document progressive loading strategies
  - Explain metadata-driven discovery
  - Define resource bundling approaches
  - Provide performance optimization patterns
- [ ] Create /Docs/Development/AgentOrchestrationGuide.md
  - Move enhanced orchestration patterns from CLAUDE.md
  - Document multi-agent coordination workflows
  - Explain working directory integration
  - Define quality gate protocols

**Estimated Effort:** 3-4 days
**Dependencies:** Understanding of skills architecture and CLAUDE.md content

**Acceptance Criteria:**
- ✅ Progressive loading strategies clear
- ✅ Orchestration patterns comprehensive
- ✅ CLAUDE.md content successfully extracted
- ✅ Integration with existing workflows documented

### Iteration 3 Handoffs

**To Iteration 4:**
- Priority 1 documentation complete
- Docs as source of truth established
- Agent refactoring documentation ready
- CLAUDE.md simplification prepared

**To All Agents:**
- Documentation grounding protocols available
- Skills and commands creation guides accessible
- Standards updates integrated

---

## Iteration 4: Agent Refactoring (Incremental Context Optimization)

**Objective:** Refactor all 11 agents with 60%+ context reduction while preserving effectiveness

### Approach: Largest Agents First for Maximum Savings

#### 4.1 High-Impact Agents (Days 1-5)
**Owner:** PromptEngineer

**Agents:**
1. FrontendSpecialist (550 → 180 lines, 67% reduction, Day 1)
2. BackendSpecialist (536 → 180 lines, 66% reduction, Day 2)
3. TestEngineer (524 → 200 lines, 62% reduction, Day 3)
4. DocumentationMaintainer (534 → 190 lines, 64% reduction, Day 4)
5. WorkflowEngineer (510 → 200 lines, 61% reduction, Day 5)

**Per-Agent Process:**
- [ ] Extract redundant patterns to skills (working-directory, documentation-grounding, domain skills)
- [ ] Add skill references with 2-3 line summaries
- [ ] Update authority sections for clarity
- [ ] Validate orchestration integration
- [ ] Test agent engagement with skill loading
- [ ] Document any issues or refinements needed

**Cumulative Savings:** ~1,800 lines eliminated after Day 5

**Acceptance Criteria per Agent:**
- ✅ 60%+ context reduction achieved
- ✅ Agent effectiveness preserved (successful task completion)
- ✅ Skill references clear and functional
- ✅ Orchestration integration validated
- ✅ Progressive loading works correctly

#### 4.2 Medium-Impact Agents (Days 6-8)
**Owner:** PromptEngineer

**Agents:**
6. CodeChanger (488 → 170 lines, 65% reduction, Day 6)
7. SecurityAuditor (453 → 180 lines, 60% reduction, Day 7)
8. BugInvestigator (449 → 170 lines, 62% reduction, Day 8)

**Cumulative Savings:** ~2,650 lines eliminated after Day 8

**Acceptance Criteria:** Same as 4.1

#### 4.3 Lower-Impact Agents (Days 9-11)
**Owner:** PromptEngineer

**Agents:**
9. ArchitecturalAnalyst (437 → 170 lines, 61% reduction, Day 9)
10. PromptEngineer (413 → 180 lines, 56% reduction, Day 10)
11. ComplianceOfficer (316 → 160 lines, 49% reduction, Day 11)

**Total Savings:** ~3,230 lines eliminated (62% average reduction)

**Acceptance Criteria:** Same as 4.1

### Iteration 4 Validation

**After Each Agent:**
- Test agent engagement with typical tasks
- Validate skill loading and progressive access
- Confirm orchestration handoff patterns
- Verify working directory communication
- Document any effectiveness regression

**After All Agents:**
- Comprehensive multi-agent workflow testing
- Token savings measurement across typical sessions
- Performance benchmarking (loading latency)
- Quality gate integration verification
- Team acceptance testing

### Iteration 4 Handoffs

**To Iteration 5:**
- All 11 agents refactored and validated
- Total context reduction: 62% average
- Agent effectiveness preserved
- CLAUDE.md optimization ready

**To Claude (Codebase Manager):**
- Agent refactoring complete
- Orchestration patterns validated
- Integration testing results

---

## Iteration 5: Integration & Validation (Quality Assurance)

**Objective:** Optimize CLAUDE.md, comprehensive testing, performance validation, team training

### Deliverables

#### 5.1 CLAUDE.md Optimization
**Owner:** PromptEngineer

**Tasks:**
- [ ] Extract detailed agent descriptions to agent files (30 lines saved)
- [ ] Extract working directory protocols to skill reference (30 lines saved)
- [ ] Streamline context package template (15 lines saved)
- [ ] Optimize project context with Docs references (30 lines saved)
- [ ] Reduce tool/command details with skill/guide references (20 lines saved)
- [ ] Streamline agent reporting format (10 lines saved)
- [ ] Update multi-agent team section (60 lines saved)
- [ ] Validate orchestration logic preservation

**Target:** 673 → 475 lines (198 lines = 29% reduction)

**Estimated Effort:** 2-3 days
**Dependencies:** All agents refactored (Iteration 4), documentation complete (Iteration 3)

**Acceptance Criteria:**
- ✅ 25-30% reduction achieved (29% target)
- ✅ Orchestration logic 100% preserved
- ✅ Skill references integrated clearly
- ✅ Docs cross-references comprehensive
- ✅ Delegation protocols maintained

#### 5.2 Comprehensive Integration Testing
**Owner:** TestEngineer (testing), ComplianceOfficer (validation)

**Tasks:**
- [ ] Test all 11 agents with skill loading
  - Validate progressive loading for each agent
  - Confirm resource access patterns work
  - Test multi-agent coordination workflows
- [ ] Test all 4 workflow commands
  - Execute /workflow-status with various arguments
  - Validate /coverage-report data accuracy
  - Test /create-issue end-to-end automation
  - Execute /merge-coverage-prs dry-run and live
- [ ] Integration testing across components
  - Agent skill loading → skill execution → artifact generation
  - Command invocation → skill delegation → workflow execution
  - Documentation grounding → standards loading → agent work
- [ ] Performance testing
  - Measure token savings in typical sessions
  - Validate progressive loading latency acceptable
  - Benchmark skill discovery overhead
- [ ] Quality gate validation
  - ComplianceOfficer pre-PR validation
  - AI Sentinels compatibility
  - Coverage excellence integration

**Estimated Effort:** 4-5 days
**Dependencies:** All components complete (Iterations 1-4)

**Acceptance Criteria:**
- ✅ All agents load skills successfully
- ✅ All commands execute functionally
- ✅ Multi-agent workflows seamless
- ✅ Token savings >8,000 per session validated
- ✅ Performance targets met
- ✅ Quality gates passing

#### 5.3 Performance Validation and Optimization
**Owner:** ArchitecturalAnalyst (analysis), PromptEngineer (optimization)

**Tasks:**
- [ ] Measure actual context savings
  - Agent definition reduction validation
  - CLAUDE.md reduction confirmation
  - Session-level token savings measurement
- [ ] Identify optimization opportunities
  - Skill content refinement
  - Template enhancement
  - Resource organization improvements
- [ ] Refine based on performance data
  - Optimize high-latency skills
  - Enhance frequently-used resources
  - Improve metadata discovery efficiency

**Estimated Effort:** 2-3 days
**Dependencies:** Integration testing complete (5.2)

**Acceptance Criteria:**
- ✅ 20-30% context reduction validated (62% avg achieved)
- ✅ >8,000 tokens saved per session confirmed
- ✅ Progressive loading <150 token overhead
- ✅ Skill loading latency acceptable (<1 sec)
- ✅ Optimization opportunities documented

#### 5.4 Documentation Finalization
**Owner:** DocumentationMaintainer

**Tasks:**
- [ ] Create comprehensive cross-references
  - Add "Related Documents" to all major files
  - Ensure bidirectional linking
  - Validate all internal links functional
- [ ] Update DOCUMENTATION_INDEX.md
  - Add epic documentation sections
  - Update coverage gap analysis
  - Create visual documentation map
- [ ] Archive legacy workflows
  - Create /Docs/Development/Legacy/ subdirectory
  - Move superseded files with deprecation notices
  - Update Development README.md
- [ ] Epic specification archive
  - Move working-dir artifacts to /Docs/Specs/epic-291-skills-commands/
  - Preserve implementation decisions
  - Document historical context

**Estimated Effort:** 2-3 days
**Dependencies:** All documentation created (Iteration 3)

**Acceptance Criteria:**
- ✅ Cross-references comprehensive
- ✅ Navigation <5 min from any entry point
- ✅ DOCUMENTATION_INDEX.md current
- ✅ Legacy content clearly marked
- ✅ Epic history preserved

#### 5.5 Team Training and Rollout
**Owner:** Claude (Codebase Manager)

**Tasks:**
- [ ] Create training materials
  - Skills usage guide for all agents
  - Commands reference for workflows
  - Documentation navigation training
- [ ] Conduct training sessions
  - Agent skill adoption patterns
  - Command execution demonstrations
  - Documentation grounding updates
- [ ] Gradual rollout preparation
  - Opt-in initial usage
  - Monitoring and feedback collection
  - Issue tracking and resolution
- [ ] Final acceptance validation
  - All acceptance criteria verified
  - Team proficiency confirmed
  - Epic completion review

**Estimated Effort:** 3-4 days
**Dependencies:** All components complete and validated

**Acceptance Criteria:**
- ✅ Training materials comprehensive
- ✅ Team proficiency demonstrated
- ✅ All epic acceptance criteria met
- ✅ No critical issues or regressions
- ✅ Epic ready for closure

### Iteration 5 Completion

**Epic Success Validation:**
- ✅ All 11 agents refactored with 60%+ reduction
- ✅ CLAUDE.md optimized with 25%+ reduction
- ✅ All 5 core skills operational
- ✅ All 3 meta-skills enabling scalability
- ✅ All 4 commands functional
- ✅ Priority 1 documentation complete
- ✅ All quality gates passing
- ✅ Performance targets achieved
- ✅ Team trained and proficient

---

## Handoff Protocols Between Iterations

### Iteration → Next Iteration
**Deliverables:**
- Working directory artifact with completion summary
- Outstanding issues documented
- Next iteration dependencies clarified
- Recommendations based on learnings

### Iteration → Ongoing Work
**Continuous Integration:**
- Documentation updates as patterns evolve
- Template refinements based on usage
- Skill/command additions following established patterns
- Ongoing optimization based on metrics

### Escalation Triggers
**When to Escalate to Claude:**
- Acceptance criteria not met after reasonable iteration
- Breaking changes discovered requiring architectural decisions
- Resource constraints impacting delivery
- Cross-component integration failures
- User feedback requiring scope adjustments

---

**Implementation Approach:** Iterative and incremental with clear acceptance criteria at each stage. Each iteration builds upon previous foundations with explicit handoff protocols ensuring continuity and quality.
